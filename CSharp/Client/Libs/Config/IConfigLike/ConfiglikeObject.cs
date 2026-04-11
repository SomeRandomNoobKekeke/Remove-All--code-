using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BaroJunk
{
  public class ConfiglikeObject : IConfiglike
  {
    public static Type SubConfigType = typeof(IConfig);
    public static BindingFlags pls = BindingFlags.Public | BindingFlags.Instance;

    public object Target { get; }
    public bool IsValid { get; }
    public bool AmISubConfig { get; }
    public string ID => IsValid ? $"{Target.GetType().Namespace}_{Target.GetType().Name}" : "[!]";
    public string Name => Target.GetType().Name;
    public DirectEntryLocator Locator { get; }

    public ConfigCore Core
    {
      get
      {
        if (Target is not IConfig config) return null;
        return config.Core;
      }
    }

    public bool HasProp(string key)
      => String.IsNullOrEmpty(key) ? false
      : Target?.GetType()?.GetProperty(key, pls) is not null;

    public Type TypeOfProp(string key)
      => String.IsNullOrEmpty(key) ? null
      : Target?.GetType()?.GetProperty(key, pls)?.PropertyType;

    public bool IsPropASubConfig(string key)
      => String.IsNullOrEmpty(key) ? false
      : Target?.GetType()?.GetProperty(key, pls)?.PropertyType.IsAssignableTo(SubConfigType) ?? false;

    //THINK this is sneaky, it's not possible to get type from null -> get correct answer if this is a config
    // For nullable props you should use IsPropASubConfig
    // mb i should just remove this method
    public bool IsSubConfig(object o) => o is null ? false : o.GetType().IsAssignableTo(SubConfigType);
    public bool IsSubConfig(Type T) => T is null ? false : T.IsAssignableTo(SubConfigType);

    public object GetValue(string key)
      => String.IsNullOrEmpty(key) ? null
      : Target?.GetType()?.GetProperty(key, pls)?.GetValue(Target);

    public bool SetValue(string key, object value)
    {
      if (String.IsNullOrEmpty(key)) return false;
      try
      {
        PropertyInfo pi = Target?.GetType()?.GetProperty(key, pls);
        if (pi is null) return false;
        pi.SetValue(Target, value);
        return true;
      }
      catch (Exception e)
      {
        //REWORK ig it should return SimpleResult
        // throw;
        return false;
      }
    }

    public IConfiglike CreateDefaultForType(Type T)
    {
      if (T is null || !T.IsAssignableTo(SubConfigType)) return new ConfiglikeObject(null);
      try
      {
        return new ConfiglikeObject(Activator.CreateInstance(T));
      }
      catch (Exception e)
      {
        return new ConfiglikeObject(null);
      }
    }

    public IConfiglike GetPropAsConfig(string key) => ToConfig(GetValue(key));
    public IConfiglike ToConfig(object o) => new ConfiglikeObject(o);

    public IEnumerable<string> Keys
    {
      get
      {
        if (!IsValid) return new string[0];
        return Target.GetType().GetProperties(pls).Select(pi => pi.Name);
      }
    }

    public Dictionary<string, object> AsDict
      => !IsValid ? new Dictionary<string, object>()
         : Target.GetType().GetProperties(pls)
         .ToDictionary(pi => pi.Name, pi => pi.GetValue(Target));

    public ConfiglikeObject(object target)
    {
      Target = target;
      IsValid = Target is not null;
      AmISubConfig = IsValid && Target.GetType().IsAssignableTo(SubConfigType);
      Locator = new DirectEntryLocator(new IConfigLikeLocatorAdapter(this));
    }

    public override string ToString() => $"ConfiglikeObject [{Target} ({Target?.GetHashCode()})]";

    public override bool Equals(object obj)
    {
      if (obj is not ConfiglikeObject other) return false;
      return Object.Equals(Target, other.Target);
    }
  }
}