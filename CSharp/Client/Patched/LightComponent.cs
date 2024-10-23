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
  partial class Plugin
  {
    public static Dictionary<LightSource, LightComponent> lightSource_lightComponent = new Dictionary<LightSource, LightComponent>();


    //public static void LightComponent_Constructor_Postfix(LightComponent __instance)
    //{
    //  try
    //  {
    //    // how the fuck you're still not initialised?
    //    if (lightSource_lightComponent == null)
    //    {
    //      lightSource_lightComponent = new Dictionary<LightSource, LightComponent>();
    //    }

    //    lightSource_lightComponent[__instance.Light] = __instance;
    //  }
    //  catch (Exception e)
    //  {
    //    log($"Something terrible happened in LightComponent_Constructor_Postfix on line 37");
    //    log($"tell me how you got to this point");
    //  }
    //}

    public static void findLightSources()
    {
      try
      {
        // how the fuck you're still not initialised?
        if (lightSource_lightComponent == null)
        {
          lightSource_lightComponent = new Dictionary<LightSource, LightComponent>();
        }

        if (lightSource_lightComponent == null) log($"bruh \n", Color.Yellow);

        lightSource_lightComponent.Clear();

        foreach (Item item in Item.ItemList)
        {
          foreach (LightComponent lc in item.GetComponents<LightComponent>())
          {
            lightSource_lightComponent[lc.Light] = lc;
          }
        }
      }
      catch (Exception e)
      {
        findLightSourcesRetries++;

        log(e, Color.Orange);

        if (findLightSourcesRetries < 3)
        {
          GameMain.LuaCs.Timer.Wait((object[] args) =>
          {
            findLightSources();
          }, 1000);
        }

        if (findLightSourcesRetries >= 3)
        {
          log("I tried 3 times and something is still null, what do you want from me? bruh", Color.Orange);
        }
      }
    }

    public static int findLightSourcesRetries = 0;

    public static void GameSession_StartRound_clearLightDict(LevelData? levelData, bool mirrorLevel, SubmarineInfo? startOutpost, SubmarineInfo? endOutpost)
    {
      findLightSourcesRetries = 0;
      findLightSources();
    }

    public void patchLightComponent()
    {
      // harmony.Patch(
      //   original: typeof(LightComponent).GetConstructors()[0],
      //   postfix: new HarmonyMethod(typeof(Plugin).GetMethod("LightComponent_Constructor_Postfix"))
      // );

      harmony.Patch(
        original: typeof(GameSession).GetMethod("StartRound", new Type[]{
          typeof(LevelData),
          typeof(bool),
          typeof(SubmarineInfo),
          typeof(SubmarineInfo)
        }),
        postfix: new HarmonyMethod(typeof(Plugin).GetMethod("GameSession_StartRound_clearLightDict"))
      );
    }
  }
}