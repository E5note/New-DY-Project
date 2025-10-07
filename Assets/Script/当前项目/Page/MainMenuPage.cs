using System;

namespace Script.当前项目.Page
{
    public class MainMenuPage : PanelBase
    {
        public static MainMenuPage Instance;
        public override void Init(params object[] objs)
        {
            base.Init(objs);
            Instance = this;
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