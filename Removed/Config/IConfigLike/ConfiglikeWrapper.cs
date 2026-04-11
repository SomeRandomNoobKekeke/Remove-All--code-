using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BaroJunk
{
  /// <summary>
  /// This class is supposed to wrap raw object in appropriate configlike
  /// </summary>
  public class ConfiglikeWrapper
  {
    public static IConfiglike Wrap(object o)
    {
      ArgumentNullException.ThrowIfNull(o);
      return new ConfiglikeObject(o);
    }
  }
}