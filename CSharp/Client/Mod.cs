using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using BaroJunk;
using System.Text.Json;
using System.IO;
using Barotrauma.LuaCs.Data;

namespace RemoveAll
{

  public partial class Mod : IAssemblyPlugin
  {
    public IPluginManagementService PluginService { get; set; }
    public IConfigService ConfigService { get; set; }
    public ILoggerService LoggerService { get; set; }

    public static Harmony Harmony { get; private set; } = new Harmony("Remove.All");
    public static Settings Settings { get; private set; } = new();
    public static LuaSettings LuaSettings { get; private set; } = new();
    public static BlackList BlackList { get; private set; } = new();
    public static Logger Logger { get; private set; } = new()
    {
      PrintFilePath = false
    };

    public static LightSourceTracker LightSourceTracker { get; private set; } = new();
    public static ContentPackage Package { get; private set; }

    public void Initialize()
    {
      PluginService.TryGetPackageForPlugin<Mod>(out ContentPackage package);
      Package = package;

      PatchAll();
      AddCommands();

      LightSourceTracker.FindLightSources();

      Settings.Settings().CommandName = "ra_config";
      Settings.UseStrategy(ConfigStrategy.MultiplayerClientside);

      Settings.OnPropChanged((key, value) =>
      {
        if (key == "Blacklist")
        {
          if (BlackList.Load((string)value)) Logger.Log($"Loaded {value}.json");
        }
      });

      Settings.OnUpdated(() =>
      {
        if (BlackList.Load(Settings.Blacklist)) Logger.Log($"Loaded {Settings.Blacklist}.json");
      });
      BlackList.Load(Settings.Blacklist);
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
      LevelGenerationParamsPatch.Patch(Harmony);

      LightSourceTracker.Patch(Harmony);
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