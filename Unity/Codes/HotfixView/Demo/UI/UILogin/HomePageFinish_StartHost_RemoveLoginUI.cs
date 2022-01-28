

namespace ET
{
	public class HomePageFinish_StartHost_RemoveLoginUI: AEvent<EventType.HomePageFinish_StartHost>
	{
		protected override async ETTask Run(EventType.HomePageFinish_StartHost args)
		{
			await UIHelper.Remove(args.OldScene, UIType.UILogin);
		}
	}
}
