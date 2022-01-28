namespace ET
{
    public class HomePageFinish_StartClient_RemoveLoginUI: AEvent<EventType.HomePageFinish_StartClient>
    {
        protected override async ETTask Run(EventType.HomePageFinish_StartClient args)
        {
            await UIHelper.Remove(args.OldScene, UIType.UILogin);
        }
    }
}