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
      if (Mod.Settings.Hide.Decals && __instance.Prefab != null &&
          Mod.BlackList.Decals.Has(__instance.Prefab.Identifier.HashCode)
      )
      {
        return false;
      }

      return true;
    }
  }
}