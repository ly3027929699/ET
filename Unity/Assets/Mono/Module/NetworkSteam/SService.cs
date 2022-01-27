using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    public sealed class SService: AService
    {
        public P2PSend[] channels = new P2PSend[] { P2PSend.Reliable, P2PSend.UnreliableNoDelay };
        public int internal_channel => channels.Length;
        private readonly Dictionary<long, SChannel> idChannels = new Dictionary<long, SChannel>();
        private readonly Dictionary<ulong, long> steamIdToChannelId = new Dictionary<ulong, long>();
        private string SteamAppID = "480";
        private bool AllowSteamRelay = true;
        private int maxConnect;

        public SService(ThreadSynchronizationContext threadSynchronizationContext, ServiceType serviceType, int maxConnect)
        {
            this.ServiceType = serviceType;
            this.ThreadSynchronizationContext = threadSynchronizationContext;
            this.maxConnect = maxConnect;

            InitSteamConfig();
            StartAccept().Coroutine();
        }

        public SService(ThreadSynchronizationContext threadSynchronizationContext, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.ThreadSynchronizationContext = threadSynchronizationContext;

            InitSteamConfig();
            SteamNetworking.OnP2PSessionRequest += OnNewConnection;
            SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);
        }

        public override void Update()
        {
            if (this.task != null && SteamUtility.CanReceive(out P2Packet data, this.internal_channel))
            {
                var task = this.task;
                this.task = null;
                task.SetResult(data);
            }

            foreach (var keyValuePair in this.idChannels)
            {
                keyValuePair.Value?.Update();
            }
        }

        private async ETTask StartAccept()
        {
            try
            {
                if (!SteamClient.IsValid)
                {
                    Log.Error("SteamWorks not initialized. Server could not be started.");
                    return;
                }

                SteamNetworking.AllowP2PPacketRelay(AllowSteamRelay);

                SteamNetworking.OnP2PSessionRequest += OnNewConnection;

                if (!SteamClient.IsValid)
                {
                    Log.Error("SteamWorks not initialized.");
                }

                while (true)
                {
                    try
                    {
                        var packet = await AcceptConnectAsync();
                        OnReceiveConnectData(packet);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private ETTask<P2Packet> task;

        private ETTask<P2Packet> AcceptConnectAsync()
        {
            if (this.task != null)
                return this.task;
            task = ETTask<P2Packet>.Create(true);
            return this.task;
        }

        private void OnNewConnection(SteamId steamID)
        {
            Log.Info($"Incoming request from SteamId {steamID}.");
            SteamNetworking.AcceptP2PSessionWithUser(steamID);
        }

        private void CloseP2PSessionWithUser(SteamId clientSteamID)
        {
            SteamNetworking.CloseP2PSessionWithUser(clientSteamID);
        }

        private void InitSteamConfig()
        {
            const string fileName = "steam_appid.txt";
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                if (content != SteamAppID)
                {
                    File.WriteAllText(fileName, SteamAppID.ToString());
                    Log.Info($"Updating {fileName}. Previous: {content}, new SteamAppID {SteamAppID}");
                }
            }
            else
            {
                File.WriteAllText(fileName, SteamAppID.ToString());
                Log.Info($"New {fileName} written with SteamAppID {SteamAppID}");
            }

            SteamClient.Init(uint.Parse(SteamAppID), true);
        }

        /// <summary>
        /// 服务器处理
        /// </summary>
        /// <param name="packet"></param>
        private void OnReceiveConnectData(P2Packet packet)
        {
            try
            {
                SChannel.InternalMessages type = (SChannel.InternalMessages) packet.Data[0];
                SteamId clientSteamID = packet.SteamId;

                switch (type)
                {
                    case SChannel.InternalMessages.CONNECT:
                        this.CreateChannelToClient(clientSteamID);
                        Log.Info($"Client with SteamID {clientSteamID} connected");
                        break;
                    case SChannel.InternalMessages.DISCONNECT:
                        if (!steamIdToChannelId.TryGetValue(clientSteamID, out long channelId))
                        {
                            Log.Warning($"Conn't find channel which steamId is {clientSteamID}");
                            return;
                        }
                        this.OnError(channelId,ErrorCore.ERR_SteamDisconnectByClient);
                        Log.Info($"Client with SteamID {clientSteamID} disconnected.");
                        break;
                    default:
                        Log.Info("Received unknown message type");
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void CreateChannelToClient(SteamId clientSteamID)
        {
            if (this.idChannels.Count >= this.maxConnect)
                throw new Exception($"connect is max : {this.maxConnect}");
            long id = this.CreateAcceptChannelId(0);
            SChannel channel = new SChannel(id, clientSteamID, this);
            this.idChannels.Add(channel.Id, channel);
            this.steamIdToChannelId.Add(clientSteamID, channel.Id);
            this.OnAccept(channel.Id, channel.targetSteamId.ToString());
        }

        private void CreateChannelToServer(long id, SteamId steamId)
        {
            SChannel channel = new SChannel(id, steamId, this, ChannelType.Connect);
            this.idChannels.Add(channel.Id, channel);
            this.steamIdToChannelId.Add(steamId, channel.Id);
            channel.RegisterOnConnectEvent(CodeLoader.Instance.OnConnectEvent);
        }

        public override void Remove(long id)
        {
            SChannel sChannel = this.Get(id);
            if (sChannel == null)
            {
                Log.Warning($"schannel is null where id is {id} and count {this.idChannels.Count}");
                return;
            }
            SteamId clientSteamID = sChannel.targetSteamId;
            this.steamIdToChannelId.Remove(clientSteamID);
            this.idChannels.Remove(id);
            sChannel.Dispose();
        }

        public override bool IsDispose()
        {
            return this.ThreadSynchronizationContext == null;
        }

        protected override void Get(long id, string address)
        {
            if (this.idChannels.TryGetValue(id, out _))
            {
                return;
            }

            this.CreateChannelToServer(id, ulong.Parse(address));
        }

        public override void Dispose()
        {
            ThreadSynchronizationContext = null;

            foreach (long id in this.idChannels.Keys.ToArray())
            {
                SChannel channel = this.idChannels[id];
                channel.Dispose();
            }

            this.idChannels.Clear();
            this.steamIdToChannelId.Clear();
        }

        protected override void Send(long channelId, long actorId, MemoryStream stream)
        {
            try
            {
                SChannel channel = Get(channelId);
                if (channel == null)
                {
                    this.OnError(channelId, ErrorCore.ERR_SendMessageNotFoundSChannel);
                    return;
                }

                channel.Send(actorId, stream);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private SChannel Get(long channelId)
        {
            SChannel channel = null;
            this.idChannels.TryGetValue(channelId, out channel);
            return channel;
        }
    }
}