using ENet;
using NetStack.Buffers;
using System;

namespace NetLib
{
    public abstract class NetSocketBase
    {
        protected Host host;
        private ArrayPool<byte> bytes;


     

        
        public NetSocketBase(int maxArrayLength, int maxArraysPerBucket)
        {
           

            bytes = ArrayPool<byte>.Create(maxArrayLength, maxArraysPerBucket);
            NetBuffer.Init();
            ENet.Library.Initialize();
            host = new Host();
        }

#if SERVER
        public void InitializeServer(string ip, ushort port, int maxConnections)
        {
            Address address = ParseIPAndAddress(ip, port);
            host.Create(address, maxConnections, 32);
        }
#elif CLIENT
        public void InitializeClient()
        {
            host.Create(1, 32);
        }
#endif

        private Address ParseIPAndAddress(string ip, ushort port)
        {
            Address address = new Address();
            address.SetHost(ip);
            address.Port = port;
            return address;
        }


        public void Connect(string ip, ushort port)
        {
            Address address = ParseIPAndAddress(ip, port);
            host.Connect(address);
        }
#if SERVER
        public void SendMessage(Peer target, ref NetBufferData buffer, PacketFlags flags)
#elif CLIENT
        public void SendMessage(ref NetBufferData buffer, PacketFlags flags)
#endif
        {
            var packet = default(Packet);
            var data = bytes.Rent(NetBuffer.GetLength(ref buffer));
            NetBuffer.ToArray(ref buffer, data);


            packet.Create(data, flags);
#if SERVER
            target.Send(0, ref packet);
#elif CLIENT
            host.Broadcast(0, ref packet);
#endif
            bytes.Return(data);
        }


        public void Broadcast(ref NetBufferData buffer, PacketFlags flags, Peer excludedPeer)
        {
            var packet = default(Packet);
            var data = bytes.Rent(NetBuffer.GetLength(ref buffer));
            NetBuffer.ToArray(ref buffer, data);


            packet.Create(data, flags);
            host.Broadcast(0, ref packet, excludedPeer);
            bytes.Return(data, true); 
        }

        public void Broadcast(ref NetBufferData buffer, PacketFlags flags, Peer[] peers)
        {
            var packet = default(Packet);
            var data = bytes.Rent(NetBuffer.GetLength(ref buffer));
            NetBuffer.ToArray(ref buffer, data);


            packet.Create(data, flags);
            host.Broadcast(0, ref packet, peers);
            bytes.Return(data, true);
        }

        public void Disconnect(Peer peer, uint reason)
        {
            peer.DisconnectNow(reason);
        }

        public void Broadcast(ref NetBufferData buffer, PacketFlags flags)
        {
            var packet = default(Packet);
            var data = bytes.Rent(NetBuffer.GetLength(ref buffer));
            NetBuffer.ToArray(ref buffer, data);


            packet.Create(data, flags);
            host.Broadcast(0, ref packet);
            bytes.Return(data, true);
        }
        public void Tick()
        {
            ENet.Event netEvent = default;
            host.Service(0, out netEvent);
            HandleNetEvent(ref netEvent);
            
        }


        ~NetSocketBase()
        {
            host.Flush();
            host.Dispose();
            ENet.Library.Deinitialize();
        }
        private void HandleNetEvent(ref ENet.Event netEvent)
        {
            switch(netEvent.Type)
            {
                case EventType.Connect:

                    OnSocketConnect(netEvent.Peer);
                    break;

                case EventType.Disconnect:
                    OnSocketDisconnect(netEvent.Peer);
                    break;

                case EventType.Timeout:
                    OnSocketTimeout(netEvent.Peer);
                    break;

                case EventType.Receive:

                     var data = bytes.Rent(netEvent.Packet.Length);
                    netEvent.Packet.CopyTo(data);
                    var netBuffer = NetBuffer.FromArray(data, data.Length);
                    var id = NetBuffer.ReadUShort(ref netBuffer);
                    OnSocketReceived(netEvent.Peer, id, ref netBuffer);
                    bytes.Return(data, true);
                    NetBuffer.Destroy(ref netBuffer);
                    break;
            }
        }

        public abstract void OnSocketConnect(Peer peer);
        public abstract void OnSocketDisconnect(Peer peer);
        public abstract void OnSocketTimeout(Peer peer);
        public abstract void OnSocketReceived(Peer peer, ushort messageID, ref NetBufferData data);
    }
}