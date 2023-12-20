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

    public static bool Update(float deltaTime, Camera cam, LevelRenderer __instance)
    {
      LevelRenderer _ = __instance;

      if (updateSettings.UpdateCollapseEffect)
      {
        if (_.CollapseEffectStrength > 0.0f)
        {
          _.CollapseEffectStrength = Math.Max(0.0f, _.CollapseEffectStrength - deltaTime);
        }
        if (_.ChromaticAberrationStrength > 0.0f)
        {
          _.ChromaticAberrationStrength = Math.Max(0.0f, _.ChromaticAberrationStrength - deltaTime * 10.0f);
        }
      }

      if (updateSettings.UpdateFlashes)
      {
        if (_.level.GenerationParams.FlashInterval.Y > 0)
        {
          PropertyInfo FlashColorProp = typeof(LevelRenderer).GetProperty("FlashColor");

          _.flashCooldown -= deltaTime;
          if (_.flashCooldown <= 0.0f)
          {
            _.flashTimer = 1.0f;
            if (_.level.GenerationParams.FlashSound != null)
            {
              _.level.GenerationParams.FlashSound.Play(1.0f, "default");
            }
            _.flashCooldown = Rand.Range(_.level.GenerationParams.FlashInterval.X, _.level.GenerationParams.FlashInterval.Y, Rand.RandSync.Unsynced);
          }
          if (_.flashTimer > 0.0f)
          {
            float brightness = _.flashTimer * 1.1f - PerlinNoise.GetPerlin((float)Timing.TotalTime, (float)Timing.TotalTime * 0.66f) * 0.1f;
            FlashColorProp.SetValue(_, _.level.GenerationParams.FlashColor.Multiply(MathHelper.Clamp(brightness, 0.0f, 1.0f)));
            _.flashTimer -= deltaTime * 0.5f;
          }
          else
          {
            FlashColorProp.SetValue(_, Color.TransparentBlack);
          }
        }
      }

      if (updateSettings.UpdateWaterParticleVel)
      {
        //calculate the sum of the forces of nearby level triggers
        //and use it to move the water texture and water distortion effect
        Vector2 currentWaterParticleVel = _.level.GenerationParams.WaterParticleVelocity;
        foreach (LevelObject levelObject in _.level.LevelObjectManager.GetVisibleObjects())
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

        if (_.level.GenerationParams.WaterParticles != null)
        {
          Vector2 waterTextureSize = _.level.GenerationParams.WaterParticles.size * _.level.GenerationParams.WaterParticleScale;
          _.waterParticleOffset += new Vector2(_.waterParticleVelocity.X, -_.waterParticleVelocity.Y) * _.level.GenerationParams.WaterParticleScale * deltaTime;
          while (_.waterParticleOffset.X <= -waterTextureSize.X) { _.waterParticleOffset.X += waterTextureSize.X; }
          while (_.waterParticleOffset.X >= waterTextureSize.X) { _.waterParticleOffset.X -= waterTextureSize.X; }
          while (_.waterParticleOffset.Y <= -waterTextureSize.Y) { _.waterParticleOffset.Y += waterTextureSize.Y; }
          while (_.waterParticleOffset.Y >= waterTextureSize.Y) { _.waterParticleOffset.Y -= waterTextureSize.Y; }
        }
      }


      return false;
    }

  }
}