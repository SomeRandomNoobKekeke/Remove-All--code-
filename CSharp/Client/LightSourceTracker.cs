using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using BaroJunk;

using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Items.Components;

namespace RemoveAll
{

  public partial class LightSourceTracker
  {
    public Dictionary<LightSource, LightComponent> ReverseLookup = new();

    public void FindLightSources()
    {
      ReverseLookup.Clear();

      foreach (Item item in Item.ItemList)
      {
        foreach (LightComponent lc in item.GetComponents<LightComponent>())
        {
          ReverseLookup[lc.Light] = lc;
        }
      }
    }
  }
}