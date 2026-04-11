using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class LightManagerSettings : IConfig
  {
    public bool DrawHalo { get; set; } = true;
    public bool GhostCharacters { get; set; } = false;
    public bool HightlightItems { get; set; } = true;
    public bool DrawGapGlow { get; set; } = true;
    public float HaloScale { get; set; } = 0.5f;
    public float HaloBrightness { get; set; } = 0.3f;
    public float HullAmbientBrightness { get; set; } = 1.0f;
    public Color HullAmbientColor { get; set; } = Color.Transparent;
    public bool GlobalLightBrightness { get; set; } = 1.0f;
    public bool LevelAmbientBrightness { get; set; } = 1.0f;
  }

}