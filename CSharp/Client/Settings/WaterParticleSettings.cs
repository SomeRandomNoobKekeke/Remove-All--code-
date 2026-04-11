using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class WaterParticleSettings : IConfig
  {
    public int ColdCaverns { get; set; } = 4;
    public int Europanridge { get; set; } = 4;
    public int Theaphoticplateau { get; set; } = 4;
    public int Thegreatsea { get; set; } = 4;
    public int Hydrothermalwastes { get; set; } = 4;
    public int Endzone { get; set; } = 4;
    public int Outpost { get; set; } = 4;
  }

}