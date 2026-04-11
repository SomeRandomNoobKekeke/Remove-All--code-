using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BaroJunk
{
  public partial interface IConfig : IDirectlyLocatable, IReactiveLocatable
  {
    // It's a duplicate of ConfiglikeObject.ID
    #region HACK
    public static string TypeID<T>() => TypeID(typeof(T));
    public static string TypeID(Type T) => $"{T.Namespace}_{T.Name}";
    #endregion
    public static ConditionalWeakTable<IConfig, ConfigCore> Cores = new();

    public ConfigCore Core => Cores.GetValue(this, c => new ConfigCore(c));


    public IConfiglike Host => Core.Host;
    public ReactiveCore ReactiveCore => Core.ReactiveCore;
    public string ID => Core.ID;

    DirectEntryLocator IDirectlyLocatable.Locator => Core.Locator;
    ReactiveEntryLocator IReactiveLocatable.ReactiveLocator => Core.ReactiveLocator;
    public ConfigManager Manager => Core.Manager;

    public string DefaultSavePath => Core.DefaultSavePath;
    public void UseStrategy(ConfigStrategy strategy) => Core.UseStrategy(strategy);
    public void OnPropChanged(Action<string, object> action) => Core.OnPropChanged(action);
    public void OnUpdated(Action action) => Core.OnUpdated(action);

    public SimpleParser Parser
    {
      get => Core.Parser;
      set => Core.Parser = value;
    }
    public NetParser NetParser
    {
      get => Core.NetParser;
      set => Core.NetParser = value;
    }
    public XMLParser XMLParser
    {
      get => Core.XMLParser;
      set => Core.XMLParser = value;
    }
    public Logger Logger
    {
      get => Core.Logger;
      set => Core.Logger = value;
    }

    public IConfigFacades Facades
    {
      get => Core.Facades;
      set => Core.Facades = value;
    }
    public ConfigSettings Settings => Core.Settings;
  }
}