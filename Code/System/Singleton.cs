using System;
using System.Collections.Generic;
using System.Text;
using SoulFab.Core.Helper;

namespace SoulFab.Core.System
{
    public class Singleton<T> where T: class
    {
        static private T _Instance;
        static private readonly object InstanceLock = new object();


        static public T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = ReflectHelper.CreateObject<T>();
                        }
                    }
                }

                return _Instance;
            }
        }
    }
}
