using System;
using System.Diagnostics;

namespace ET
{
    class SteamServerProcessComponentAwakeSystem: AwakeSystem<SteamServerProcessComponent, int>
    {
        public override void Awake(SteamServerProcessComponent self, int a)
        {
            self.processId = a;
        }
    }

    class SteamServerProcessComponentDestroySystem: DestroySystem<SteamServerProcessComponent>
    {
        public override void Destroy(SteamServerProcessComponent self)
        {
            Process process = null;
            try
            {
                 process = Process.GetProcessById(self.processId);
                if (!process.CloseMainWindow())
                    process.Kill();
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            finally
            {
                process?.Dispose();
            }
        }
    }

    public static class SteamServerProcessComponentSystem
    {
    }
}