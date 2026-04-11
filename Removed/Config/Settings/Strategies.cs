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
  public class ConfigStrategy
  {
    /// <summary>
    /// Do nothing
    /// </summary>
    public static ConfigStrategy Passive = new ConfigStrategy()
    {
      Name = "Passive",
    };

    /// <summary>
    /// It auto loads and auto saves settings in singleplayer and on client side in multiplayer
    /// But doens't sync it with server
    /// </summary>
    public static ConfigStrategy MultiplayerClientside = new ConfigStrategy()
    {
      Name = "MultiplayerClientside",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        AutoSave = true,
        LoadOnInit = true,
        SaveOnQuit = true,
        SaveEveryRound = true,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = false,
      }
    };

    /// <summary>
    /// In multiplayer server loads config from file and syncs it with clients
    /// </summary>
    public static ConfigStrategy MultiplayerBothSides = new ConfigStrategy()
    {
      Name = "MultiplayerBothSides",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        AutoSave = true,
        LoadOnInit = true,
        SaveOnQuit = true,
        SaveEveryRound = true,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = true,
      }
    };

    /// <summary>
    /// Yes
    /// </summary>
    public static ConfigStrategy OnlySingleplayer = new ConfigStrategy()
    {
      Name = "OnlySingleplayer",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        AutoSave = true,
        LoadOnInit = true,
        SaveOnQuit = true,
        SaveEveryRound = true,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = false,
      }
    };

    /// <summary>
    /// Just for test
    /// </summary>
    public static ConfigStrategy OnlyAutosave = new ConfigStrategy()
    {
      Name = "OnlyAutosave",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = true,
        },
        AutoSave = true,
        LoadOnInit = true,
        SaveOnQuit = true,
        SaveEveryRound = true,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = false,
      }
    };

    /// <summary>
    /// Just for test
    /// </summary>
    public static ConfigStrategy OnlyNetworking = new ConfigStrategy()
    {
      Name = "OnlyNetworking",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = false,
          ShouldSave = false,
        },
        AutoSave = false,
        LoadOnInit = false,
        SaveOnQuit = false,
        SaveEveryRound = false,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = true,
      }
    };


    /// <summary>
    /// Just for test
    /// </summary>
    public static ConfigStrategy OnlyLoading = new ConfigStrategy()
    {
      Name = "OnlyLoading",
      AutoSaverStrategy = new AutoSaverStrategy()
      {
        OnClient = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = false,
        },
        OnServer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = false,
        },
        InSingleplayer = new SaveLoadingStrategy()
        {
          ShouldLoad = true,
          ShouldSave = false,
        },
        AutoSave = true,
        LoadOnInit = true,
        SaveOnQuit = false,
        SaveEveryRound = false,
      },
      NetManagerStrategy = new NetManagerStrategy()
      {
        NetSync = false,
      }
    };

    #region Own Props
    #endregion
    public string Name { get; set; } = "Unknown Config Strategy";
    public AutoSaverStrategy AutoSaverStrategy { get; set; } = new();
    public NetManagerStrategy NetManagerStrategy { get; set; } = new();
  }

  public class SaveLoadingStrategy
  {
    public bool ShouldLoad { get; set; }
    public bool ShouldSave { get; set; }
  }
  public class AutoSaverStrategy
  {
    public SaveLoadingStrategy OnClient { get; set; } = new();
    public SaveLoadingStrategy OnServer { get; set; } = new();
    public SaveLoadingStrategy InSingleplayer { get; set; } = new();
    public bool AutoSave { get; set; }
    public bool LoadOnInit { get; set; }
    public bool SaveOnQuit { get; set; }
    public bool SaveEveryRound { get; set; }
  }

  public class NetManagerStrategy
  {
    public bool NetSync { get; set; }
  }
}