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

namespace BaroJunk
{
  public partial class ConfigManager
  {
    public void ConfigLoaded()
    {
      Config.ReactiveCore.RaiseUpdated();

      if (Config.Facades.NetFacade.IsClient)
      {
        ClientNetManager.ConfigUpdated();
      }
      else
      {
        ServerNetManager.ConfigUpdated();
      }
    }

    public void RecievedConfigSync()
    {
      Config.ReactiveCore.RaiseUpdated();
    }

    public void ReactivePropChanged()
    {
      if (Config.Facades.NetFacade.IsClient)
      {
        ClientNetManager.ReactivePropChanged();
      }
      else
      {
        ServerNetManager.ReactivePropChanged();
      }
    }

    public void ConfigUpdatedCalledByUser()
    {
      // Don't
    }
  }
}