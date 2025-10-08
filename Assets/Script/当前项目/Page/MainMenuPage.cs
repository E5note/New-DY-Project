using System;
using System.Collections.Generic;
using Script.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Script.当前项目.Page
{
    public class MainMenuPage : PanelBase
    {
        public static MainMenuPage Instance;
        
        // 开始游戏按钮
        public SoraBtn startGameBtn;
        // 侧边栏按钮
        public SoraBtn sideBarBtn;
        public override void Init(params object[] objs)
        {
            base.Init(objs);
            Instance = this;
        }

        void Start()
        {
            startGameBtn.OnClick(() =>
            {
                UIMgr.Instance.ShowPage(new List<GameObject>(){gameObject},GamePage.Instance.gameObject);
            });
            sideBarBtn.OnClick(() =>
            {
                UIMgr.Instance.ShowPage(new List<GameObject>(){gameObject},SidebarPage.Instance.gameObject);
            });
        }
        
        protected override void DelayShow(Action hideEvent = null)
        {
            base.DelayShow(hideEvent);
            FadeIn();
        }

        protected override void DelayHide()
        {
            base.DelayHide();
            FadeOut();
        }
    }
}