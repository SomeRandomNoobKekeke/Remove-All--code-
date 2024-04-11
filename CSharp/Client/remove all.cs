using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace RemoveAll
{
  partial class RemoveAllMod : IAssemblyPlugin
  {
    public static string BarotraumaFolder = "";
    public static string modSettingsFolder = "ModSettings\\";
    public static string settingsFolder = "ModSettings\\RemoveAll\\";

    public string ModVersion = "1.0.0";
    public string ModDir = "";
    public Harmony harmony;

    public static Settings settings;

    public static Dictionary<string, Dictionary<string, bool>> blacklist;
    public static Dictionary<string, bool> mapEntityBlacklist;

    public void Initialize()
    {
      harmony = new Harmony("remove.all");

      figureOutModVersionAndDirPath();

      loadSettings();
      PatchAll();

      GameMain.GameScreen.Cam.MinZoom = 0.004f;
    }


    public void loadSettings()
    {
      string s;
      //string s = File.ReadAllText(ModDir + "/befaultSettings.json");
      //settings = JsonSerializer.Deserialize<Settings>(s);

      settings = new Settings();
      s = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
      log(s);
      File.WriteAllText(ModDir + "/befaultSettings.json", s);

      s = File.ReadAllText(ModDir + "/should draw.json");
      blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(s);

      mapEntityBlacklist = new Dictionary<string, bool>();
      foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
      foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
      foreach (var id in blacklist["levelObjects"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
    }


    public void figureOutModVersionAndDirPath()
    {
      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name == "Remove all")
        {
          ModVersion = p.ModVersion;
          ModDir = Path.GetFullPath(p.Dir);
        }
      }
    }

    public void PatchAll()
    {
      if (settings.patch.BackgroundCreatureManager) patchBackgroundCreatureManager();
      if (settings.patch.GameScreen) patchGameScreen();
      if (settings.patch.GUI) patchGUI();
      if (settings.patch.Level) patchLevel();
      if (settings.patch.LevelObjectManager) patchLevelObjectManager();
      if (settings.patch.LevelRenderer) patchLevelRenderer();
      if (settings.patch.LightManager) patchLightManager();
      if (settings.patch.Submarine) patchSubmarine();
      if (settings.patch.LightSource) patchLightSource();
      if (settings.patch.WaterRenderer) patchWaterRenderer();
    }

    public static void log(object msg, Color? cl = null, [CallerLineNumber] int lineNumber = 0)
    {
      if (cl == null) cl = Color.Cyan;
      DebugConsole.NewMessage($"{lineNumber}| {msg ?? "null"}", cl);
    }

    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

      settings = null;

    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

  }
}