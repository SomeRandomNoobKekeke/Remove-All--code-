using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace BaroJunk
{
  public static class IConfigExtensions
  {
    public static IConfig Self(this IConfig config) => config;

    public static ConfigCore GetCore(this IConfig config) => config.Core;
    public static IConfiglike GetHost(this IConfig config) => config.Host;
    public static ReactiveCore GetReactiveCore(this IConfig config) => config.ReactiveCore;
    public static string GetID(this IConfig config) => config.ID;

    public static DirectEntryLocator GetLocator(this IConfig config) => config.Locator;
    public static ReactiveEntryLocator GetReactiveLocator(this IConfig config) => config.ReactiveLocator;
    public static ConfigManager GetManager(this IConfig config) => config.Manager;


    public static string GetDefaultSavePath(this IConfig config) => config.DefaultSavePath;
    public static void UseStrategy(this IConfig config, ConfigStrategy strategy) => config.UseStrategy(strategy);
    public static void OnPropChanged(this IConfig config, Action<string, object> action) => config.OnPropChanged(action);
    public static void OnUpdated(this IConfig config, Action action) => config.OnUpdated(action);

    public static SimpleParser GetParser(this IConfig config) => config.Parser;
    public static void SetParser(this IConfig config, SimpleParser value) => config.Parser = value;

    public static NetParser GetNetParser(this IConfig config) => config.NetParser;
    public static void SetNetParser(this IConfig config, NetParser value) => config.NetParser = value;

    public static XMLParser GetXMLParser(this IConfig config) => config.XMLParser;
    public static void SetXMLParser(this IConfig config, XMLParser value) => config.XMLParser = value;

    public static Logger GetLogger(this IConfig config) => config.Logger;
    public static void SetLogger(this IConfig config, Logger value) => config.Logger = value;

    public static IConfigFacades GetFacades(this IConfig config) => config.Facades;
    public static void SetFacades(this IConfig config, IConfigFacades value) => config.Facades = value;

    public static ConfigSettings Settings(this IConfig config) => config.Settings;



    public static bool EqualsTo(this IConfig config, IConfig other) => config.EqualsTo(other);
    public static ConfigCompareResult CompareTo(this IConfig config, IConfig other) => config.CompareTo(other);

    public static void Clear(this IConfig config) => config.Clear();
    public static void Restore(this IConfig config) => config.Restore();
    public static void CopyTo(this IConfig config, IConfig other) => config.CopyTo(other);
    public static string GetNetHeader(this IConfig config) => config.NetHeader;

    public static void NetEncode(this IConfig config, IWriteMessage msg) => config.NetEncode(msg);
    public static void NetDecode(this IConfig config, IReadMessage msg) => config.NetDecode(msg);

#if CLIENT
    public static SimpleResult Ask(this IConfig config) => config.Ask();
    public static SimpleResult Sync(this IConfig config) => config.Sync();
#elif SERVER
    public static SimpleResult Sync(this IConfig config)=> config.Sync();
#endif

    public static SimpleResult LoadSave(this IConfig config, string path) => config.LoadSave(path);
    public static SimpleResult Save(this IConfig config, string path) => config.Save(path);
    public static SimpleResult Load(this IConfig config, string path) => config.Load(path);
    public static string ToText(this IConfig config) => config.ToText();
    public static XElement ToXML(this IConfig config) => config.ToXML();
    public static Func<string[][]> ToHints(this IConfig config) => config.ToHints();
    public static void FromXML(this IConfig config, XElement element) => config.FromXML(element);
  }
}