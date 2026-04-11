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
  public class ConfigSettings
  {
    public ConfigCore Config;
    public ConfigSettings(ConfigCore config) => Config = config;

    /// <summary>
    /// When set CommandsManager will create and setup new command
    /// </summary>
    public string CommandName
    {
      get => Config.Manager.CommandsManager.CommandName;
      set => Config.Manager.CommandsManager.CommandName = value;
    }

    /// <summary>
    /// Deep reactivity wasn't planned and is very unoptimized, so it's optional and defaults to false
    /// When true OnPropChanged and OnUpdated will also trigger on nested configs with paths relative to them
    /// </summary>
    public bool DeeplyReactive
    {
      get => Config.ReactiveCore.DeeplyReactive;
      set => Config.ReactiveCore.DeeplyReactive = value;
    }

    /// <summary>
    /// This is a complex setting that should be set as a whole to avoid temporal coupling
    /// Setting strategy activates required managers
    /// Use static options on ConfigStrategy class
    /// </summary>
    public ConfigStrategy Strategy
    {
      get => strategy;
      set
      {
        strategy = value;
        Config.Manager.UseStrategy(value);
      }
    }
    private ConfigStrategy strategy;

    /// <summary>
    /// If true will Sync config over network on any reactive prop change, why not?
    /// </summary>
    public bool SyncOnPropChanged { get; set; } = true;

    /// <summary>
    /// Set this if you want to save config in some specific location
    /// If left unset, AutoSaver will set it to ModSettings/Configs/{Target.GetType().Namespace}_{Target.GetType().Name}.xml
    /// </summary>
    public string SavePath { get; set; } = null;

    /// <summary>
    /// If you wan't to log it as xml, it was usefull when config serialization was borked, now it's obsolete
    /// </summary>
    public bool PrintAsXML { get; set; } = false;
  }
}