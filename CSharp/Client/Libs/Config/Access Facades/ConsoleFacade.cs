using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.Networking;

namespace BaroJunk
{
  public interface IConsoleFacade
  {
    public void Remove(DebugConsole.Command command);
    public void Insert(DebugConsole.Command command);
    public void Execute(string command);
  }

  public class ConsoleFacade : IConsoleFacade
  {
    public void Remove(DebugConsole.Command command)
      => DebugConsole.Commands.Remove(command);

    public void Insert(DebugConsole.Command command)
      => DebugConsole.Commands.Insert(0, command);
    public void Execute(string command)
      => DebugConsole.ExecuteCommand(command);
  }

}
