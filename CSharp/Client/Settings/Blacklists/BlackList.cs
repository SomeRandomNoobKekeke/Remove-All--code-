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
  }

}