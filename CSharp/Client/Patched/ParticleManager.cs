using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Barotrauma.Particles;

// arghhhh
using System.Runtime.CompilerServices;
[assembly: IgnoresAccessChecksTo("Barotrauma")]
[assembly: IgnoresAccessChecksTo("DedicatedServer")]
[assembly: IgnoresAccessChecksTo("BarotraumaCore")]

namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public static bool ParticleManager_CreateParticle_Prefix(ParticlePrefab prefab, Vector2 position, Vector2 velocity, float rotation, Hull hullGuess, bool drawOnTop, float collisionIgnoreTimer, float lifeTimeMultiplier, Tuple<Vector2, Vector2> tracerPoints, ParticleManager __instance, Particle __result)
    {
      ParticleManager _ = __instance;

      if (settings.hide.particles && prefab != null)
      {
        if (blacklist["particles"].TryGetValue(prefab.Identifier.Value, out bool value))
        {
          if (!value)
          {
            __result = null;
            return false;
          };
        }
      }

      int MaxParticles = Math.Min(_.MaxParticles, settings.maxParticles);


      if (prefab == null || prefab.Sprites.Count == 0) { __result = null; return false; }
      if (_.particleCount >= MaxParticles)
      {
        for (int i = 0; i < _.particleCount; i++)
        {
          if (_.particles[i].Prefab.Priority < prefab.Priority ||
              (!_.particles[i].Prefab.DrawAlways && prefab.DrawAlways))
          {
            _.RemoveParticle(i);
            break;
          }
        }
        if (_.particleCount >= MaxParticles) { __result = null; return false; }
      }

      Vector2 particleEndPos = prefab.CalculateEndPosition(position, velocity);

      Vector2 minPos = new Vector2(Math.Min(position.X, particleEndPos.X), Math.Min(position.Y, particleEndPos.Y));
      Vector2 maxPos = new Vector2(Math.Max(position.X, particleEndPos.X), Math.Max(position.Y, particleEndPos.Y));

      if (tracerPoints != null)
      {
        minPos = new Vector2(
            Math.Min(Math.Min(minPos.X, tracerPoints.Item1.X), tracerPoints.Item2.X),
            Math.Min(Math.Min(minPos.Y, tracerPoints.Item1.Y), tracerPoints.Item2.Y));
        maxPos = new Vector2(
            Math.Max(Math.Max(maxPos.X, tracerPoints.Item1.X), tracerPoints.Item2.X),
            Math.Max(Math.Max(maxPos.Y, tracerPoints.Item1.Y), tracerPoints.Item2.Y));
      }

      Rectangle expandedViewRect = MathUtils.ExpandRect(_.cam.WorldView, ParticleManager.MaxOutOfViewDist);

      if (!prefab.DrawAlways)
      {
        if (minPos.X > expandedViewRect.Right || maxPos.X < expandedViewRect.X) { __result = null; return false; }
        if (minPos.Y > expandedViewRect.Y || maxPos.Y < expandedViewRect.Y - expandedViewRect.Height) { __result = null; return false; }
      }

      if (_.particles[_.particleCount] == null) { _.particles[_.particleCount] = new Particle(); }
      Particle particle = _.particles[_.particleCount];

      particle.Init(prefab, position, velocity, rotation, hullGuess, drawOnTop, collisionIgnoreTimer, lifeTimeMultiplier, tracerPoints: tracerPoints);
      _.particleCount++;
      _.particlesInCreationOrder.AddFirst(particle);

      __result = particle; return false;
    }

    public void patchParticleManager()
    {
      harmony.Patch(
        original: typeof(ParticleManager).GetMethod("CreateParticle", new Type[]{
          typeof(ParticlePrefab),
          typeof(Vector2),
          typeof(Vector2),
          typeof(float),
          typeof(Hull),
          typeof(bool),
          typeof(float),
          typeof(float),
          typeof(Tuple<Vector2, Vector2> )
        }),
        prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("ParticleManager_CreateParticle_Prefix"))
      );
    }
  }
}