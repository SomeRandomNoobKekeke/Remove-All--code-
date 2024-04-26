using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Items.Components;



namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public static Dictionary<LightSource, LightComponent> lightSource_lightComponent = new Dictionary<LightSource, LightComponent>();

    public static void LightComponent_Constructor_Postfix(LightComponent __instance)
    {
      lightSource_lightComponent[__instance.Light] = __instance;
    }

    public static void findLightSources()
    {
      lightSource_lightComponent.Clear();

      foreach (Item item in Item.ItemList)
      {
        foreach (LightComponent lc in item.GetComponents<LightComponent>())
        {
          lightSource_lightComponent[lc.Light] = lc;
        }
      }
    }

    // i'm not sure if i need this
    public static void GameSession_StartRound_clearLightDict(LevelData? levelData, bool mirrorLevel, SubmarineInfo? startOutpost, SubmarineInfo? endOutpost)
    {
      lightSource_lightComponent.Clear();
    }

    public void patchLightComponent()
    {
      harmony.Patch(
        original: typeof(LightComponent).GetConstructors()[0],
        postfix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("LightComponent_Constructor_Postfix"))
      );

      harmony.Patch(
        original: typeof(GameSession).GetMethod("StartRound", new Type[]{
          typeof(LevelData),
          typeof(bool),
          typeof(SubmarineInfo),
          typeof(SubmarineInfo)
        }),
        postfix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("GameSession_StartRound_clearLightDict"))
      );
    }
  }
}