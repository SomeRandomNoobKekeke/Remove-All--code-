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
      public LevelRendererSettings LR { get; set; } = new LevelRendererSettings();
    }

    public static Settings settings;

  }
}