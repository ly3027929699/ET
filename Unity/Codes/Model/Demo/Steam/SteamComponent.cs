#if !SERVER
using Steamworks;
using Steamworks.Data;
#endif

namespace ET
{
    public class SteamComponent:Entity,IAwake,IDestroy
    { 
        public const string LobbyNameKey = "LobbyName";
        public const string LobbyHostIdKey = "LobbyHostId";
        public const string LobbySearchKey = "LobbySearchKey";
        public const string LobbySearchValue = "LobbySearchValue_ETP2P_Test_Key_Cann't_Same";
        public const int MaxPlayer = 5;
#if !SERVER
        public SteamId HostId;
        public Lobby Lobby;
        public string LobbyName;
#endif

    }
}