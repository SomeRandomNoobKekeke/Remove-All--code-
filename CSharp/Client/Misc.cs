using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace RemoveAll
{
  partial class RemoveAllMod : IAssemblyPlugin
  {


    public static string consoleDelim = new string('-', 119);
    public static void log(object msg, Color? cl = null, int printStack = 0, [CallerLineNumber] int lineNumber = 0)
    {
      if (cl == null) cl = Color.Cyan;

      if (testing)
      {
        DebugConsole.NewMessage($"{lineNumber}| {msg ?? "null"}", cl);
      }
      else
      {
        DebugConsole.NewMessage($"{msg ?? "null"}", cl);
      }


      if (printStack > 0)
      {
        StackTrace st = new StackTrace(true);

        for (int i = 3; i < st.FrameCount && i < printStack + 3; i++)
        {
          StackFrame sf = st.GetFrame(i);
          string filePath = shortFileName(sf.GetFileName());
          DebugConsole.NewMessage($"--{filePath} {sf.GetMethod()}:{sf.GetFileLineNumber()}", cl * 0.75f);
        }

        DebugConsole.NewMessage(consoleDelim, cl);
      }

      string shortFileName(string full)
      {
        try
        {
          if (full == null) return "";

          int i = full.LastIndexOf("Source") - 6;
          if (i < 0 || i > full.Length) return full;
          return full.Substring(i);
        }
        catch (Exception e)
        {
          log(e, Color.Green);
        }
        return "";
      }
    }


    // omg, all c# file functions are case insensitive
    // how windows even works on this crap?
    public static bool FileExistsCaseSensitive(string filePath)
    {
      string name = Path.GetFileName(filePath);
      string dir = Path.GetDirectoryName(filePath);

      return Array.Exists(Directory.GetFiles(dir), s => name == Path.GetFileName(s));
    }

    public static void copyIfNotExists(string source, string target)
    {
      bool justExists = File.Exists(target);
      bool existsCaseSensitive = FileExistsCaseSensitive(target);

      // it just doesn't exist
      if (!justExists)
      {
        if (source != "") File.Copy(source, target);
        return;
      }

      // it exists, but letter cases are different
      if (justExists && !existsCaseSensitive)
      {
        string backup = Path.Combine(
          Path.GetDirectoryName(target),
          Path.GetFileNameWithoutExtension(target) + "-old" +
          Path.GetExtension(target)
        );

        if (File.Exists(backup)) File.Delete(backup);

        File.Move(target, backup);

        if (source != "") File.Copy(source, target);

        return;
      }
    }


  }
}