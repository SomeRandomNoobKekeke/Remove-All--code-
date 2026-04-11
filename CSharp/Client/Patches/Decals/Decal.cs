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
  public static class DecalPatch
  {
    public static void Patch(Harmony harmony)
    {
      harmony.Patch(
         original: typeof(Decal).GetMethod("Draw"),
         prefix: new HarmonyMethod(typeof(DecalPatch).GetMethod("Decal_Draw_Prefix"))
       );
    }


    public static bool Decal_Draw_Prefix(SpriteBatch spriteBatch, Hull hull, float depth, Decal __instance)
    {
      // if (Mod.settings.hide.decals && __instance.Prefab != null)
      // {
      //   //log(__instance.Prefab.Identifier.Value);
      //   if (Mod.blacklist["decals"].TryGetValue(__instance.Prefab.Identifier.Value, out bool value))
      //   {
      //     if (!value)
      //     {
      //       return false; // don't
      //     }
      //     ;
      //   }
      // }

      return !Mod.Settings.Hide.Decals;
    }
  }
}