using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Barotrauma;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;

namespace BaroJunk
{
  public class ConfigAutoSaver
  {
    public static string DefaultSavePathFor(ConfigCore config)
      => Path.Combine("ModSettings", "Configs", $"{config.ID}.xml");

    public ConfigCore Config;
    public ConfigAutoSaver(ConfigCore config)
    {
      Config = config;

      Config.OnUpdated(() => TrySaveAfterUpdate());
      Config.OnPropChanged((key, value) => TrySaveAfterUpdate());
    }

    private bool enabled; public bool Enabled
    {
      get => enabled;
      set
      {
        bool wasEnabled = enabled;
        enabled = value;
        if (wasEnabled && !enabled) Deactivate();
        if (!wasEnabled && enabled) Initialize();
      }
    }

    public bool ShouldSaveInMultiplayer { get; set; }
    public bool LoadOnInit { get; set; }
    public bool SaveOnQuit { get; set; }
    public bool SaveEveryRound { get; set; }
    public bool ShouldSave { get; set; }
    public bool ShouldLoad { get; set; }

    //TODO test
    //It seems to be working but i'm to lazy to test it
    public bool ShouldSaveAfterUpdate { get; set; } = false;
    public void TrySaveAfterUpdate()
    {
      if (!Enabled || !ShouldSaveAfterUpdate || !ShouldSave) return;
      Config?.Save(Config.Settings.SavePath);
    }

    public void UseStrategy(AutoSaverStrategy strategy)
    {
      if (Config.Facades.NetFacade.IsMultiplayer)
      {
        if (Config.Facades.NetFacade.IsClient)
        {
          ShouldLoad = strategy.OnClient.ShouldLoad;
          ShouldSave = strategy.OnClient.ShouldSave;
        }
        else
        {
          ShouldLoad = strategy.OnServer.ShouldLoad;
          ShouldSave = strategy.OnServer.ShouldSave;
        }
      }
      else
      {
        ShouldLoad = strategy.InSingleplayer.ShouldLoad;
        ShouldSave = strategy.InSingleplayer.ShouldSave;
      }

      LoadOnInit = strategy.LoadOnInit;
      SaveOnQuit = strategy.SaveOnQuit;
      SaveEveryRound = strategy.SaveEveryRound;
      Enabled = strategy.AutoSave;
    }



    private void Initialize()
    {
      Config.Settings.SavePath ??= DefaultSavePathFor(Config);

      if (ShouldLoad && LoadOnInit)
      {
        Config.Load(Config.Settings.SavePath);
      }

      Config.Facades.HooksFacade.AddHook("stop", $"save {Config.ID} config on quit", (object[] args) =>
      {
        if (ShouldSave && SaveOnQuit) Config?.Save(Config.Settings.SavePath);
        return null;
      });

      Config.Facades.HooksFacade.AddHook("roundend", $"save {Config.ID} config on round end", (object[] args) =>
      {
        if (ShouldSave && SaveEveryRound) Config?.Save(Config.Settings.SavePath);
        return null;
      });
    }

    private void Deactivate()
    {
      Config.Facades.HooksFacade.AddHook("stop", $"save {Config.ID} config on quit", (object[] args) => null);
      Config.Facades.HooksFacade.AddHook("roundend", $"save {Config.ID} config on round end", (object[] args) => null);
    }


  }
}