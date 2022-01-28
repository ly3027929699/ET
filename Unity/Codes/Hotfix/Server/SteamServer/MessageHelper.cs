using System.Collections.Generic;

namespace ET
{
    public static class MessageHelper
    {
        public static void Broadcast(Unit unit, IActorMessage message)
        {
            var scene = unit.DomainScene();
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();
            foreach (var unitComponentChild in unitComponent.Children)
            {
                var _unit = unitComponentChild.Value as Unit;
                SendToClient(_unit, message);
            }
        }

        public static void SendToClient(Unit unit, IActorMessage message)
        {
            SendMessage(unit.GetComponent<UnitGateComponent>().sessionId, message);
        }

        public static void SendMessage(long actorId, IActorMessage message)
        {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
            instanceIdStruct.Process = Game.Options.Process;
            long realActorId = instanceIdStruct.ToLong();
                    
                    
            Entity entity = Game.EventSystem.Get(realActorId);
            if (entity == null)
            {
                Log.Error($"not found actor:{realActorId} {message}");
                return;
            }
                    
            if (entity is Session gateSession)
            {
                // 发送给客户端
                gateSession.Send(message);
                return;
            }
        }
    }
}