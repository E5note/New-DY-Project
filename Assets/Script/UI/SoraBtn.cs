using System;
using DG.Tweening;
using Script.ConfigEnum;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Script.UI
{
    public class SoraBtn : SoraMono
    {
        [FieldName("按钮引用")]
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
        }

        public void OnClick(UnityAction callback)
        {
            _button.onClick.AddListener(callback);
        }

        // 播放音效
        public void PlayClickSound()
        {
            // 播放动画
            _button.transform.DOScale(1.1f, 0.1f).OnComplete(() =>
            {
                _button.transform.DOScale(1f, 0.1f);
            });
            // 播放音效
            AudioMgr.PlaySound(EnumAudioClip.按钮点击);
            // 如果是手机平台，则播放震动
            #if UNITY_ANDROID || UNITY_IOS
            TT.VibrateShort(new VibrateShortParam());
            #endif
        }
    }
}