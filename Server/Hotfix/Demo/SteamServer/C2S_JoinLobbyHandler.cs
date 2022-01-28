using System;

namespace ET
{
    public class C2S_JoinLobbyHandler: AMRpcHandler<C2S_JoinLobby, S2C_JoinLobby>
    {
        protected override async ETTask Run(Session session, C2S_JoinLobby request, S2C_JoinLobby response, Action reply)
        {
            var playerComponent = session.DomainScene().GetComponent<PlayerComponent>();
            if (!playerComponent.CanAddPlayer())
            {
                response.Error = ErrorCode.JoinLobbyFail_MaxPlayer;
                reply();
                return;
            }
            int err = playerComponent.Add(session);
            response.Error = err;
            reply();
            await ETTask.CompletedTask;
        }
    }
}