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
  /// This object is exposing DirectEntryLocator and is mapping location calls to it
  /// </summary>
  public interface IDirectlyLocatable
  {
    public DirectEntryLocator Locator { get; }


    public IConfiglike Host => Locator.Host;
    public ConfigEntry GetEntry(string propPath) => Locator.GetEntry(propPath);
    public object GetValue(string propPath) => Locator.GetValue(propPath);
    public bool SetValue(string propPath, object value) => Locator.SetValue(propPath, value);
    public IEnumerable<ConfigEntry> GetEntries() => Locator.GetEntries();
    public IEnumerable<ConfigEntry> GetAllEntries() => Locator.GetAllEntries();
    public IEnumerable<ConfigEntry> GetSubConfigs() => Locator.GetSubConfigs();
    public IEnumerable<ConfigEntry> GetEntriesRec() => Locator.GetEntriesRec();
    public IEnumerable<ConfigEntry> GetAllEntriesRec() => Locator.GetAllEntriesRec();
    public Dictionary<string, ConfigEntry> GetFlat() => Locator.GetFlat();
    public Dictionary<string, ConfigEntry> GetAllFlat() => Locator.GetAllFlat();
    public Dictionary<string, object> GetFlatValues() => Locator.GetFlatValues();
    public Dictionary<string, object> GetAllFlatValues() => Locator.GetAllFlatValues();
  }

  public static class IDirectlyLocatableExtensions
  {
    public static IConfiglike GetHost(this IDirectlyLocatable locatable) => locatable.Host;
    public static ConfigEntry GetEntry(this IDirectlyLocatable locatable, string propPath) => locatable.GetEntry(propPath);
    public static object GetValue(this IDirectlyLocatable locatable, string propPath) => locatable.GetValue(propPath);
    public static bool SetValue(this IDirectlyLocatable locatable, string propPath, object value) => locatable.SetValue(propPath, value);
    public static IEnumerable<ConfigEntry> GetEntries(this IDirectlyLocatable locatable) => locatable.GetEntries();
    public static IEnumerable<ConfigEntry> GetAllEntries(this IDirectlyLocatable locatable) => locatable.GetAllEntries();
    public static IEnumerable<ConfigEntry> GetSubConfigs(this IDirectlyLocatable locatable) => locatable.GetSubConfigs();
    public static IEnumerable<ConfigEntry> GetEntriesRec(this IDirectlyLocatable locatable) => locatable.GetEntriesRec();
    public static IEnumerable<ConfigEntry> GetAllEntriesRec(this IDirectlyLocatable locatable) => locatable.GetAllEntriesRec();
    public static Dictionary<string, ConfigEntry> GetFlat(this IDirectlyLocatable locatable) => locatable.GetFlat();
    public static Dictionary<string, ConfigEntry> GetAllFlat(this IDirectlyLocatable locatable) => locatable.GetAllFlat();
    public static Dictionary<string, object> GetFlatValues(this IDirectlyLocatable locatable) => locatable.GetFlatValues();
    public static Dictionary<string, object> GetAllFlatValues(this IDirectlyLocatable locatable) => locatable.GetAllFlatValues();
  }
}