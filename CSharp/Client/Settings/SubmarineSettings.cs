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
    public int CullMarginX { get; set; } = 500;
    public int CullMarginY { get; set; } = 500;
    public float CullMoveThreshold { get; set; } = 50f;
  }

}