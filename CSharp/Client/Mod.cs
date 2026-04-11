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

    public static Harmony Harmony { get; set; } = new Harmony("Remove.All");
    public static Settings Settings { get; } = new();
    public static BlackList BlackList { get; } = new();
    public static Logger Logger = new Logger()
    {
      PrintFilePath = false
    };




    public void Initialize()
    {
      Logger.Log(Settings.LevelRenderer.WaterParticles.ColdCaverns);
    }

    public void PatchAll()
    {
      DecalPatch.Patch(Harmony);
      BackgroundCreatureManagerPatch.Patch(Harmony);
      LevelPatch.Patch(Harmony);
      LevelObjectManagerPatch.Patch(Harmony);
      LevelRendererPatch.Patch(Harmony);
      LightManagerPatch.Patch(Harmony);
      LightSourcePatch.Patch(Harmony);
      ParticleManagerPatch.Patch(Harmony);
      SubmarinePatch.Patch(Harmony);
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
      Harmony.UnpatchSelf();
      DestroyStaticFields();
    }
  }
}