using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;

// arghhhh
using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace RemoveAll
{
  partial class RemoveAllMod : IAssemblyPlugin
  {
    public static string ModVersion = "1.0.0";
    // must match name in filelist or we won't find mod folder
    public static string modName = "Remove All (source code)";
    public static string ModDir = "";

    public static bool testing = false;
    public Harmony harmony;

    public static Settings settings;

    public static Dictionary<string, Dictionary<string, bool>> blacklist = new Dictionary<string, Dictionary<string, bool>>();
    public static Dictionary<string, bool> mapEntityBlacklist = new Dictionary<string, bool>();

    public void Initialize()
    {
      if (testing) log("Compiled!");

      harmony = new Harmony("remove.all");

      figureOutModVersionAndDirPath();

      Settings.load();

      PatchAll();

      GameMain.GameScreen.Cam.MinZoom = 0.004f;
      GameMain.PerformanceCounter.DrawTimeGraph = new Graph(1000);

      addCommands();

      if (GameMain.GameSession.IsRunning)
      {
        findLightSources();
      }
    }

    public void figureOutModVersionAndDirPath()
    {
      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name == modName)
        {
          ModVersion = p.ModVersion;
          ModDir = Path.GetFullPath(p.Dir);
        }
      }
    }

    public void PatchAll()
    {
      if (!settings.patch.doPatching) return;

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
      if (settings.patch.LightComponent) patchLightComponent();
      if (settings.patch.ParticleManager) patchParticleManager();
      if (settings.patch.Decal) patchDecal();

      harmony.Patch(
        original: typeof(LuaGame).GetMethod("IsCustomCommandPermitted"),
        postfix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("permitCommands"))
      );
    }


    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

      settings = null;

      lightSource_lightComponent.Clear(); lightSource_lightComponent = null;
      blacklist.Clear(); blacklist = null;
      mapEntityBlacklist.Clear(); mapEntityBlacklist = null;
      presets.Clear(); presets = null;


      removeCommands();
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

  }
}