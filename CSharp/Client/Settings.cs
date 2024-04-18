using System;
using System.Reflection;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public class patchingSettings
    {
      public bool BackgroundCreatureManager { get; set; } = true;
      public bool GameScreen { get; set; } = true;
      public bool GUI { get; set; } = true;
      public bool Level { get; set; } = true;
      public bool LevelObjectManager { get; set; } = true;
      public bool LevelRenderer { get; set; } = true;
      public bool LightManager { get; set; } = true;
      public bool LightSource { get; set; } = true;
      public bool Submarine { get; set; } = true;
      public bool WaterRenderer { get; set; } = true;
    }

    public class Settings
    {
      // [JsonPropertyName("Level Renderer Settings")]
      public LevelRendererSettings LevelRenderer { get; set; } = new LevelRendererSettings();
      public LevelObjectManagerSettings LevelObjectManager { get; set; } = new LevelObjectManagerSettings();
      public LightManagerSettings LightManager { get; set; } = new LightManagerSettings();

      public SubmarineSettings Submarine { get; set; } = new SubmarineSettings();
      public int maxBackgroundCreaturesCount { get; set; } = 0;



      public patchingSettings patch { get; set; } = new patchingSettings();
    }
  }
}