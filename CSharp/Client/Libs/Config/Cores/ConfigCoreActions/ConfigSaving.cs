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

  public partial class ConfigCore
  {
    public SimpleResult LoadSave(string path)
    {
      SimpleResult result = Load(path);
      Save(path);
      return result;
    }

    public SimpleResult Save(string path)
    {
      try
      {
        Facades.IOFacade.EnsureDirectory(Path.GetDirectoryName(path));

        XDocument xdoc = new XDocument();
        xdoc.Add(ToXML());
        Facades.IOFacade.SaveXDoc(xdoc, path);
      }
      catch (Exception e)
      {
        return new SimpleResult()
        {
          Ok = false,
          Details = $"Can't save config: {e.Message}",
          Exception = e,
        };
      }

      return SimpleResult.Success();
    }

    public SimpleResult Load(string path)
    {
      if (!Facades.IOFacade.FileExists(path)) return SimpleResult.Failure($"Can't load config: [{path}] not found");

      try
      {
        XDocument xdoc = Facades.IOFacade.LoadXDoc(path);
        this.FromXML(xdoc.Root);
      }
      catch (Exception e)
      {
        return new SimpleResult()
        {
          Ok = false,
          Details = $"Can't load config: {e.Message}",
          Exception = e,
        };
      }

      return SimpleResult.Success();
    }
  }

}