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
using Microsoft.Xna.Framework;

namespace RemoveAll
{

  public partial class Mod : IAssemblyPlugin
  {
    public void AddCommands()
    {
      PluginCommands.Add("ra", RA_Command, () => new string[][] { RAActions.Keys.ToArray() });

      PluginCommands.Add("ra_blacklist", (args) =>
        {
          if (args.Length == 0) return;
          Mod.Settings.ReactiveSetValue("Blacklist", args[0]);
        },
        () => new string[][] {
          Directory.GetFiles(Path.Combine(Mod.Package.Dir, BlackList.BlacklistsDir),"*.json")
          .Select(path=>Path.GetFileNameWithoutExtension(path)).ToArray()
        }
      );

      PluginCommands.Add("light", (args) =>
      {
        GameMain.LightManager.LightingEnabled = !GameMain.LightManager.LightingEnabled;
      });

      PluginCommands.Add("ra_exposure", (args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int ticks))
        {
          ticks = Math.Clamp(ticks, 10, 100000);

          GameMain.PerformanceCounter.DrawTimeGraph = new Graph(ticks);
          GameMain.PerformanceCounter.UpdateTimeGraph = new Graph(ticks);
        }

        Logger.Log($"Showperf Graph frame size is: [{GameMain.PerformanceCounter.DrawTimeGraph.values.Length}]");
      });
    }

    public void RA_Command(string[] args)
    {
      if (args.Length == 0)
      {
        Logger.Log($"Available actions: {Logger.Wrap.AsJson(RAActions.Keys)}");
        return;
      }

      if (!RAActions.ContainsKey(args[0]))
      {
        Logger.Log($"no such action [{args[0]}] in {Logger.Wrap.AsJson(RAActions.Keys)}");
        return;
      }

      RAActions[args[0]].Invoke();
    }

    public Dictionary<string, Action> RAActions = new()
    {
      ["hide_level_objects"] = () =>
      {
        Mod.Settings.Hide.LevelObjects = !Mod.Settings.Hide.LevelObjects;
        if (Mod.Settings.Hide.LevelObjects)
        {
          Mod.Settings.LevelObjectManager.CutOffdepth = 10;
        }
        else
        {
          Mod.Settings.LevelObjectManager.CutOffdepth = 10000;
        }

        Logger.LogVars(Mod.Settings.Hide.LevelObjects);
        Logger.LogVars(Mod.Settings.LevelObjectManager.CutOffdepth);
      },
      ["hide_entities"] = () =>
      {
        Mod.Settings.Hide.Entities = !Mod.Settings.Hide.Entities;
        Logger.LogVars(Mod.Settings.Hide.Entities);
      },
      ["hide_particles"] = () =>
      {
        Mod.Settings.Hide.Particles = !Mod.Settings.Hide.Particles;
        Logger.LogVars(Mod.Settings.Hide.Particles);
      },
      ["hide_lights"] = () =>
      {
        Mod.Settings.Hide.ItemLights = !Mod.Settings.Hide.ItemLights;
        Logger.LogVars(Mod.Settings.Hide.ItemLights);
      },
      ["hide_decals"] = () =>
      {
        Mod.Settings.Hide.Decals = !Mod.Settings.Hide.Decals;
        Logger.LogVars(Mod.Settings.Hide.Decals);
      },
      ["water_particles"] = () =>
      {
        Mod.Settings.LevelRenderer.DrawWaterParticles = !Mod.Settings.LevelRenderer.DrawWaterParticles;
        Logger.LogVars(Mod.Settings.LevelRenderer.DrawWaterParticles);
      },
      ["ghost_characters"] = () =>
      {
        Mod.Settings.LightManager.GhostCharacters = !Mod.Settings.LightManager.GhostCharacters;
        Logger.LogVars(Mod.Settings.LightManager.GhostCharacters);
      },
      ["background_fishes"] = () =>
      {
        if (Mod.Settings.MaxBackgroundCreaturesCount == 0)
        {
          Mod.Settings.MaxBackgroundCreaturesCount = 100;
        }
        else
        {
          Mod.Settings.MaxBackgroundCreaturesCount = 0;
        }

        BackgroundCreatureManagerPatch.ReloadBackroundCreatures();
        Logger.LogVars(Mod.Settings.MaxBackgroundCreaturesCount);
      },
      ["default"] = () =>
      {
        Mod.Settings.Load(Path.Combine(Mod.Package.Dir, "Presets", "Default.xml"));
        Logger.Log("Loaded Default.xml");
      },
      ["vanilla"] = () =>
      {
        Mod.Settings.Load(Path.Combine(Mod.Package.Dir, "Presets", "Vanilla.xml"));
        Logger.Log("Loaded Vanilla.xml");
      },
      ["all"] = () =>
      {
        Mod.Settings.Hide.Decals = true;
        Mod.Settings.Hide.Entities = true;
        Mod.Settings.Hide.ItemLights = true;
        Mod.Settings.Hide.LevelObjects = true;
        Mod.Settings.Hide.Particles = true;

        Mod.Settings.LevelRenderer.DrawWaterParticles = false;
        Mod.Settings.MaxBackgroundCreaturesCount = 0;

        BackgroundCreatureManagerPatch.ReloadBackroundCreatures();
        Mod.Settings.ReactiveSetValue("Blacklist", "HideAll");
        Logger.Log("Removed All");
      },
      ["darkmode"] = () =>
      {
        Mod.Settings.LightManager.DrawGapGlow = false;
        Mod.Settings.LightManager.HightlightItems = false;
        Mod.Settings.LightManager.DrawHalo = true;
        Mod.Settings.LightManager.HaloScale = 0.75f;
        Mod.Settings.LightManager.HaloBrightness = 0.1f;
        Mod.Settings.LightManager.HullAmbientBrightness = 1.0f;
        Mod.Settings.LightManager.HullAmbientColor = new Color(4, 4, 4, 255);
        Mod.Settings.LightManager.GlobalLightBrightness = 0.7f;
        Mod.Settings.LightManager.LevelAmbientBrightness = 0;
        Logger.Log("Applied darkmode");
      },
    };
  }
}