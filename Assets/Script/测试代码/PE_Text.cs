using UnityEngine;

namespace Script.测试代码
{
    public class PE_Text : MonoBehaviour
    {
        void Start()
        {
            TimeMgr.Timer.AddTimeTask((id) => {
                Debug.Log("2秒后执行");
            }, 2, TimeUnit.Second, 1);
            
            // 帧任务
            TimeMgr.Timer.AddFrameTask((id) => { Debug.Log("下一帧执行"); }, 60, 2);

            TimeMgr.Timer.AddFixedFrameTask((id) => { Debug.Log("固定帧执行"); }, 60, 2);
            TimeMgr.Timer.AddUnscaledTimeTask((id) => { Debug.Log("不受时间缩放影响的任务"); }, 2, TimeUnit.Second, 1);
        }
    }
}