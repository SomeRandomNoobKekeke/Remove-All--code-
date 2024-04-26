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
      {"hide_level_objects",new ToggleableAction((state)=>{
        settings.hide.levelObjects = !settings.hide.levelObjects;
        if(state ){
          settings.LevelObjectManager.cutOffdepth = 0;
        } else {
          settings.LevelObjectManager.cutOffdepth = 10000;
        }

        Settings.saveSettings();
        Settings.justLoadBlacklist(settings.realBlacklistPath);

        log($"hide.levelObjects = {settings.hide.levelObjects}");
        log($"LevelObjectManager.cutOffdepth = {settings.LevelObjectManager.cutOffdepth}");
      })},

      {"hide_entities",new ToggleableAction((state)=>{
        settings.hide.entities = !settings.hide.entities;
        Settings.saveSettings();
        Settings.justLoadBlacklist(settings.realBlacklistPath);

        log($"hide.entities = {settings.hide.entities}");
      })},

      {"hide_particles",new ToggleableAction((state)=>{
        settings.hide.particles = !settings.hide.particles;
        Settings.saveSettings();
        Settings.justLoadBlacklist(settings.realBlacklistPath);

        log($"hide.particles = {settings.hide.particles}");
      })},

      {"hide_lights",new ToggleableAction((state)=>{
        settings.hide.itemLights = !settings.hide.itemLights;
        Settings.saveSettings();
        Settings.justLoadBlacklist(settings.realBlacklistPath);

        log($"hide.itemLights = {settings.hide.itemLights}");
      })},

      {"hide_decals",new ToggleableAction((state)=>{
        settings.hide.decals = !settings.hide.decals;
        Settings.saveSettings();
        Settings.justLoadBlacklist(settings.realBlacklistPath);

        log($"hide.decals = {settings.hide.decals}");
      })},

      {"reset",new ToggleableAction((state)=>{
        settings = new Settings();
        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(ModDir, stuffFolder, blacklistFileName));
        Settings.saveBlacklist();

        log($"everything is reset to default");
      })},

      {"vanilla",new ToggleableAction((state)=>{
        Settings.justLoad(Path.Combine(ModDir, stuffFolder,"Vanilla.json"));
        settings.version = ModVersion;

        // hacky workaround 
        settings.LevelRenderer.waterParticleLayers = new Dictionary<string, int>{
          {"coldcaverns", 4},
          {"europanridge", 4},
          {"theaphoticplateau", 4},
          {"thegreatsea", 4},
          {"hydrothermalwastes", 4},
          {"endzone", 4},
          {"outpost", 4},
        };


        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(ModDir, stuffFolder, "Entity Blacklist Vanilla.json"));
        Settings.saveBlacklist();

        log($"everything is vanilla");
      })},

      {"all", new ToggleableAction((state)=>{
        settings.hide.decals = true;
        settings.hide.entities = true;
        settings.hide.itemLights = true;
        settings.hide.levelObjects = true;
        settings.hide.particles = true;

        settings.LevelRenderer.drawWaterParticles = false;
        settings.maxBackgroundCreaturesCount = 0;
        reloadBackroundCreatures();

        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(ModDir, stuffFolder, "All.json"));
        Settings.saveBlacklist();

        log($"everything is hidden");
      })},

      {"water_particles",new ToggleableAction((state)=>{
        settings.LevelRenderer.drawWaterParticles = !settings.LevelRenderer.drawWaterParticles;
        Settings.saveSettings();

        log($"LevelRenderer.drawWaterParticles = {settings.LevelRenderer.drawWaterParticles}");
      })},

      {"ghost_characters",new ToggleableAction((state)=>{
        settings.LightManager.ghostCharacters = !settings.LightManager.ghostCharacters;
        Settings.saveSettings();

        log($"LightManager.ghostCharacters = {settings.LightManager.ghostCharacters}");
      })},

      {"background_fishes",new ToggleableAction((state)=>{
        if(state ){
          settings.maxBackgroundCreaturesCount = 100;
        } else {
          settings.maxBackgroundCreaturesCount = 0;
        }
        Settings.saveSettings();

        reloadBackroundCreatures();

        log($"maxBackgroundCreaturesCount = {settings.maxBackgroundCreaturesCount}");
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

        Settings.saveSettings();

        log($"darkmode {(state ? "On":"Off")}");
      })}
    };

    public static void addCommands()
    {
      DebugConsole.Commands.Add(
        new DebugConsole.Command("ra",
        "Megacommand to set and toggle settings. options are:\n" +
        "water_particles \n" +
        "darkmode  \n" +
        "background_fishes  \n" +
        "ghost_characters \n\n" +
        "reset - resets all settings to default \n" +
        "all - hides everything \n" +
        "vanilla - everything as in vanilla \n\n" +
        "hide_decals \n" +
        "hide_lights \n" +
        "hide_particles \n" +
        "hide_entities \n" +
        "hide_level_objects", (string[] args) =>
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
        Settings.saveSettings();
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_printsettings", "for debugging", (string[] args) =>
      {
        log(JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_printblacklist", "for debugging | ra_printblacklist category id1 id2 id3...", (string[] args) =>
      {
        if (args.Length > 1 && !blacklist.ContainsKey(args[0]))
        {
          log("no such category, try one of these:");
          string cats = "";

          foreach (var cat in blacklist.Keys) cats += cat + " ";
          log(cats);
          return;
        }

        if (args.Length < 2)
        {
          log("ra_printblacklist category id1 id2 id3...");
          return;
        }

        try
        {
          for (int i = 1; i < args.Length; i++)
          {
            log($"{args[i]}: {blacklist[args[0]][args[i]]}");
          }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
      }));


    }

    public static void removeCommands()
    {
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("light"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_exposure"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_loadsettings"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_savesettings"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_printsettings"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra_printblacklist"));
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("ra"));
    }

    public static void permitCommands(Identifier command, ref bool __result)
    {
      if (command.Value == "light") __result = true;
      if (command.Value == "ra_exposure") __result = true;
      if (command.Value == "ra_loadsettings") __result = true;
      if (command.Value == "ra_savesettings") __result = true;
      if (command.Value == "ra_printsettings") __result = true;
      if (command.Value == "ra_printblacklist") __result = true;
      if (command.Value == "ra") __result = true;
    }
  }
}