using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using BaroJunk;
using System.Text.Json;

using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Items.Components;

namespace RemoveAll
{

  public partial class LightSourceTracker
  {
    public static void Patch(Harmony harmony)
    {
      //BRUH is it even working?
      harmony.Patch(
        original: typeof(LightComponent).GetConstructors()[0],
        postfix: new HarmonyMethod(typeof(LightSourceTracker).GetMethod("LightComponent_Constructor_Postfix"))
      );

      harmony.Patch(
        original: typeof(GameSession).GetMethod("StartRound", new Type[]{
          typeof(LevelData),
          typeof(bool),
          typeof(SubmarineInfo),
          typeof(SubmarineInfo)
        }),
        postfix: new HarmonyMethod(typeof(LightSourceTracker).GetMethod("GameSession_StartRound_Postfix"))
      );
    }

    public static void GameSession_StartRound_Postfix()
    {
      Mod.LightSourceTracker?.FindLightSources();
    }

    public static void LightComponent_Constructor_Postfix(LightComponent __instance)
    {
      if (Mod.LightSourceTracker is null) return;
      Mod.LightSourceTracker.ReverseLookup[__instance.Light] = __instance;
    }
  }
}