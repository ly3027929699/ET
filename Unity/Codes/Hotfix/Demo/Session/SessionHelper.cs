namespace ET
{
    public static class SessionHelper
    {
        public static bool IsHost;
        private static Session GetSession(Scene scene)
        {
            if (IsHost)
            {
               return scene.GetComponent<SessionComponent>().LocalSession;
            }
            else
            {
                return scene.GetComponent<SessionComponent>().Session;
            }
        }

        public static void Send(Scene scene, IMessage message)
        {
            Session session = GetSession(scene);
            session.Send(message);
        }
        public static ETTask<IResponse> Call(Scene scene,IRequest request)
        {
            Session session = GetSession(scene);
            return session.Call(request);
        }
    }
}