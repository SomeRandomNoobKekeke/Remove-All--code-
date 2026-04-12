using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class WaterParticleSettings : IConfig
  {
    public int ColdCaverns { get; set; } = 1;
    public int Europanridge { get; set; } = 0;
    public int Theaphoticplateau { get; set; } = 0;
    public int Thegreatsea { get; set; } = 0;
    public int Hydrothermalwastes { get; set; } = 0;
    public int Endzone { get; set; } = 4;
    public int Outpost { get; set; } = 0;

    public int Get(string key) => Map[key].Invoke(this);
    private Dictionary<string, Func<WaterParticleSettings, int>> Map = new()
    {
      ["coldcaverns"] = (self) => self.ColdCaverns,
      ["europanridge"] = (self) => self.Europanridge,
      ["theaphoticplateau"] = (self) => self.Theaphoticplateau,
      ["thegreatsea"] = (self) => self.Thegreatsea,
      ["hydrothermalwastes"] = (self) => self.Hydrothermalwastes,
      ["endzone"] = (self) => self.Endzone,
      ["outpost"] = (self) => self.Outpost,
    };
  }

}