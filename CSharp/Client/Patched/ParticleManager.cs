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

namespace RemoveAll
{
  partial class RemoveAllMod
  {

    public static bool ParticleManager_CreateParticle_Prefix(ParticlePrefab prefab, Vector2 position, Vector2 velocity, float rotation, Hull hullGuess, bool drawOnTop, float collisionIgnoreTimer, float lifeTimeMultiplier, Tuple<Vector2, Vector2> tracerPoints, Particle __result)
    {
      if (settings.hide.particles && prefab != null)
      {

        if (blacklist["particles"].TryGetValue(prefab.Identifier.Value, out bool value))
        {
          //log($"{prefab.Identifier.Value} {value}");
          if (!value)
          {
            __result = null;
            return false;
          };
        }
      }
      return true;
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