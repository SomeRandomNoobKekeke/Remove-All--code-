using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class LevelObjectManagerSettings : IConfig
  {
    public int MaxVisibleLevelObjects { get; set; } = 500;
    public float CutOffdepth { get; set; } = 10;
    public bool RemoveDepth { get; set; } = false;
  }

}