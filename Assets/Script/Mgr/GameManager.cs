using Script.当前项目.Page;
namespace Script.Mgr
{
    public class GameManager : MonoSingleton<GameManager>
    {
        // 打包后是否输出日志
        public bool logDebug;

        public void Start()
        {
            LoadPage.Instance.Show();
        }
    }
}