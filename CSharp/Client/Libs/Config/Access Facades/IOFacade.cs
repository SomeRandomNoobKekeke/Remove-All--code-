using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace BaroJunk
{
  public interface IIOFacade
  {
    public XDocument LoadXDoc(string path);
    public void SaveXDoc(XDocument xdoc, string path);
    public bool FileExists(string path);
    public void EnsureDirectory(string path);

  }
  public class IOFacade : IIOFacade
  {
    public XDocument LoadXDoc(string path) => XDocument.Load(path);
    public void SaveXDoc(XDocument xdoc, string path) => xdoc.Save(path);
    public bool FileExists(string path) => File.Exists(path);
    public void EnsureDirectory(string path)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
    }
  }



}
