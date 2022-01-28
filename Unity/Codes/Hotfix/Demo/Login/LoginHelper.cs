using System;
using System.Net;
using Steamworks;
using UnityEngine;

namespace ET
{
    public static class LoginHelper
    {
        private static string localAdress = "127.0.0.1:10005";

        public static async ETTask StartHost(Scene zoneScene)
        {
            try
            {
                await ETTask.CompletedTask;

                SessionHelper.IsHost = true;

                Session session = zoneScene
                        .AddComponent<NetKcpComponent, int>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter).Create(localAdress);
                CodeLoader.Instance.OnClientConnectToServer?.Invoke(SteamCoreHelper.GetId());

                session.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().LocalSession = session;

                Log.Info("create host!");
                Game.EventSystem.Publish(
                    new EventType.HomePageFinish_StartHost() { OldScene = zoneScene, ZoneScene = zoneScene });
                Game.EventSystem.Publish(new EventType.SteamServerStart() { ZoneScene = zoneScene });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static async ETTask StartClient(Scene zoneScene)
        {
            try
            {
                await ETTask.CompletedTask;
                SessionHelper.IsHost = false;


                Game.EventSystem.Publish(
                    new EventType.HomePageFinish_StartClient() { OldScene = zoneScene, ZoneScene = zoneScene});
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}