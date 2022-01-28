using UnityEngine;

namespace ET
{
    public class TransferHelper
    {
        public static async ETTask Transfer(Unit unit, Scene scene)
        {
            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = new M2C_StartSceneChange() { SceneInstanceId = scene.InstanceId, SceneName = scene.Name };
            MessageHelper.SendToClient(unit, m2CStartSceneChange);

            // M2M_UnitTransferRequest request = new M2M_UnitTransferRequest();
            // request.Unit = unit;
            // foreach (Entity entity in unit.Components.Values)
            // {
            // 	if (entity is ITransfer)
            // 	{
            // 		request.Entitys.Add(entity);
            // 	}
            // }
            // 删除Mailbox,让发给Unit的ActorLocation消息重发
            // unit.RemoveComponent<MailBoxComponent>();

            // location加锁
            long oldInstanceId = unit.InstanceId;
            await Run(unit, scene);
        }

        private static async ETTask Run(Unit unit, Scene scene)
        {
            await ETTask.CompletedTask;
            UnitComponent unitComponent = scene.GetComponent<UnitComponent>();

            unitComponent.AddChild(unit);
            unitComponent.Add(unit);

            unit.AddComponent<MoveComponent>();
            unit.AddComponent<PathfindingComponent, string>(scene.Name);
            unit.Position = new Vector3(-10, 0, -10);

            // 通知客户端创建My Unit
            M2C_CreateMyUnit m2CCreateUnits = new M2C_CreateMyUnit();
            m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
            MessageHelper.SendToClient(unit, m2CCreateUnits);

            // 加入aoi
            // unit.AddComponent<AOIEntity, int, Vector3>(9 * 1000, unit.Position);
        }
    }
}