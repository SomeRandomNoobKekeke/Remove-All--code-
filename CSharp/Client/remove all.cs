using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

//using System.Text.Json; :(
using System.IO;

namespace RemoveAll
{

  partial class RemoveAllMod : IAssemblyPlugin
  {
    public class settingsFileLayout
    {
      public LevelRendererPatch.DrawBackgroundSettings drawBackgroundSettings { get; set; }
      public LevelRendererPatch.DrawForegroundSettings drawForegroundSettings { get; set; }
      public LevelRendererPatch.UpdateSettings updateSettings { get; set; }
      public SubmarinePatch.CullEntitiesSettings cullEntitiesSettings { get; set; }
      public string version { get; set; }
    }

    public void loadSettings()
    {
      if (!Directory.Exists(modSettingsFolder)) Directory.CreateDirectory(modSettingsFolder);
      if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);

      if (
        File.Exists(Path.Combine(ModDir, "Brotrauma item list.html")) &&
        !File.Exists(Path.Combine(settingsFolder, "Brotrauma item list.html"))
      )
      {
        File.Copy(
          Path.Combine(ModDir, "Brotrauma item list.html"),
          Path.Combine(settingsFolder, "Brotrauma item list.html")
        );
      }


      if (
        File.Exists(Path.Combine(ModDir, "Decorations.json")) &&
        !File.Exists(Path.Combine(settingsFolder, "Decorations.json"))
      )
      {
        File.Copy(
          Path.Combine(ModDir, "Decorations.json"),
          Path.Combine(settingsFolder, "Decorations.json")
        );
      }

      if (File.Exists(settingsFolder + "settings.json"))
      {
        settingsFileLayout settings;
        using (StreamReader reader = new StreamReader(settingsFolder + "settings.json"))
        {
          settings = JSON.parse<settingsFileLayout>(reader.ReadToEnd());

          LevelRendererPatch.drawBackgroundSettings = settings.drawBackgroundSettings;
          LevelRendererPatch.drawForegroundSettings = settings.drawForegroundSettings;
          LevelRendererPatch.updateSettings = settings.updateSettings;
          SubmarinePatch.cullEntitiesSettings = settings.cullEntitiesSettings;
        }

        if (settings.version == null || String.Compare(settings.version, ModVersion) < 0)
        {
          saveSettings();
        }
      }
      else
      { saveSettings(); }

      if (File.Exists(settingsFolder + "Decorations.json"))
      {
        using (StreamReader reader = new StreamReader(settingsFolder + "Decorations.json"))
        {
          SubmarinePatch.whitelist = JSON.parse<Dictionary<string, bool>>(reader.ReadToEnd());
        }
      }

      DebugConsole.NewMessage("settings loaded!", new Color(255, 100, 255));
    }

    public void saveSettings()
    {
      if (!Directory.Exists(modSettingsFolder)) Directory.CreateDirectory(modSettingsFolder);
      if (!Directory.Exists(settingsFolder)) Directory.CreateDirectory(settingsFolder);

      settingsFileLayout settings = new settingsFileLayout();
      settings.drawBackgroundSettings = LevelRendererPatch.drawBackgroundSettings;
      settings.drawForegroundSettings = LevelRendererPatch.drawForegroundSettings;
      settings.updateSettings = LevelRendererPatch.updateSettings;
      settings.cullEntitiesSettings = SubmarinePatch.cullEntitiesSettings;
      settings.version = ModVersion;


      using (StreamWriter writer = new StreamWriter(settingsFolder + "settings.json", false))
      {
        writer.Write(JSON.stringify(settings));
      }

      DebugConsole.NewMessage("settings saved!", new Color(255, 100, 255));
    }

    public static string BarotraumaFolder = "";
    public static string modSettingsFolder = "ModSettings\\";
    public static string settingsFolder = "ModSettings\\RemoveAll\\";

    public Harmony harmony;
    public string ModVersion = "1.0.0";
    public string ModDir = "";


    public void Initialize()
    {
      harmony = new Harmony("remove.all");

      figureOutModVersionAndDirPath();
      loadSettings();
      PatchAll();
      AddConsoleCommands();
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

    public void figureOutModVersionAndDirPath()
    {
      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name == "Remove all")
        {
          ModVersion = p.ModVersion;
          ModDir = p.Dir;
        }
      }
    }

    public void PatchAll()
    {
      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("DrawBackground"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("DrawBackground"))
      );

      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("Update"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("Update"))
      );

      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("DrawForeground"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("DrawForeground"))
      );

      harmony.Patch(
        original: typeof(Submarine).GetMethod("CullEntities"),
        prefix: new HarmonyMethod(typeof(SubmarinePatch).GetMethod("CullEntities"))
      );

      // harmony.Patch(
      //   original: typeof(GameScreen).GetMethod("DrawMap"),
      //   prefix: new HarmonyMethod(typeof(GameScreenPatch).GetMethod("DrawMap"))
      // );
    }

    public void AddConsoleCommands()
    {
      if (DebugConsole.FindCommand("r_savesettings") != null)
      {
        DebugConsole.Commands.RemoveAll(c => c.Names[0] == "r_savesettings");
      }
      if (DebugConsole.FindCommand("r_loadsettings") != null)
      {
        DebugConsole.Commands.RemoveAll(c => c.Names[0] == "r_loadsettings");
      }

      if (DebugConsole.FindCommand("r_loadsettings") == null)
      {
        DebugConsole.Commands.Add(new DebugConsole.Command("r_loadsettings", "loading settings from Barotrauma/ModSettings/RemoveAll", (string[] args) =>
        {
          loadSettings();
        }));
      }

      if (DebugConsole.FindCommand("r_savesettings") == null)
      {
        DebugConsole.Commands.Add(new DebugConsole.Command("r_savesettings", "save all settings to Barotrauma/ModSettings/RemoveAll", (string[] args) =>
        {
          saveSettings();
        }));
      }
    }

    public void Dispose()
    {
      JSON.context.Unload();
      JSON.context = null;

      // for some reason this doesn't work :(
      harmony.UnpatchAll();
      harmony = null;

      // GameScreenPatch.BackCharactersItemsBuffer.Dispose();
      // GameScreenPatch.BackCharactersItemsBuffer = null;

      DebugConsole.Commands.RemoveAll(c => c.Names[0] == "r_loadsettings");
      DebugConsole.Commands.RemoveAll(c => c.Names[0] == "r_savesettings");
    }

  }
}