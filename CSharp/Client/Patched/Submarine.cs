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
  public class SubmarineSettings
  {
    /// <summary>
    /// Interval at which we force culled entites to be updated, regardless if the camera has moved
    /// </summary>
    public float CullInterval { get; set; } = 0.25f;
    /// <summary>
    /// Margin applied around the view area when culling entities (i.e. entities that are this far outside the view are still considered visible)
    /// </summary>
    public int CullMarginX { get; set; } = 0;
    public int CullMarginY { get; set; } = 0;
    /// <summary>
    /// Update entity culling when any corner of the view has moved more than this
    /// </summary>
    public int CullMoveThreshold { get; set; } = 50;
  }

  partial class RemoveAllMod
  {

    public static bool Submarine_CullEntities_Prefix(Camera cam, Submarine __instance)
    {
      Submarine _ = __instance;

      Rectangle camView = cam.WorldView;

      camView = new Rectangle(
        camView.X - settings.Submarine.CullMarginX,
        camView.Y + settings.Submarine.CullMarginY,
        camView.Width + settings.Submarine.CullMarginX * 2,
        camView.Height + settings.Submarine.CullMarginY * 2
      );

      if (Level.Loaded?.Renderer?.CollapseEffectStrength is > 0.0f)
      {
        //force everything to be visible when the collapse effect (which moves everything to a single point) is active
        camView = Rectangle.Union(Submarine.AbsRect(camView.Location.ToVector2(), camView.Size.ToVector2()), new Rectangle(Point.Zero, Level.Loaded.Size));
        camView.Y += camView.Height;
      }

      if (Math.Abs(camView.X - Submarine.prevCullArea.X) < settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Y - Submarine.prevCullArea.Y) < settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Right - Submarine.prevCullArea.Right) < settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Bottom - Submarine.prevCullArea.Bottom) < settings.Submarine.CullMoveThreshold &&
          Submarine.prevCullTime > Timing.TotalTime - settings.Submarine.CullInterval)
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


        // note: linked subs are entities without prefab
        if (settings.hide.entities && entity.Prefab != null)
        {
          string id = entity.Prefab.Identifier.Value;

          if (mapEntityBlacklist.TryGetValue(id, out bool value)) { if (!value) continue; }
        }



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
        prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("Submarine_CullEntities_Prefix"))
      );
    }
  }
}