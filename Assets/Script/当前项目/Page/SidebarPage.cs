using System;
using System.Collections.Generic;
using Manager;
using Script.UI;
using TTSDK;
using UnityEngine;

namespace Script.当前项目.Page
{
    public class SidebarPage : PanelBase
    {
        public static SidebarPage Instance;
        public SoraBtn sidebarButton;
        public SoraBtn closeButton;
        
        public override void Init(params object[] objs)
        {
            base.Init(objs); // 调用父类初始化
            Instance = this; // 设置单例实例
        }
        
        protected void Start()
        {
            TT.PlayerPrefs.SetInt("IsSidebar", 0);
            sidebarButton.OnClick(() =>
            {
                DYManager.Instance.OpenSidebar(() =>
                {
                    TT.PlayerPrefs.SetInt("IsSidebar", 1);
                    UpdatePanel();
                });
            });
            closeButton.OnClick(ClosePage);
            UpdatePanel();
        }

        private void UpdatePanel()
        {
            if (TT.PlayerPrefs.GetInt("IsSidebar", 0) == 0)
            {
                //还没有进入侧边栏
                sidebarButton.gameObject.SetActive(true);
            }
            else
            {
                //已经进入侧边栏
                sidebarButton.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 关闭页面
        /// </summary>
        public void ClosePage()
        {
            UIMgr.Instance.ShowPage(new List<GameObject>() { gameObject},
                MainMenuPage.Instance.gameObject);
        }
        
        /// <summary>
        /// 延迟显示页面（重写父类方法）
        /// </summary>
        /// <param name="hideEvent">隐藏事件回调</param>
        protected override void DelayShow(Action hideEvent = null)
        {
            base.DelayShow(hideEvent); // 调用父类方法
            FadeIn(); // 执行淡入动画
        }

        /// <summary>
        /// 延迟隐藏页面（重写父类方法）
        /// </summary>
        protected override void DelayHide()
        {
            base.DelayHide(); // 调用父类方法
            FadeOut(); // 执行淡出动画
        }
    }
}