using System.Collections.Generic;

namespace ET
{
    public static class MessageHelper
    {
        public static void Broadcast(Unit unit, IActorMessage message)
        {
        }

        public static void SendToClient(Unit unit, IActorMessage message)
        {
            SendActor(unit.GetComponent<UnitGateComponent>().sessionId, message);
        }

        /// <summary>
        /// 发送协议给Actor
        /// </summary>
        /// <param name="actorId">注册Actor的InstanceId</param>
        /// <param name="message"></param>
        public static void SendActor(long actorId, IActorMessage message)
        {
            
        }

        /// <summary>
        /// 发送协议给ActorLocation
        /// </summary>
        /// <param name="id">注册Actor的Id</param>
        /// <param name="message"></param>
        public static void SendToLocationActor(long id, IActorLocationMessage message)
        {
            
        }
    }
}