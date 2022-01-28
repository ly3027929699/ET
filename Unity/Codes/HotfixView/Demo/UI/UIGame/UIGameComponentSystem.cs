using System;
using System.Net;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    [ObjectSystem]
    public class UIGameComponentAwakeSystem: AwakeSystem<UIGameComponent>
    {
        public override void Awake(UIGameComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.btnTest1 = rc.Get<GameObject>("btnTest1").GetComponent<Button>();
            self.btnTest2 = rc.Get<GameObject>("btnTest2").GetComponent<Button>();

            self.btnTest1.onClick.AddListener(() => { self.Test1(); });
            self.btnTest2.onClick.AddListener(() => { self.Test2(); });
        }
    }

    public static class UIGameComponentSystem
    {
        public static void Test1(this UIGameComponent self)
        {
            GameHelper.Test1(self.ZoneScene());
        }

        public static void Test2(this UIGameComponent self)
        {
            GameHelper.Test2(self.ZoneScene());
        }
    }
}