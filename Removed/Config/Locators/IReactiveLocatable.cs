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
  /// This object is exposing ReactiveEntryLocator and is mapping location calls to it
  /// </summary>
  public interface IReactiveLocatable
  {
    public ReactiveEntryLocator ReactiveLocator { get; }


    public IConfiglike Host => ReactiveLocator.Host;
    public ReactiveEntry ReactiveGetEntry(string propPath) => ReactiveLocator.ReactiveGetEntry(propPath);
    public object ReactiveGetValue(string propPath) => ReactiveLocator.ReactiveGetValue(propPath);
    public bool ReactiveSetValue(string propPath, object value) => ReactiveLocator.ReactiveSetValue(propPath, value);
    public IEnumerable<ReactiveEntry> ReactiveGetEntries() => ReactiveLocator.ReactiveGetEntries();
    public IEnumerable<ReactiveEntry> ReactiveGetAllEntries() => ReactiveLocator.ReactiveGetAllEntries();
    public IEnumerable<ReactiveEntry> ReactiveGetSubConfigs() => ReactiveLocator.ReactiveGetSubConfigs();
    public IEnumerable<ReactiveEntry> ReactiveGetEntriesRec() => ReactiveLocator.ReactiveGetEntriesRec();
    public IEnumerable<ReactiveEntry> ReactiveGetAllEntriesRec() => ReactiveLocator.ReactiveGetAllEntriesRec();
    public Dictionary<string, ReactiveEntry> ReactiveGetFlat() => ReactiveLocator.ReactiveGetFlat();
    public Dictionary<string, ReactiveEntry> ReactiveGetAllFlat() => ReactiveLocator.ReactiveGetAllFlat();
    public Dictionary<string, object> ReactiveGetFlatValues() => ReactiveLocator.ReactiveGetFlatValues();
    public Dictionary<string, object> ReactiveGetAllFlatValues() => ReactiveLocator.ReactiveGetAllFlatValues();
  }

  public static class IReactiveLocatableExtensions
  {
    public static ReactiveEntry ReactiveGetEntry(this IReactiveLocatable locatable, string propPath) => locatable.ReactiveGetEntry(propPath);
    public static object ReactiveGetValue(this IReactiveLocatable locatable, string propPath) => locatable.ReactiveGetValue(propPath);
    public static bool ReactiveSetValue(this IReactiveLocatable locatable, string propPath, object value) => locatable.ReactiveSetValue(propPath, value);
    public static IEnumerable<ReactiveEntry> ReactiveGetEntries(this IReactiveLocatable locatable) => locatable.ReactiveGetEntries();
    public static IEnumerable<ReactiveEntry> ReactiveGetAllEntries(this IReactiveLocatable locatable) => locatable.ReactiveGetAllEntries();
    public static IEnumerable<ReactiveEntry> ReactiveGetSubConfigs(this IReactiveLocatable locatable) => locatable.ReactiveGetSubConfigs();
    public static IEnumerable<ReactiveEntry> ReactiveGetEntriesRec(this IReactiveLocatable locatable) => locatable.ReactiveGetEntriesRec();
    public static IEnumerable<ReactiveEntry> ReactiveGetAllEntriesRec(this IReactiveLocatable locatable) => locatable.ReactiveGetAllEntriesRec();
    public static Dictionary<string, ReactiveEntry> ReactiveGetFlat(this IReactiveLocatable locatable) => locatable.ReactiveGetFlat();
    public static Dictionary<string, ReactiveEntry> ReactiveGetAllFlat(this IReactiveLocatable locatable) => locatable.ReactiveGetAllFlat();
  }
}