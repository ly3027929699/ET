using ET.EventType;

namespace ET
{
    public class JoinLobbyFinish_CreateUIGame:AEvent<JoinLobbyFinish>
    {
        protected override async ETTask Run(JoinLobbyFinish a)
        {
            await ETTask.CompletedTask;
            // await UIHelper.Create(a.ZoneScene, UIType.UIGame);
        }
    }
}