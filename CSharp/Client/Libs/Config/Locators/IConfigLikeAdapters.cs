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

  public interface IConfigLikeContainer
  {
    public IConfiglike Host { get; }
  }

  /// <summary>
  /// Cringe, but i'm rofling at this point
  /// The other options are:
  /// Create separate duplicate property just for locator
  /// Pass a getter
  /// </summary>
  public class ConfigEntryLocatorAdapter : IConfigLikeContainer
  {
    public ConfigEntry Entry { get; }
    public IConfiglike Host => Entry.Host.GetPropAsConfig(Entry.Key);
    public ConfigEntryLocatorAdapter(ConfigEntry entry) => Entry = entry;

    public override bool Equals(object obj)
    {
      if (obj is not IConfigLikeContainer other) return false;
      return Object.Equals(Host, other.Host);
    }
  }

  public class IConfigLikeLocatorAdapter : IConfigLikeContainer
  {
    public IConfiglike Host { get; }
    public IConfigLikeLocatorAdapter(IConfiglike host) => Host = host;

    public override bool Equals(object obj)
    {
      if (obj is not IConfigLikeContainer other) return false;
      return Object.Equals(Host, other.Host);
    }
  }

}