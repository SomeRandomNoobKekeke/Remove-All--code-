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

    public class Settings
    {
      [JsonPropertyName("Level Renderer Settings")]
      public LevelRendererSettings LevelRenderer { get; set; } = new LevelRendererSettings();

      [JsonPropertyName("Level Object Manager Settings")]
      public LevelObjectManagerSettings LevelObjectManager { get; set; } = new LevelObjectManagerSettings();

      [JsonPropertyName("Light Manager Settings")]
      public LightManagerSettings LightManager { get; set; } = new LightManagerSettings();


      [JsonPropertyName("Max Background Creatures Count")]
      public int maxBackgroundCreaturesCount { get; set; } = 0;
    }

    public static Settings settings;

  }
}