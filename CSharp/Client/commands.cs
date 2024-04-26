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
        if(state) Settings.load();
        else Settings.saveSettings();

        settings.hide.levelObjects = !settings.hide.levelObjects;
      })},

      {"hide_entities",new ToggleableAction((state)=>{
        if(state) Settings.load();
        else Settings.saveSettings();

        settings.hide.entities = !settings.hide.entities;
      })},

      {"hide_particles",new ToggleableAction((state)=>{
        if(state) Settings.load();
        else Settings.saveSettings();

        settings.hide.particles = !settings.hide.particles;
      })},

      {"hide_lights",new ToggleableAction((state)=>{
        if(state) Settings.load();
        else Settings.saveSettings();

        settings.hide.itemLights = !settings.hide.itemLights;
      })},

      {"hide_decals",new ToggleableAction((state)=>{
        if(state) Settings.load();
        else Settings.saveSettings();

        settings.hide.decals = !settings.hide.decals;
      })},

      {"reset",new ToggleableAction((state)=>{
        settings = new Settings();
        settings.version = ModVersion;
        Settings.saveSettings();

        try
        {
          blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(Path.Combine(ModDir, stuffFolder, blacklistFileName))
          );

          mapEntityBlacklist = new Dictionary<string, bool>();
          foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
          foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
        Settings.saveBlacklist();
      })},

      {"vanilla",new ToggleableAction((state)=>{
        Settings.justLoad(Path.Combine(ModDir, stuffFolder,"Vanilla.json"));
        settings.version = ModVersion;
        Settings.saveSettings();


        try
        {
          blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(Path.Combine(ModDir, stuffFolder, "Entity Blacklist Vanilla.json"))
          );

          mapEntityBlacklist = new Dictionary<string, bool>();
          foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
          foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
        Settings.saveBlacklist();
      })},

      {"all", new ToggleableAction((state)=>{
        try
        {
          blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(Path.Combine(ModDir, stuffFolder, "All.json"))
          );

          mapEntityBlacklist = new Dictionary<string, bool>();
          foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
          foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
        Settings.saveBlacklist();
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
  }
}