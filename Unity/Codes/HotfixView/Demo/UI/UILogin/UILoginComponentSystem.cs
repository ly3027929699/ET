using System;
using System.Net;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class UILoginComponentAwakeSystem: AwakeSystem<UILoginComponent>
    {
        public override void Awake(UILoginComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.btnClient = rc.Get<GameObject>("btnClient").GetComponent<Button>();
            self.btnHost = rc.Get<GameObject>("btnHost").GetComponent<Button>();

            self.btnHost.onClick.AddListener(() => { self.OnStartHost(); });
            self.btnClient.onClick.AddListener(() => { self.OnStartClient(); });
        }
    }

    public static class UILoginComponentSystem
    {
        public static void OnStartHost(this UILoginComponent self)
        {
            LoginHelper.StartHost(self.ZoneScene());
        }

        public static void OnStartClient(this UILoginComponent self)
        {
            LoginHelper.StartClient(self.ZoneScene(), 1.ToString());
        }
    }
}