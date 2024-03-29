using System;
using System.Reflection;
using System.Collections.Generic;

using System.Text.Json;
using System.Text.Json.Serialization;

using HarmonyLib;
using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace RemoveAll
{
  public class TemplateSettings
  {

  }

  partial class RemoveAllMod
  {

    public void patchTemplate()
    {
      // harmony.Patch(
      //   original: typeof(LevelRenderer).GetMethod("Update"),
      //   prefix: new HarmonyMethod(typeof(RemoveAllMod).GetMethod("LevelRenderer_Update_Prefix"))
      // );
    }
  }
}