using System.IO;
using Steamworks;

namespace ET
{
    public static class SteamCoreHelper
    {
        
        public static SteamId GetId()
        {
            return SteamClient.SteamId;
        }

        public static string GetName()
        {
            return SteamClient.Name;
        }

        public static void Init()
        {
            SteamClient.Init(GetSteamAppId());
        }

        public static uint GetSteamAppId()
        {
            return 480;
        }

        public static void InitSteamConfig()
        {
            const string fileName = "steam_appid.txt";
            string steamAppId = SteamCoreHelper.GetSteamAppId().ToString();
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                if (content != steamAppId)
                {
                    File.WriteAllText(fileName, steamAppId);
                    Log.Info($"Updating {fileName}. Previous: {content}, new SteamAppID {SteamCoreHelper.GetSteamAppId()}");
                }
            }
            else
            {
                File.WriteAllText(fileName, steamAppId);
                Log.Info($"New {fileName} written with SteamAppID {SteamCoreHelper.GetSteamAppId()}");
            }

            Init();
        }
    }
}