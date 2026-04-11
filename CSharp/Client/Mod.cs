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


    public static Settings Settings { get; } = new();
    public static Logger Logger = new Logger()
    {
      PrintFilePath = false
    };




    public void Initialize()
    {
      Logger.Log(Settings.LevelRenderer.WaterParticles.ColdCaverns);

    }

    public void DestroyStaticFields()
    {
      foreach (FieldInfo fi in typeof(Mod).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
      {
        if (!fi.FieldType.IsPrimitive)
        {
          fi.SetValue(this, null);
        }
      }
    }


    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {
      DestroyStaticFields();

    }
  }
}