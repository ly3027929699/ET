namespace ET
{
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<EventType.SceneChangeFinish>
    {
        protected override async ETTask Run(EventType.SceneChangeFinish args)
        {
            await ETTask.CompletedTask;
            // await UIHelper.Create(args.CurrentScene, UIType.UIHelp);
        }
    }
}
