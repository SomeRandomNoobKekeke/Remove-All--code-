using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using System.Text.Json;
using System.IO;



namespace RemoveAll
{
  partial class Plugin : IAssemblyPlugin
  {
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

    public static void findModFolder()
    {
      bool found = false;

      foreach (ContentPackage p in ContentPackageManager.EnabledPackages.All)
      {
        if (p.Name.Contains(ModName))
        {
          found = true;
          Mod.ModDir = Path.GetFullPath(p.Dir);
          Mod.ModVersion = p.ModVersion;
          break;
        }
      }

      if (!found) err($"Couldn't find {ModName} mod folder");
    }
  }
}