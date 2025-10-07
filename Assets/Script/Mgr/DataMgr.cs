using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.ConfigEnum;

namespace Script.Mgr
{
    public class DataMgr
    {
        // 资源路径字典：键是注释名，值是资源路径
        public static Dictionary<string, string> AssetPathDic { get; private set; }

        // 音频枚举 -> 路径
        public static Dictionary<EnumAudioClip, string> AudioClipPathDic { get; private set; }

        // 音频枚举 -> 配置数据
        private static Dictionary<EnumAudioClip, RowCfgAudioClip> m_AudioClipPathDic1;

        public static Dictionary<EnumAudioClip, RowCfgAudioClip> AudioClipPathDic1
        {
            get
            {
                if (m_AudioClipPathDic1 == null || m_AudioClipPathDic1.Count == 0)
                {
                    m_AudioClipPathDic1 = new Dictionary<EnumAudioClip, RowCfgAudioClip>();
                    ConfigManager.Instance.cfgAudioClip.AllConfigs.ForEach(a =>
                    {
                        m_AudioClipPathDic1[(EnumAudioClip)Enum.Parse(typeof(EnumAudioClip), a.enumName)] = a;
                    });
                }
                return m_AudioClipPathDic1;
            }
        }


        // 游戏数据初始化
        public static async Task InitializeAsync()
        {
         
            AssetPathDic = new Dictionary<string, string>();
            AudioClipPathDic = new Dictionary<EnumAudioClip, string>();
            // 加载配置
            await ConfigManager.Instance.LoadConfigs();
            
        }
        /// <summary>
        /// 添加资源路径，防止重复键
        /// </summary>
        public static void Add(string annotate, string path)
        {
            if (!AssetPathDic.ContainsKey(annotate))
            {
                AssetPathDic.Add(annotate, path);
            }
            else
            {
                LogUtil.Warning("存在同名资源: " + annotate);
            }
        }
    }
}