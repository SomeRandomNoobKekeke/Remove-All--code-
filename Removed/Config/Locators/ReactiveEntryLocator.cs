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
  public class ReactiveEntryLocator
  {
    public ReactiveCore Core { get; }
    public IConfigLikeContainer Target { get; }
    public IConfiglike Host => Target.Host;
    public string CurrentPath { get; }

    private string RelativePath(string path)
      => string.IsNullOrEmpty(CurrentPath) ? path : String.Join('.', CurrentPath, path);


    public ReactiveEntry ReactiveGetEntry(string propPath)
      => new ReactiveEntry(Core, Host.Locator.GetEntry(propPath), RelativePath(propPath));

    public object ReactiveGetValue(string propPath) => ReactiveGetEntry(propPath).Value;
    public bool ReactiveSetValue(string propPath, object value) => ReactiveGetEntry(propPath).SetValue(value);

    public IEnumerable<ReactiveEntry> ReactiveGetEntries()
    {
      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        if (!Host.IsPropASubConfig(key))
        {
          yield return new ReactiveEntry(Core, new ConfigEntry(Host, key), RelativePath(key));
        }
      }
    }

    public IEnumerable<ReactiveEntry> ReactiveGetAllEntries()
    {
      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        yield return new ReactiveEntry(Core, new ConfigEntry(Host, key), RelativePath(key));
      }
    }

    public IEnumerable<ReactiveEntry> ReactiveGetSubConfigs()
    {
      Dictionary<string, object> props = Host.AsDict;

      foreach (var (key, value) in props)
      {
        if (Host.IsPropASubConfig(key))
        {
          yield return new ReactiveEntry(Core, new ConfigEntry(Host, key), RelativePath(key));
        }
      }
    }

    public IEnumerable<ReactiveEntry> ReactiveGetEntriesRec()
    {
      IEnumerable<ReactiveEntry> scanPropsRec(IConfiglike cfg, string path = null)
      {
        Dictionary<string, object> props = Host.AsDict;

        foreach (var (key, value) in props)
        {
          string newPath = path is null ? key : String.Join('.', path, key);

          if (cfg.IsSubConfig(value))
          {
            IConfiglike subConfig = Host.ToConfig(value);
            if (!subConfig.IsValid) continue;
            foreach (ReactiveEntry entry in scanPropsRec(subConfig, newPath))
            {
              yield return entry;
            }
          }
          else
          {
            yield return new ReactiveEntry(Core, new ConfigEntry(cfg, newPath), RelativePath(newPath));
          }
        }
      }

      foreach (ReactiveEntry entry in scanPropsRec(Host))
      {
        yield return entry;
      }
    }

    public IEnumerable<ReactiveEntry> ReactiveGetAllEntriesRec()
    {
      IEnumerable<ReactiveEntry> scanPropsRec(IConfiglike cfg, string path = null)
      {
        Dictionary<string, object> props = Host.AsDict;

        foreach (var (key, value) in props)
        {
          string newPath = path is null ? key : String.Join('.', path, key);

          yield return new ReactiveEntry(Core, new ConfigEntry(cfg, newPath), RelativePath(newPath));

          if (cfg.IsSubConfig(value))
          {
            IConfiglike subConfig = Host.ToConfig(value);
            if (!subConfig.IsValid) continue;
            foreach (ReactiveEntry entry in scanPropsRec(subConfig, newPath))
            {
              yield return entry;
            }
          }
        }
      }

      foreach (ReactiveEntry entry in scanPropsRec(Host))
      {
        yield return entry;
      }
    }

    public Dictionary<string, ReactiveEntry> ReactiveGetFlat()
      => Host.Locator.GetFlat().ToDictionary(
        kvp => kvp.Key,
        kvp => new ReactiveEntry(Core, kvp.Value, RelativePath(kvp.Key))
      );

    public Dictionary<string, ReactiveEntry> ReactiveGetAllFlat()
      => Host.Locator.GetAllFlat().ToDictionary(
        kvp => kvp.Key,
        kvp => new ReactiveEntry(Core, kvp.Value, RelativePath(kvp.Key))
      );

    public Dictionary<string, object> ReactiveGetFlatValues() => Host.Locator.GetFlatValues();
    public Dictionary<string, object> ReactiveGetAllFlatValues() => Host.Locator.GetFlatValues();

    public ReactiveEntryLocator(ReactiveCore core, IConfigLikeContainer target, string path)
    {
      Core = core;
      Target = target;
      CurrentPath = path;
    }

    public override string ToString() => $"ReactiveEntryLocator [{GetHashCode()}] Core: [{Core}] Host: [{Host}] CurrentPath: [{CurrentPath}]";
  }


}