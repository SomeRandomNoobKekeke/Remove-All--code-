using Barotrauma.Extensions;
using Barotrauma.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Linq;
using Barotrauma;

// using System.Runtime.CompilerServices;
// [assembly: IgnoresAccessChecksTo("Barotrauma")]

namespace RemoveAll
{
  partial class GameScreenPatch
  {

    public static RenderTarget2D BackCharactersItemsBuffer;
    public static double BackCharactersItemsTiming;

    public static Matrix lastTransform;

    public static float lastZoom;
    public static Vector2 lastPosition;

    static GameScreenPatch()
    {
      BackCharactersItemsBuffer = new RenderTarget2D(GameMain.Instance.GraphicsDevice, GameMain.GraphicsWidth, GameMain.GraphicsHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

      lastTransform = Matrix.Identity;
    }

    public static bool DrawMap(GraphicsDevice graphics, SpriteBatch spriteBatch, double deltaTime,
    GameScreen __instance, RenderTarget2D ___renderTargetBackground, RenderTarget2D ___renderTarget, RenderTarget2D ___renderTargetWater, RenderTarget2D ___renderTargetFinal, Camera ___cam, float ___fadeToBlackState)
    {

      foreach (Submarine sub in Submarine.Loaded)
      {
        sub.UpdateTransform();
      }

      GameMain.ParticleManager.UpdateTransforms();

      Stopwatch sw = new Stopwatch();
      sw.Start();

      GameMain.LightManager.ObstructVision =
          Character.Controlled != null &&
          Character.Controlled.ObstructVision &&
          (Character.Controlled.ViewTarget == Character.Controlled || Character.Controlled.ViewTarget == null);

      GameMain.LightManager.UpdateObstructVision(graphics, spriteBatch, ___cam, Character.Controlled?.CursorWorldPosition ?? Vector2.Zero);

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:LOS", sw.ElapsedTicks);
      sw.Restart();


      static bool IsFromOutpostDrawnBehindSubs(Entity e)
          => e.Submarine is { Info.OutpostGenerationParams.DrawBehindSubs: true };

      //------------------------------------------------------------------------
      graphics.SetRenderTarget(___renderTarget);
      graphics.Clear(Color.Transparent);
      //Draw background structures and wall background sprites 
      //(= the background texture that's revealed when a wall is destroyed) into the background render target
      //These will be visible through the LOS effect.
      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
      Submarine.DrawBack(spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && !IsFromOutpostDrawnBehindSubs(e));
      Submarine.DrawPaintedColors(spriteBatch, false);
      spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackStructures", sw.ElapsedTicks);
      sw.Restart();

      graphics.SetRenderTarget(null);
      GameMain.LightManager.RenderLightMap(graphics, spriteBatch, ___cam, ___renderTarget);

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Lighting", sw.ElapsedTicks);
      sw.Restart();

      //------------------------------------------------------------------------
      graphics.SetRenderTarget(___renderTargetBackground);
      if (Level.Loaded == null)
      {
        graphics.Clear(new Color(11, 18, 26, 255));
      }
      else
      {
        //graphics.Clear(new Color(255, 255, 255, 255));
        Level.Loaded.DrawBack(graphics, spriteBatch, ___cam);
      }

      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
      Submarine.DrawBack(spriteBatch, false, e => e is Structure s && (e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null) && IsFromOutpostDrawnBehindSubs(e));
      spriteBatch.End();

      //draw alpha blended particles that are in water and behind subs
#if LINUX || OSX
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
#else
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
#endif
      GameMain.ParticleManager.Draw(spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
      spriteBatch.End();

      //draw additive particles that are in water and behind subs
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, ___cam.Transform);
      GameMain.ParticleManager.Draw(spriteBatch, true, false, Barotrauma.Particles.ParticleBlendState.Additive);
      spriteBatch.End();
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None);
      spriteBatch.Draw(___renderTarget, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
      spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackLevel", sw.ElapsedTicks);
      sw.Restart();

      //----------------------------------------------------------------------------

      //Start drawing to the normal render target (stuff that can't be seen through the LOS effect)

      graphics.SetRenderTarget(___renderTarget);
      graphics.BlendState = BlendState.NonPremultiplied;
      graphics.SamplerStates[0] = SamplerState.LinearWrap;
      Quad.UseBasicEffect(___renderTargetBackground);
      Quad.Render();

      //Draw the rest of the structures, characters and front structures

      graphics.SetRenderTarget(BackCharactersItemsBuffer);


      if (Timing.TotalTime - BackCharactersItemsTiming >= 1.0 / 60.0)
      {
        lastTransform = ___cam.Transform;
        lastZoom = ___cam.zoom;
        lastPosition = ___cam.position;


        graphics.Clear(Color.Transparent);
        spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
        Submarine.DrawBack(spriteBatch, false, e => !(e is Structure) || e.SpriteDepth < 0.9f);
        DrawCharacters(deformed: false, firstPass: true);
        spriteBatch.End();

        BackCharactersItemsTiming = Timing.TotalTime;
      }


      Vector2 interpolatedPosition = 2 * ___cam.position - lastPosition;
      float interpolatedZoom = 2 * ___cam.zoom - lastZoom;

      Matrix fakeMatrix = Matrix.CreateTranslation(
          new Vector3(-interpolatedPosition.X, interpolatedPosition.Y, 0)) *
          Matrix.CreateScale(new Vector3(interpolatedZoom, interpolatedZoom, 1)) *
          Matrix.CreateRotationZ(___cam.rotation) * ___cam.viewMatrix;

      graphics.SetRenderTarget(___renderTarget);


      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, fakeMatrix);
      spriteBatch.Draw(BackCharactersItemsBuffer, new Rectangle(___cam.WorldView.X, -___cam.WorldView.Y, ___cam.WorldView.Width, ___cam.WorldView.Height), Color.White);
      spriteBatch.End();

      // spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, lastTransform);
      // spriteBatch.Draw(BackCharactersItemsBuffer, new Rectangle(___cam.WorldView.X, -___cam.WorldView.Y, ___cam.WorldView.Width, ___cam.WorldView.Height), Color.Red);
      // spriteBatch.End();

      // spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, fakeMatrix);
      // spriteBatch.Draw(BackCharactersItemsBuffer, new Rectangle(___cam.WorldView.X, -___cam.WorldView.Y, ___cam.WorldView.Width, ___cam.WorldView.Height), Color.Blue);
      // spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackCharactersItems", sw.ElapsedTicks);
      sw.Restart();

      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
      DrawCharacters(deformed: true, firstPass: true);
      DrawCharacters(deformed: true, firstPass: false);
      DrawCharacters(deformed: false, firstPass: false);
      spriteBatch.End();

      void DrawCharacters(bool deformed, bool firstPass)
      {
        //backwards order to render the most recently spawned characters in front (characters spawned later have a larger sprite depth)
        for (int i = Character.CharacterList.Count - 1; i >= 0; i--)
        {
          Character c = Character.CharacterList[i];
          if (!c.IsVisible) { continue; }
          if (c.Params.DrawLast == firstPass) { continue; }
          if (deformed)
          {
            if (c.AnimController.Limbs.All(l => l.DeformSprite == null)) { continue; }
          }
          else
          {
            if (c.AnimController.Limbs.Any(l => l.DeformSprite != null)) { continue; }
          }
          c.Draw(spriteBatch, ___cam);
        }
      }

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:DeformableCharacters", sw.ElapsedTicks);
      sw.Restart();

      Level.Loaded?.DrawFront(spriteBatch, ___cam);

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontLevel", sw.ElapsedTicks);
      sw.Restart();

      //draw the rendertarget and particles that are only supposed to be drawn in water into renderTargetWater
      graphics.SetRenderTarget(___renderTargetWater);

      graphics.BlendState = BlendState.Opaque;
      graphics.SamplerStates[0] = SamplerState.LinearWrap;
      Quad.UseBasicEffect(___renderTarget);
      Quad.Render();

      //draw alpha blended particles that are inside a sub
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, ___cam.Transform);
      GameMain.ParticleManager.Draw(spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
      spriteBatch.End();

      graphics.SetRenderTarget(___renderTarget);

      //draw alpha blended particles that are not in water
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, DepthStencilState.DepthRead, null, null, ___cam.Transform);
      GameMain.ParticleManager.Draw(spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.AlphaBlend);
      spriteBatch.End();

      //draw additive particles that are not in water
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.None, null, null, ___cam.Transform);
      GameMain.ParticleManager.Draw(spriteBatch, false, null, Barotrauma.Particles.ParticleBlendState.Additive);
      spriteBatch.End();

      graphics.DepthStencilState = DepthStencilState.DepthRead;
      graphics.SetRenderTarget(___renderTargetFinal);

      WaterRenderer.Instance.ResetBuffers();
      Hull.UpdateVertices(___cam, WaterRenderer.Instance);
      WaterRenderer.Instance.RenderWater(spriteBatch, ___renderTargetWater, ___cam);
      WaterRenderer.Instance.RenderAir(graphics, ___cam, ___renderTarget, ___cam.ShaderTransform);
      graphics.DepthStencilState = DepthStencilState.None;

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontParticles", sw.ElapsedTicks);
      sw.Restart();

      __instance.DamageEffect.CurrentTechnique = __instance.DamageEffect.Techniques["StencilShader"];
      spriteBatch.Begin(SpriteSortMode.Immediate,
          BlendState.NonPremultiplied, SamplerState.LinearWrap,
          null, null,
          __instance.DamageEffect,
          ___cam.Transform);
      Submarine.DrawDamageable(spriteBatch, __instance.DamageEffect, false);
      spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontDamageable", sw.ElapsedTicks);
      sw.Restart();

      spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, DepthStencilState.None, null, null, ___cam.Transform);
      Submarine.DrawFront(spriteBatch, false, null);
      spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontStructuresItems", sw.ElapsedTicks);
      sw.Restart();

      //draw additive particles that are inside a sub
      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, DepthStencilState.Default, null, null, ___cam.Transform);
      GameMain.ParticleManager.Draw(spriteBatch, true, true, Barotrauma.Particles.ParticleBlendState.Additive);
      foreach (var discharger in Barotrauma.Items.Components.ElectricalDischarger.List)
      {
        discharger.DrawElectricity(spriteBatch);
      }
      spriteBatch.End();
      if (GameMain.LightManager.LightingEnabled)
      {
        graphics.DepthStencilState = DepthStencilState.None;
        graphics.SamplerStates[0] = SamplerState.LinearWrap;
        graphics.BlendState = CustomBlendStates.Multiplicative;
        Quad.UseBasicEffect(GameMain.LightManager.LightMap);
        Quad.Render();
      }

      spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, null, null, ___cam.Transform);
      foreach (Character c in Character.CharacterList)
      {
        c.DrawFront(spriteBatch, ___cam);
      }

      Level.Loaded?.DrawDebugOverlay(spriteBatch, ___cam);
      if (GameMain.DebugDraw)
      {
        MapEntity.MapEntityList.ForEach(me => me.AiTarget?.Draw(spriteBatch));
        Character.CharacterList.ForEach(c => c.AiTarget?.Draw(spriteBatch));
        if (GameMain.GameSession?.EventManager != null)
        {
          GameMain.GameSession.EventManager.DebugDraw(spriteBatch);
        }
      }
      spriteBatch.End();

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:FrontMisc", sw.ElapsedTicks);
      sw.Restart();

      if (GameMain.LightManager.LosEnabled && GameMain.LightManager.LosMode != LosMode.None && Barotrauma.Lights.LightManager.ViewTarget != null)
      {
        GameMain.LightManager.LosEffect.CurrentTechnique = GameMain.LightManager.LosEffect.Techniques["LosShader"];

        GameMain.LightManager.LosEffect.Parameters["blurDistance"].SetValue(0.005f);
        GameMain.LightManager.LosEffect.Parameters["xTexture"].SetValue(___renderTargetBackground);
        GameMain.LightManager.LosEffect.Parameters["xLosTexture"].SetValue(GameMain.LightManager.LosTexture);
        GameMain.LightManager.LosEffect.Parameters["xLosAlpha"].SetValue(GameMain.LightManager.LosAlpha);

        Color losColor;
        if (GameMain.LightManager.LosMode == LosMode.Transparent)
        {
          //convert the los color to HLS and make sure the luminance of the color is always the same
          //as the luminance of the ambient light color
          float r = Character.Controlled?.CharacterHealth == null ?
              0.0f : Math.Min(Character.Controlled.CharacterHealth.DamageOverlayTimer * 0.5f, 0.5f);
          Vector3 ambientLightHls = GameMain.LightManager.AmbientLight.RgbToHLS();
          Vector3 losColorHls = Color.Lerp(GameMain.LightManager.AmbientLight, Color.Red, r).RgbToHLS();
          losColorHls.Y = ambientLightHls.Y;
          losColor = ToolBox.HLSToRGB(losColorHls);
        }
        else
        {
          losColor = Color.Black;
        }

        GameMain.LightManager.LosEffect.Parameters["xColor"].SetValue(losColor.ToVector4());

        graphics.BlendState = BlendState.NonPremultiplied;
        graphics.SamplerStates[0] = SamplerState.PointClamp;
        graphics.SamplerStates[1] = SamplerState.PointClamp;
        GameMain.LightManager.LosEffect.CurrentTechnique.Passes[0].Apply();
        Quad.Render();
        graphics.SamplerStates[0] = SamplerState.LinearWrap;
        graphics.SamplerStates[1] = SamplerState.LinearWrap;
      }

      if (Character.Controlled is { } character)
      {
        float grainStrength = character.GrainStrength;
        Rectangle screenRect = new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, effect: __instance.GrainEffect);
        GUI.DrawRectangle(spriteBatch, screenRect, Color.White, isFilled: true);
        __instance.GrainEffect.Parameters["seed"].SetValue(Rand.Range(0f, 1f, Rand.RandSync.Unsynced));
        __instance.GrainEffect.Parameters["intensity"].SetValue(grainStrength);
        __instance.GrainEffect.Parameters["grainColor"].SetValue(character.GrainColor.ToVector4());
        spriteBatch.End();
      }

      graphics.SetRenderTarget(null);

      float BlurStrength = 0.0f;
      float DistortStrength = 0.0f;
      Vector3 chromaticAberrationStrength = GameSettings.CurrentConfig.Graphics.ChromaticAberration ?
          new Vector3(-0.02f, -0.01f, 0.0f) : Vector3.Zero;

      if (Level.Loaded?.Renderer != null)
      {
        chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0.0f) * Level.Loaded.Renderer.ChromaticAberrationStrength;
      }

      if (Character.Controlled != null)
      {
        BlurStrength = Character.Controlled.BlurStrength * 0.005f;
        DistortStrength = Character.Controlled.DistortStrength;
        if (GameSettings.CurrentConfig.Graphics.RadialDistortion)
        {
          chromaticAberrationStrength -= Vector3.One * Character.Controlled.RadialDistortStrength;
        }
        chromaticAberrationStrength += new Vector3(-0.03f, -0.015f, 0.0f) * Character.Controlled.ChromaticAberrationStrength;
      }
      else
      {
        BlurStrength = 0.0f;
        DistortStrength = 0.0f;
      }

      string postProcessTechnique = "";
      if (BlurStrength > 0.0f)
      {
        postProcessTechnique += "Blur";
        __instance.PostProcessEffect.Parameters["blurDistance"].SetValue(BlurStrength);
      }
      if (chromaticAberrationStrength != Vector3.Zero)
      {
        postProcessTechnique += "ChromaticAberration";
        __instance.PostProcessEffect.Parameters["chromaticAberrationStrength"].SetValue(chromaticAberrationStrength);
      }
      if (DistortStrength > 0.0f)
      {
        postProcessTechnique += "Distort";
        __instance.PostProcessEffect.Parameters["distortScale"].SetValue(Vector2.One * DistortStrength);
        __instance.PostProcessEffect.Parameters["distortUvOffset"].SetValue(WaterRenderer.Instance.WavePos * 0.001f);
      }

      graphics.BlendState = BlendState.Opaque;
      graphics.SamplerStates[0] = SamplerState.LinearClamp;
      graphics.DepthStencilState = DepthStencilState.None;
      if (string.IsNullOrEmpty(postProcessTechnique))
      {
        Quad.UseBasicEffect(___renderTargetFinal);
      }
      else
      {
        __instance.PostProcessEffect.Parameters["MatrixTransform"].SetValue(Matrix.Identity);
        __instance.PostProcessEffect.Parameters["xTexture"].SetValue(___renderTargetFinal);
        __instance.PostProcessEffect.CurrentTechnique = __instance.PostProcessEffect.Techniques[postProcessTechnique];
        __instance.PostProcessEffect.CurrentTechnique.Passes[0].Apply();
      }
      Quad.Render();

      if (___fadeToBlackState > 0.0f)
      {
        spriteBatch.Begin(SpriteSortMode.Deferred);
        GUI.DrawRectangle(spriteBatch, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.Lerp(Color.TransparentBlack, Color.Black, ___fadeToBlackState), isFilled: true);
        spriteBatch.End();
      }

      if (GameMain.LightManager.DebugLos)
      {
        GameMain.LightManager.DebugDrawLos(spriteBatch, ___cam);
      }

      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:PostProcess", sw.ElapsedTicks);
      sw.Restart();

      return false;
    }

  }
}