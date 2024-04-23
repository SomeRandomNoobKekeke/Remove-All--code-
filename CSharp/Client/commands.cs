using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Barotrauma.Extensions;
using System.Linq;
using System.Xml.Linq;


namespace RemoveAll
{
  partial class RemoveAllMod
  {
    public static void addCommands()
    {
      DebugConsole.Commands.Add(new DebugConsole.Command("r_lighting|r_lights", "= light", (string[] args) =>
      {
        if (args.None() || !bool.TryParse(args[0], out bool state))
        {
          state = !GameMain.LightManager.LightingEnabled;
        }
        GameMain.LightManager.LightingEnabled = state;
        log("Lighting " + (GameMain.LightManager.LightingEnabled ? "enabled" : "disabled"));
      }));
    }

    public static void removeCommands()
    {
      DebugConsole.Commands.RemoveAll(c => c.Names.Contains("r_lighting"));
    }
  }
}