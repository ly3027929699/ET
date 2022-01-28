using ET.EventType;

namespace ET
{
    public class CreateLobbyFinish_CreateUIGame: AEvent<CreateLobbyFinish>
    {
        protected override async ETTask Run(CreateLobbyFinish a)
        {
            await ETTask.CompletedTask;
            // await UIHelper.Create(a.ZoneScene, UIType.UIGame);
        }
    }
}