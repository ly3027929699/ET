using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class UILobbyComponentAwakeSystem: AwakeSystem<UILobbyComponent>
    {
        public override void Awake(UILobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.btnStartGame = rc.Get<GameObject>("btnStartGame").GetComponent<Button>();
            self.inpHostId = rc.Get<GameObject>("inpHostId").GetComponent<InputField>();
            self.btnStartGame.onClick.AddListener(() => { self.CreateLobby().Coroutine(); });
        }
    }

    public static class UILobbyComponentSystem
    {
        public static async ETTask CreateLobby(this UILobbyComponent self)
        {
            string value = self.inpHostId.text;
            string name = string.IsNullOrWhiteSpace(value)?$"{SteamHelper.GetId()}的房间":value;
            await SteamHelper.CreateLobby(self.ZoneScene(),name);
            await UIHelper.Remove(self.ZoneScene(), UIType.UILobby);
        }
    }
}