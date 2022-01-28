namespace ET
{
    public class HomePageFinish_StartClient_CreateCreateLobbyUI: AEvent<EventType.HomePageFinish_StartClient>
    {
        protected override async ETTask Run(EventType.HomePageFinish_StartClient args)
        {
            await UIHelper.Create(args.ZoneScene, UIType.UIJoinLobby);
        }
    }
}