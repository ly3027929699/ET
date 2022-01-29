using System;
using System.Diagnostics;
using System.Net;
using Steamworks;
using UnityEngine;

namespace ET
{
    public static class GameHomePageHelper
    {
        private static string localAdress = "127.0.0.1:10005";

        public static async ETTask StartHost(Scene zoneScene)
        {
            await ETTask.CompletedTask;
            try
            {
                SessionHelper.IsHost = true;
                string serverExePath = PathHelper.ServerExePath;
                int processId = ProcessHelper.Run(serverExePath, null, PathHelper.ServerExeDirPath).Id;
                zoneScene.AddComponent<SteamServerProcessComponent, int>(processId);

                Session session = zoneScene
                        .AddComponent<NetKcpComponent, int>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter).Create(localAdress);
                CodeLoader.Instance.OnClientConnectToServer?.Invoke(SteamCoreHelper.GetId());

                session.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().LocalSession = session;
            }
            catch (Exception e)
            {
                Log.Error(e);
                SessionHelper.IsHost = false;
                zoneScene.RemoveComponent<SteamServerProcessComponent>();
                zoneScene.RemoveComponent<NetKcpComponent>();
                zoneScene.RemoveComponent<PingComponent>();
                zoneScene.RemoveComponent<SessionComponent>();
                return;
            }

            try
            {
                Log.Info("create host!");
                Game.EventSystem.Publish(new EventType.HomePageFinish_StartHost() { OldScene = zoneScene, ZoneScene = zoneScene });
                Game.EventSystem.Publish(new EventType.SteamServerStart() { ZoneScene = zoneScene });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return;
            }
        }

        public static async ETTask StartClient(Scene zoneScene)
        {
            try
            {
                await ETTask.CompletedTask;
                SessionHelper.IsHost = false;
                Game.EventSystem.Publish(new EventType.HomePageFinish_StartClient() { OldScene = zoneScene, ZoneScene = zoneScene });
            }
            catch (Exception e)
            {
                Log.Error(e);
                return;
            }
        }
    }
}