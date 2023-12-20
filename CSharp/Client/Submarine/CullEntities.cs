using Barotrauma;
using Barotrauma.Extensions;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RemoveAll
{

  partial class SubmarinePatch
  {
    public class CullEntitiesSettings
    {
      /// <summary>
      /// Interval at which we force culled entites to be updated, regardless if the camera has moved
      /// </summary>
      public float CullInterval { get; set; } = 0.25f;
      /// <summary>
      /// Margin applied around the view area when culling entities (i.e. entities that are this far outside the view are still considered visible)
      /// </summary>
      public int CullMarginX { get; set; } = 500;
      public int CullMarginY { get; set; } = 500;
      /// <summary>
      /// Update entity culling when any corner of the view has moved more than this
      /// </summary>
      public int CullMoveThreshold { get; set; } = 50;

      public bool CullOutDecorations { get; set; } = false;
      public CullEntitiesSettings() { }
    }

    public static CullEntitiesSettings cullEntitiesSettings = new CullEntitiesSettings();

    public static Dictionary<string, bool> whitelist { get; set; } = new Dictionary<string, bool>();

    public static bool CullEntities(Camera cam,
      ref Rectangle ___prevCullArea, ref double ___prevCullTime,
      HashSet<Submarine> ___visibleSubs, ref List<MapEntity> ___visibleEntities)
    {
      Rectangle camView = cam.WorldView;
      camView = new Rectangle(
        camView.X - cullEntitiesSettings.CullMarginX,
        camView.Y + cullEntitiesSettings.CullMarginY,
        camView.Width + cullEntitiesSettings.CullMarginX * 2,
        camView.Height + cullEntitiesSettings.CullMarginY * 2
      );

      if (Level.Loaded?.Renderer?.CollapseEffectStrength is > 0.0f)
      {
        //force everything to be visible when the collapse effect (which moves everything to a single point) is active
        camView = Rectangle.Union(Submarine.AbsRect(camView.Location.ToVector2(), camView.Size.ToVector2()), new Rectangle(Point.Zero, Level.Loaded.Size));
        camView.Y += camView.Height;
      }

      if (Math.Abs(camView.X - ___prevCullArea.X) < cullEntitiesSettings.CullMoveThreshold &&
          Math.Abs(camView.Y - ___prevCullArea.Y) < cullEntitiesSettings.CullMoveThreshold &&
          Math.Abs(camView.Right - ___prevCullArea.Right) < cullEntitiesSettings.CullMoveThreshold &&
          Math.Abs(camView.Bottom - ___prevCullArea.Bottom) < cullEntitiesSettings.CullMoveThreshold &&
          ___prevCullTime > Timing.TotalTime - cullEntitiesSettings.CullInterval)
      {
        return false;
      }

      ___visibleSubs.Clear();
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
          ___visibleSubs.Add(sub);
        }
      }

      if (___visibleEntities == null)
      {
        ___visibleEntities = new List<MapEntity>(MapEntity.MapEntityList.Count);
      }
      else
      {
        ___visibleEntities.Clear();
      }

      foreach (MapEntity entity in MapEntity.MapEntityList)
      {
        if (entity == null || entity.Removed) { continue; }

        if (cullEntitiesSettings.CullOutDecorations && entity.Prefab != null)
        {
          if (whitelist.ContainsKey(entity.Prefab.Identifier.Value))
          {
            if (!whitelist[entity.Prefab.Identifier.Value])
            {
              continue;
            }
          }
        }

        if (entity.Submarine != null)
        {
          if (!___visibleSubs.Contains(entity.Submarine)) { continue; }
        }
        if (entity.IsVisible(camView)) { ___visibleEntities.Add(entity); }
      }

      ___prevCullArea = camView;
      ___prevCullTime = Timing.TotalTime;

      return false;
    }
  }
}