using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    public class SChannel: AChannel
    {
        public enum InternalMessages: byte
        {
            CONNECT,
            ACCEPT_CONNECT,
            DISCONNECT
        }

        private SService Service;
        public SteamId targetSteamId;

        private readonly Queue<byte[]> queue = new Queue<byte[]>();
        private long connectionTimeout = 25 * 1000;
        private readonly MemoryStream recvStream;
        private bool isSending;
        private bool isConnected;
        private long timer;
        private ETTask<P2Packet> recvTask;
        private ETTask<P2Packet> sendTask;

        public event Action<SteamId> OnClientConnectToServer;

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            Log.Info($"channel dispose: {this.Id} {this.targetSteamId}");

            this.SendInternal(this.targetSteamId, InternalMessages.DISCONNECT);

            this.Id = 0;
            this.isSending = false;
            this.isConnected = false;
            this.targetSteamId = 0;
        }

        /// <summary>
        /// client
        /// </summary>
        public SChannel(long id, SteamId hostSteamID, SService service, ChannelType channelType = ChannelType.Connect)
        {
            this.Id = id;
            this.ChannelType = channelType;
            this.targetSteamId = hostSteamID;
            this.Service = service;
            this.recvStream = new MemoryStream(ushort.MaxValue);

            this.isConnected = false;
            this.isSending = false;

            SteamNetworking.OnP2PConnectionFailed += OnConnectFail;
            this.Service.ThreadSynchronizationContext.PostNext(this.ConnectAsync().Coroutine);
        }

        /// <summary>
        /// server
        /// </summary>
        public SChannel(long id, SteamId clientSteamID, SService sService)
        {
            this.Id = id;
            this.targetSteamId = clientSteamID;
            this.Service = sService;

            this.ChannelType = ChannelType.Accept;
            this.recvStream = new MemoryStream(ushort.MaxValue);

            this.isConnected = true;
            this.isSending = false;

            SteamNetworking.OnP2PConnectionFailed += OnConnectFail;
            //给客户端回复
            this.SendInternal(clientSteamID, InternalMessages.ACCEPT_CONNECT);

            this.Service.ThreadSynchronizationContext.PostNext(() =>
            {
                this.StartRecv().Coroutine();
                this.StartSend();
            });
        }

        public void Update()
        {
            P2Packet data;
            if (!this.isConnected && this.sendTask != null)
            {
                var now = TimeHelper.ClientNow();
                if (now - this.timer > connectionTimeout)
                {
                    this.timer = 0;
                    this.OnError(ErrorCore.ERR_SteamConnectTimeout);
                    return;
                }

                if (SteamUtility.CanReceive(out data, this.Service.internal_channel))
                {
                    var task = this.sendTask;
                    this.sendTask = null;
                    task.SetResult(data);
                }

                return;
            }

            if (SteamUtility.CanReceive(out data, this.Service.internal_channel))
            {
                OnConnectAccepted(data);
            }

            if (this.recvTask != null)
            {
                CheckData();
            }
        }

        private void OnConnectFail(SteamId steamID, P2PSessionError err)
        {
            this.Service.ThreadSynchronizationContext.Post(() =>
            {
                Log.Info(err.ToString());
                if (this.IsDisposed)
                    return;
                try
                {
                    OnConnectionFailed(steamID);
                    // CloseP2PSessionWithUser(steamID);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    throw;
                }
            });
        }

        private void OnConnectionFailed(SteamId remoteId)
        {
            this.OnError(ErrorCore.ERR_SteamSendError);
        }

        private void CloseP2PSessionWithUser(SteamId clientSteamID)
        {
            SteamNetworking.CloseP2PSessionWithUser(clientSteamID);
        }

        private async ETTask ConnectAsync()
        {
            try
            {
                this.timer = TimeHelper.ClientNow();
                var packet = await SendFirstDataAsync();

                OnConnectAccepted(packet);
                if (this.isConnected)
                {
                    this.StartRecv().Coroutine();
                    this.StartSend();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                this.OnError(ErrorCore.ERR_SteamConnectError);
            }
        }

        private ETTask<P2Packet> SendFirstDataAsync()
        {
            sendTask = ETTask<P2Packet>.Create(true);
            var hostSteamID = this.targetSteamId;
            this.SendInternal(hostSteamID, InternalMessages.CONNECT);
            return this.sendTask;
        }

        private async ETTask StartRecv()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                while (true)
                {
                    var packet = await ReceiveDataAsync();
                    var data = packet.Data;
                    int receiveCount = data.Length;
                    this.recvStream.SetLength(receiveCount);
                    this.recvStream.Seek(2, SeekOrigin.Begin);
                    Array.Copy(data, 0, this.recvStream.GetBuffer(), 0, receiveCount);
                    this.OnRead(this.recvStream);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(ErrorCore.ERR_SteamRecvError);
            }
        }

        private ETTask<P2Packet> ReceiveDataAsync()
        {
            if (this.recvTask != null)
                return this.recvTask;
            recvTask = ETTask<P2Packet>.Create(true);
            return this.recvTask;
        }

        private void CheckData()
        {
            for (int chNum = 0; chNum < this.Service.channels.Length; chNum++)
            {
                while (SteamUtility.CanReceive(out P2Packet packet, chNum))
                {
                    var clientSteamID = packet.SteamId;

                    if (clientSteamID != this.targetSteamId)
                    {
                        Log.Error($"steamId: {this.targetSteamId} senderSteamId: {clientSteamID}");
                        this.OnError(ErrorCore.ERR_SteamRececiveDataError);
                    }

                    var task = this.recvTask;
                    this.recvTask = null;
                    task.SetResult(packet);
                }
            }
        }

        /// <summary>
        /// 客户端处理
        /// </summary>
        /// <param name="packet"></param>
        private void OnConnectAccepted(P2Packet packet)
        {
            InternalMessages type = (InternalMessages) packet.Data[0];
            SteamId clientSteamID = packet.SteamId;
            switch (type)
            {
                case InternalMessages.ACCEPT_CONNECT:
                    this.isConnected = true;
                    OnClientConnectToServer?.Invoke(packet.SteamId);
                    Log.Info($"{SteamClient.Name} connect to {clientSteamID}  ACCEPT_CONNECT");
                    break;
                case InternalMessages.DISCONNECT:
                    this.isConnected = false;
                    this.OnError(ErrorCore.ERR_SteamDisconnectByServer);
                    Log.Info($"Client with SteamID {clientSteamID} closed.");
                    break;
                default:
                    Log.Info("Received unknown message type");
                    break;
            }
        }

        private void OnRead(MemoryStream memoryStream)
        {
            try
            {
                long channelId = this.Id;
                this.Service.OnRead(channelId, memoryStream);
            }
            catch (Exception e)
            {
                Log.Error($"{this.targetSteamId} {memoryStream.Length} {e}");
                // 出现任何消息解析异常都要断开Session，防止客户端伪造消息
                this.OnError(ErrorCore.ERR_PacketParserError);
            }
        }

        public void Send(long actorId, MemoryStream stream)
        {
            switch (this.Service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    throw new Exception("steam 无法发送内网消息");
                    break;
                }
                case ServiceType.Outer:
                {
                    byte[] bytes = new byte[stream.Length];
                    Array.Copy(stream.GetBuffer(), bytes, bytes.Length);
                    this.queue.Enqueue(bytes);

                    if (this.isConnected)
                    {
                        this.StartSend();
                    }
                }
                    break;
            }
        }

        private void StartSend()
        {
            if (this.IsDisposed)
            {
                return;
            }

            try
            {
                if (this.isSending)
                {
                    return;
                }

                this.isSending = true;

                while (true)
                {
                    if (this.queue.Count == 0)
                    {
                        this.isSending = false;
                        return;
                    }

                    byte[] bytes = this.queue.Dequeue();
                    try
                    {
                        var ret = Send(this.targetSteamId, bytes, bytes.Length, 1);
                        if (!ret)
                        {
                            Log.Error("send data fail");
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        this.OnError(ErrorCore.ERR_SteamSendError);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 默认channel = 0 , P2PSendType = Reliable
        /// </summary>
        /// <param name="host"></param>
        /// <param name="msgBuffer"></param>
        /// <param name="length"></param>
        /// <param name="channel"></param>
        private bool Send(SteamId host, byte[] msgBuffer, int length, int channel = 0)
        {
            var p2PSends = this.Service.channels;
            return SteamNetworking.SendP2PPacket(host, msgBuffer, length, channel, p2PSends[Math.Min(channel, p2PSends.Length - 1)]);
        }

        public bool SendInternal(SteamId target, InternalMessages type)
        {
            return SteamNetworking.SendP2PPacket(target, new byte[] { (byte) type }, 1, this.Service.internal_channel);
        }

        private void OnError(int error)
        {
            Log.Info($"SChannel OnError: {error} {this.targetSteamId}");

            long channelId = this.Id;

            this.Service.OnError(channelId, error);
        }

        public void RegisterOnConnectEvent(Action<SteamId> onConnectEvent)
        {
            this.OnClientConnectToServer -= onConnectEvent;
            this.OnClientConnectToServer += onConnectEvent;
        }
    }
}