using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace RemoveAll
{

  partial class Plugin
  {
    public static void reloadBackroundCreatures()
    {
      if (Level.loaded != null)
      {
        int count = Math.Max(0, Math.Min(
          Level.loaded.GenerationParams.BackgroundCreatureAmount,
          settings.maxBackgroundCreaturesCount
        ));

        // Level.loaded.backgroundCreatureManager.Clear();
        Level.loaded.backgroundCreatureManager.SpawnCreatures(Level.loaded, count);
      }
    }


    public static bool BackgroundCreatureManager_SpawnCreatures_Replace(Level level, ref int count, Vector2? position, BackgroundCreatureManager __instance)
    {
      count = Math.Max(0, Math.Min(count, settings.maxBackgroundCreaturesCount));
      return true;
    }

    public void patchBackgroundCreatureManager()
    {
      harmony.Patch(
        original: typeof(BackgroundCreatureManager).GetMethod("SpawnCreatures"),
        prefix: new HarmonyMethod(typeof(Plugin).GetMethod("BackgroundCreatureManager_SpawnCreatures_Replace"))
      );
    }
  }
}