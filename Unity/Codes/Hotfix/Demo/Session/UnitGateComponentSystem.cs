namespace ET
{
    class UnitGateComponentAwakeSystem:AwakeSystem<UnitGateComponent,long>
    {
        public override void Awake(UnitGateComponent self, long a)
        {
            self.sessionId = a;
        }
    }
    public static class UnitGateComponentSystem
    {
        
    }
}