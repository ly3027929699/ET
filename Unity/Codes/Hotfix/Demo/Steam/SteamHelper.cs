using System;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    public static class SteamHelper
    {
        public static async ETTask CreateLobby(Scene zoneScene, string lobbyName)
        {
            try
            {
                await ETTask.CompletedTask;
                var steamComponent = zoneScene.AddComponent<SteamComponent>();
                var s2c_CreateLobby = await SessionHelper.Call(zoneScene, new C2S_CreateLobby() { name = lobbyName }) as S2C_CreateLobby;
                if (s2c_CreateLobby.Error != 0)
                {
                    Log.Error(s2c_CreateLobby.Error.ToString());
                    return;
                }

                await steamComponent.CreateLobby(lobbyName);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static async ETTask<int> JoinLobby(Scene zoneScene, string hostId, SteamId lobbyId)
        {
            try
            {
                Session gateSession = zoneScene
                        .AddComponent<NetSteamComponent, int, string>(SessionStreamDispatcherType.SessionStreamDispatcherClientOuter, null)
                        .Create(hostId);

                gateSession.AddComponent<PingComponent>();
                zoneScene.AddComponent<SessionComponent>().Session = gateSession;
                //等待连接
                Log.Info($"start connect {hostId}...");
                await Game.Scene.GetComponent<ObjectWait>().Wait<WaitType.Wait_OnSteamConnectToServer>();
                //
                Log.Info("connected");
                var s2c_CreateLobby = await SessionHelper.Call(zoneScene, new C2S_JoinLobby() { SteamId = lobbyId }) as S2C_JoinLobby;
                if (s2c_CreateLobby.Error != 0)
                {
                    return s2c_CreateLobby.Error;
                }

                var steamComponent = zoneScene.AddComponent<SteamComponent>();
                int err = await steamComponent.JoinLobby(lobbyId);
                return err;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return ErrorCode.ERR_Success;
        }

        public static async ETTask LeaveLobby(Scene zoneScene)
        {
            try
            {
                var steamComponent = zoneScene.GetComponent<SteamComponent>();
                steamComponent.LeaveLobby();

                var s2c_CreateLobby = await SessionHelper.Call(zoneScene, new C2S_LeaveLobby()) as S2C_LeaveLobby;
                if (s2c_CreateLobby.Error != 0)
                {
                    Log.Error(s2c_CreateLobby.Error.ToString());
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static async ETTask<Lobby[]> GetLobbyList()
        {
            try
            {
                return await SteamMatchmaking.LobbyList.WithKeyValue(SteamComponent.LobbySearchKey, SteamComponent.LobbySearchValue).RequestAsync();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            return null;
        }
    }
}