using ET.EventType;

namespace ET
{
    public class SteamServerStart_AddComponentEvent:AEvent<SteamServerStart>
    {
        protected override async ETTask Run(SteamServerStart a)
        {
            var scene = a.ZoneScene;
            await ETTask.CompletedTask;
        }
    }
}