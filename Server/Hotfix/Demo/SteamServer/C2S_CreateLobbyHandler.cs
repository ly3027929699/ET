using System;

namespace ET
{
    public class C2S_CreateLobbyHandler: AMRpcHandler<C2S_CreateLobby, S2C_CreateLobby>
    {
        protected override async ETTask Run(Session session, C2S_CreateLobby request, S2C_CreateLobby response, Action reply)
        {
            var scene = session.DomainScene();
            var playerComponent = scene.GetComponent<PlayerComponent>();
            if (playerComponent.CurrPlayerNum() > 0)
            {
                response.Error = ErrorCode.CreateLobbyFail_AlreadyHasLobby;
                reply();
                return;
            }

            playerComponent.Add(session);
            reply();
            await ETTask.CompletedTask;
        }
    }
}