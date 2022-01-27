using Steamworks;
using Steamworks.Data;

namespace ET
{
    public static class SteamHelper
    {
        public static async ETTask CreateLobby(Scene zoneScene, string lobbyName)
        {
            await ETTask.CompletedTask;
            var steamComponent = zoneScene.AddComponent<SteamComponent>();
            await steamComponent.CreateLobby(lobbyName);
        }
        public static async ETTask JoinLobby(SteamId lobbyId)
        {
            await SteamMatchmaking.JoinLobbyAsync(lobbyId);
        }
      
        public static async ETTask<Lobby[]> GetLobbyList()
        {
            return await SteamMatchmaking.LobbyList.WithKeyValue(SteamComponent.LobbySearchKey, SteamComponent.LobbySearchValue).RequestAsync();
        }

        public static SteamId GetId()
        {
            return SteamClient.SteamId;
        }

        public static string GetName()
        {
            return SteamClient.Name;
        }

     
    }
}