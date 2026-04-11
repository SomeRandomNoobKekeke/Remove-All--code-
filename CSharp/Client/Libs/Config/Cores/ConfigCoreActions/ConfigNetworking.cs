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
using Barotrauma.Networking;

namespace BaroJunk
{

  public partial class ConfigCore
  {
    public string NetHeader => ID;
    public void NetEncode(IWriteMessage msg)
    {
      foreach (ConfigEntry entry in this.GetEntriesRec())
      {
        SimpleResult result = NetParser.Encode(msg, entry.Value, entry.Type);
        if (!result.Ok)
        {
          Logger.Warning(result.Details);
        }
      }
    }

    public void NetDecode(IReadMessage msg)
    {
      foreach (ConfigEntry entry in this.GetEntriesRec())
      {
        SimpleResult result = NetParser.Decode(msg, entry.Type);
        if (result.Ok) entry.Value = result.Result;
        else Logger.Warning(result.Details);
      }

      Manager.RecievedConfigSync();
    }


    public SimpleResult Ask()
    {
      if (!Facades.NetFacade.IsMultiplayer) return SimpleResult.Failure("It's not multiplayer");

      if (Facades.NetFacade.IsClient)
      {
        Facades.NetFacade.ClientSend(NetHeader + "_ask");
      }
      return SimpleResult.Success();
    }

    public SimpleResult Sync()
    {
      if (!Facades.NetFacade.IsMultiplayer) return SimpleResult.Failure("It's not multiplayer");

      if (Facades.NetFacade.IsClient)
      {
        if (!Facades.NetFacade.DoIHavePermissions()) return SimpleResult.Failure(Facades.NetFacade.DontHavePermissionsString);

        Facades.NetFacade.ClientEncondeAndSend(NetHeader + "_sync", this);
      }
      else
      {
        if (!Facades.NetFacade.IsMultiplayer) return SimpleResult.Failure("It's not multiplayer");
        Facades.NetFacade.ServerEncondeAndBroadcast(NetHeader + "_sync", this);
      }
      return SimpleResult.Success();
    }
  }

}