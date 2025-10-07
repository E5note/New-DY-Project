using UnityEngine;

namespace Script.测试代码
{
    public class Pool_Text : MonoBehaviour
    {
        void Start()
        {
            PoolMgr.InitPool<GameObject>(() =>
            {
                print("创建对象的方法");
                return new GameObject("Text");
            }, (obj) =>
            {
                print("重置对象的方法");
                return obj;
            },10,20,10,1,2);
            for (int i = 0; i < 10; i++)
            {
                var obj = PoolMgr.Spawn<GameObject>();
                obj.transform.position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            }
        }
    }
}