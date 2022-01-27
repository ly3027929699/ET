using Steamworks;
using Steamworks.Data;

namespace ET
{
    public class SteamComponent:Entity,IAwake,IDestroy
    { 
        public const string LobbyNameKey = "LobbyName";
        public const string LobbyHostIdKey = "LobbyHostId";
        public const string LobbySearchKey = "LobbySearchKey";
        public const string LobbySearchValue = "LobbySearchValue_ET_P2P_Test_Key_Cann't_Same";
        public const int MaxPlayer = 5;
        
        public SteamId HostId;
        public Lobby Lobby;
        public string LobbyName;
    }
}