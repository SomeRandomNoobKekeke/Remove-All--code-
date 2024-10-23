using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Extensions;
using Barotrauma.Lights;
using System.Threading;



namespace RemoveAll
{
  public class LightManagerSettings
  {
    public bool drawHalo { get; set; } = true;
    public bool ghostCharacters { get; set; } = false;
    public bool highlightItems { get; set; } = true;
    public bool drawGapGlow { get; set; } = true;

    public float haloScale { get; set; } = 0.5f;
    public float haloBrightness { get; set; } = 0.3f;
    public float hullAmbientBrightness { get; set; } = 1.0f;
    public Color hullAmbientColor { get; set; } = new Color(0, 0, 0, 0);
    public float globalLightBrightness { get; set; } = 1f;
    public float levelAmbientBrightness { get; set; } = 1f;
  }

  partial class Plugin
  {

    public static bool LightManager_UpdateObstructVision_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, Vector2 lookAtPosition, LightManager __instance)
    {
      LightManager _ = __instance;

      if ((!_.LosEnabled || _.LosMode == LosMode.None) && _.ObstructVisionAmount <= 0.0f) { return false; }
      if (LightManager.ViewTarget == null) return false;

      graphics.SetRenderTarget(_.LosTexture);

      if (_.ObstructVisionAmount > 0.0f)
      {
        graphics.Clear(Color.Black);
        Vector2 diff = lookAtPosition - LightManager.ViewTarget.WorldPosition;
        diff.Y = -diff.Y;
        if (diff.LengthSquared() > 20.0f * 20.0f) { _.losOffset = diff; }
        float rotation = MathUtils.VectorToAngle(_.losOffset);

        //the visible area stretches to the maximum when the cursor is this far from the character
        const float MaxOffset = 256.0f;
        //the magic numbers here are just based on experimentation
        float MinHorizontalScale = MathHelper.Lerp(3.5f, 1.5f, _.ObstructVisionAmount);
        float MaxHorizontalScale = MinHorizontalScale * 1.25f;
        float VerticalScale = MathHelper.Lerp(4.0f, 1.25f, _.ObstructVisionAmount);

        //Starting point and scale-based modifier that moves the point of origin closer to the edge of the texture if the player moves their mouse further away, or vice versa.
        float relativeOriginStartPosition = 0.1f; //Increasing this value moves the origin further behind the character
        float originStartPosition = _.visionCircle.Width * relativeOriginStartPosition * MinHorizontalScale;
        float relativeOriginLookAtPosModifier = -0.055f; //Increase this value increases how much the vision changes by moving the mouse
        float originLookAtPosModifier = _.visionCircle.Width * relativeOriginLookAtPosModifier;

        Vector2 scale = new Vector2(
            MathHelper.Clamp(_.losOffset.Length() / MaxOffset, MinHorizontalScale, MaxHorizontalScale), VerticalScale);

        spriteBatch.Begin(SpriteSortMode.Deferred, transformMatrix: cam.Transform * Matrix.CreateScale(new Vector3(GameSettings.CurrentConfig.Graphics.LightMapScale, GameSettings.CurrentConfig.Graphics.LightMapScale, 1.0f)));
        spriteBatch.Draw(_.visionCircle, new Vector2(LightManager.ViewTarget.WorldPosition.X, -LightManager.ViewTarget.WorldPosition.Y), null, Color.White, rotation,
            new Vector2(originStartPosition + (scale.X * originLookAtPosModifier), _.visionCircle.Height / 2), scale, SpriteEffects.None, 0.0f);
        spriteBatch.End();
      }
      else
      {
        graphics.Clear(Color.White);
      }


      //--------------------------------------

      if (_.LosEnabled && _.LosMode != LosMode.None && LightManager.ViewTarget != null)
      {
        Vector2 pos = LightManager.ViewTarget.DrawPosition;
        bool centeredOnHead = false;
        if (LightManager.ViewTarget is Character character &&
            character.AnimController?.GetLimb(LimbType.Head) is Limb head &&
            !head.IsSevered && !head.Removed)
        {
          pos = head.body.DrawPosition;
          centeredOnHead = true;
        }

        Rectangle camView = new Rectangle(cam.WorldView.X, cam.WorldView.Y - cam.WorldView.Height, cam.WorldView.Width, cam.WorldView.Height);
        Matrix shadowTransform = cam.ShaderTransform
            * Matrix.CreateOrthographic(GameMain.GraphicsWidth, GameMain.GraphicsHeight, -1, 1) * 0.5f;

        var convexHulls = ConvexHull.GetHullsInRange(LightManager.ViewTarget.Position, cam.WorldView.Width * 0.75f, LightManager.ViewTarget.Submarine);

        //make sure the head isn't peeking through any LOS segments, and if it is,
        //center the LOS on the character's collider instead
        if (centeredOnHead)
        {
          foreach (var ch in convexHulls)
          {
            if (!ch.Enabled) { continue; }
            Vector2 currentViewPos = pos;
            Vector2 defaultViewPos = LightManager.ViewTarget.DrawPosition;
            if (ch.ParentEntity?.Submarine != null)
            {
              defaultViewPos -= ch.ParentEntity.Submarine.DrawPosition;
              currentViewPos -= ch.ParentEntity.Submarine.DrawPosition;
            }
            //check if a line from the character's collider to the head intersects with the los segment (= head poking through it)
            if (ch.LosIntersects(defaultViewPos, currentViewPos))
            {
              pos = LightManager.ViewTarget.DrawPosition;
            }
          }
        }

        if (convexHulls != null)
        {
          List<VertexPositionColor> shadowVerts = new List<VertexPositionColor>();
          List<VertexPositionTexture> penumbraVerts = new List<VertexPositionTexture>();
          foreach (ConvexHull convexHull in convexHulls)
          {
            if (!convexHull.Enabled || !convexHull.Intersects(camView)) { continue; }

            Vector2 relativeViewPos = pos;
            if (convexHull.ParentEntity?.Submarine != null)
            {
              relativeViewPos -= convexHull.ParentEntity.Submarine.DrawPosition;
            }

            convexHull.CalculateLosVertices(relativeViewPos);

            for (int i = 0; i < convexHull.ShadowVertexCount; i++)
            {
              shadowVerts.Add(convexHull.ShadowVertices[i]);
            }

            for (int i = 0; i < convexHull.PenumbraVertexCount; i++)
            {
              penumbraVerts.Add(convexHull.PenumbraVertices[i]);
            }
          }

          if (shadowVerts.Count > 0)
          {
            ConvexHull.shadowEffect.World = shadowTransform;
            ConvexHull.shadowEffect.CurrentTechnique.Passes[0].Apply();
            graphics.DrawUserPrimitives(PrimitiveType.TriangleList, shadowVerts.ToArray(), 0, shadowVerts.Count / 3, VertexPositionColor.VertexDeclaration);

            if (penumbraVerts.Count > 0)
            {
              ConvexHull.penumbraEffect.World = shadowTransform;
              ConvexHull.penumbraEffect.CurrentTechnique.Passes[0].Apply();
              graphics.DrawUserPrimitives(PrimitiveType.TriangleList, penumbraVerts.ToArray(), 0, penumbraVerts.Count / 3, VertexPositionTexture.VertexDeclaration);
            }
          }
        }
      }
      graphics.SetRenderTarget(null);

      return false;
    }


    public static bool LightManager_RenderLightMap_Replace(GraphicsDevice graphics, SpriteBatch spriteBatch, Camera cam, RenderTarget2D backgroundObstructor, LightManager __instance)
    {
      LightManager _ = __instance;


      if (!_.LightingEnabled) { return false; }

      if (Math.Abs(_.currLightMapScale - GameSettings.CurrentConfig.Graphics.LightMapScale) > 0.01f)
      {
        //lightmap scale has changed -> recreate render targets
        _.CreateRenderTargets(graphics);
      }

      Matrix spriteBatchTransform = cam.Transform * Matrix.CreateScale(new Vector3(GameSettings.CurrentConfig.Graphics.LightMapScale, GameSettings.CurrentConfig.Graphics.LightMapScale, 1.0f));
      Matrix transform = cam.ShaderTransform
          * Matrix.CreateOrthographic(GameMain.GraphicsWidth, GameMain.GraphicsHeight, -1, 1) * 0.5f;



      bool highlightsVisible;
      if (Mod.settings.LightManager.highlightItems)
      {
        highlightsVisible = _.UpdateHighlights(graphics, spriteBatch, spriteBatchTransform, cam);
      }
      else
      {
        highlightsVisible = false;
      }



      Rectangle viewRect = cam.WorldView;
      viewRect.Y -= cam.WorldView.Height;
      //check which lights need to be drawn
      _.recalculationCount = 0;
      _.activeLights.Clear();
      foreach (LightSource light in _.lights)
      {
        if (!light.Enabled) { continue; }
        if ((light.Color.A < 1 || light.Range < 1.0f) && !light.LightSourceParams.OverrideLightSpriteAlpha.HasValue) { continue; }

        if (Mod.settings.hide.itemLights)
        {
          string id = "";
          LightComponent lc;
          if (lightSource_lightComponent.TryGetValue(light, out lc))
          {
            id = lc.Item.Prefab.Identifier.Value;
          }



          bool value;
          if (Mod.mapEntityBlacklist.TryGetValue(id, out value)) { if (!value) continue; }
        }

        if (light.ParentBody != null)
        {
          light.ParentBody.UpdateDrawPosition();

          Vector2 pos = light.ParentBody.DrawPosition;
          if (light.ParentSub != null) { pos -= light.ParentSub.DrawPosition; }
          light.Position = pos;
        }

        //above the top boundary of the level (in an inactive respawn shuttle?)
        if (Level.IsPositionAboveLevel(light.WorldPosition)) { continue; }

        float range = light.LightSourceParams.TextureRange;
        if (light.LightSprite != null)
        {
          float spriteRange = Math.Max(
              light.LightSprite.size.X * light.SpriteScale.X * (0.5f + Math.Abs(light.LightSprite.RelativeOrigin.X - 0.5f)),
              light.LightSprite.size.Y * light.SpriteScale.Y * (0.5f + Math.Abs(light.LightSprite.RelativeOrigin.Y - 0.5f)));

          float targetSize = Math.Max(light.LightTextureTargetSize.X, light.LightTextureTargetSize.Y);
          range = Math.Max(Math.Max(spriteRange, targetSize), range);
        }
        if (!MathUtils.CircleIntersectsRectangle(light.WorldPosition, range, viewRect)) { continue; }

        light.Priority = lightPriority(range, light);

        int i = 0;
        while (i < _.activeLights.Count && light.Priority < _.activeLights[i].Priority)
        {
          i++;
        }
        _.activeLights.Insert(i, light);
      }
      LightManager.ActiveLightCount = _.activeLights.Count;

      float lightPriority(float range, LightSource light)
      {
        return
            range *
            ((Character.Controlled?.Submarine != null && light.ParentSub == Character.Controlled?.Submarine) ? 2.0f : 1.0f) *
            (light.CastShadows ? 10.0f : 1.0f) *
            (light.LightSourceParams.OverrideLightSpriteAlpha ?? (light.Color.A / 255.0f)) *
            light.PriorityMultiplier;
      }

      //find the lights with an active light volume
      _.activeShadowCastingLights.Clear();
      foreach (var activeLight in _.activeLights)
      {
        if (activeLight.Range < 1.0f || activeLight.Color.A < 1 || activeLight.CurrentBrightness <= 0.0f) { continue; }
        _.activeShadowCastingLights.Add(activeLight);
      }

      //remove some lights with a light volume if there's too many of them
      if (_.activeShadowCastingLights.Count > GameSettings.CurrentConfig.Graphics.VisibleLightLimit && Screen.Selected is { IsEditor: false })
      {
        for (int i = GameSettings.CurrentConfig.Graphics.VisibleLightLimit; i < _.activeShadowCastingLights.Count; i++)
        {
          _.activeLights.Remove(_.activeShadowCastingLights[i]);
        }
      }
      _.activeLights.Sort((l1, l2) => l1.LastRecalculationTime.CompareTo(l2.LastRecalculationTime));

      //draw light sprites attached to characters
      //render into a separate rendertarget using alpha blending (instead of on top of everything else with alpha blending)
      //to prevent the lights from showing through other characters or other light sprites attached to the same character
      //---------------------------------------------------------------------------------------------------
      graphics.SetRenderTarget(_.LimbLightMap);
      graphics.Clear(Color.Black);
      graphics.BlendState = BlendState.NonPremultiplied;
      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, transformMatrix: spriteBatchTransform);
      foreach (LightSource light in _.activeLights)
      {
        if (light.IsBackground || light.CurrentBrightness <= 0.0f) { continue; }
        //draw limb lights at this point, because they were skipped over previously to prevent them from being obstructed
        if (light.ParentBody?.UserData is Limb limb && !limb.Hide) { light.DrawSprite(spriteBatch, cam); }
      }
      spriteBatch.End();

      //draw background lights
      //---------------------------------------------------------------------------------------------------
      graphics.SetRenderTarget(_.LightMap);


      graphics.Clear(_.AmbientLight.Multiply(Mod.settings.LightManager.levelAmbientBrightness));


      graphics.BlendState = BlendState.Additive;
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: spriteBatchTransform);
      Level.Loaded?.BackgroundCreatureManager?.DrawLights(spriteBatch, cam);
      foreach (LightSource light in _.activeLights)
      {
        if (!light.IsBackground || light.CurrentBrightness <= 0.0f) { continue; }
        light.DrawLightVolume(spriteBatch, _.lightEffect, transform, _.recalculationCount < LightManager.MaxLightVolumeRecalculationsPerFrame, ref _.recalculationCount);
        light.DrawSprite(spriteBatch, cam);
      }
      GameMain.ParticleManager.Draw(spriteBatch, true, null, Barotrauma.Particles.ParticleBlendState.Additive);
      spriteBatch.End();

      //draw a black rectangle on hulls to hide background lights behind subs
      //---------------------------------------------------------------------------------------------------

      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, transformMatrix: spriteBatchTransform);
      Dictionary<Hull, Rectangle> visibleHulls = _.GetVisibleHulls(cam);
      foreach (KeyValuePair<Hull, Rectangle> hull in visibleHulls)
      {
        Color ambientColor;
        if (Mod.settings.LightManager.hullAmbientColor != Color.TransparentBlack)
        {
          ambientColor = Mod.settings.LightManager.hullAmbientColor.Multiply(Mod.settings.LightManager.hullAmbientColor.A / 255.0f * Mod.settings.LightManager.hullAmbientBrightness).Opaque();
        }
        else
        {
          ambientColor = hull.Key.AmbientLight == Color.TransparentBlack ? Color.Black : hull.Key.AmbientLight.Multiply(hull.Key.AmbientLight.A / 255.0f * Mod.settings.LightManager.hullAmbientBrightness).Opaque();
        }

        GUI.DrawRectangle(spriteBatch,
            new Vector2(hull.Value.X, -hull.Value.Y),
            new Vector2(hull.Value.Width, hull.Value.Height),
            ambientColor, true);
      }
      spriteBatch.End();

      if (Mod.settings.LightManager.drawGapGlow)
      {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: spriteBatchTransform);
        Vector3 glowColorHSV = ToolBox.RGBToHSV(_.AmbientLight);
        glowColorHSV.Z = Math.Max(glowColorHSV.Z, 0.4f);
        Color glowColor = ToolBoxCore.HSVToRGB(glowColorHSV.X, glowColorHSV.Y, glowColorHSV.Z);
        Vector2 glowSpriteSize = new Vector2(_.gapGlowTexture.Width, _.gapGlowTexture.Height);
        foreach (var gap in Gap.GapList)
        {
          if (gap.IsRoomToRoom || gap.Open <= 0.0f || gap.ConnectedWall == null) { continue; }

          float a = MathHelper.Lerp(0.5f, 1.0f,
              PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.05f, gap.GlowEffectT));

          float scale = MathHelper.Lerp(0.5f, 2.0f,
              PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.01f, gap.GlowEffectT));

          float rot = PerlinNoise.GetPerlin((float)Timing.TotalTime * 0.001f, gap.GlowEffectT) * MathHelper.TwoPi;

          Vector2 spriteScale = new Vector2(gap.Rect.Width, gap.Rect.Height) / glowSpriteSize;
          Vector2 drawPos = new Vector2(gap.DrawPosition.X, -gap.DrawPosition.Y);

          spriteBatch.Draw(_.gapGlowTexture,
              drawPos,
              null,
              glowColor * a,
              rot,
              glowSpriteSize / 2,
              scale: Math.Max(spriteScale.X, spriteScale.Y) * scale,
              SpriteEffects.None,
              layerDepth: 0);
        }
        spriteBatch.End();
      }

      GameMain.GameScreen.DamageEffect.CurrentTechnique = GameMain.GameScreen.DamageEffect.Techniques["StencilShaderSolidColor"];
      GameMain.GameScreen.DamageEffect.Parameters["solidColor"].SetValue(Color.Black.ToVector4());
      spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, transformMatrix: spriteBatchTransform, effect: GameMain.GameScreen.DamageEffect);
      Submarine.DrawDamageable(spriteBatch, GameMain.GameScreen.DamageEffect);
      spriteBatch.End();

      graphics.BlendState = BlendState.Additive;

      //draw the focused item and character to highlight them,
      //and light sprites (done before drawing the actual light volumes so we can make characters obstruct the highlights and sprites)
      //---------------------------------------------------------------------------------------------------
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: spriteBatchTransform);
      foreach (LightSource light in _.activeLights)
      {
        //don't draw limb lights at this point, they need to be drawn after lights have been obstructed by characters
        if (light.IsBackground || light.ParentBody?.UserData is Limb || light.CurrentBrightness <= 0.0f) { continue; }
        light.DrawSprite(spriteBatch, cam);
      }
      spriteBatch.End();

      if (highlightsVisible)
      {
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
        spriteBatch.Draw(_.HighlightMap, Vector2.Zero, Color.White);
        spriteBatch.End();
      }

      //draw characters to obstruct the highlighted items/characters and light sprites
      //---------------------------------------------------------------------------------------------------

      if (!Mod.settings.LightManager.ghostCharacters && cam.Zoom > LightManager.ObstructLightsBehindCharactersZoomThreshold)
      {
        _.SolidColorEffect.CurrentTechnique = _.SolidColorEffect.Techniques["SolidVertexColor"];
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, effect: _.SolidColorEffect, transformMatrix: spriteBatchTransform);
        DrawCharacters(spriteBatch, cam, drawDeformSprites: false);
        spriteBatch.End();

        DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShaderSolidVertexColor"];
        DeformableSprite.Effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, transformMatrix: spriteBatchTransform);
        DrawCharacters(spriteBatch, cam, drawDeformSprites: true);
        spriteBatch.End();
      }

      static void DrawCharacters(SpriteBatch spriteBatch, Camera cam, bool drawDeformSprites)
      {
        foreach (Character character in Character.CharacterList)
        {
          if (character.CurrentHull == null || !character.Enabled || !character.IsVisible || character.InvisibleTimer > 0.0f) { continue; }
          if (Character.Controlled?.FocusedCharacter == character) { continue; }

          Color lightColor;
          if (Mod.settings.LightManager.hullAmbientColor != Color.TransparentBlack)
          {
            lightColor = Mod.settings.LightManager.hullAmbientColor.Multiply(Mod.settings.LightManager.hullAmbientColor.A / 255.0f * Mod.settings.LightManager.hullAmbientBrightness).Opaque();
          }
          else
          {
            lightColor = character.CurrentHull.AmbientLight == Color.TransparentBlack ? Color.Black : character.CurrentHull.AmbientLight.Multiply(character.CurrentHull.AmbientLight.A / 255.0f * Mod.settings.LightManager.hullAmbientBrightness).Opaque();
          }



          foreach (Limb limb in character.AnimController.Limbs)
          {
            if (drawDeformSprites == (limb.DeformSprite == null)) { continue; }
            limb.Draw(spriteBatch, cam, lightColor);
          }
          foreach (var heldItem in character.HeldItems)
          {
            heldItem.Draw(spriteBatch, editing: false, overrideColor: Color.Black);
          }
        }
      }

      DeformableSprite.Effect.CurrentTechnique = DeformableSprite.Effect.Techniques["DeformShader"];
      graphics.BlendState = BlendState.Additive;

      //draw the actual light volumes, additive particles, hull ambient lights and the halo around the player
      //---------------------------------------------------------------------------------------------------
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, transformMatrix: spriteBatchTransform);

      spriteBatch.Draw(_.LimbLightMap, new Rectangle(cam.WorldView.X, -cam.WorldView.Y, cam.WorldView.Width, cam.WorldView.Height), Color.White);

      foreach (ElectricalDischarger discharger in ElectricalDischarger.List)
      {
        discharger.DrawElectricity(spriteBatch);
      }

      foreach (LightSource light in _.activeLights)
      {
        if (light.IsBackground || light.CurrentBrightness <= 0.0f) { continue; }
        light.DrawLightVolume(spriteBatch, _.lightEffect, transform, _.recalculationCount < LightManager.MaxLightVolumeRecalculationsPerFrame, ref _.recalculationCount);
      }

      if (ConnectionPanel.ShouldDebugDrawWiring)
      {
        foreach (MapEntity e in (Submarine.VisibleEntities ?? MapEntity.MapEntityList))
        {
          if (e is Item item && !item.IsHidden && item.GetComponent<Wire>() is Wire wire)
          {
            wire.DebugDraw(spriteBatch, alpha: 0.4f);
          }
        }
      }

      _.lightEffect.World = transform;

      GameMain.ParticleManager.Draw(spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.Additive);

      if (Mod.settings.LightManager.drawHalo)
      {
        if (Character.Controlled != null)
        {
          DrawHalo(Character.Controlled);
        }
        else
        {
          foreach (Character character in Character.CharacterList)
          {
            if (character.Submarine == null || character.IsDead || !character.IsHuman) { continue; }
            DrawHalo(character);
          }
        }
      }

      void DrawHalo(Character character)
      {
        if (character == null || character.Removed) { return; }
        Vector2 haloDrawPos = character.DrawPosition;
        haloDrawPos.Y = -haloDrawPos.Y;

        //ambient light decreases the brightness of the halo (no need for a bright halo if the ambient light is bright enough)

        float ambientBrightness = (_.AmbientLight.R + _.AmbientLight.B + _.AmbientLight.G) / 255.0f / 3.0f * Mod.settings.LightManager.levelAmbientBrightness;
        Color haloColor = Color.White.Multiply(Math.Clamp(Mod.settings.LightManager.haloBrightness - ambientBrightness, 0, 1));
        if (haloColor.A > 0)
        {
          //float scale = 512.0f / LightSource.LightTexture.Width;
          float scale = Mod.settings.LightManager.haloScale;
          spriteBatch.Draw(
              LightSource.LightTexture, haloDrawPos, null, haloColor, 0.0f,
              new Vector2(LightSource.LightTexture.Width, LightSource.LightTexture.Height) / 2, scale, SpriteEffects.None, 0.0f);
        }
      }

      spriteBatch.End();

      //draw the actual light volumes, additive particles, hull ambient lights and the halo around the player
      //---------------------------------------------------------------------------------------------------

      graphics.SetRenderTarget(null);
      graphics.BlendState = BlendState.NonPremultiplied;

      return false;
    }

    public void patchLightManager()
    {
      harmony.Patch(
        original: typeof(LightManager).GetMethod("UpdateObstructVision"),
        prefix: new HarmonyMethod(typeof(Plugin).GetMethod("LightManager_UpdateObstructVision_Replace"))
      );

      harmony.Patch(
        original: typeof(LightManager).GetMethod("RenderLightMap"),
        prefix: new HarmonyMethod(typeof(Plugin).GetMethod("LightManager_RenderLightMap_Replace"))
      );
    }
  }
}