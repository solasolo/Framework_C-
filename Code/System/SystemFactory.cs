using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace SoulFab.Core.System
{
    public interface ISystem
    {
        public string RootPath { get; }
        public bool InConsole { get; }

        public T Get<T>() where T: class;
    }

    public interface IService
    {
        public void Start();
        public void Stop();
    }

    public abstract class SystemFactory : ISystem
    {
        protected string _RootPath;
        private Dictionary<string, object> GlobalObjects;
        private Dictionary<Type, object> TypedObjects;

        public SystemFactory()
        {
            _RootPath = AppDomain.CurrentDomain.BaseDirectory;

            this.GlobalObjects = new Dictionary<string, object>();
            this.TypedObjects = new Dictionary<Type, object>();
        }

        public virtual bool InConsole { get; }

        public string RootPath
        {
            get
            {
                return this._RootPath;
            }
        }

        public T Get<T>() where T : class
        {
            T ret = null;

            Type t = typeof(T);
            if (this.TypedObjects.ContainsKey(t))
            {
                ret = (T)this.TypedObjects[t];
            }

            return ret;
        }

        public void Set<T>(T obj)
        {
            this.TypedObjects[typeof(T)] = obj;
        }

        protected T Get<T>(string key)
        {
            return (T)this.GlobalObjects[key];
        }

        protected void Set(string key, object obj)
        {
            this.GlobalObjects[key] = obj;
        }

        public virtual void Start() { }
        public virtual void Stop() { }
        protected abstract void CreateWorld();
    }

    public static class ServiceCollectionExtension
    {
        static public T GetSingleton<T>(this IServiceCollection services)
        {
            T ret = default(T);

            foreach (var svc in services)
            {
                if (svc.ServiceType == typeof(T) && svc.Lifetime == ServiceLifetime.Singleton)
                {
                    ret = (T)svc.ImplementationInstance;
                }
            }

            return ret;
        }
    }
}
