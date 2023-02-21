using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace SoulFab.Core.Helper
{
    public delegate void AssemblyCallback(Assembly assm);

    public static class ReflectHelper
    {
        static IDictionary<string, HashSet<string>> SearchPathList = new Dictionary<string, HashSet<string>>();

        static ReflectHelper()
        {
        }

        static public void RegistSearchPath(string path)
        {
            SearchPathList[path] = new HashSet<string>();
        }

        static public Assembly[] LoadAssemblyFromPath(string path) 
        {
            List<Assembly> ret = new List<Assembly>();

            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi = di.GetFiles("*.dll");
            foreach (FileInfo file in fi)
            {
                string assembly_file = file.FullName;

                var AssemblyList = new Dictionary<string, Assembly>();
                var asms = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in asms)
                {
                    if (!asm.IsDynamic)
                    {
                        AssemblyList[asm.Location] = asm;
                    }
                }

                if (!AssemblyList.ContainsKey(assembly_file))
                {
                    try
                    {
                        var TempAssembly = Assembly.LoadFile(assembly_file);
                        ret.Add(TempAssembly);
                    }
                    catch (BadImageFormatException)
                    {
                    }
                }
                else 
                {
                    ret.Add(AssemblyList[assembly_file]);
                }
            }

            return ret.ToArray();
        }


        #region Create Type

        static public Type CreateType(string name)
        {
            Type Result = null;

            if (!String.IsNullOrEmpty(name))
            {
                Result = Type.GetType(name);

                if (Result == null)
                {
                    Result = GetMemoryType(name);
                    if (Result == null)
                    {
                        Result = GetTypeFromSearchPath(name);
                    }
                    if (Result == null)
                    {
                        Result = GetSplitParadigmType(name);
                    }
                }
            }

            return Result;
        }

        static public Type CreateType(string genic_name, string para_name)
        {
            Type genic_type = CreateType(genic_name);
            Type para_type = CreateType(para_name);

            return CreateType(genic_type, para_type);
        }

        static public Type CreateType(Type genic_type, Type para_type)
        {
            Type type = null;

            if (genic_type.IsGenericType)
            {
                type = genic_type.MakeGenericType(new Type[1] { para_type });
            }

            return type;
        }

        #endregion

        #region Create Object

        static public T CreateObject<T>()
        {
            return (T)CreateObject(typeof(T));
        }

        static public object CreateObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        static public object CreateObject(string type_name)
        {
            Type type = CreateType(type_name);

            return Activator.CreateInstance(type);
        }

        static public object CreateObject(Type genic_type, Type para_type)
        {
            Type type = CreateType(genic_type, para_type);

            return Activator.CreateInstance(type);
        }

        static public object CreateObject(Type type, params object[] paras)
        {
            return Activator.CreateInstance(type, paras);
        }

        static public object CreateObject(string type_name, params object[] paras)
        {
            Type type = CreateType(type_name);

            return Activator.CreateInstance(type, paras);
        }

        static public object CreateObject(Type genic_type, Type para_type, params object[] paras)
        {
            Type type = CreateType(genic_type, para_type);

            return Activator.CreateInstance(type, paras);
        }
        #endregion

        #region Property

        static public FieldInfo GetField(Type type, string field_name)
        {
            FieldInfo ret = null;

            if (type != null)
            {
                ret = type.GetField(field_name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (ret == null)
                {
                    ret = GetField(type.BaseType, field_name);
                }
            }

            return ret;
        }

        static public PropertyInfo GetProperty(Type type, string field_name)
        {
            PropertyInfo ret = null;

            if (type != null)
            {
                ret = type.GetProperty(field_name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (ret == null)
                {
                    ret = GetProperty(type.BaseType, field_name);
                }
            }

            return ret;
        }

        static public object GetValue(object obj, string name)
        {
            object ret = null;
            Type type = obj.GetType();

            PropertyInfo Property = GetProperty(type, name);
            if (Property != null)
            {
                ret = Property.GetValue(obj, null);
            }
            else
            {
                FieldInfo Field = GetField(type, name);
                if (Field != null)
                {
                    ret = Field.GetValue(obj);
                }
            }

            return ret;
        }

        static public bool SetValue(object obj, string name, object value)
        {
            bool ret = SetPropertyValue(obj, name, value);
            if (!ret)
            {
                ret = SetFieldValue(obj, name, value);
            }

            return ret;
        }

        static public bool SetFieldValue(object obj, string name, object value)
        {
            bool ret = false;
            Type type = obj.GetType();

            if (!String.IsNullOrEmpty(name))
            {
                FieldInfo Field = type.GetField(name);
                if (Field != null)
                {
                    Type FieldType = Field.FieldType;
                    value = ConvertValue(FieldType, value);

                    Field.SetValue(obj, value);

                    ret = true;
                }
            }

            return ret;
        }

        static public bool SetPropertyValue(object obj, string name, object value)
        {
            bool ret = false;
            Type type = obj.GetType();

            if (!String.IsNullOrEmpty(name))
            {
                PropertyInfo Property = type.GetProperty(name);
                if (Property != null)
                {
                    if (Property.CanWrite)
                    {
                        Type FieldType = Property.PropertyType;
                        value = ConvertValue(FieldType, value);

                        Property.SetValue(obj, value, null);

                        ret = true;
                    }
                }
            }

            return ret;
        }

        #endregion

        #region Method

        static public object InvokeMethod(object obj, string MethodName, object[] Parameters)
        {
            Type objType = obj.GetType();

            MethodInfo ObjMethod = objType.GetMethod(MethodName);

            return ObjMethod.Invoke(obj, Parameters);
        }

        static public void InvokeEventMethod(object obj, string MethodName)
        {
            Type objType = obj.GetType();

            MethodInfo ObjMethod = objType.GetMethod(MethodName);

            if (ObjMethod != null)
            {
                ObjMethod.Invoke(obj, null);
            }
        }

        #endregion

        #region Interface

        static public bool IfImplementInterface(Type type, Type intf)
        {
            bool Result = false;

            foreach (Type i in type.GetInterfaces())
            {
                if (i == intf)
                {
                    Result = true;
                    break;
                }
            }

            return Result;
        }

        static public bool IsInterface(Type type)
        {
            return type.IsInterface;
        }

        static public void WalkThrowAssembly(AssemblyCallback callback)
        {
            foreach (string path in SearchPathList.Keys)
            {

            }
        }

        #endregion

        #region Attribute

        #endregion

        #region Private Members

        static private Type GetTypeFromAssembly(string name, string path)
        {
            Type TempType = null;
            AppDomain currentDomain = AppDomain.CurrentDomain;

            if (SearchPathList.ContainsKey(path))
            {
                HashSet<string> AssemblyInPath = SearchPathList[path];

                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] fi = di.GetFiles("*.dll");
                foreach (FileInfo file in fi)
                {
                    if (!AssemblyInPath.Contains(file.Name))
                    {
                        Assembly TempAssembly = null;
                        try
                        {
                            TempAssembly = Assembly.LoadFile(file.FullName);
                            AssemblyInPath.Add(file.Name);
                        }
                        catch (BadImageFormatException ex)
                        {
                            // TODO: if (Doctor != null) Doctor.Log(MessageType.Warning, ex.Message);
                        }

                        if (TempAssembly != null)
                        {
                            TempType = TempAssembly.GetType(name);
                        }

                        if (TempType != null) break;
                    }
                }
            }

            return TempType;
        }

        static private Type GetTypeFromSearchPath(string name)
        {
            Type Result = null;

            RefreshAssemlyLoadedState();

            foreach (string p in SearchPathList.Keys)
            {
                Result = GetTypeFromAssembly(name, p);

                if (Result != null) break;
            }

            return Result;
        }

        static private Type GetMemoryType(string name)
        {
            Type temp = null;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();

            foreach (Assembly assem in assems)
            {
                temp = assem.GetType(name);
                if (temp != null)
                {
                    break;
                }
            }

            return temp;
        }

        static private void RefreshAssemlyLoadedState()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] asms = currentDomain.GetAssemblies();

            foreach (Assembly asm in asms)
            {
                string Location = string.Empty;
                try
                {
                    Location = asm.Location;
                }
                catch
                {
                    continue;
                }

                string Path = Location.Substring(0, Location.LastIndexOf('\\') + 1);
                string BinName = Location.Substring(Path.Length);

                if (SearchPathList.ContainsKey(Path))
                {
                    SearchPathList[Path].Add(BinName);
                }
            }
        }

        static private Type GetSplitParadigmType(string name)
        {
            Type Result = null;
            string[] StrList = name.Split('[');
            if (StrList.Length > 1)
            {
                string Paradigm = StrList[0];
                string ObjType = StrList[1].Substring(0, StrList[1].Length - 1);
                Result = CreateType(Paradigm, ObjType);
            }

            return Result;
        }

        static private object ConvertValue(Type type, object value)
        {
            object Result = value;

            if (value != null)
            {
                if (type != typeof(string) && value.GetType() == typeof(string))
                {
                    string valueStr = value.ToString();

                    if (!String.IsNullOrEmpty(valueStr))
                    {
                        if (type == typeof(int))
                        {
                            Result = Convert.ToInt32(value);
                        }
                        else if (type == typeof(decimal))
                        {
                            Result = Convert.ToDecimal(value);
                        }
                        else if (type == typeof(float))
                        {
                            Result = Convert.ToSingle(value);
                        }
                        else if (type == typeof(double))
                        {
                            Result = Convert.ToDouble(value);
                        }
                        else if (type == typeof(DateTime))
                        {
                            Result = Convert.ToDateTime(value);
                        }
                    }
                }
            }

            return Result;
        }

        #endregion
    }

}
