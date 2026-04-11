using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;
using Barotrauma.Networking;

namespace BaroJunk
{
  public class ConfigClientNetManager
  {
    public ConfigCore Config;
    public ConfigClientNetManager(ConfigCore config) => Config = config;

    private bool enabled; public bool Enabled
    {
      get => enabled;
      set
      {
        bool wasEnabled = enabled;
        enabled = value;
        if (!wasEnabled && enabled) Initialize();
      }
    }

    public void ReactivePropChanged()
    {
      if (!Enabled || !Config.Settings.SyncOnPropChanged) return;
      Config.Sync();
    }

    public void ConfigUpdated()
    {
      if (!Enabled) return;
      Config.Sync();
    }

    public void UseStrategy(NetManagerStrategy strategy)
    {
      Enabled = strategy.NetSync;
    }

    private void Initialize()
    {
      if (!Config.Facades.NetFacade.IsMultiplayer) return;
      Config.Facades.NetFacade.ListenForServer(Config.NetHeader + "_sync", Receive);

      Config.Facades.NetFacade.ClientSend(Config.NetHeader + "_ask");
    }

    public void Receive(IReadMessage msg)
    {
      if (!Enabled) return;
      Config?.NetDecode(msg);
    }
  }
}