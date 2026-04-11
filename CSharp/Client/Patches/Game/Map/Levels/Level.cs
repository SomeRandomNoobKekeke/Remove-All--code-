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
  public static class LevelPatch
  {
    public static void Patch(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(Level).GetMethod("DrawBack"),
        prefix: new HarmonyMethod(typeof(LevelPatch).GetMethod("Level_DrawBack_Replace"))
      );
    }

    public static bool Level_DrawBack_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, Level __instance)
    {
      Level _ = __instance;

      float brightness = MathHelper.Clamp(1.1f + (cam.Position.Y - _.Size.Y) / 100000.0f, 0.1f, 1.0f);
      var lightColorHLS = _.GenerationParams.AmbientLightColor.RgbToHLS();
      lightColorHLS.Y *= brightness;

      GameMain.LightManager.AmbientLight = ToolBox.HLSToRGB(lightColorHLS);

      graphics.Clear(_.BackgroundColor);

      if (_.renderer != null)
      {
        GameMain.LightManager.AmbientLight = GameMain.LightManager.AmbientLight.Add(_.renderer.FlashColor);
        _.renderer?.DrawBackground(spriteBatch, cam, _.LevelObjectManager, _.backgroundCreatureManager);
      }

      return false;
    }
  }
}