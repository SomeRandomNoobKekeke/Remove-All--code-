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
  public class DirectEntryLocator
  {
    public IConfigLikeContainer Target { get; }
    public IConfiglike Host => Target.Host;
    public ConfigEntry GetEntry(string propPath)
    {
      if (Host is null || !Host.IsValid) return ConfigEntry.Empty;
      if (propPath is null) return new ConfigEntry(Host, null);

      IEnumerable<string> names = propPath.Split('.').Select(s => s.Trim());

      if (names.Count() == 0) return ConfigEntry.Empty;

      IConfiglike o = Host;

      foreach (string name in names.SkipLast(1))
      {
        if (o is null || !o.IsValid) return ConfigEntry.Empty;
        if (name == "") return ConfigEntry.Empty;
        o = o.GetPropAsConfig(name);
      }

      return new ConfigEntry(o, names.Last());
    }

    public object GetValue(string propPath) => GetEntry(propPath).Value;
    public bool SetValue(string propPath, object value) => GetEntry(propPath).SetValue(value);

    public IEnumerable<ConfigEntry> GetEntries()
    {
      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        if (!Host.IsPropASubConfig(key))
        {
          yield return new ConfigEntry(Host, key);
        }
      }
    }

    public IEnumerable<ConfigEntry> GetAllEntries()
    {
      if (Host is null) yield break;

      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        yield return new ConfigEntry(Host, key);
      }
    }

    public IEnumerable<ConfigEntry> GetSubConfigs()
    {
      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        if (Host.IsPropASubConfig(key))
        {
          yield return new ConfigEntry(Host, key);
        }
      }
    }

    public IEnumerable<ConfigEntry> GetEntriesRec()
    {
      if (Host is null) yield break;

      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        if (!Host.IsPropASubConfig(key))
        {
          yield return new ConfigEntry(Host, key);
        }
      }

      foreach (var (key, value) in props)
      {
        if (Host.IsPropASubConfig(key))
        {
          IConfiglike subConfig = Host.ToConfig(value);
          if (!subConfig.IsValid) continue;

          foreach (ConfigEntry entry in subConfig.Locator.GetEntriesRec())
          {
            yield return entry;
          }
        }
      }
    }
    public IEnumerable<ConfigEntry> GetAllEntriesRec()
    {
      if (Host is null) yield break;

      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        yield return new ConfigEntry(Host, key);
      }

      foreach (var (key, value) in props)
      {
        if (Host.IsPropASubConfig(key))
        {
          IConfiglike subConfig = Host.ToConfig(value);
          if (!subConfig.IsValid) continue;

          foreach (ConfigEntry entry in subConfig.Locator.GetAllEntriesRec())
          {
            yield return entry;
          }
        }
      }
    }
    public Dictionary<string, ConfigEntry> GetFlat()
    {
      if (Host is null) return new Dictionary<string, ConfigEntry>() { };

      Dictionary<string, ConfigEntry> flat = new();

      void scanPropsRec(IConfiglike cfg, string path = null)
      {
        Dictionary<string, object> props = cfg.AsDict;

        foreach (var (key, value) in props)
        {
          string fullPath = path is null ? key : String.Join('.', path, key);

          if (cfg.IsPropASubConfig(key))
          {
            IConfiglike subConfig = Host.ToConfig(value);
            if (!subConfig.IsValid) continue;
            scanPropsRec(subConfig, fullPath);
          }
          else
          {
            flat[fullPath] = new ConfigEntry(cfg, key);
          }
        }
      }

      scanPropsRec(Host);

      return flat;
    }
    public Dictionary<string, ConfigEntry> GetAllFlat()
    {
      if (Host is null) return new Dictionary<string, ConfigEntry>() { };

      Dictionary<string, ConfigEntry> flat = new();

      void scanPropsRec(IConfiglike cfg, string path = null)
      {
        Dictionary<string, object> props = cfg.AsDict;

        //BRUH more string concatenations pls
        foreach (var (key, value) in props)
        {
          string newPath = path is null ? key : String.Join('.', path, key);

          flat[newPath] = new ConfigEntry(cfg, key);
        }

        foreach (var (key, value) in props)
        {
          string newPath = path is null ? key : String.Join('.', path, key);

          if (cfg.IsPropASubConfig(key))
          {
            IConfiglike subConfig = Host.ToConfig(value);
            if (!subConfig.IsValid) continue;
            scanPropsRec(subConfig, newPath);
          }
        }
      }

      scanPropsRec(Host);

      return flat;
    }
    public Dictionary<string, object> GetFlatValues()
      => GetFlat().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
    public Dictionary<string, object> GetAllFlatValues()
      => GetAllFlat().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);


    public DirectEntryLocator(IConfiglike host)
    {
      Target = new IConfigLikeLocatorAdapter(host);
    }

    public DirectEntryLocator(IConfigLikeContainer target)
    {
      Target = target ?? new IConfigLikeLocatorAdapter(null);
    }

    public override string ToString() => $"DirectEntryLocator [{GetHashCode()}] Host: [{Host}]";

    public override bool Equals(object obj)
    {
      if (obj is not DirectEntryLocator other) return false;
      return Object.Equals(Target, other.Target);
    }
  }


}