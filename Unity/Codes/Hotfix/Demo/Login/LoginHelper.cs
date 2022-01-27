using System;
using System.Net;
using Steamworks;
using UnityEngine;

namespace ET
{
    public static class LoginHelper
    {
        private static string localAdress = "127.0.0.1:23003";

        public static async ETTask StartHost(Scene zoneScene)
        {
            try
            {
                await ETTask.CompletedTask;

                SessionHelper.IsHost = true;

                var steamSceneComponent = Game.Scene.AddComponent<SteamSceneComponent>();
                steamSceneComponent.ServerScene = SceneFactory.CreateZoneScene(2, "Server", Game.Scene);
                steamSceneComponent.ClientScene = SceneFactory.CreateZoneScene(3, "Client", Game.Scene);

                steamSceneComponent.ServerScene.AddComponent<NetKcpComponent, IPEndPoint, int>(NetworkHelper.ToIPEndPoint(localAdress),
                    SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);
                steamSceneComponent.ServerScene.AddComponent<NetSteamComponent, int, int>(5,
                    SessionStreamDispatcherType.SessionStreamDispatcherServerOuter);

                Session session = steamSceneComponent.ClientScene
                        .AddComponent<NetKcpComponent, int>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter).Create(localAdress);
                CodeLoader.Instance.OnClientConnectToServer?.Invoke(SteamHelper.GetId());

                session.AddComponent<PingComponent>();
                steamSceneComponent.ClientScene.AddComponent<SessionComponent>().LocalSession = session;

                Log.Info("create host!");
                Game.EventSystem.Publish(new EventType.LoginFinish() {OldScene=zoneScene, ZoneScene = steamSceneComponent.ClientScene });
                await Game.EventSystem.PublishAsync(new EventType.SteamServerStart() { ZoneScene = steamSceneComponent.ServerScene });
                await Game.EventSystem.PublishAsync(new EventType.SteamClientStart() { ZoneScene = steamSceneComponent.ClientScene });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static async ETTask StartClient(Scene zoneScene, string host)
        {
            try
            {
                await ETTask.CompletedTask;
                SessionHelper.IsHost = false;

                var steamSceneComponent = Game.Scene.AddComponent<SteamSceneComponent>();
                var scene = steamSceneComponent.ClientScene = SceneFactory.CreateZoneScene(3, "Client", Game.Scene);

                // 创建一个ETModel层的Session
                // R2C_Login r2CLogin;
                Session gateSession = scene
                        .AddComponent<NetSteamComponent, int, string>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter, null)
                        .Create(host);

                gateSession.AddComponent<PingComponent>();
                scene.AddComponent<SessionComponent>().Session = gateSession;
                //等待连接
                await zoneScene.GetComponent<ObjectWait>().Wait<WaitType.Wait_OnSteamConnectToServer>();

                await Game.EventSystem.PublishAsync(new EventType.SteamClientStart() { ZoneScene = scene });
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}