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
  public partial interface IConfig
  {
    public static bool IsEqual(IConfig configA, IConfig configB)
      => ConfigCore.IsEqual(configA.Core, configB.Core);
    public bool EqualsTo(IConfig other) => Core.EqualsTo(other.Core);

    public ConfigCompareResult CompareTo(IConfig other) => ConfigCore.Compare(Core, other.Core);
    public static ConfigCompareResult Compare(IConfig configA, IConfig configB)
      => new ConfigCompareResult(configA.Core, configB.Core);

    public void Clear() => Core.Clear();
    public void Restore() => Core.Restore();
    public void CopyTo(IConfig other) => Core.CopyTo(other.Core);

    public string NetHeader => Core.ID;
    public void NetEncode(IWriteMessage msg) => Core.NetEncode(msg);
    public void NetDecode(IReadMessage msg) => Core.NetDecode(msg);

#if CLIENT
    public SimpleResult Ask() => Core.Ask();
    public SimpleResult Sync() => Core.Sync();
#elif SERVER
    public SimpleResult Sync()=> Core.Sync();
#endif
    public SimpleResult LoadSave(string path) => Core.LoadSave(path);
    public SimpleResult Save(string path) => Core.Save(path);
    public SimpleResult Load(string path) => Core.Load(path);
    public string ToText() => Core.ToText();
    public XElement ToXML() => Core.ToXML();
    public void FromXML(XElement element) => Core.FromXML(element);
    public Func<string[][]> ToHints() => Core.ToHints();
  }
}