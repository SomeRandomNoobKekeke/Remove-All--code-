using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class BlackList
  {
    public Dictionary<int, bool> LevelObjects { get; set; } = new();
    public Dictionary<int, bool> MapEntity { get; set; } = new();
    public Dictionary<int, bool> Particles { get; set; } = new();
    public Dictionary<int, bool> Decals { get; set; } = new();
  }

}