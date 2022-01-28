namespace ET
{
    public static class SceneHelper
    {
        public static int DomainZone(this Entity entity)
        {
            return ((Scene) entity.Domain)?.Zone ?? 0;
        }

        public static Scene DomainScene(this Entity entity)
        {
            return (Scene) entity.Domain;
        }

        public static bool IsServer(this Scene zoneScene)
        {
            return zoneScene.Zone == 2;
        }

        public static bool IsClient(this Scene zoneScene)
        {
            return zoneScene.Zone == 3;
         }
    }
}