using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

using BaroJunk;

namespace RemoveAll
{
  public class BlackList
  {
    public static string BlacklistsDir = "Blacklists";

    public Dictionary<int, bool> LevelObjects { get; set; } = new();
    public Dictionary<int, bool> MapEntity { get; set; } = new();
    public Dictionary<int, bool> Particles { get; set; } = new();
    public Dictionary<int, bool> Decals { get; set; } = new();

    public void Load(string path)
    {
      //BRUH probably should return result and handle logging somewhere else
      string fullPath = Path.Combine(Mod.Package.Dir, BlacklistsDir, $"{path}.json");
      if (!File.Exists(fullPath))
      {
        Mod.Logger.Log($"Can't find [{path}.json] in Blacklists folder");
        return;
      }

      Dictionary<string, Dictionary<string, bool>> all =
        JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, bool>>>(
          File.ReadAllText(fullPath)
        );

      void MergeDict(Dictionary<string, bool> source, Dictionary<int, bool> target)
      {
        foreach (string key in source.Keys)
        {
          int hash = key.ToIdentifier().HashCode;

          // There's only 1 hash collision, guess the item :BaroDev(wide):
          // if (target.ContainsKey(hash))
          // {
          //   Logger.Default.Log($"hash collision for {key}");
          // }

          target[hash] = source[key];
        }
      }

      Clear();

      MergeDict(all["structures"], MapEntity);
      MergeDict(all["items"], MapEntity);
      MergeDict(all["decals"], Decals);
      MergeDict(all["levelObjects"], LevelObjects);
      MergeDict(all["particles"], Particles);

      Mod.Logger.Log($"Loaded {path}.json");
    }

    public void Clear()
    {
      MapEntity.Clear();
      Decals.Clear();
      LevelObjects.Clear();
      Particles.Clear();
    }
  }

}