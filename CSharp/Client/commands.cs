using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma.Extensions;
using System.Linq;
using System.Xml.Linq;

using System.IO;


namespace RemoveAll
{
  partial class RemoveAllMod
  {

    public class ToggleableAction
    {
      public Action<bool> action;
      public bool state;

      public void act()
      {
        state = !state;
        action(state);
      }
      public ToggleableAction(Action<bool> action, bool state = false)
      {
        this.action = action;
        this.state = state;
      }
    }



    public static Dictionary<string, ToggleableAction> presets = new Dictionary<string, ToggleableAction>(){
      {"water_particles",new ToggleableAction((state)=>{
        settings.LevelRenderer.drawWaterParticles = !settings.LevelRenderer.drawWaterParticles;
      })},

      {"hide_level_objects",new ToggleableAction((state)=>{
        settings.LevelObjectManager.cullLevelObjects = !settings.LevelObjectManager.cullLevelObjects;
      })},

      {"hide_entities",new ToggleableAction((state)=>{
        if(state) Settings.load();

        settings.Submarine.CullEntities = !settings.Submarine.CullEntities;
      })},

      {"reset",new ToggleableAction((state)=>{
        Settings.justLoad(Path.Combine(ModDir,"Settings.json"));
        Settings.save();
      })},

      {"vanilla",new ToggleableAction((state)=>{
        Settings.justLoad(Path.Combine(ModDir,"Settings presets/Vanilla.json"));
        Settings.save();
      })},

      {"ghost_characters",new ToggleableAction((state)=>{
        settings.LightManager.ghostCharacters = !settings.LightManager.ghostCharacters;
      })},

      {"background_fishes",new ToggleableAction((state)=>{
        if(state ){
          settings.maxBackgroundCreaturesCount = 100;
        } else {
          settings.maxBackgroundCreaturesCount = 0;
        }

        reloadBackroundCreatures();
      }, true)},

      {"darkmode",new ToggleableAction((state)=>{
        if(state){
          settings.LightManager.drawGapGlow = false;
          settings.LightManager.highlightItems = false;
          settings.LightManager.drawHalo = true;
          settings.LightManager.haloScale = 0.75f;
          settings.LightManager.haloBrightness = 0.1f;
          settings.LightManager.hullAmbientBrightness = 1.0f;
          settings.LightManager.hullAmbientColor = new Color(4, 4, 4, 255);
          settings.LightManager.globalLightBrightness = 0.7f;
          settings.LightManager.levelAmbientBrightness = 0;
        } else{
          settings.LightManager.drawGapGlow = true;
          settings.LightManager.highlightItems = true;
          settings.LightManager.drawHalo = true;
          settings.LightManager.haloScale = 0.5f;
          settings.LightManager.haloBrightness = 0.3f;
          settings.LightManager.hullAmbientBrightness = 1.0f;
          settings.LightManager.hullAmbientColor = new Color(0, 0, 0, 0);
          settings.LightManager.globalLightBrightness = 1.0f;
          settings.LightManager.levelAmbientBrightness = 1.0f;
        }
      })}
    };

    public static void addCommands()
    {
      DebugConsole.Commands.Add(new DebugConsole.Command("ra_exposure", "sets width of showperf graphs in ticks", (string[] args) =>
      {
        if (args.Length > 0 && int.TryParse(args[0], out int ticks))
        {
          ticks = Math.Clamp(ticks, 10, 100000);

          GameMain.PerformanceCounter.DrawTimeGraph = new Graph(ticks);
          GameMain.PerformanceCounter.UpdateTimeGraph = new Graph(ticks);
        }
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("light", "= lights", (string[] args) =>
      {
        if (args.None() || !bool.TryParse(args[0], out bool state))
        {
          state = !GameMain.LightManager.LightingEnabled;
        }
        GameMain.LightManager.LightingEnabled = state;
        log("Lighting " + (GameMain.LightManager.LightingEnabled ? "enabled" : "disabled"));
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_loadsettings", "load settings, patching isn't affected, if you want to repatch use cl_reloadlua", (string[] args) =>
      {
        Settings.load();

        reloadBackroundCreatures();
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_savesettings", "save settings, settings are saved automatically, so, you don't need it", (string[] args) =>
      {
        Settings.save();
      }));

      DebugConsole.Commands.Add(
        new DebugConsole.Command("ra", "toggles some cool stuff", (string[] args) =>
        {
          if (args.Length > 0)
          {
            if (presets.TryGetValue(args[0], out ToggleableAction ta))
            {
              ta.act();
            }
          }
        },
        () => new string[][] { presets.Keys.ToArray() })
      );
    }

    public static void removeCommands()
    {
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("light"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("debugexposure"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_loadsettings"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_savesettings"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra"));
    }
  }
}