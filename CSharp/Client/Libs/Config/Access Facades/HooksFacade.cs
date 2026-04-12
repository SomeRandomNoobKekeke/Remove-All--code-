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
      Barotrauma.LuaCs.Compatibility.ILuaCsHook.HookMethodType hookType = Barotrauma.LuaCs.Compatibility.ILuaCsHook.HookMethodType.Before
    );
  }

  public class HooksFacade : IHooksFacade
  {
    public void CallHook(string name, params object[] args) { }
    public void CallPatch(MethodBase method) { }


    public void AddHook(string name, string identifier, LuaCsFunc func)
      => LuaCsSetup.Instance.EventService.Add(name, identifier, func);


    public void Patch(
      string identifier,
      MethodBase method,
      LuaCsPatchFunc patch,
      Barotrauma.LuaCs.Compatibility.ILuaCsHook.HookMethodType hookType = Barotrauma.LuaCs.Compatibility.ILuaCsHook.HookMethodType.Before
    ) => ((LuaCsSetup.Instance.EventService as EventService)
           ._luaPatcher as LuaPatcherService)
           .Patch(identifier, method, patch, hookType);


  }



}
