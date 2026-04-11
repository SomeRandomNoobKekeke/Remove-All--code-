using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class LevelObjectManagerSettings : IConfig
  {
    public int MaxVisibleLevelObjects { get; set; } = 600;
    public float CutOffdepth { get; set; } = 1000000;
    public bool RemoveDepth { get; set; } = false;
  }

}