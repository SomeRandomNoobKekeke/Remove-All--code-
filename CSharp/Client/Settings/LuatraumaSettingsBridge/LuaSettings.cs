using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

using BaroJunk;

namespace RemoveAll
{
  //Luatrauma settings are borked at this moment
  public class LuaSettings
  {
    public void SyncForward()
    {
      foreach (var (path, entry) in Mod.Settings.GetFlat())
      {
        Mod.Logger.Log(path);
      }
    }
  }

}