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
  public interface IHooksFacade
  {
    public void CallHook(string name, params object[] args);
    public void CallPatch(MethodBase method);
    public void AddHook(string name, string identifier, LuaCsFunc func);
    public void Patch(
      string identifier,
      MethodBase method,
      LuaCsPatchFunc patch,
      LuaCsHook.HookMethodType hookType = LuaCsHook.HookMethodType.Before
    );
  }

  public class HooksFacade : IHooksFacade
  {
    public void CallHook(string name, params object[] args) { }
    public void CallPatch(MethodBase method) { }

    //TODO fix
    public void AddHook(string name, string identifier, LuaCsFunc func) { }
    // => GameMain.LuaCs.Hook.Add(name, identifier, func);

    //TODO fix
    public void Patch(
      string identifier,
      MethodBase method,
      LuaCsPatchFunc patch,
      LuaCsHook.HookMethodType hookType = LuaCsHook.HookMethodType.Before
    )
    { }
    // => GameMain.LuaCs.Hook.Patch(identifier, method, patch, hookType);

  }



}
