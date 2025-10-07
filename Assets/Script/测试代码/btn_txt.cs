using Script.UI;
using UnityEngine;

namespace Script.测试代码
{
    public class btn_txt : MonoBehaviour
    {
        public SoraBtn btn;

        public void Start()
        {
            print("按钮初始化");
           btn.OnClick(() =>
           {
               Debug.Log("按钮被点击了");
           }); 
        }
    }
}