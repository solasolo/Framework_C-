using System;
using System.Reflection;

namespace SoulFab.Core.Helper
{
    public class ObjectHelper
    {
        static public void Copy(object src, object dest)
        {
            Type type = src.GetType();
            foreach (FieldInfo item in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Type FieldType = item.FieldType;

                if (FieldType.IsValueType || FieldType == typeof(string))
                {
                    item.SetValue(dest, item.GetValue(src));
                }
            }

            foreach (PropertyInfo item in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Type PropertyType = item.PropertyType;

                if (item.CanWrite && PropertyType.IsValueType || PropertyType == typeof(string))
                {
                    item.SetValue(dest, item.GetValue(src));
                }
            }
        }
    }
}
