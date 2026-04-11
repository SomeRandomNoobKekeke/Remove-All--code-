using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Text;

using Barotrauma;

namespace BaroJunk
{
  public interface IConfigEntry
  {
    public string Key { get; }
    public Type Type { get; }
    public object Value { get; set; }
    public bool SetValue(object value);
    public bool IsConfig { get; }
    public bool IsValid { get; }
  }
}