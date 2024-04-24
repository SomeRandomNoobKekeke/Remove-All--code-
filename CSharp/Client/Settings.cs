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
    public static string blacklistPath = "Entity Blacklist.json";
    public static string blacklistGenPath = "Entity Blacklist.html";


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

      public string version { get; set; }

      public string customBlacklistPath { get; set; } = "C:\\Users\\user\\Desktop\\Entity Blacklist.json";


      public static void createStuffIfItDoesntExist()
      {
        if (!Directory.Exists(modSettingsFolder)) Directory.CreateDirectory(modSettingsFolder);
        if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);


        copyIfNotExists(
          Path.Combine(ModDir, blacklistGenPath),
          Path.Combine(settingsFolder, blacklistGenPath)
        );

        copyIfNotExists(
          Path.Combine(ModDir, blacklistPath),
          Path.Combine(settingsFolder, blacklistPath)
        );

        copyIfNotExists(
          Path.Combine(ModDir, settingsFileName),
          Path.Combine(settingsFolder, settingsFileName)
        );
      }

      public static void ohNoItsOutdated(Settings outdated)
      {
        File.Delete(Path.Combine(settingsFolder, blacklistGenPath));
        File.Copy(
          Path.Combine(ModDir, blacklistGenPath),
          Path.Combine(settingsFolder, blacklistGenPath)
        );

        File.Delete(Path.Combine(settingsFolder, blacklistPath));
        File.Copy(
          Path.Combine(ModDir, blacklistPath),
          Path.Combine(settingsFolder, blacklistPath)
        );


        string blacklistOld = Path.Combine(
          Path.GetDirectoryName(Path.Combine(settingsFolder, blacklistPath)),
          Path.GetFileNameWithoutExtension(Path.Combine(settingsFolder, blacklistPath)) + "-old" +
          Path.GetExtension(Path.Combine(settingsFolder, blacklistPath))
        );

        if (File.Exists(blacklistOld)) File.Delete(blacklistOld);

        File.Move(Path.Combine(settingsFolder, blacklistPath), blacklistOld);
        File.Copy(
          Path.Combine(ModDir, blacklistPath),
          Path.Combine(settingsFolder, blacklistPath)
        );

        migrate(outdated);
      }

      public static void migrate(Settings outdated)
      {
        // TODO: implement

        save();
      }

      public static void save(string path = "")
      {
        if (path == "") path = Path.Combine(settingsFolder, settingsFileName);

        File.WriteAllText(
          path,
          JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true })
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

      public static void load()
      {
        createStuffIfItDoesntExist();

        try
        {
          Settings newSettings = JsonSerializer.Deserialize<Settings>(
            File.ReadAllText(Path.Combine(settingsFolder, settingsFileName))
          );

          if (String.Compare(newSettings.version, settings.version) < 0)
          {
            ohNoItsOutdated(newSettings);
          }
          else
          {
            settings = newSettings;
          }
        }
        catch (Exception e)
        {
          log(e.Message, Color.Orange);
        }

        try
        {
          if (settings.customBlacklistPath == "" && File.Exists(settings.customBlacklistPath))
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(Path.Combine(settingsFolder, blacklistPath))
            );
          }
          else
          {
            blacklist = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
              File.ReadAllText(settings.customBlacklistPath)
            );
          }
        }
        catch (Exception e)
        {
          log(e.Message, Color.Orange);
        }



        mapEntityBlacklist = new Dictionary<string, bool>();
        foreach (var id in blacklist["items"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        foreach (var id in blacklist["structures"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
        //foreach (var id in blacklist["levelObjects"]) { mapEntityBlacklist.TryAdd(id.Key, id.Value); }
      }
    }
  }
}