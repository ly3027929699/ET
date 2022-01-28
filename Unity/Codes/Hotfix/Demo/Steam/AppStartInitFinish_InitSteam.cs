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
            InitSteamConfig();
        }
        private void InitSteamConfig()
        {
            const string fileName = "steam_appid.txt";
            string steamAppId = SteamHelper.GetSteamAppId().ToString();
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                if (content != steamAppId)
                {
                    File.WriteAllText(fileName,  steamAppId);
                    Log.Info($"Updating {fileName}. Previous: {content}, new SteamAppID { SteamHelper.GetSteamAppId()}");
                }
            }
            else
            {
                File.WriteAllText(fileName,  steamAppId);
                Log.Info($"New {fileName} written with SteamAppID { SteamHelper.GetSteamAppId()}");
            }

            SteamClient.Init(SteamHelper.GetSteamAppId(), true);
        }
    }
}