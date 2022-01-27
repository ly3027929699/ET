

namespace ET
{
	public class AppStartInitFinish_CreateLoginUI: AEvent<EventType.AppStartInitFinish>
	{
		protected override async ETTask Run(EventType.AppStartInitFinish args)
		{
			CodeLoader.Instance.OnClientConnectToServer += (id) =>
			{
				var scene = args.ZoneScene;
				scene.GetComponent<ObjectWait>().Notify(new WaitType.Wait_OnSteamConnectToServer());
			};
			await UIHelper.Create(args.ZoneScene, UIType.UILogin);
		}
	}
}
