using System.IO;
using ET.EventType;
using Steamworks;

namespace ET
{
    public class AppStartInitFinish_InitSteam:AEvent<AppStartInitFinish>
    {
        protected override async ETTask Run(AppStartInitFinish args)
        {
            await ETTask.CompletedTask;
            CodeLoader.Instance.OnClientConnectToServer += (id) =>
            {
                Game.Scene.GetComponent<ObjectWait>().Notify(new WaitType.Wait_OnSteamConnectToServer());
            };
            SteamCoreHelper.InitSteamConfig();
        }
     
    }
}