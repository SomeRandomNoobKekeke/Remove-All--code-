using Barotrauma;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi2;

using System.Reflection;


namespace RemoveAll
{

  partial class LevelRendererPatch
  {

    public class UpdateSettings
    {
      public bool UpdateCollapseEffect { get; set; } = true;
      public bool UpdateFlashes { get; set; } = true;
      public bool UpdateWaterParticleVel { get; set; } = false;
      public UpdateSettings() { }
    }
    public static UpdateSettings updateSettings = new UpdateSettings();

    public static bool Update(float deltaTime, Camera cam,
    LevelRenderer __instance, Level ___level, ref float ___flashCooldown, ref float ___flashTimer, ref Vector2 ___waterParticleOffset, ref Vector2 ___waterParticleVelocity)
    {
      if (updateSettings.UpdateCollapseEffect)
      {
        if (__instance.CollapseEffectStrength > 0.0f)
        {
          __instance.CollapseEffectStrength = Math.Max(0.0f, __instance.CollapseEffectStrength - deltaTime);
        }
        if (__instance.ChromaticAberrationStrength > 0.0f)
        {
          __instance.ChromaticAberrationStrength = Math.Max(0.0f, __instance.ChromaticAberrationStrength - deltaTime * 10.0f);
        }
      }

      if (updateSettings.UpdateFlashes)
      {
        if (___level.GenerationParams.FlashInterval.Y > 0)
        {
          PropertyInfo FlashColorProp = typeof(LevelRenderer).GetProperty("FlashColor");

          ___flashCooldown -= deltaTime;
          if (___flashCooldown <= 0.0f)
          {
            ___flashTimer = 1.0f;
            if (___level.GenerationParams.FlashSound != null)
            {
              ___level.GenerationParams.FlashSound.Play(1.0f, "default");
            }
            ___flashCooldown = Rand.Range(___level.GenerationParams.FlashInterval.X, ___level.GenerationParams.FlashInterval.Y, Rand.RandSync.Unsynced);
          }
          if (___flashTimer > 0.0f)
          {
            float brightness = ___flashTimer * 1.1f - PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.66f) * 0.1f;
            FlashColorProp.SetValue(__instance, ___level.GenerationParams.FlashColor.Multiply(MathHelper.Clamp(brightness, 0.0f, 1.0f)));
            ___flashTimer -= deltaTime * 0.5f;
          }
          else
          {
            FlashColorProp.SetValue(__instance, Color.TransparentBlack);
          }
        }
      }

      if (updateSettings.UpdateWaterParticleVel)
      {
        //calculate the sum of the forces of nearby level triggers
        //and use it to move the water texture and water distortion effect
        Vector2 currentWaterParticleVel = ___level.GenerationParams.WaterParticleVelocity;
        foreach (LevelObject levelObject in ___level.LevelObjectManager.GetVisibleObjects())
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

        ___waterParticleVelocity = Vector2.Lerp(___waterParticleVelocity, currentWaterParticleVel, deltaTime);

        WaterRenderer.Instance?.ScrollWater(___waterParticleVelocity, deltaTime);

        if (___level.GenerationParams.WaterParticles != null)
        {
          Vector2 waterTextureSize = ___level.GenerationParams.WaterParticles.size * ___level.GenerationParams.WaterParticleScale;
          ___waterParticleOffset += new Vector2(___waterParticleVelocity.X, -___waterParticleVelocity.Y) * ___level.GenerationParams.WaterParticleScale * deltaTime;
          while (___waterParticleOffset.X <= -waterTextureSize.X) { ___waterParticleOffset.X += waterTextureSize.X; }
          while (___waterParticleOffset.X >= waterTextureSize.X) { ___waterParticleOffset.X -= waterTextureSize.X; }
          while (___waterParticleOffset.Y <= -waterTextureSize.Y) { ___waterParticleOffset.Y += waterTextureSize.Y; }
          while (___waterParticleOffset.Y >= waterTextureSize.Y) { ___waterParticleOffset.Y -= waterTextureSize.Y; }
        }
      }


      return false;
    }

  }
}