using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using BaroJunk;
using System.Text.Json;

namespace RemoveAll
{
  public static class DictionaryExtensions
  {
    public static bool Has(this Dictionary<string, bool> dict, string key)
      => dict.TryGetValue(key, out bool value) && value;

    public static bool Has(this Dictionary<int, bool> dict, int key)
      => dict.TryGetValue(key, out bool value) && value;
  }

}