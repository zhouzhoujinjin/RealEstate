using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Approval.Utils
{
  public static class DotSplittedKeyDictionaryToObjectConverter
  {
    public static T Parse<T>(Dictionary<string, string> value)
    {
      var type = typeof(T);
      return (T)Parse(value, type);
    }

    public static object Parse(Dictionary<string, string> value, Type type)
    {
      var model = Activator.CreateInstance(type);
      foreach (var kv in value)
      {
        var parts = kv.Key.Split(".");
        SetValue(model, kv.Value, kv.Key);
      }
      return model;
    }

    public static Dictionary<string, string> Dump(object value)
    {
      var dict = new Dictionary<string, string>();
      var type = value.GetType();
      var fields = GetFields(type);
      foreach (var field in fields)
      {
        dict[field] = GetValue(value, field);
      }
      return dict;
    }

    public static IEnumerable<string> GetFields(Type type, string parent = null)
    {
      var fields = new List<string>();
      foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && !p.PropertyType.IsGenericType))
      {
        var field = parent == null ? pi.Name : $"{parent}.{pi.Name}";
        if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string))
        {
          fields.Add(field);
        }
        else
        {
          fields.AddRange(GetFields(pi.PropertyType, field));
        }
      }
      return fields;
    }

    public static string GetValue(object value, string field)
    {
      var parts = field.Split(".");
      Type type = value.GetType();
      foreach (var part in parts)
      {
        if (value == null)
        {
          return null;
        }
        var r = int.TryParse(part, out var index);
        if (r)
        {
          var enumerator = (value as IEnumerable)?.GetEnumerator();
          if (enumerator == null)
          {
            return null;
          }
          var j = -1;
          while (j < index)
          {
            enumerator.MoveNext();
            j++;
          }
          value = enumerator.Current;
          if (value != null) type = value.GetType();
        }
        else
        {
          var pi = type.GetProperties().FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.CurrentCultureIgnoreCase));
          if (pi != null)
          {
            value = pi.GetValue(value);
            type = value.GetType();
          }
          else
          {
            return null;
          }
        }
      }
      return value.ToString();
    }


    public static void SetValue(object entry, string value, string key)
    {
      var parts = key.Split(".");
      var parent = entry;
      foreach (var part in parts)
      {
        var type = parent.GetType();
        if (type.IsGenericType)
        {
          type = type.GenericTypeArguments[0];
        }
        var pi = type.GetProperties().FirstOrDefault(x => string.Equals(x.Name, part, StringComparison.CurrentCultureIgnoreCase));
        if (pi != null && (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string)))
        {
          switch (pi.PropertyType.Name)
          {
            case "Int32":
              pi.SetValue(parent, Convert.ToInt32(value));
              break;
            case "Boolean":
              pi.SetValue(parent, Convert.ToBoolean(value));
              break;
            case "String":
              if (value == null)
              {
                pi.SetValue(parent, null);
              }
              else
              {
                pi.SetValue(parent, value.ToString());
              }
              break;
          }
        }
        else if (pi != null && pi.PropertyType == typeof(DateTime))
        {
          pi.SetValue(parent, DateTime.Parse(value));
        }
        else
        {
          if (int.TryParse(part, out var index))
          {
            var list = (parent as IList);
            switch (type.Name)
            {
              case "Int32":
                list.Add(Convert.ToInt32(value));
                break;
              case "Boolean":
                list.Add(Convert.ToBoolean(value));
                break;
              case "String":
                list.Add(value);
                break;
              default:
                while (list.Count <= index)
                {
                  list.Add(Activator.CreateInstance(type));
                }
                break;
            }
            parent = list[index];
          }
          else if (pi != null)
          {
            var data = pi.GetValue(parent);

            if (data == null)
            {
              data = Activator.CreateInstance(pi.PropertyType);
              pi.SetValue(parent, data);
            }
            parent = data;
          }
        }
      }
    }
  }
}
