using System;

namespace ET
{
    public class C2R_LoginHandler:AMRpcHandler<C2R_Login,R2C_Login>
    {
        protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response, Action reply)
        {
            response.Message = "hello world !";
            reply();
            await ETTask.CompletedTask;
        }
    }
}