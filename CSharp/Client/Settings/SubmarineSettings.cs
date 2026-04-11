using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class SubmarineSettings : IConfig
  {
    public float CullInterval { get; set; } = 0.25f;
    public float CullMarginX { get; set; } = 500f;
    public float CullMarginY { get; set; } = 500f;
    public float CullMoveThreshold { get; set; } = 50f;
  }

}