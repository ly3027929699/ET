using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    public class SChannel: AChannel
    {
        public class DataInfo
        {
            public readonly byte[] bytes;
            public SteamId steamId;

            public DataInfo()
            {
                bytes = new byte[1200];
                steamId = 0;
            }
        }

        public struct MessageInfo
        {
            public uint messageSize;
            public DataInfo messageInfo;
            public MessageInfo(uint messageSize)
            {
                this.messageSize = messageSize;
                this.messageInfo = MonoPool.Instance.Fetch(typeof (DataInfo)) as DataInfo;
            }
            public void Recyle()
            {
                if (this.messageInfo == null)
                    throw new Exception("messageInfo is null");
                MonoPool.Instance.Recycle(this.messageInfo);
            }
        }

        public enum InternalMessages: byte
        {
            CONNECT,
            ACCEPT_CONNECT,
            DISCONNECT
        }

        private const long ConnectionTimeout = 25 * 1000;

        private SService Service;
        public SteamId targetSteamId;

        private readonly Queue<MessageInfo> queue = new Queue<MessageInfo>();
        private readonly MemoryStream recvStream = new MemoryStream(ushort.MaxValue);
        private bool isSending;
        private bool isConnected;
        private long timer;
        private ETTask<MessageInfo> recvTask;
        private ETTask<MessageInfo> sendTask;
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
            MessageInfo messageInfo;
            if (!this.isConnected && this.sendTask != null)
            {
                var now = TimeHelper.ClientNow();
                if (now - this.timer > ConnectionTimeout)
                {
                    this.timer = 0;
                    this.OnError(ErrorCore.ERR_SteamConnectTimeout);
                    return;
                }

                if (SteamUtility.CanReceive(out messageInfo, this.Service.internal_channel))
                {
                    var task = this.sendTask;
                    this.sendTask = null;
                    task.SetResult(messageInfo);
                }

                return;
            }

            if (SteamUtility.CanReceive(out messageInfo, this.Service.internal_channel))
            {
                OnConnectAccepted(messageInfo);
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
                    CloseP2PSessionWithUser(steamID);
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
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                this.OnError(ErrorCore.ERR_SteamConnectError);
            }
        }

        private ETTask<MessageInfo> SendFirstDataAsync()
        {
            sendTask = ETTask<MessageInfo>.Create(true);
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
                    var messageInfo = await ReceiveDataAsync();
                    var data = messageInfo.messageInfo.bytes;
                    uint receiveCount = messageInfo.messageSize;
                    this.recvStream.SetLength(receiveCount);
                    this.recvStream.Seek(2, SeekOrigin.Begin);
                    Array.Copy(data, 0, this.recvStream.GetBuffer(), 0, receiveCount);
                    messageInfo.Recyle();
                    this.OnRead(this.recvStream);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                this.OnError(ErrorCore.ERR_SteamRecvError);
            }
        }

        private ETTask<MessageInfo> ReceiveDataAsync()
        {
            if (this.recvTask != null)
                return this.recvTask;
            recvTask = ETTask<MessageInfo>.Create(true);
            return this.recvTask;
        }

        private void CheckData()
        {
            while (SteamUtility.CanReceive(out MessageInfo messageInfo, 0))
            {
                var clientSteamID = messageInfo.messageInfo.steamId;

                if (clientSteamID != this.targetSteamId)
                {
                    Log.Error($"steamId: {this.targetSteamId} senderSteamId: {clientSteamID}");
                    this.OnError(ErrorCore.ERR_SteamRececiveDataError);
                }

                var task = this.recvTask;
                this.recvTask = null;
                task.SetResult(messageInfo);
            }
        }

        /// <summary>
        /// 客户端处理
        /// </summary>
        /// <param name="packet"></param>
        private void OnConnectAccepted(MessageInfo packet)
        {
            InternalMessages type = (InternalMessages) packet.messageInfo.bytes[0];
            SteamId clientSteamID = packet.messageInfo.steamId;
            Log.Info($"{type}");
            switch (type)
            {
                case InternalMessages.ACCEPT_CONNECT:
                    this.isConnected = true;
                    this.StartRecv().Coroutine();
                    this.StartSend();
                    OnClientConnectToServer?.Invoke(clientSteamID);
                    Log.Info($"{SteamClient.Name} Connect to {clientSteamID} ACCEPT_CONNECT");
                    break;
                case InternalMessages.DISCONNECT:
                    if (!this.isConnected)
                    {
                        //avoid to recive last disconnect message
                        return;
                    }

                    this.isConnected = false;
                    this.OnError(ErrorCore.ERR_SteamDisconnectByServer);
                    Log.Info($"Client with SteamID {clientSteamID} Closed.");
                    break;
                default:
                    Log.Info($"Received unknown message type:{type}");
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
                }
                case ServiceType.Outer:
                {
                    uint messageSize = (uint) (stream.Length - stream.Position);
                    MessageInfo messageInfo = new MessageInfo(messageSize);
                    // byte[] bytes = new byte[messageSize];
                    Array.Copy(stream.GetBuffer(), stream.Position, messageInfo.messageInfo.bytes, 0, messageSize);
                    this.queue.Enqueue(messageInfo);

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

                    MessageInfo sendInfo = this.queue.Dequeue();
                    try
                    {
                        var ret = Send(this.targetSteamId, sendInfo.messageInfo.bytes, sendInfo.messageSize, P2PSend.Unreliable);
                        if (!ret)
                        {
                            Log.Error("send data fail");
                        }

                        sendInfo.Recyle();
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
        /// <param name="sendType"></param>
        private unsafe bool Send(SteamId host, byte[] msgBuffer, uint length, P2PSend sendType)
        {
            fixed (byte* p = msgBuffer)
            {
                return SteamNetworking.SendP2PPacket(host, p, length, 0, sendType);
            }
        }

        public unsafe void SendInternal(SteamId target, InternalMessages type)
        {
            uint messageSize = 1;
            MessageInfo messageInfo = new MessageInfo(1);
            messageInfo.messageInfo.bytes[0] = (byte) type;
            fixed (byte* p = messageInfo.messageInfo.bytes)
            {
                var ret= SteamNetworking.SendP2PPacket(target,p, messageSize, this.Service.internal_channel);
                
                if (ret)
                {
                    messageInfo.Recyle();
                }
            }
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