using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;

using Barotrauma;

namespace BaroJunk
{
  /// <summary>
  /// It's just an object where you can listen for reactive events
  /// </summary>
  public class ReactiveCore
  {
    public ConfigCore Core { get; }
    public IConfiglike Host => Core.Host;
    public ReactiveEntryLocator Locator { get; }


    public bool DeeplyReactive { get; set; } = false;

    public event Action<string, object> PropChanged;
    public event Action Updated;

    public Action<string, object> OnPropChanged { set { PropChanged += value; } }
    public Action OnUpdated { set { Updated += value; } }

    //TODO test garbage input, i'm sure it's very fragile
    public void RaisePropChanged(string propPath, object value)
    {
      PropChanged?.Invoke(propPath, value);

      if (DeeplyReactive)
      {
        IConfiglike next = Core.Host;

        while (propPath.IndexOf('.') != -1)
        {
          string configPath = propPath.Substring(0, propPath.IndexOf('.')).Trim();
          if (configPath == "") break;

          next = next.Core.Host.GetPropAsConfig(configPath);
          propPath = propPath.Substring(propPath.IndexOf('.') + 1);
          next.Core?.ReactiveCore.RaisePropChanged(propPath, value);
        }
      }
    }
    public void RaiseUpdated()
    {
      Updated?.Invoke();

      if (DeeplyReactive)
      {
        foreach (ConfigEntry entry in Core.GetAllEntriesRec())
        {
          if (entry.IsConfig)
          {
            entry.Host.GetPropAsConfig(entry.Key).Core?.ReactiveCore.RaiseUpdated();
          }
        }
      }
    }

    public ReactiveCore(ConfigCore core)
    {
      Core = core;
      Locator = new ReactiveEntryLocator(this, new IConfigLikeLocatorAdapter(Host), null);
    }

    public override string ToString() => $"ReactiveCore [{GetHashCode()}]";
  }

}