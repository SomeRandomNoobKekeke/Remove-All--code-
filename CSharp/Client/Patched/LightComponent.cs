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

// arghhhh
using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public static Dictionary<LightSource, LightComponent> lightSource_lightComponent = new Dictionary<LightSource, LightComponent>();


    // cursed, dont't touch
    // public static void LightComponent_Constructor_Postfix(LightComponent __instance)
    // {
    //   try
    //   {
    //     lightSource_lightComponent[__instance.Light] = __instance;
    //   }
    //   catch (Exception e)
    //   {
    //     log($"Something terrible happened in LightComponent_Constructor_Postfix on line 37");
    //     log($"tell me how you got to this point");
    //   }
    // }

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


    public static void GameSession_StartRound_clearLightDict(LevelData? levelData, bool mirrorLevel, SubmarineInfo? startOutpost, SubmarineInfo? endOutpost)
    {
      findLightSources();
    }

    public void patchLightComponent()
    {

      // cursed, dont't touch
      // harmony.Patch(
      //   original: typeof(LightComponent).GetConstructors()[0],
      //   postfix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("LightComponent_Constructor_Postfix"))
      // );

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