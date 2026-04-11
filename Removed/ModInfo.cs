using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Runtime.CompilerServices;

namespace BaroJunk
{

  /// <summary>
  /// Static class with some info about package
  /// Generally a wrapper around this magnificence
  /// public bool TryGetPackageForPlugin<T>(out ContentPackage package) where T : IAssemblyPlugin
  /// </summary>
  public class ModInfo(IPluginManagementService PluginService)
  {
    public string AssemblyName => Assembly.GetExecutingAssembly().GetName().Name;
    public string HookId => Assembly.GetExecutingAssembly().GetName().Name;
    public string BarotraumaPath => Path.GetFullPath("./");


    public ContentPackage ModPackage<T>() => ModPackage(typeof(T));
    public ContentPackage ModPackage(Type T)
    {


      foreach (var (guid, set) in _._pluginTypes)
      {
        if (set.Contains(T)) return _._reverseLookupGuidList[guid];
      }

      // throw new System.ExecutionEngineException("you died");
      return null;
    }

    public string ModDir<PluginType>() where PluginType : IAssemblyPlugin => ModDir(typeof(PluginType));
    public string ModDir(Type PluginType) => ModPackage(PluginType).Dir;

    public string ModVersion<PluginType>() where PluginType : IAssemblyPlugin => ModVersion(typeof(PluginType));
    public string ModVersion(Type PluginType) => ModPackage(PluginType).ModVersion;

    public string ModName<PluginType>() where PluginType : IAssemblyPlugin => ModName(typeof(PluginType));
    public string ModName(Type PluginType) => ModPackage(PluginType).Name;
  }
}