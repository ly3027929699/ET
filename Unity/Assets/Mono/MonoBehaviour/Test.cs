// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Text;
// using ET;
// using Steamworks;
// using Steamworks.Data;
// using UnityEngine;
// using Debug = UnityEngine.Debug;
//
// public class Test : MonoBehaviour
// {
//     enum InternalMessage
//     {
//         CONNECT,
//         CONNECT_ACCEPT,
//         DISCONNECT
//     }
//
//    public enum SteamType
//     {
//        None, Host,Client
//     }
//     public ulong steamId;
//     private int internalChannel = 2;
//     public SteamType steamType;
//     public bool isConnected;
//     private Stopwatch stopwatch;
//
//     void Start()
//     {
//         SteamClient.Init(480);
//         // SteamNetworking.OnP2PSessionRequest += OnNewConnection;
//     }
//
//     private void OnNewConnection(SteamId obj)
//     {
//         Debug.Log($"{obj} request");
//         SteamNetworking.AcceptP2PSessionWithUser(obj);
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         SteamUtility.ReceiveInternalData(OnReciveInternalData,this.internalChannel);
//         if (IsKey(KeyCode.H))
//         {
//             if (steamType != SteamType.None) return;
//             SteamNetworking.AllowP2PPacketRelay(true);
//             steamType = SteamType.Host;
//             Debug.Log("create host");
//         }   
//         if (IsKey(KeyCode.C))
//         {
//             if (steamType != SteamType.None) return;
//             SteamNetworking.AllowP2PPacketRelay(true);
//             stopwatch = Stopwatch.StartNew();
//             steamType = SteamType.Client;
//             SendInternal(steamId, new byte[] { (byte) InternalMessage.CONNECT });
//             Debug.Log("create client");
//         }
//
//         if (IsKey(KeyCode.M))
//         {
//             Send(steamId, Encoding.Default.GetBytes(DateTime.Now.ToString()));
//         }
//
//         ReceiveData();
//     }
//     private void ReceiveData()
//     {
//         try
//         {
//             while (SteamUtility.CanReceive(out P2Packet packet, 0))
//             {
//                 OnReceiveData(packet.Data, packet.SteamId, 0);
//             }
//         }
//         catch (Exception e)
//         {
//             Log.Error(e);
//         }
//     }
//
//     private void OnReceiveData(byte[] data, SteamId clientSteamID, int i)
//     { 
//         Log.Info($"get data: {clientSteamID} {Encoding.Default.GetString(data)}");
//     }
//
//     private bool SendInternal(SteamId steamId,byte[] bytes)
//     {
//        return SteamNetworking.SendP2PPacket(steamId,bytes, 1, internalChannel);
//     }
//     private bool Send(SteamId steamId,byte[] bytes)
//     {
//         return SteamNetworking.SendP2PPacket(steamId,bytes, bytes.Length, 0);
//     }
//     private void OnReciveInternalData(P2Packet packet)
//     {
//         try
//         {
//             InternalMessage type=(InternalMessage) packet.Data[0];
//             SteamId clientSteamID = packet.SteamId;
//                 
//             Log.Info($"service:{type} {Encoding.Default.GetString(packet.Data)}");
//          
//             switch (type)
//             {
//                 case InternalMessage.CONNECT:
//                     SendInternal(clientSteamID, new byte[] { (byte) InternalMessage.CONNECT_ACCEPT });
//                     steamId = clientSteamID;
//                     isConnected = true;
//                     Log.Info($"Client with SteamID {clientSteamID} connected");
//                     break;
//                 case InternalMessage.CONNECT_ACCEPT:
//                     isConnected = true;
//                     stopwatch.Stop();
//                     Debug.Log(this.stopwatch.ElapsedMilliseconds);
//                     Log.Info($"Client with SteamID {clientSteamID} connect accepted");
//                     break;
//                 case InternalMessage.DISCONNECT:
//                     isConnected = false;
//                     Log.Info($"Client with SteamID {clientSteamID} disconnected.");
//                     break;
//                 default:
//                     Log.Info("Received unknown message type");
//                     break;
//             }
//         }
//         catch (Exception e)
//         {
//             Log.Error(e);
//         }
//     }
//
//     bool IsKey(KeyCode keyCode) => Input.GetKeyDown(keyCode);
// }
