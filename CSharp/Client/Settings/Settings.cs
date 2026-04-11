using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using BaroJunk;

namespace RemoveAll
{
  public class Settings : IConfig
  {
    public LevelRendererSettings LevelRenderer { get; set; }
    public LevelObjectManagerSettings LevelObjectManager { get; set; }
    public LightManagerSettings LightManager { get; set; }
    public SubmarineSettings Submarine { get; set; }
    public HidingSettings Hide { get; set; }


    public int MaxBackgroundCreaturesCount { get; set; } = 0;
    public int MaxParticles { get; set; } = 100000;

    public Settings()
    {
      this.Restore();
    }
  }

}