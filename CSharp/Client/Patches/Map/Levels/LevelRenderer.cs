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


    public static bool LevelRenderer_Update_Replace(float deltaTime, Camera cam, LevelRenderer __instance)
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
        foreach (LevelObject levelObject in _.level.LevelObjectManager.GetAllVisibleObjects())
        {
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

    public static bool LevelRenderer_DrawBackground_Replace(SpriteBatch spriteBatch, Camera cam, LevelObjectManager backgroundSpriteManager, BackgroundCreatureManager backgroundCreatureManager, ParticleManager particleManager, LevelRenderer __instance)
    {
      LevelRenderer _ = __instance;

      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap);

      _.level.GenerationParams.DrawBackgrounds(spriteBatch, cam);

      spriteBatch.End();

      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null,
          cam.Transform);

      backgroundSpriteManager?.DrawObjectsBack(spriteBatch, cam);

      if (cam.Zoom > 0.05f)
      {
        backgroundCreatureManager?.Draw(spriteBatch, cam);
      }

      if (Mod.Settings.LevelRenderer.DrawWaterParticles && _.level.GenerationParams.WaterParticles != null && cam.Zoom > 0.05f)
      {
        float textureScale = _.level.GenerationParams.WaterParticleScale;

        Rectangle srcRect = new Rectangle(0, 0, 2048, 2048);
        Vector2 origin = new Vector2(cam.WorldView.X, -cam.WorldView.Y);
        Vector2 offset = -origin + _.waterParticleOffset;
        while (offset.X <= -srcRect.Width * textureScale) offset.X += srcRect.Width * textureScale;
        while (offset.X > 0.0f) offset.X -= srcRect.Width * textureScale;
        while (offset.Y <= -srcRect.Height * textureScale) offset.Y += srcRect.Height * textureScale;
        while (offset.Y > 0.0f) offset.Y -= srcRect.Height * textureScale;



        string biome = _.level.GenerationParams.AnyBiomeAllowed
          ? "outpost"
          : _.level.GenerationParams.AllowedBiomeIdentifiers.First().Value;

        int waterParticlelayerCount = Mod.Settings.LevelRenderer.WaterParticles.Get(biome);

        for (int i = 0; i < waterParticlelayerCount; i++)
        {
          float scale = (1.0f - i * 0.2f);

          //alpha goes from 1.0 to 0.0 when scale is in the range of 0.1 - 0.05
          float alpha = (cam.Zoom * scale) < 0.1f ? (cam.Zoom * scale - 0.05f) * 20.0f : 1.0f;
          if (alpha <= 0.0f) continue;

          Vector2 offsetS = offset * scale
              + new Vector2(cam.WorldView.Width, cam.WorldView.Height) * (1.0f - scale) * 0.5f
              - new Vector2(256.0f * i);

          float texScale = scale * textureScale;

          while (offsetS.X <= -srcRect.Width * texScale) offsetS.X += srcRect.Width * texScale;
          while (offsetS.X > 0.0f) offsetS.X -= srcRect.Width * texScale;
          while (offsetS.Y <= -srcRect.Height * texScale) offsetS.Y += srcRect.Height * texScale;
          while (offsetS.Y > 0.0f) offsetS.Y -= srcRect.Height * texScale;

          _.level.GenerationParams.WaterParticles.DrawTiled(
              spriteBatch, origin + offsetS,
              new Vector2(cam.WorldView.Width - offsetS.X, cam.WorldView.Height - offsetS.Y),
              color: _.level.GenerationParams.WaterParticleColor * alpha, textureScale: new Vector2(texScale));
        }
      }

      GameMain.ParticleManager?.Draw(spriteBatch, inWater: true, inSub: false, ParticleBlendState.AlphaBlend, background: true);

      spriteBatch.End();

      _.RenderWalls(GameMain.Instance.GraphicsDevice, cam);

      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null,
          cam.Transform);
      backgroundSpriteManager?.DrawObjectsMid(spriteBatch, cam);
      spriteBatch.End();


      return false;
    }

    public static bool LevelRenderer_DrawForeground_Replace(SpriteBatch spriteBatch, Camera cam, LevelObjectManager backgroundSpriteManager, LevelRenderer __instance)
    {
      spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null,
          cam.Transform);
      backgroundSpriteManager?.DrawObjectsFront(spriteBatch, cam);
      spriteBatch.End();

      return false;
    }



  }
}