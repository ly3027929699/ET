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
            self.btnTest1 = rc.Get<GameObject>("btnTest1").GetComponent<Button>();
            self.btnTest2 = rc.Get<GameObject>("btnTest2").GetComponent<Button>();
            self.inpHostId = rc.Get<GameObject>("inpHostId").GetComponent<InputField>();

            self.btnHost.onClick.AddListener(() => { self.OnStartHost(); });
            self.btnClient.onClick.AddListener(() => { self.OnStartClient(); });
            self.btnTest1.onClick.AddListener(() => { self.Test1().Coroutine(); });
            self.btnTest2.onClick.AddListener(() => { self.Test2(); });

            self.btnTest1.interactable = false;
            self.btnTest2.interactable = false;
            CodeLoader.Instance.OnClientConnectToServer += (steamId) =>
            {
                self.btnTest1.interactable = true;
                self.btnTest2.interactable = true;
            };
        }
    }

    public static class UILoginComponentSystem
    {
        public static void OnStartHost(this UILoginComponent self)
        {
            LoginHelper.StartHost(self.DomainScene());
        }

        public static void OnStartClient(this UILoginComponent self)
        {
            if (!ulong.TryParse(self.inpHostId.text, out var id) || id == 0)
            {
                Log.Error("输入正确的host");
            }

            LoginHelper.StartClient(self.DomainScene(), id.ToString());
        }

        public static async ETTask Test1(this UILoginComponent self)
        {
            var scene = Game.Scene.GetComponent<SteamSceneComponent>().ClientScene;
            R2C_Login r2CLogin = (R2C_Login) await SessionHelper.Call(scene,new C2R_Login() { Account = "account", Password = "password" });
            Log.Info(r2CLogin.Message);
        }

        public static void Test2(this UILoginComponent self)
        {
            
        }
    }
}