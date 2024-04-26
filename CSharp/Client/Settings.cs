using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

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
      public bool GameScreen { get; set; } = true;
      public bool GUI { get; set; } = true;
      public bool Level { get; set; } = true;
      public bool LevelObjectManager { get; set; } = true;
      public bool LevelRenderer { get; set; } = true;
      public bool LightManager { get; set; } = true;
      public bool LightSource { get; set; } = true;
      public bool Submarine { get; set; } = true;
      public bool WaterRenderer { get; set; } = true;
    }

    public class Settings
    {
      // [JsonPropertyName("Level Renderer Settings")]
      public LevelRendererSettings LevelRenderer { get; set; } = new LevelRendererSettings();
      public LevelObjectManagerSettings LevelObjectManager { get; set; } = new LevelObjectManagerSettings();
      public LightManagerSettings LightManager { get; set; } = new LightManagerSettings();

      public SubmarineSettings Submarine { get; set; } = new SubmarineSettings();
      public int maxBackgroundCreaturesCount { get; set; } = 0;
      public patchingSettings patch { get; set; } = new patchingSettings();

      public string version { get; set; } = "undefined";

      // used instead of default path if != ""
      // could for example point to your download folder so you could update blacklist in one less step  
      public string customBlacklistPath { get; set; } = "C:\\Users\\user\\Desktop\\Entity Blacklist.json";





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

        // copyIfNotExists(
        //   Path.Combine(ModDir, stuffFolder, settingsFileName),
        //   Path.Combine(settingsFolder, settingsFileName)
        // );
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
          if (settings.customBlacklistPath != "" && File.Exists(settings.customBlacklistPath))
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.customBlacklistPath)
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

        Dictionary<string, Dictionary<string, bool>> oldBlacklist = new Dictionary<string, Dictionary<string, bool>>();
        try
        {
          if (settings.customBlacklistPath != "" && File.Exists(settings.customBlacklistPath))
          {
            oldBlacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.customBlacklistPath)
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



        // string blacklistOld = Path.Combine(
        //   Path.GetDirectoryName(Path.Combine(settingsFolder, blacklistFileName)),
        //   Path.GetFileNameWithoutExtension(Path.Combine(settingsFolder, blacklistFileName)) + "-old" +
        //   Path.GetExtension(Path.Combine(settingsFolder, blacklistFileName))
        // );

        // if (File.Exists(blacklistOld)) File.Delete(blacklistOld);

        // File.Move(Path.Combine(settingsFolder, blacklistFileName), blacklistOld);
        // File.Copy(
        //   Path.Combine(ModDir, stuffFolder, blacklistFileName),
        //   Path.Combine(settingsFolder, blacklistFileName)
        // );



        saveSettings();
        saveBlacklist();
      }

      public static void itsOK()
      {
        try
        {
          if (settings.customBlacklistPath != "" && File.Exists(settings.customBlacklistPath))
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.customBlacklistPath)
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
        settings.version = ModVersion;

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
        if (settings.customBlacklistPath != "")
        {
          File.WriteAllText(
            settings.customBlacklistPath,
            JsonSerializer.Serialize(blacklist, new JsonSerializerOptions { WriteIndented = true })
          );
        }
        else
        {
          File.WriteAllText(
            Path.Combine(settingsFolder, blacklistFileName),
            JsonSerializer.Serialize(blacklist, new JsonSerializerOptions { WriteIndented = true })
          );
        }
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
    }
  }
}