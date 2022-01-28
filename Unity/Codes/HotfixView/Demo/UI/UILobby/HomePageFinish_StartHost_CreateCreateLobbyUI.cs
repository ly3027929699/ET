

namespace ET
{
	public class HomePageFinish_StartHost_CreateCreateLobbyUI: AEvent<EventType.HomePageFinish_StartHost>
	{
		protected override async ETTask Run(EventType.HomePageFinish_StartHost args)
		{
			await UIHelper.Create(args.ZoneScene, UIType.UICreateLobby);
		}
	}
}
