using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

// arghhhh
using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public static string BarotraumaFolder = "";
    public static string modSettingsFolder = "ModSettings\\";
    public static string settingsFolder = "ModSettings\\RemoveAll\\";
    public static string settingsFileName = "Settings.json";
    public static string blacklistFileName = "Entity Blacklist.json";
    public static string blacklistGenFileName = "Entity Blacklist.html";
    public static string stuffFolder = "Stuff";





    public class patchingSettings
    {
      public bool doPatching { get; set; } = true;
      public bool BackgroundCreatureManager { get; set; } = true;
      public bool GameScreen { get; set; } = false;
      public bool GUI { get; set; } = false;
      public bool Level { get; set; } = true;
      public bool LevelObjectManager { get; set; } = true;
      public bool LevelRenderer { get; set; } = true;
      public bool LightManager { get; set; } = true;
      public bool LightSource { get; set; } = true;
      public bool Submarine { get; set; } = true;
      public bool WaterRenderer { get; set; } = false;
      public bool LightComponent { get; set; } = true;
      public bool ParticleManager { get; set; } = true;
      public bool Decal { get; set; } = true;
    }

    public class HidingSettings
    {
      public bool entities { get; set; } = false;
      public bool itemLights { get; set; } = false;
      public bool levelObjects { get; set; } = true;
      public bool particles { get; set; } = false;
      public bool decals { get; set; } = false;
    }



    public class Settings
    {
      // [JsonPropertyName("Level Renderer Settings")]
      public LevelRendererSettings LevelRenderer { get; set; } = new LevelRendererSettings();
      public LevelObjectManagerSettings LevelObjectManager { get; set; } = new LevelObjectManagerSettings();
      public LightManagerSettings LightManager { get; set; } = new LightManagerSettings();

      public SubmarineSettings Submarine { get; set; } = new SubmarineSettings();


      [JsonIgnore]
      public patchingSettings patch { get; set; } = new patchingSettings();

      public HidingSettings hide { get; set; } = new HidingSettings();

      public int maxBackgroundCreaturesCount { get; set; } = 0;
      public int maxParticles { get; set; } = 100000;

      public string version { get; set; } = "undefined";

      // used instead of default path if != ""
      // could for example point to your download folder so you could update blacklist in one less step  
      [JsonIgnore]
      public string customBlacklistPath { get; set; } = "";

      public string realBlacklistPath = "uhh";


      public Settings()
      {
        version = ModVersion;
        if (customBlacklistPath != "") realBlacklistPath = customBlacklistPath;
        else realBlacklistPath = Path.Combine(settingsFolder, blacklistFileName);
      }


      public static void createStuffIfItDoesntExist()
      {
        // this is relative to barotrauma folder
        if (!Directory.Exists(modSettingsFolder)) Directory.CreateDirectory(modSettingsFolder);
        if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);


        copyIfNotExists(
          Path.Combine(ModDir, stuffFolder, blacklistGenFileName),
          Path.Combine(settingsFolder, blacklistGenFileName)
        );


        copyIfNotExists(
          Path.Combine(ModDir, stuffFolder, blacklistFileName),
          Path.Combine(settingsFolder, blacklistFileName)
        );

        // don't copy, just move it aside if it exists 
        copyIfNotExists("", Path.Combine(settingsFolder, settingsFileName));
        if (!File.Exists(Path.Combine(settingsFolder, settingsFileName))) saveSettings();
      }



      public static void ohNoItsOutdated()
      {
        // actually settings already merged by json.Deserialize
        // i just need to update version
        settings.version = ModVersion;


        // replace Entity Blacklist.html
        File.Delete(Path.Combine(settingsFolder, blacklistGenFileName));
        File.Copy(
          Path.Combine(ModDir, stuffFolder, blacklistGenFileName),
          Path.Combine(settingsFolder, blacklistGenFileName)
        );


        // merge old blacklist into new one
        try
        {
          blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(Path.Combine(ModDir, stuffFolder, blacklistFileName))
          );
        }
        catch (Exception e) { log(e.Message, Color.Orange); }

        Dictionary<string, Dictionary<string, bool>> oldBlacklist = new Dictionary<string, Dictionary<string, bool>>();
        try
        {
          if (File.Exists(settings.realBlacklistPath))
          {
            oldBlacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.realBlacklistPath)
            );
          }
          else
          {
            oldBlacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(Path.Combine(settingsFolder, blacklistFileName))
            );
          }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }

        foreach (var category in oldBlacklist)
        {
          foreach (var rule in category.Value)
          {
            blacklist[category.Key][rule.Key] = rule.Value;
          }
        }

        saveSettings();
        saveBlacklist();
      }

      public static void itsOK()
      {
        try
        {
          if (File.Exists(settings.realBlacklistPath))
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.realBlacklistPath)
            );
          }
          else
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(Path.Combine(settingsFolder, blacklistFileName))
            );
          }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }

        // in case where version is uptodate but you deleted some property
        saveSettings();
      }



      public static void load()
      {
        settings = new Settings();

        createStuffIfItDoesntExist();

        try
        {
          settings = JsonSerializer.Deserialize<Settings>(
            File.ReadAllText(Path.Combine(settingsFolder, settingsFileName))
          );
        }
        catch (Exception e)
        {
          log(e.Message, Color.Orange);
        }

        if (String.Compare(settings.version, ModVersion) < 0)
        {
          ohNoItsOutdated();
        }
        else
        {
          itsOK();
        }

        mapEntityBlacklist = new Dictionary<string, bool>();
        foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        //foreach (var id in blacklist["levelObjects"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
      }

      public static void saveSettings(string path = "")
      {
        if (path == "") path = Path.Combine(settingsFolder, settingsFileName);

        File.WriteAllText(
          path,
          JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true })
        );
      }

      public static void saveBlacklist()
      {
        File.WriteAllText(
          settings.realBlacklistPath,
          JsonSerializer.Serialize(blacklist, new JsonSerializerOptions { WriteIndented = true })
        );
      }

      public static void justLoad(string filePath)
      {
        try
        {
          settings = JsonSerializer.Deserialize<Settings>(
            File.ReadAllText(filePath)
          );
        }
        catch (Exception e)
        {
          log(e.Message, Color.Orange);
        }
      }

      public static void justLoadBlacklist(string filePath)
      {
        try
        {
          blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(filePath)
          );

          mapEntityBlacklist = new Dictionary<string, bool>();
          foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
          foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
      }
    }
  }
}