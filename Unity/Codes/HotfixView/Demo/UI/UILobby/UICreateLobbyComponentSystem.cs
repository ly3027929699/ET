using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    class UICreateLobbyComponentAwakeSystem: AwakeSystem<UICreateLobbyComponent>
    {
        public override void Awake(UICreateLobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.btnStartGame = rc.Get<GameObject>("btnStartGame").GetComponent<Button>();
            self.inpHostId = rc.Get<GameObject>("inpHostId").GetComponent<InputField>();
            self.btnStartGame.onClick.AddListener(() => { self.CreateLobby().Coroutine(); });
        }
    }

    public static class UICreateLobbyComponentSystem
    {
        public static async ETTask CreateLobby(this UICreateLobbyComponent self)
        {
            string value = self.inpHostId.text;
            string name = string.IsNullOrWhiteSpace(value)? $"{SteamHelper.GetId()}的房间" : value;
            Scene zoneScene = self.ZoneScene();
            
            await SteamHelper.CreateLobby(zoneScene, name);
            
            EnterMapHelper.EnterMapAsync(zoneScene).Coroutine();
            Game.EventSystem.Publish(new EventType.CreateLobbyFinish { ZoneScene = zoneScene });
            
            await UIHelper.Remove(zoneScene, UIType.UICreateLobby);
        }
    }
}