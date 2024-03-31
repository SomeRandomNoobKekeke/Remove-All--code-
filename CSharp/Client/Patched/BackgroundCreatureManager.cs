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

  partial class RemoveAllMod
  {

    public static bool BackgroundCreatureManager_SpawnCreatures_Prefix(Level level, ref int count, Vector2? position, BackgroundCreatureManager __instance)
    {
      count = Math.Max(0, Math.Min(count, settings.maxBackgroundCreaturesCount));
      return true;
    }

    public void patchBackgroundCreatureManager()
    {
      harmony.Patch(
        original: typeof(BackgroundCreatureManager).GetMethod("SpawnCreatures"),
        prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("BackgroundCreatureManager_SpawnCreatures_Prefix"))
      );
    }
  }
}