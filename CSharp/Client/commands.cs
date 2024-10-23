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
  partial class Plugin
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
        Mod.settings.hide.levelObjects = !Mod.settings.hide.levelObjects;
        if(Mod.settings.hide.levelObjects){
          Mod.settings.LevelObjectManager.cutOffdepth = 10;
        } else {
          Mod.settings.LevelObjectManager.cutOffdepth = 10000;
        }

        Settings.saveSettings();
        Settings.justLoadBlacklist(Mod.settings.realBlacklistPath);

        log($"hide.levelObjects = {Mod.settings.hide.levelObjects}");
        log($"LevelObjectManager.cutOffdepth = {Mod.settings.LevelObjectManager.cutOffdepth}");
      })},

      {"hide_entities",new ToggleableAction((state)=>{
        Mod.settings.hide.entities = !Mod.settings.hide.entities;
        Settings.saveSettings();
        Settings.justLoadBlacklist(Mod.settings.realBlacklistPath);

        log($"hide.entities = {Mod.settings.hide.entities}");
      })},

      {"hide_particles",new ToggleableAction((state)=>{
        Mod.settings.hide.particles = !Mod.settings.hide.particles;
        Settings.saveSettings();
        Settings.justLoadBlacklist(Mod.settings.realBlacklistPath);

        log($"hide.particles = {Mod.settings.hide.particles}");
      })},

      {"hide_lights",new ToggleableAction((state)=>{
        Mod.settings.hide.itemLights = !Mod.settings.hide.itemLights;
        Settings.saveSettings();
        Settings.justLoadBlacklist(Mod.settings.realBlacklistPath);

        log($"hide.itemLights = {Mod.settings.hide.itemLights}");
      })},

      {"hide_decals",new ToggleableAction((state)=>{
        Mod.settings.hide.decals = !Mod.settings.hide.decals;
        Settings.saveSettings();
        Settings.justLoadBlacklist(Mod.settings.realBlacklistPath);

        log($"hide.decals = {Mod.settings.hide.decals}");
      })},

      {"default",new ToggleableAction((state)=>{
        Mod.settings = new Settings();
        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(Mod.ModDir, stuffFolder, blacklistFileName));
        Settings.saveBlacklist();

        log($"everything is reset to default");
      })},

      {"vanilla",new ToggleableAction((state)=>{
        Settings.justLoad(Path.Combine(Mod.ModDir, stuffFolder,"Vanilla.json"));
        Mod.settings.version = Mod.ModVersion;

        // hacky workaround 
        Mod.settings.LevelRenderer.waterParticleLayers = new Dictionary<string, int>{
          {"coldcaverns", 4},
          {"europanridge", 4},
          {"theaphoticplateau", 4},
          {"thegreatsea", 4},
          {"hydrothermalwastes", 4},
          {"endzone", 4},
          {"outpost", 4},
        };


        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(Mod.ModDir, stuffFolder, "Entity Blacklist Vanilla.json"));
        Settings.saveBlacklist();

        log($"everything is vanilla");
      })},

      {"all", new ToggleableAction((state)=>{
        Mod.settings.hide.decals = true;
        Mod.settings.hide.entities = true;
        Mod.settings.hide.itemLights = true;
        Mod.settings.hide.levelObjects = true;
        Mod.settings.hide.particles = true;

        Mod.settings.LevelRenderer.drawWaterParticles = false;
        Mod.settings.maxBackgroundCreaturesCount = 0;
        reloadBackroundCreatures();

        Settings.saveSettings();

        Settings.justLoadBlacklist(Path.Combine(Mod.ModDir, stuffFolder, "All.json"));
        Settings.saveBlacklist();

        log($"everything is hidden");
      })},

      {"water_particles",new ToggleableAction((state)=>{
        Mod.settings.LevelRenderer.drawWaterParticles = !Mod.settings.LevelRenderer.drawWaterParticles;
        Settings.saveSettings();

        log($"LevelRenderer.drawWaterParticles = {Mod.settings.LevelRenderer.drawWaterParticles}");
      })},

      {"ghost_characters",new ToggleableAction((state)=>{
        Mod.settings.LightManager.ghostCharacters = !Mod.settings.LightManager.ghostCharacters;
        Settings.saveSettings();

        log($"LightManager.ghostCharacters = {Mod.settings.LightManager.ghostCharacters}");
      })},

      {"background_fishes",new ToggleableAction((state)=>{
        if(state ){
          Mod.settings.maxBackgroundCreaturesCount = 100;
        } else {
          Mod.settings.maxBackgroundCreaturesCount = 0;
        }
        Settings.saveSettings();

        reloadBackroundCreatures();

        log($"maxBackgroundCreaturesCount = {Mod.settings.maxBackgroundCreaturesCount}");
      }, false)},

      {"darkmode",new ToggleableAction((state)=>{
        if(state){
          Mod.settings.LightManager.drawGapGlow = false;
          Mod.settings.LightManager.highlightItems = false;
          Mod.settings.LightManager.drawHalo = true;
          Mod.settings.LightManager.haloScale = 0.75f;
          Mod.settings.LightManager.haloBrightness = 0.1f;
          Mod.settings.LightManager.hullAmbientBrightness = 1.0f;
          Mod.settings.LightManager.hullAmbientColor = new Color(4, 4, 4, 255);
          Mod.settings.LightManager.globalLightBrightness = 0.7f;
          Mod.settings.LightManager.levelAmbientBrightness = 0;
        } else{
          Mod.settings.LightManager.drawGapGlow = true;
          Mod.settings.LightManager.highlightItems = true;
          Mod.settings.LightManager.drawHalo = true;
          Mod.settings.LightManager.haloScale = 0.5f;
          Mod.settings.LightManager.haloBrightness = 0.3f;
          Mod.settings.LightManager.hullAmbientBrightness = 1.0f;
          Mod.settings.LightManager.hullAmbientColor = new Color(0, 0, 0, 0);
          Mod.settings.LightManager.globalLightBrightness = 1.0f;
          Mod.settings.LightManager.levelAmbientBrightness = 1.0f;
        }

        Settings.saveSettings();

        log($"darkmode {(state ? "On":"Off")}");
      })}
    };

    public static void AddCommands()
    {
      DebugConsole.Commands.Add(
        new DebugConsole.Command("ra",
        "Megacommand to set and toggle Mod.settings. options are:\n" +
        "water_particles \n" +
        "darkmode  \n" +
        "background_fishes  \n" +
        "ghost_characters \n\n" +
        "default - resets all Mod.settings to default \n" +
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

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_loadsettings", "load Mod.settings, patching isn't affected, if you want to repatch use cl_reloadlua", (string[] args) =>
      {
        Settings.load();

        reloadBackroundCreatures();
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_savesettings", "save Mod.settings, Mod.settings are saved automatically, so, you don't need it", (string[] args) =>
      {
        Settings.saveSettings();
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_printsettings", "for debugging", (string[] args) =>
      {
        log(JsonSerializer.Serialize(Mod.settings, new JsonSerializerOptions { WriteIndented = true }));
      }));

      DebugConsole.Commands.Add(new DebugConsole.Command("ra_printblacklist", "for debugging | ra_printblacklist category id1 id2 id3...", (string[] args) =>
      {
        if (args.Length > 1 && !Mod.blacklist.ContainsKey(args[0]))
        {
          log("no such category, try one of these:");
          string cats = "";

          foreach (var cat in Mod.blacklist.Keys) cats += cat + " ";
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
            log($"{args[i]}: {Mod.blacklist[args[0]][args[i]]}");
          }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
      }));


    }

    public static void RemoveCommands()
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