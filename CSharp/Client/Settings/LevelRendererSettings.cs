using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class LevelRendererSettings : IConfig
  {
    public WaterParticleSettings WaterParticles { get; set; }
    public bool DrawWaterParticles { get; set; } = true;
  }

}