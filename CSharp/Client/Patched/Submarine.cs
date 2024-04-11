using System;
using System.Reflection;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;

using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RemoveAll
{
  partial class RemoveAllMod
  {

    public static bool CullEntities(Camera cam, Submarine __instance)
    {
      Submarine _ = __instance;

      Rectangle camView = cam.WorldView;
      camView = new Rectangle(camView.X - Submarine.CullMargin, camView.Y + Submarine.CullMargin, camView.Width + Submarine.CullMargin * 2, camView.Height + Submarine.CullMargin * 2);

      if (Level.Loaded?.Renderer?.CollapseEffectStrength is > 0.0f)
      {
        //force everything to be visible when the collapse effect (which moves everything to a single point) is active
        camView = Rectangle.Union(Submarine.AbsRect(camView.Location.ToVector2(), camView.Size.ToVector2()), new Rectangle(Point.Zero, Level.Loaded.Size));
        camView.Y += camView.Height;
      }

      if (Math.Abs(camView.X - Submarine.prevCullArea.X) < Submarine.CullMoveThreshold &&
          Math.Abs(camView.Y - Submarine.prevCullArea.Y) < Submarine.CullMoveThreshold &&
          Math.Abs(camView.Right - Submarine.prevCullArea.Right) < Submarine.CullMoveThreshold &&
          Math.Abs(camView.Bottom - Submarine.prevCullArea.Bottom) < Submarine.CullMoveThreshold &&
          Submarine.prevCullTime > Timing.TotalTime - Submarine.CullInterval)
      {
        return false;
      }

      Submarine.visibleSubs.Clear();
      foreach (Submarine sub in Submarine.Loaded)
      {
        if (Level.Loaded != null && sub.WorldPosition.Y < Level.MaxEntityDepth) { continue; }

        Rectangle worldBorders = new Rectangle(
            sub.VisibleBorders.X + (int)sub.WorldPosition.X,
            sub.VisibleBorders.Y + (int)sub.WorldPosition.Y,
            sub.VisibleBorders.Width,
            sub.VisibleBorders.Height);

        if (Submarine.RectsOverlap(worldBorders, camView))
        {
          Submarine.visibleSubs.Add(sub);
        }
      }

      if (Submarine.visibleEntities == null)
      {
        Submarine.visibleEntities = new List<MapEntity>(MapEntity.MapEntityList.Count);
      }
      else
      {
        Submarine.visibleEntities.Clear();
      }

      foreach (MapEntity entity in MapEntity.MapEntityList)
      {
        if (entity == null || entity.Removed) { continue; }

        string id = entity.Prefab.Identifier.Value;


        bool value;
        if (mapEntityBlacklist.TryGetValue(id, out value)) { if (!value) continue; }



        if (entity.Submarine != null)
        {
          if (!Submarine.visibleSubs.Contains(entity.Submarine)) { continue; }
        }
        if (entity.IsVisible(camView)) { Submarine.visibleEntities.Add(entity); }
      }

      Submarine.prevCullArea = camView;
      Submarine.prevCullTime = Timing.TotalTime;

      return false;
    }

    public void patchSubmarine()
    {
      harmony.Patch(
        original: typeof(Submarine).GetMethod("CullEntities"),
        prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("CullEntities"))
      );
    }
  }
}