using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoulFab.Core.Communication
{
    public class MessageEnity<T>
    {
        public T Code { get; set; }
        public byte[] Data { get; set; }
    }

    public interface IChannel
    {
        void Send(byte[] data);
    }

    public interface IAsyncChannel
    {
        Task Send(byte[] data);
    }

    public interface IFrameChannel<T> : IChannel
    {
        void Send(T cmd, byte[] payload);
    }

    public interface IAsyncFrameChannel<T> : IAsyncChannel
    {
        Task Send(T cmd, byte[] payload);
    }

    public interface IFrameChannelCallback
    {
        void OnConnected();
    }

    public interface IMessageBuffer
    {
    }

    public interface IFrameCodec<T>
    {
        bool Decode(StreamBuffer stream, ref MessageEnity<T> entity);
        byte[] Encode(T cmd, byte[] data);

        byte[] EncodeHeartbeat();
    }

    public interface IMessageCodec<T>
    {
        object Decode(T cmd, byte[] buf);
        byte[] Encode(T cmd, object obj);
    }

    public interface IMessageBuilder
    {
        int Size { get; }
        byte[] ReadAll();
    }

    public interface IMessageParser
    {

    }

    public interface IChannelCallback<T>
    {
    }

    public interface IFrameChannelCallback<T>
    {
        void OnConnected(IChannel channel);
        void OnData(IChannel channel, T cmd, byte[] data);
        void OnError(Exception ex);
    }
}
