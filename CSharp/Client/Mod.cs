using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using BaroJunk;
using System.Text.Json;

namespace RemoveAll
{

  public partial class Mod : IAssemblyPlugin
  {
    public IPluginManagementService PluginService { get; set; }


    public static Logger Logger = new Logger()
    {
      PrintFilePath = true
    };

    public void Initialize()
    {


    }


    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {

    }
  }
}