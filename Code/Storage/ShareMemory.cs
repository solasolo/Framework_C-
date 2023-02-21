using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Reflection;
using SoulFab.Core.Communication;

namespace SoulFab.Core.Storage
{
    public class ShareMemory
    {
        protected MemoryMappedFile Memory;
        private MemoryMappedViewAccessor Accessor;

        protected long Size;

        public ShareMemory()
        {
            this.Size = 0;
            this.Memory = null;
        }

        public ShareMemory(string file, string name, long size)
        {
            this.Memory = null;

            this.Init(file, name, size);
            this.Accessor = this.Memory.CreateViewAccessor(0, this.Size, MemoryMappedFileAccess.ReadWrite);
        }

        public ShareMemory(string name, long size)
        {

        }

        public void Init(string file, string name, long size)
        {
            this.Size = size;

            if (Memory != null)
            {
                this.Memory.Dispose();
            }

            this.Memory = MemoryMappedFile.CreateFromFile(file, FileMode.OpenOrCreate, name, size);
        }

        public void Write(int pos, byte[] data, int start, int len)
        {
            this.Accessor.WriteArray<byte>(pos, data, start, len);
        }

        public byte[] Read(int pos, int start, int len)
        {
            byte[] ret = new byte[this.Size];

            this.Accessor.ReadArray<byte>(pos, ret, start, len);

            return ret;
        }
    }

    public class ShareObject<T> : ShareMemory
            where T : struct
    {
        static protected int StructSize()
        {
            return Marshal.SizeOf(typeof(T));
        }

        public ShareObject(string file, string name)
            : base(file, name, StructSize())
        {
        }

        public ShareObject(string name)
            : base(name, StructSize())
        {
        }

        public void Write(T obj)
        {
            byte[] bytes = new byte[this.Size];

            IntPtr buf = Marshal.AllocHGlobal((int)this.Size);
            try
            {
                Marshal.StructureToPtr(obj, buf, false);
                Marshal.Copy(buf, bytes, 0, (int)this.Size);
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
                buf = IntPtr.Zero;
            }

            this.Write(0, bytes, 0, (int)this.Size);
        }

        public T Read()
        {
            T ret = default(T);

            var bytes = this.Read(0, 0, (int)this.Size);

            IntPtr buf = Marshal.AllocHGlobal((int)this.Size);
            try
            {
                Marshal.Copy(bytes, 0, buf, (int)this.Size);
                ret = Marshal.PtrToStructure<T>(buf);
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
                buf = IntPtr.Zero;
            }

            return ret;
        }
    }

    public class ProxyShareObject<T> : DispatchProxy
    {
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {

            throw new NotImplementedException();
        }
    }

    public class BinaryShareObject : ShareMemory
    {
        MemorySerdes Serdes;
        int Size;

        public BinaryShareObject(string file, string name, TelegramScheme scheme)
            : base(file, name, scheme.TotalSize)
        {
            this.Serdes = new MemorySerdes(scheme);
            this.Size = scheme.TotalSize;
        }

        public void Save(object obj)
        {
            var buf = this.Serdes.Serialize(obj);
            this.Write(0, buf, 0, this.Size);
        }

        public void Load(ref object obj)
        {
            var buf = this.Read(0, 0, this.Size);
            this.Serdes.Deserialize(buf, ref obj);
        }

        public string ToJson(object obj)
        {
            return this.Serdes.ToJSON(obj);
        }
    }
}

