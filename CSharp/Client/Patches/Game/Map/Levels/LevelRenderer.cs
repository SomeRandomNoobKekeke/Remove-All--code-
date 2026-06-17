using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using Barotrauma.Particles;
using Barotrauma.Extensions;
using Voronoi2;


namespace RemoveAll
{
  public static class LevelRendererPatch
  {
    public static void Patch(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("Update"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("LevelRenderer_Update_Replace"))
      );

      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("DrawBackground"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("LevelRenderer_DrawBackground_Replace"))
      );

      harmony.Patch(
        original: typeof(LevelRenderer).GetMethod("DrawForeground"),
        prefix: new HarmonyMethod(typeof(LevelRendererPatch).GetMethod("LevelRenderer_DrawForeground_Replace"))
      );
    }


    public static bool LevelRenderer_Update_Replace(LevelRenderer __instance, float deltaTime, Camera cam)
    {
      LevelRenderer _ = __instance;

      if (_.CollapseEffectStrength > 0.0f)
      {
        _.CollapseEffectStrength = Math.Max(0.0f, _.CollapseEffectStrength - deltaTime);
      }
      if (_.ChromaticAberrationStrength > 0.0f)
      {
        _.ChromaticAberrationStrength = Math.Max(0.0f, _.ChromaticAberrationStrength - deltaTime * 10.0f);
      }

      if (_.level.GenerationParams.FlashInterval.Y > 0)
      {
        _.flashCooldown -= deltaTime;
        if (_.flashCooldown <= 0.0f)
        {
          _.flashTimer = 1.0f;
          _.level.GenerationParams.FlashSound?.Play(1.0f, Barotrauma.Sounds.SoundManager.SoundCategoryDefault);
          _.flashCooldown = Rand.Range(_.level.GenerationParams.FlashInterval.X, _.level.GenerationParams.FlashInterval.Y, Rand.RandSync.Unsynced);
        }
        if (_.flashTimer > 0.0f)
        {
          float brightness = _.flashTimer * 1.1f - PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.66f) * 0.1f;
          _.FlashColor = _.level.GenerationParams.FlashColor.Multiply(MathHelper.Clamp(brightness, 0.0f, 1.0f));
          _.flashTimer -= deltaTime * 0.5f;
        }
        else
        {
          _.FlashColor = Color.TransparentBlack;
        }
      }

      //calculate the sum of the forces of nearby level triggers
      //and use it to move the water texture and water distortion effect

      if (Mod.Settings.LevelRenderer.DrawWaterParticles)
      {
        Vector2 currentWaterParticleVel = _.level.GenerationParams.WaterParticleVelocity;
        foreach (ILevelRenderableObject obj in _.level.LevelObjectManager.GetAllVisibleObjects())
        {
          if (obj is not LevelObject levelObject) { continue; }
          if (levelObject.Triggers == null) { continue; }
          //use the largest water flow velocity of all the triggers
          Vector2 objectMaxFlow = Vector2.Zero;
          foreach (LevelTrigger trigger in levelObject.Triggers)
          {
            Vector2 vel = trigger.GetWaterFlowVelocity(cam.WorldViewCenter);
            if (vel.LengthSquared() > objectMaxFlow.LengthSquared())
            {
              objectMaxFlow = vel;
            }
          }
          currentWaterParticleVel += objectMaxFlow;
        }



        _.waterParticleVelocity = Vector2.Lerp(_.waterParticleVelocity, currentWaterParticleVel, deltaTime);

        WaterRenderer.Instance?.ScrollWater(_.waterParticleVelocity, deltaTime);

        Vector2 waterParticleOffsetProxy = Vector2.Zero;
        _.level.GenerationParams.UpdateWaterParticleOffset(ref waterParticleOffsetProxy, _.waterParticleVelocity, deltaTime);
        _.waterParticleOffset = waterParticleOffsetProxy;

      }

      return false;
    }

    public static bool LevelRenderer_DrawBackground_Replace(
               LevelRenderer __instance,
               SpriteBatch spriteBatch, Camera cam,
               LevelObjectManager backgroundSpriteManager = null,
               BackgroundCreatureManager backgroundCreatureManager = null,
               ParticleManager particleManager = null)
    {
      LevelRenderer _ = __instance;

      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);

      _.level.GenerationParams.DrawBackgrounds(spriteBatch, cam);

      spriteBatch.End();

      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null,
          cam.Transform);

      backgroundSpriteManager?.DrawObjectsBack(spriteBatch, backgroundCreatureManager, cam);

      _.level.GenerationParams.DrawWaterParticles(spriteBatch, cam, _.waterParticleOffset);

      GameMain.ParticleManager?.Draw(spriteBatch, inWater: true, inSub: false, ParticleBlendState.AlphaBlend, background: true);

      spriteBatch.End();

      _.RenderWalls(GameMain.Instance.GraphicsDevice, cam);

      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null,
          cam.Transform);
      backgroundSpriteManager?.DrawObjectsMid(spriteBatch, backgroundCreatureManager, cam);
      spriteBatch.End();

      return false;
    }

    public static bool LevelRenderer_DrawForeground_Replace(LevelRenderer __instance, SpriteBatch spriteBatch, Camera cam, BackgroundCreatureManager backgroundCreatureManager, LevelObjectManager backgroundSpriteManager = null)
    {
      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null,
          cam.Transform);
      backgroundSpriteManager?.DrawObjectsFront(spriteBatch, backgroundCreatureManager, cam);
      spriteBatch.End();

      return false;
    }



  }
}