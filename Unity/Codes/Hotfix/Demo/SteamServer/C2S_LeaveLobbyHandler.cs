﻿using System;
using Steamworks;

namespace ET
{
    public class C2S_LeaveLobbyHandler: AMRpcHandler<C2S_LeaveLobby, S2C_LeaveLobby>
    {
        protected override async ETTask Run(Session session, C2S_LeaveLobby request, S2C_LeaveLobby response, Action reply)
        {
            await ETTask.CompletedTask;
            var scene = session.ZoneScene();
            var playerComponent = scene.GetComponent<PlayerComponent>();
            Player player = session.GetComponent<SessionPlayerComponent>().GetPlayer();
            if (player == null)
            {
                Log.Error($"player == null where address is {session.RemoteAddress}");
                response.Error = ErrorCode.Err_System;
                reply();
                return;
            }

            playerComponent.Remove(player.Id);
            reply();
        }
    }
}