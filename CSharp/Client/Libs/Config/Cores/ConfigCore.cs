using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BaroJunk
{
  public partial class ConfigCore : IConfigLikeContainer, IDirectlyLocatable, IReactiveLocatable
  {
    public object RawTarget => Host.Target;
    public IConfiglike Host { get; }
    public ReactiveCore ReactiveCore { get; }
    public string ID => Host.ID;

    public DirectEntryLocator Locator { get; }
    public ReactiveEntryLocator ReactiveLocator { get; }
    public ConfigManager Manager { get; }

    public SimpleParser Parser { get; set; }
    public NetParser NetParser { get; set; }
    public XMLParser XMLParser { get; set; }
    public Logger Logger { get; set; }

    public IConfigFacades Facades { get; set; }
    public ConfigSettings Settings { get; }

    public string DefaultSavePath => ConfigAutoSaver.DefaultSavePathFor(this);

    public void UseStrategy(ConfigStrategy strategy) => Settings.Strategy = strategy;

    public void OnPropChanged(Action<string, object> action) => ReactiveCore.OnPropChanged = action;
    public void OnUpdated(Action action) => ReactiveCore.OnUpdated = action;

    public ConfigCore(object target) : this(ConfiglikeWrapper.Wrap(target)) { }
    public ConfigCore(IConfiglike host)
    {
      Host = host;

      Locator = new DirectEntryLocator(this);
      ReactiveCore = new ReactiveCore(this);
      ReactiveLocator = ReactiveCore.Locator;

      Parser = new SimpleParser();
      NetParser = new NetParser(Parser);
      XMLParser = new XMLParser(Parser);
      Logger = new Logger();

      Facades = new ConfigFacades();
      Manager = new ConfigManager(this);
      Settings = new ConfigSettings(this);
    }

  }
}