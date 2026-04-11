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
    public ConfigCore Config;

    public void UseStrategy(ConfigStrategy strategy)
    {
      CurrentStrategy = strategy;

      AutoSaver.UseStrategy(strategy.AutoSaverStrategy);

      if (Config.Facades.NetFacade.IsClient)
      {
        ClientNetManager.UseStrategy(strategy.NetManagerStrategy);
      }
      else
      {
        ServerNetManager.UseStrategy(strategy.NetManagerStrategy);
      }
    }

    public ConfigStrategy CurrentStrategy;

    public ConfigAutoSaver AutoSaver;
    public ConfigClientNetManager ClientNetManager;
    public ConfigServerNetManager ServerNetManager;
    public ConfigCommandsManager CommandsManager;


    public ConfigManager(ConfigCore config)
    {
      Config = config;
      AutoSaver = new ConfigAutoSaver(config);
      ClientNetManager = new ConfigClientNetManager(config);
      ServerNetManager = new ConfigServerNetManager(config);
      CommandsManager = new ConfigCommandsManager(config);
    }
  }
}