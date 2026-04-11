using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class HidingSettings : IConfig
  {
    public bool Entities { get; set; } = false;
    public bool ItemLights { get; set; } = false;
    public bool LevelObjects { get; set; } = false;
    public bool Particles { get; set; } = false;
    public bool Decals { get; set; } = false;
  }

}