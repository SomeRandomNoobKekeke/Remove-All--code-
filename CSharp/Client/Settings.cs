using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace RemoveAll
{
  partial class Plugin
  {
    public static string BarotraumaFolder = "";
    public static string modSettingsFolder = "ModSettings";
    public static string settingsFolder = Path.Combine(modSettingsFolder, "RemoveAll");
    public static string settingsFileName = "Settings.json";
    public static string blacklistFileName = "Entity Blacklist.json";
    public static string blacklistGenFileName = "Entity Blacklist.html";
    public static string stuffFolder = "Stuff";





    public class patchingSettings
    {
      public bool doPatching { get; set; } = true;
      public bool BackgroundCreatureManager { get; set; } = true;
      public bool Level { get; set; } = true;
      public bool LevelObjectManager { get; set; } = true;
      public bool LevelRenderer { get; set; } = true;
      public bool LightManager { get; set; } = true;
      public bool LightSource { get; set; } = true;
      public bool Submarine { get; set; } = true;
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
        version = Mod.ModVersion;
        if (customBlacklistPath != "") realBlacklistPath = customBlacklistPath;
        else realBlacklistPath = Path.Combine(settingsFolder, blacklistFileName);
      }

      public void forceChangeSomething()
      {
        // turns out that ballast flora is level object with z slightly deeper than 0
        // so this should keep it
        if (LevelObjectManager.cutOffdepth == 0) LevelObjectManager.cutOffdepth = 10;
      }

      public static void createStuffIfItDoesntExist()
      {
        // this is relative to barotrauma folder
        if (!Directory.Exists(modSettingsFolder)) Directory.CreateDirectory(modSettingsFolder);
        if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);


        copyIfNotExists(
          Path.Combine(Mod.ModDir, stuffFolder, blacklistGenFileName),
          Path.Combine(settingsFolder, blacklistGenFileName)
        );


        copyIfNotExists(
          Path.Combine(Mod.ModDir, stuffFolder, blacklistFileName),
          Path.Combine(settingsFolder, blacklistFileName)
        );

        // don't copy, just move it aside if it exists 
        copyIfNotExists("", Path.Combine(settingsFolder, settingsFileName));
        if (!File.Exists(Path.Combine(settingsFolder, settingsFileName))) saveSettings();
      }



      public void ohNoItsOutdated()
      {
        // actually settings already merged by json.Deserialize
        // i just need to update version
        version = Mod.ModVersion;


        // replace Entity Blacklist.html
        File.Delete(Path.Combine(settingsFolder, blacklistGenFileName));
        File.Copy(
          Path.Combine(Mod.ModDir, stuffFolder, blacklistGenFileName),
          Path.Combine(settingsFolder, blacklistGenFileName)
        );


        // merge old blacklist into new one
        try
        {
          Mod.blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(Path.Combine(Mod.ModDir, stuffFolder, blacklistFileName))
          );
        }
        catch (Exception e) { log(e.Message, Color.Orange); }

        Dictionary<string, Dictionary<string, bool>> oldBlacklist = new Dictionary<string, Dictionary<string, bool>>();
        try
        {
          if (File.Exists(realBlacklistPath))
          {
            oldBlacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(realBlacklistPath)
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
            Mod.blacklist[category.Key][rule.Key] = rule.Value;
          }
        }

        forceChangeSomething();

        saveSettings();
        saveBlacklist();
      }

      public void itsOK()
      {
        try
        {
          if (File.Exists(realBlacklistPath))
          {
            Mod.blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(realBlacklistPath)
            );
          }
          else
          {
            Mod.blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
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
        Mod.settings = new Settings();

        createStuffIfItDoesntExist();

        try
        {
          Mod.settings = JsonSerializer.Deserialize<Settings>(
            File.ReadAllText(Path.Combine(settingsFolder, settingsFileName))
          );
        }
        catch (Exception e)
        {
          log(e.Message, Color.Orange);
        }

        if (String.Compare(Mod.settings.version, Mod.ModVersion) < 0)
        {
          Mod.settings.ohNoItsOutdated();
        }
        else
        {
          Mod.settings.itsOK();
        }

        Mod.mapEntityBlacklist = new Dictionary<string, bool>();
        foreach (var id in Mod.blacklist["items"]) { Mod.mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        foreach (var id in Mod.blacklist["structures"]) { Mod.mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        //foreach (var id in blacklist["levelObjects"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
      }

      public static void saveSettings(string path = "")
      {
        if (path == "") path = Path.Combine(settingsFolder, settingsFileName);

        File.WriteAllText(
          path,
          JsonSerializer.Serialize(Mod.settings, new JsonSerializerOptions { WriteIndented = true })
        );
      }

      public static void saveBlacklist()
      {
        File.WriteAllText(
          Mod.settings.realBlacklistPath,
          JsonSerializer.Serialize(Mod.blacklist, new JsonSerializerOptions { WriteIndented = true })
        );
      }

      public static void justLoad(string filePath)
      {
        try
        {
          Mod.settings = JsonSerializer.Deserialize<Settings>(
            File.ReadAllText(filePath)
          );
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
      }

      public static void justLoadBlacklist(string filePath)
      {
        try
        {
          Mod.blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
            File.ReadAllText(filePath)
          );

          Mod.mapEntityBlacklist = new Dictionary<string, bool>();
          foreach (var id in Mod.blacklist["items"]) { Mod.mapEntityBlacklist.TryAdd(id.Key, id.Value); }
          foreach (var id in Mod.blacklist["structures"]) { Mod.mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        }
        catch (Exception e) { log(e.Message, Color.Orange); }
      }
    }
  }
}