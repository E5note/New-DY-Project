using Script.UI;
using UnityEngine;

namespace Script.工具使用代码示例
{
    public class BtnTxt : MonoBehaviour
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