using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;

using Barotrauma.Items.Components;
using Barotrauma.Lights;

using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace RemoveAll
{
  partial class Plugin : IAssemblyPlugin
  {

    public static string ModName = "Remove All";
    public static Plugin Mod;
    public Harmony harmony;
    public string ModVersion = "1.0.0";
    public string ModDir = "";
    public bool Debug;

    public Settings settings;
    public Dictionary<string, Dictionary<string, bool>> blacklist = new Dictionary<string, Dictionary<string, bool>>();
    public Dictionary<string, bool> mapEntityBlacklist = new Dictionary<string, bool>();


    public void Initialize()
    {
      Mod = this;

      harmony = new Harmony("remove.all");

      findModFolder();
      if (ModDir.Contains("LocalMods"))
      {
        Debug = true;
        info($"found {ModName} in LocalMods, debug: {Debug}");
      }

      Settings.load();

      PatchAll();

      GameMain.GameScreen.Cam.MinZoom = 0.004f;
      GameMain.PerformanceCounter.DrawTimeGraph = new Graph(1000);

      AddCommands();

      if (GameMain.GameSession != null && GameMain.GameSession.IsRunning)
      {
        findLightSources();
      }
    }



    public void PatchAll()
    {
      if (!settings.patch.doPatching) return;

      if (settings.patch.BackgroundCreatureManager) patchBackgroundCreatureManager();
      if (settings.patch.Level) patchLevel();
      if (settings.patch.LevelObjectManager) patchLevelObjectManager();
      if (settings.patch.LevelRenderer) patchLevelRenderer();
      if (settings.patch.LightManager) patchLightManager();
      if (settings.patch.Submarine) patchSubmarine();
      if (settings.patch.LightSource) patchLightSource();
      if (settings.patch.LightComponent) patchLightComponent();
      if (settings.patch.ParticleManager) patchParticleManager();
      if (settings.patch.Decal) patchDecal();

      harmony.Patch(
        original: typeof(LuaGame).GetMethod("IsCustomCommandPermitted"),
        postfix: new HarmonyMethod(typeof(Plugin).GetMethod("permitCommands"))
      );
    }


    public void Dispose()
    {
      lightSource_lightComponent.Clear();
      blacklist.Clear();
      mapEntityBlacklist.Clear();
      presets.Clear();

      RemoveCommands();
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

  }
}