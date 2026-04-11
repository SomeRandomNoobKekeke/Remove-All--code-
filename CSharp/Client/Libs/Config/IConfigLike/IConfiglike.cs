using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BaroJunk
{
  /// <summary>
  /// Something like Dictionary<string, object>
  /// Half of this props are unused, idk what i need
  /// </summary>
  public interface IConfiglike : IDirectlyLocatable
  {
    public DirectEntryLocator Locator { get; }

    public object Target { get; }
    public bool IsValid { get; }
    public string ID { get; }
    public string Name { get; }

    public IEnumerable<string> Keys { get; }
    public Dictionary<string, object> AsDict { get; }


    // Not every Configlike is actually a full config with a core
    public ConfigCore Core { get; }

    public bool IsSubConfig(object o);
    public bool IsSubConfig(Type T);
    public IConfiglike ToConfig(object o);
    public bool AmISubConfig { get; }

    public bool HasProp(string key);
    public Type TypeOfProp(string key);
    public bool IsPropASubConfig(string key);
    public object GetValue(string key);
    public bool SetValue(string key, object value);
    public IConfiglike GetPropAsConfig(string key);
    public IConfiglike CreateDefaultForType(Type T);
  }
}