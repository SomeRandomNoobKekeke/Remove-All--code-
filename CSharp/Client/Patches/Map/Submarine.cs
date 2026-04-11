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
  public static class SubmarinePatch
  {
    public static void Patch(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(Submarine).GetMethod("CullEntities"),
        prefix: new HarmonyMethod(typeof(SubmarinePatch).GetMethod("Submarine_CullEntities_Replace"))
      );
    }

    public static bool Submarine_CullEntities_Replace(Camera cam, Submarine __instance)
    {
      Submarine _ = __instance;

      Rectangle camView = cam.WorldView;

      camView = new Rectangle(
        camView.X - Mod.Settings.Submarine.CullMarginX,
        camView.Y + Mod.Settings.Submarine.CullMarginY,
        camView.Width + Mod.Settings.Submarine.CullMarginX * 2,
        camView.Height + Mod.Settings.Submarine.CullMarginY * 2
      );

      if (Level.Loaded?.Renderer?.CollapseEffectStrength is > 0.0f)
      {
        //force everything to be visible when the collapse effect (which moves everything to a single point) is active
        camView = Rectangle.Union(Submarine.AbsRect(camView.Location.ToVector2(), camView.Size.ToVector2()), new Rectangle(Point.Zero, Level.Loaded.Size));
        camView.Y += camView.Height;
      }

      if (Math.Abs(camView.X - Submarine.prevCullArea.X) < Mod.Settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Y - Submarine.prevCullArea.Y) < Mod.Settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Right - Submarine.prevCullArea.Right) < Mod.Settings.Submarine.CullMoveThreshold &&
          Math.Abs(camView.Bottom - Submarine.prevCullArea.Bottom) < Mod.Settings.Submarine.CullMoveThreshold &&
          Submarine.prevCullTime > Timing.TotalTime - Mod.Settings.Submarine.CullInterval)
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


        //TODO
        // note: linked subs are entities without prefab
        // if (Mod.Settings.Hide.Entities && entity.Prefab != null)
        // {
        //   string id = entity.Prefab.Identifier.Value;

        //   if (Mod.mapEntityBlacklist.TryGetValue(id, out bool value)) { if (!value) continue; }
        // }



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
  }
}