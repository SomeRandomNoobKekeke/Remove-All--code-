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
  public class WaterRendererSettings
  {

  }

  partial class RemoveAllMod
  {
    public static bool WaterRenderer_RenderWater_Prefix(SpriteBatch spriteBatch, RenderTarget2D texture, Camera cam, WaterRenderer __instance)
    {
      // return false;

      WaterRenderer _ = __instance;

      spriteBatch.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

      _.WaterEffect.Parameters["xTexture"].SetValue(texture);
      Vector2 distortionStrength = cam == null ? WaterRenderer.DistortionStrength : WaterRenderer.DistortionStrength * cam.Zoom;
      _.WaterEffect.Parameters["xWaveWidth"].SetValue(distortionStrength.X);
      _.WaterEffect.Parameters["xWaveHeight"].SetValue(distortionStrength.Y);
      if (WaterRenderer.BlurAmount > 0.0f)
      {
        _.WaterEffect.CurrentTechnique = _.WaterEffect.Techniques["WaterShaderBlurred"];
        _.WaterEffect.Parameters["xBlurDistance"].SetValue(WaterRenderer.BlurAmount / 100.0f);
      }
      else
      {
        _.WaterEffect.CurrentTechnique = _.WaterEffect.Techniques["WaterShader"];
      }

      Vector2 offset = _.WavePos;
      if (cam != null)
      {
        offset += (cam.Position - new Vector2(cam.WorldView.Width / 2.0f, -cam.WorldView.Height / 2.0f));
        offset.Y += cam.WorldView.Height;
        offset.X += cam.WorldView.Width;
#if LINUX || OSX
                offset.X += cam.WorldView.Width;
#endif
        offset *= WaterRenderer.DistortionScale;
      }
      offset.Y = -offset.Y;
      _.WaterEffect.Parameters["xUvOffset"].SetValue(new Vector2((offset.X / GameMain.GraphicsWidth) % 1.0f, (offset.Y / GameMain.GraphicsHeight) % 1.0f));
      _.WaterEffect.Parameters["xBumpPos"].SetValue(Vector2.Zero);

      if (cam != null)
      {
        _.WaterEffect.Parameters["xBumpScale"].SetValue(new Vector2(
                (float)cam.WorldView.Width / GameMain.GraphicsWidth * WaterRenderer.DistortionScale.X,
                (float)cam.WorldView.Height / GameMain.GraphicsHeight * WaterRenderer.DistortionScale.Y));
        _.WaterEffect.Parameters["xTransform"].SetValue(cam.ShaderTransform
            * Matrix.CreateOrthographic(GameMain.GraphicsWidth, GameMain.GraphicsHeight, -1, 1) * 0.5f);
        _.WaterEffect.Parameters["xUvTransform"].SetValue(cam.ShaderTransform
            * Matrix.CreateOrthographicOffCenter(0, spriteBatch.GraphicsDevice.Viewport.Width * 2, spriteBatch.GraphicsDevice.Viewport.Height * 2, 0, 0, 1) * Matrix.CreateTranslation(0.5f, 0.5f, 0.0f));
      }
      else
      {
        _.WaterEffect.Parameters["xBumpScale"].SetValue(new Vector2(1.0f, 1.0f));
        _.WaterEffect.Parameters["xTransform"].SetValue(Matrix.Identity * Matrix.CreateTranslation(-1.0f, 1.0f, 0.0f));
        _.WaterEffect.Parameters["xUvTransform"].SetValue(Matrix.CreateScale(0.5f, -0.5f, 0.0f));
      }

      _.WaterEffect.CurrentTechnique.Passes[0].Apply();

      Rectangle view = cam != null ? cam.WorldView : spriteBatch.GraphicsDevice.Viewport.Bounds;

      _.tempCorners[0] = new Vector3(view.X, view.Y, 0.1f);
      _.tempCorners[1] = new Vector3(view.Right, view.Y, 0.1f);
      _.tempCorners[2] = new Vector3(view.Right, view.Y - view.Height, 0.1f);
      _.tempCorners[3] = new Vector3(view.X, view.Y - view.Height, 0.1f);

      WaterVertexData backGroundColor = new WaterVertexData(0.1f, 0.1f, 0.5f, 1.0f);
      _.tempVertices[0] = new VertexPositionColorTexture(_.tempCorners[0], backGroundColor, Vector2.Zero);
      _.tempVertices[1] = new VertexPositionColorTexture(_.tempCorners[1], backGroundColor, Vector2.Zero);
      _.tempVertices[2] = new VertexPositionColorTexture(_.tempCorners[2], backGroundColor, Vector2.Zero);
      _.tempVertices[3] = new VertexPositionColorTexture(_.tempCorners[0], backGroundColor, Vector2.Zero);
      _.tempVertices[4] = new VertexPositionColorTexture(_.tempCorners[2], backGroundColor, Vector2.Zero);
      _.tempVertices[5] = new VertexPositionColorTexture(_.tempCorners[3], backGroundColor, Vector2.Zero);

      spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _.tempVertices, 0, 2);

      foreach (KeyValuePair<EntityGrid, VertexPositionColorTexture[]> subVerts in _.IndoorsVertices)
      {
        if (!_.PositionInIndoorsBuffer.ContainsKey(subVerts.Key) || _.PositionInIndoorsBuffer[subVerts.Key] == 0) { continue; }

        offset = _.WavePos;
        if (subVerts.Key.Submarine != null) { offset -= subVerts.Key.Submarine.WorldPosition; }
        if (cam != null)
        {
          offset += cam.Position - new Vector2(cam.WorldView.Width / 2.0f, -cam.WorldView.Height / 2.0f);
          offset.Y += cam.WorldView.Height;
          offset.X += cam.WorldView.Width;
          offset *= WaterRenderer.DistortionScale;
        }
        offset.Y = -offset.Y;
        _.WaterEffect.Parameters["xUvOffset"].SetValue(new Vector2((offset.X / GameMain.GraphicsWidth) % 1.0f, (offset.Y / GameMain.GraphicsHeight) % 1.0f));

        _.WaterEffect.CurrentTechnique.Passes[0].Apply();

        spriteBatch.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, subVerts.Value, 0, _.PositionInIndoorsBuffer[subVerts.Key] / 3);
      }

      _.WaterEffect.Parameters["xTexture"].SetValue((Texture2D)null);
      _.WaterEffect.CurrentTechnique.Passes[0].Apply();

      return false;
    }

    public void patchWaterRenderer()
    {

      harmony.Patch(
        original: typeof(WaterRenderer).GetMethod("RenderWater"),
        prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("WaterRenderer_RenderWater_Prefix"))
      );
    }
  }
}