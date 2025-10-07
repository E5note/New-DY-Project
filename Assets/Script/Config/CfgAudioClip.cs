using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.ConfigEnum;
using Script.Mgr;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class RowCfgAudioClip
{
    public int id; //id
    public string annotate; //注释
    public string enumName; //枚举名
    public float volume; //音量大小
    public float cD; //作为高频音效的CD
    public string path; //路径
}

    public class CfgAudioClip
    {
        private readonly Dictionary<int, RowCfgAudioClip> _configs = new Dictionary<int, RowCfgAudioClip>(); //cfgId映射row
        public RowCfgAudioClip this[Enum cid] => _configs.ContainsKey(cid.GetHashCode()) ? _configs[cid.GetHashCode()] : throw new Exception($"找不到配置 Cfg:{GetType()} configId:{cid}");
        public RowCfgAudioClip this[int cid] => _configs.ContainsKey(cid) ? _configs[cid.GetHashCode()] : throw new Exception($"找不到配置 Cfg:{GetType()} configId:{cid}");
        public List<RowCfgAudioClip> AllConfigs => _configs.Values.ToList();

        /// <summary>
        /// 获取行数据
        /// </summary>
        public RowCfgAudioClip Find(int i)
        {
            return this[i];
        }

        /// <summary>
        /// 加载表数据
        /// </summary>
        public async Task Load(UnityAction callback = null)
        {
            TextAsset configAsset = await Addressables.LoadAssetAsync<TextAsset>("Assets/AddressableAssets/Config/CfgAudioClip.txt").Task;

            if (configAsset == null)
            {
                Debug.LogError("加载配置失败: CfgAudioClip.txt");
                return;
            }

            var reader = new CsvReader();
            reader.LoadTextFromString(configAsset.text, 3); // 假设你有支持 string 输入的版本
            var rows = reader.GetRowCount();
            for (var i = 0; i < rows; ++i)
            {
                var row = reader.GetColValueArray(i);
                var data = ParseRow(row);
                if (!_configs.ContainsKey(data.id))
                {
                    _configs.Add(data.id, data);
                }
            }
            LogUtil.Debug($"加载配置完成: CfgAudioClip.txt, count:{_configs.Count}");
            
            callback?.Invoke();
        }
        
        /// <summary>
        /// 解析行
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        private RowCfgAudioClip ParseRow(string[] col)
        {
            //列越界
            if (col.Length < 6)
            {
                Debug.LogError($"配置表字段行数越界:{GetType()}");
                return null;
            }

            var data = new RowCfgAudioClip();
            var rowHelper = new RowHelper(col);
            data.id = CsvUtility.ToInt(rowHelper.ReadNextCol()); //id
            data.annotate = CsvUtility.ToString(rowHelper.ReadNextCol()); //注释
            data.enumName = CsvUtility.ToString(rowHelper.ReadNextCol()); //枚举名
            data.volume = CsvUtility.ToFloat(rowHelper.ReadNextCol()); //音量大小
            data.cD = CsvUtility.ToFloat(rowHelper.ReadNextCol()); //作为高频音效的CD
            data.path = CsvUtility.ToString(rowHelper.ReadNextCol()); //路径
            return data;
        }
    }
