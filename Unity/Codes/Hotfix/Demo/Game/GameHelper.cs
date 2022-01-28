namespace ET
{
    public static class GameHelper
    {
        public static async ETTask Test1(Scene zoneScene)
        {
            var ret = await SessionHelper.Call(zoneScene, new C2R_Login()) as R2C_Login;
            Log.Info(ret.Message);
        }

        public static void Test2(Scene zoneScene)
        {
        }
    }
}