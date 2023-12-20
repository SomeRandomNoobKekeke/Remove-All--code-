using System;
using System.Reflection;
using System.IO;
using System.Runtime.Loader;

namespace RemoveAll
{
  class JSON
  {
    static JSON()
    {
      context = new AssemblyLoadContext(name: "JSON", isCollectible: true);

      asm = context.LoadFromAssemblyPath(
        Path.Combine(Directory.GetCurrentDirectory(), "System.Text.Json.dll"));

      JsonSerializer = asm.GetType("System.Text.Json.JsonSerializer");

      JsonSerializerOptions = asm.GetType("System.Text.Json.JsonSerializerOptions");

      Serialize = JsonSerializer.GetMethod("Serialize", new Type[] { typeof(object), typeof(Type), JsonSerializerOptions });

      Deserialize = JsonSerializer.GetMethod("Deserialize", new Type[] { typeof(string), typeof(Type), JsonSerializerOptions });

      theOptions = Activator.CreateInstance(JsonSerializerOptions);
      JsonSerializerOptions.GetProperty("WriteIndented").SetValue(theOptions, true);
    }

    public static AssemblyLoadContext context;

    public static Assembly asm;
    public static Type JsonSerializer;
    public static Type JsonSerializerOptions;

    public static object theOptions;
    public static MethodInfo Serialize;
    public static MethodInfo Deserialize;


    public static T parse<T>(string json)
    {
      return (T)Deserialize.Invoke(null, new object[] { json, typeof(T), null }); ;
    }

    public static string stringify(Object o)
    {
      return (string)Serialize.Invoke(null, new object[] { o, o.GetType(), theOptions });
    }
  }
}