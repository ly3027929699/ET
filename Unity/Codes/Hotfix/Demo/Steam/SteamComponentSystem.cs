using System;
using System.Text;
using Steamworks;
using Steamworks.Data;

namespace ET
{
    class SteamComponentAwakeSystem:AwakeSystem<SteamComponent>
    {
        public override void Awake(SteamComponent self)
        {
            self.Awake();
        }
    } class SteamComponentDestroySystem:DestroySystem<SteamComponent>
    {
        public override void Destroy(SteamComponent self)
        {
            self.Destroy();
        }
    }
    public static class SteamComponentSystem
    {
        public static void Awake(this SteamComponent self)
        {
            SteamMatchmaking.OnLobbyCreated += self.OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += self.OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += self. OnLobbyMemberJoined;
        }

        public static void Destroy(this SteamComponent self)
        {
            SteamMatchmaking.OnLobbyCreated -= self.OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= self.OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= self. OnLobbyMemberJoined;
            self.LeaveLobby();
        }

        public static async ETTask CreateLobby(this SteamComponent self, string lobbyName)
        {
            var _lobby = await SteamMatchmaking.CreateLobbyAsync(SteamComponent.MaxPlayer);
            if (_lobby == null)
            {
                throw new Exception("lobby create fail");
            }

            self.LobbyName = lobbyName;
        }

        private static void OnLobbyCreated(this SteamComponent self, Result arg1, Lobby lobby)
        {
            Log.Warning(nameof (OnLobbyCreated));
            if (arg1 != Result.OK)
            {
                Log.Error(arg1.ToString());
                return;
            }

            self.Lobby = lobby;

            self.SetLobbyData(SteamComponent.LobbyNameKey, self.LobbyName);
            self.SetLobbyData(SteamComponent.LobbyHostIdKey, self.HostId.ToString());
            self.SetLobbyData(SteamComponent.LobbySearchKey, SteamComponent.LobbySearchValue);
        }

        private static void OnLobbyEntered(this SteamComponent self, Lobby lobby)
        {
            Log.Warning(nameof (OnLobbyEntered));
            self.Lobby = lobby;
            var playerComponent = self.ZoneScene().GetComponent<PlayerComponent>();
            var player = playerComponent.AddChild<Player>();
            player.SteamId = SteamHelper.GetId();
            self.SendChatMessage(SteamHelper.GetName() + " Joined the Lobby!..", false);
        }

        private static void OnLobbyMemberJoined(this SteamComponent self,  Lobby lobby,Friend friend)
        {
            Log.Warning($"{nameof (OnLobbyEntered)} {friend.Name} Joined");
            var playerComponent = self.ZoneScene().GetComponent<PlayerComponent>();
            var player = playerComponent.AddChild<Player>();
            player.SteamId = friend.Id;
        }

        public static string GetLobbyData(this SteamComponent self, string _name)
        {
            return self.Lobby.GetData(_name);
        }

        public static void SetLobbyData(this SteamComponent self, string _name, string value)
        {
            var setDataRet = self.Lobby.SetData(_name, value);
            if (!setDataRet)
                Log.Error($"set data fail when key = {_name}");
        }

        public static void LeaveLobby(this SteamComponent self)
        {
            self.SendChatMessage(SteamHelper.GetName() + " Left the Lobby", false);

            self.Lobby.Leave();
            self.Lobby = default;
        }

        public static void SendChatMessage(this SteamComponent self, string t, bool isChat = true)
        {
            var content = isChat? DateTime.Now.ToString("HH:mm") + " " + SteamClient.Name + ": " + t + '\0' : t;
            var bytes = Encoding.Default.GetBytes(content);
            self.Lobby.SendChatBytes(bytes, bytes.Length);
        }
    }
}