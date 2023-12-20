using Barotrauma;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Voronoi2;

namespace RemoveAll
{


  partial class LevelRendererPatch
  {

    public class DrawForegroundSettings
    {
      public bool Draw { get; set; } = true;
      public DrawForegroundSettings() { }
    }
    public static DrawForegroundSettings drawForegroundSettings = new DrawForegroundSettings();

    public static bool DrawForeground(SpriteBatch spriteBatch, Camera cam, LevelObjectManager backgroundSpriteManager = null)
    {
      if (drawForegroundSettings.Draw)
      {
        spriteBatch.Begin(SpriteSortMode.Deferred,
          BlendState.NonPremultiplied,
          SamplerState.LinearClamp, DepthStencilState.DepthRead, null, null,
          cam.Transform);
        backgroundSpriteManager?.DrawObjectsFront(spriteBatch, cam);
        spriteBatch.End();
      }

      return false;
    }
  }
}