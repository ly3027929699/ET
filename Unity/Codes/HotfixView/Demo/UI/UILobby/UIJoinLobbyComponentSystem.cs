using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    class UIJoinLobbyComponentAwakeSystem: AwakeSystem<UIJoinLobbyComponent>
    {
        public override void Awake(UIJoinLobbyComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            self.btnRefresh = rc.Get<GameObject>("btnRefresh").GetComponent<Button>();
            self.item_prefab = rc.Get<GameObject>("Item").GetComponent<SingleLobbyButton>();
            self.scrList = rc.Get<GameObject>("scrList").GetComponent<ScrollRect>();
            self.btnRefresh.onClick.AddListener(() => { self.RefreshLobbyList().Coroutine(); });

            self.ShowLobbies();
        }
    }

    public static class UIJoinLobbyComponentSystem
    {
        public static async ETTask RefreshLobbyList(this UIJoinLobbyComponent self)
        {
            await ETTask.CompletedTask;
            // await UIHelper.Remove(self.ZoneScene(), UIType.UIJoinLobby);
        }

        public static async void ShowLobbies(this UIJoinLobbyComponent self)
        {
            if (self.isRequesting)
            {
                Debug.Log("请求列表中！");
                return;
            }

            self.isRequesting = true;
            var lobbies = await SteamHelper.GetLobbyList();
            self.isRequesting = false;
            if (lobbies == null) return;
            if (self.m_items != null && self.m_items.Length > 0)
                foreach (var t in self.m_items)
                {
                    UnityEngine.Object.Destroy(t.gameObject);
                }

            var nLobbies = lobbies.Length;
            var container = self.scrList.content;
            float sizeDeltaY = self.item_prefab.GetComponent<RectTransform>().sizeDelta.y;
            container.sizeDelta = new Vector2(container.sizeDelta.x,
                nLobbies * sizeDeltaY);

            self.m_items = new SingleLobbyButton[nLobbies];
            for (var i = 0; i < nLobbies; ++i)
            {
                SingleLobbyButton singleLobbyButton
                        = self.m_items[i]
                                = UnityEngine.Object.Instantiate(self.item_prefab, container.transform, false);
                Vector2 pos = new Vector2(container.anchoredPosition.x, -i * sizeDeltaY);
                singleLobbyButton.GetComponent<RectTransform>().anchoredPosition = pos;
                singleLobbyButton.gameObject.SetActive(true);

                var lobby = lobbies[i];
                singleLobbyButton.ID.text = i.ToString();
                singleLobbyButton.Name.text = lobby.GetData(SteamComponent.LobbyNameKey);
                if (singleLobbyButton.Name.text == "") singleLobbyButton.Name.text = "NULL";
                singleLobbyButton.MumberCount.text = lobby.MemberCount + "/" + lobby.MaxMembers;
                singleLobbyButton.ServerId = lobby.GetData(SteamComponent.LobbyHostIdKey);
                singleLobbyButton.lobbyId = lobby.Id;
                singleLobbyButton.Add((a,b) => self.OnClickJoin(a,b).Coroutine());
            }
        }

        private static async ETTask OnClickJoin(this UIJoinLobbyComponent self, string id,SteamId lobbyId)
        {
            Scene zoneScene = self.ZoneScene();
            int err= await SteamHelper.JoinLobby(zoneScene, id,lobbyId);
           if (err != ErrorCode.ERR_Success)
           {
               Log.Info(err.ToString());
           }
           
           EnterMapHelper.EnterMapAsync(zoneScene).Coroutine();
           Game.EventSystem.Publish(new EventType.CreateLobbyFinish{ZoneScene = zoneScene});
           await UIHelper.Remove(self.ZoneScene(), UIType.UIJoinLobby);
        }
    }
}