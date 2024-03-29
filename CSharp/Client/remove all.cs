using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace RemoveAll
{
  partial class RemoveAllMod : IAssemblyPlugin
  {
    public static string BarotraumaFolder = "";
    public static string modSettingsFolder = "ModSettings\\";
    public static string settingsFolder = "ModSettings\\RemoveAll\\";

    public string ModVersion = "1.0.0";
    public string ModDir = "";
    public Harmony harmony;


    public void Initialize()
    {
      harmony = new Harmony("remove.all");

      figureOutModVersionAndDirPath();
      PatchAll();

      loadSettings();
    }


    public void loadSettings()
    {
      string s;
      //string s = File.ReadAllText(ModDir + "/befaultSettings.json");

      //settings = JsonSerializer.Deserialize<Settings>(s);

      settings = new Settings();

      s = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

      log(s);

      File.WriteAllText(ModDir + "/befaultSettings.json", s);
    }


    public void figureOutModVersionAndDirPath()
    {
      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name == "Remove all")
        {
          ModVersion = p.ModVersion;
          ModDir = Path.GetFullPath(p.Dir);
        }
      }
    }

    public void PatchAll()
    {
      patchLevelRenderer();
      patchSubmarine();
      patchLevelObjectManager();
      patchWaterRenderer();
    }

    public static void log(object msg, Color? cl = null, [CallerLineNumber] int lineNumber = 0)
    {
      if (cl == null) cl = Color.Cyan;
      DebugConsole.NewMessage($"{lineNumber}| {msg ?? "null"}", cl);
    }

    public void Dispose()
    {
      harmony.UnpatchAll(harmony.Id);
      harmony = null;

      settings = null;

    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }

  }
}