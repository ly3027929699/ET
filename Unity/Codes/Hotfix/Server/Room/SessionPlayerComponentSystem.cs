namespace ET
{
    class SessionPlayerComponentAwakeSystem:AwakeSystem<SessionPlayerComponent,long>
    {
        public override void Awake(SessionPlayerComponent self, long a)
        {
            self.PlayerId = a;
        }
    }
    class SessionPlayerComponentDestroySystem:DestroySystem<SessionPlayerComponent>
    {
        public override void Destroy(SessionPlayerComponent self)
        {
            var playerComponent = self.DomainScene().GetComponent<PlayerComponent>();
            playerComponent.Remove(self.GetPlayer());
        }
    }

    public static class SessionPlayerComponentSystem
    {
        public static Player GetPlayer(this SessionPlayerComponent self)
        {
          return  self.DomainScene().GetComponent<PlayerComponent>().GetChild<Player>(self.PlayerId);
        }
    }
}