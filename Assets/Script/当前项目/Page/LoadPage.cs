using System;
using System.Collections;
using System.Collections.Generic;
using Script.Mgr;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Script.当前项目.Page
{
    public class LoadPage: PanelBase
    {
        public static LoadPage Instance;
        public Slider slider;
        public Text progressText;
        // 资源数目
        private int _assetCount = 1;
        // 当前已加载资源数目
        private int _loadedCount;
        
        public override void Init(params object[] objs)
        {
            base.Init(objs);
            Instance = this;
        }
        private void Start()
        {
            EventMgr.RegisterEvent(EventName.AssetLoadProgress,OnProgressUpdated);
            // 初始化数据管理器
            StartCoroutine(InitializeData());
        }
        
        private IEnumerator InitializeData()
        {
            // 调用数据管理器初始化方法
            var task = DataMgr.InitializeAsync();
            while (!task.IsCompleted)
            {
                yield return null;
            }
            // 等待进度条完成动画
            yield return new WaitForSeconds(0.1f);
            
            // 关闭加载页
            UIMgr.Instance.ShowPage(new List<GameObject>(){gameObject},MainMenuPage.Instance.gameObject);
        }

        // 进度条更新事件
        private object OnProgressUpdated(object[] arg)
        {
            _loadedCount++;
            var progress = (float)_loadedCount / _assetCount;
            // 更新滑动条
            slider.value = progress;
            // 可以在这里添加进度文本显示
            progressText.text = $"{(int)(progress * 100)}%";
            return progress;
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