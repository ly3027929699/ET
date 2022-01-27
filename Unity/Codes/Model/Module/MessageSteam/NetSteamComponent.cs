using Steamworks;

namespace ET
{
    public class NetSteamComponent:Entity,IAwake<int,int>,IAwake<int,string>
    {
        public AService Service;
        
        public int SessionStreamDispatcherType { get; set; }

    }
}