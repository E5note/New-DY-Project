using System;
using System.Threading.Tasks;
using Script.ConfigEnum;
using Script.Mgr;

public sealed class ConfigManager : BaseSingleTon<ConfigManager>
{
    public readonly CfgAudioClip cfgAudioClip = new CfgAudioClip();

    public async Task LoadConfigs()
    {
        // 加载AudioClip配置
        await cfgAudioClip.Load(() =>
        {
            // 加入到资源路径字典
            cfgAudioClip.AllConfigs.ForEach(a => DataMgr.Add(a.annotate, a.path));
            // 加入到音频路径字典
            cfgAudioClip.AllConfigs.ForEach(a =>
            {
                if (!string.IsNullOrEmpty(a.enumName) && DataMgr.AssetPathDic.TryGetValue(a.annotate, out string path))
                {
                    var enumVal = (EnumAudioClip)Enum.Parse(typeof(EnumAudioClip), a.enumName);
                    DataMgr.AudioClipPathDic[enumVal] = path;
                }
                else
                {
                    LogUtil.Warning($"AudioClip 初始化失败：annotate= {a.annotate} ");
                }
            });
            LogUtil.Debug($"AudioClip 初始化完成, count:{DataMgr.AudioClipPathDic.Count}");

            // 通知事件
            EventMgr.ExecuteEvent(EventName.AssetLoadProgress);
        });
    }
}