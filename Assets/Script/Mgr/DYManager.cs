using System;
using System.Collections;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using TTSDK;
using TTSDK.UNBridgeLib.LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace Manager
{

    public class DYManager : MonoBehaviour
    {
        public static DYManager Instance;
        private readonly string _rewardVideoAdId = "32qo9g424p26shigue";
        private readonly string _interstitialAdId = "fd7egf5k2kce4kiui3";
        private readonly string _shareTitle = "多英雄冒险小游戏";

        private TTRewardedVideoAd _mRewardAd = null;

        private TTInterstitialAd _mInterstitialAd = null;

        //点击ID
        private string _clickId;

        //巨量访问api url
        private readonly string _apiUrl = "https://analytics.oceanengine.com/api/v2/conversion";

        // 分享视频的图片
        public Sprite shareSp; // 视频截屏的Sprite
        private bool _isRecord; //是否录制视频
        private bool _canShare; //是否能分享视频
        private bool _isCanShowInsert = true;
        private Coroutine _insertCooldownCoroutine = null; // 插屏冷却协程引用

        /*private int ReportRewardVideoCount;
    private int ReportInsertVideoCount;*/
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
            TT.InitSDK();
            PreloadRewardVideoAd();
            PreloadInterstitialAd();
            OnAppStart();

            /*ReportRewardVideoCount = TT.PlayerPrefs.GetInt("ReportRewardCount", 0);
        ReportInsertVideoCount = TT.PlayerPrefs.GetInt("ReportInsertCount", 0);*/
        }

        #region 冷却控制

        private IEnumerator IEShowInsert(int time)
        {
            yield return new WaitForSecondsRealtime(time);
            Debug.Log("插屏广告冷却完成");
            _isCanShowInsert = true;
            _insertCooldownCoroutine = null;
        }

        private void StartInsertCooldown(int seconds)
        {
            if (_insertCooldownCoroutine != null)
            {
                StopCoroutine(_insertCooldownCoroutine);
                Debug.Log("已有冷却协程，停止并重新开始");
            }

            _isCanShowInsert = false;
            _insertCooldownCoroutine = StartCoroutine(IEShowInsert(seconds));
        }


        #endregion

        #region 侧边栏

        public void OpenSidebar(Action callback)
        {
            var data = new JsonData { ["scene"] = "sidebar" };
            TT.NavigateToScene(data, () => { callback?.Invoke(); }, () => { },
                (errCode, errMsg) => { Debug.Log($"navigate to scene error, errCode:{errCode}, errMsg:{errMsg}"); });
        }

        #endregion

        #region 广告

        /// <summary>
        /// 激励广告
        /// </summary>
        /// <param name="completeCallback"></param>
        public void OpenRewardVideo(Action completeCallback)
        {
            if (_mRewardAd == null)
            {
                Debug.Log("激励视频广告未准备好");
                PreloadRewardVideoAd();
                return;
            }

            _mRewardAd.OnClose += (isend, count) =>
            {
                if (isend)
                {
                    //上报巨量
                    Request_Watch();
                    //回调
                    completeCallback?.Invoke();
                }

                //预加载
                PreloadRewardVideoAd();
            };
            Request_active_pay();
            //显示激励视频
            _mRewardAd.Show();
            //冷却
            StartInsertCooldown(60); // 替换旧的协程调用
        }

        /// <summary>
        /// 插屏广告
        /// </summary>
        public void OpenInsert()
        {
            if (!_isCanShowInsert || Time.time < 30)
            {
                if (!_isCanShowInsert)
                {
                    Debug.Log("插屏广告冷却中");
                }
                else
                {
                    Debug.Log("小游戏启动前30秒，不能展示插屏广告");
                }

                return;
            }

            if (_mInterstitialAd == null)
            {
                Debug.Log("插屏广告未准备好");
                PreloadInterstitialAd();
                return;
            }

            Debug.Log("展示预加载的插屏广告");
            StartInsertCooldown(60); // 替换旧的协程调用

            _mInterstitialAd.OnClose += () =>
            {
                Debug.Log("插屏广告关闭");
                PreloadInterstitialAd();
            };
            _mInterstitialAd.Show();
            //上报巨量
          
            Request_Watch();
        }

        #endregion

        #region 录屏

        /// <summary>
        /// 开始录屏
        /// </summary>
        public void RecordVideo(UnityAction callback)
        {
            // 正在录制 && 能分享
            if (_isRecord && _canShare)
            {
                // 停止录屏
                TT.GetGameRecorder().Stop();
                _isRecord = false;
                _canShare = false;
                callback?.Invoke();
                return;
            }

            // 没有录制
            if (!_isRecord)
            {
                TT.GetGameRecorder().Start();
                _isRecord = true;
                shareSp = null;
                shareSp = CaptureFrame();
                StartCoroutine(StartRecord());
            }
        }

        IEnumerator StartRecord()
        {
            yield return new WaitForSeconds(3);
            _canShare = true;
        }


        /// <summary>
        /// 分享录屏视频
        /// </summary>
        public void ShareVideo()
        {
            TT.GetGameRecorder().ShareVideo(
                // 分享成功的回调
                (result) =>
                {
                    // 处理分享成功的逻辑
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
                        }
                    }

                    Console.WriteLine("视频分享成功！");
                },
                // 分享失败的回调
                (errMsg) =>
                {
                    // 处理分享失败的逻辑
                    Console.WriteLine($"视频分享失败: {errMsg}");
                },
                // 分享取消的回调
                () =>
                {
                    // 处理分享取消的逻辑
                    Console.WriteLine("用户取消了视频分享");
                }
            );
        }


        /// <summary>
        /// 截屏
        /// </summary>
        /// <returns></returns>
        public Sprite CaptureFrame()
        {
            var captureCamera = Camera.main;
            if (captureCamera == null)
            {
                Debug.LogError("没有找到可用的相机！");
                return null;
            }

            // 获取屏幕分辨率
            int width = Screen.width;
            int height = Screen.height;

            // 创建一个RenderTexture用于渲染相机画面
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            // 保存相机原来的目标纹理
            RenderTexture originalTarget = captureCamera.targetTexture;

            // 设置相机的目标纹理为我们创建的RenderTexture
            captureCamera.targetTexture = renderTexture;
            // 让相机渲染一帧
            captureCamera.Render();

            // 激活RenderTexture以便读取像素
            RenderTexture.active = renderTexture;

            // 创建纹理并读取像素
            var capturedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            capturedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capturedTexture.Apply();

            // 恢复相机原来的设置
            captureCamera.targetTexture = originalTarget;
            RenderTexture.active = null;
            // 释放RenderTexture资源
            Destroy(renderTexture);
            return Sprite.Create(
                capturedTexture,
                new Rect(0, 0, capturedTexture.width, capturedTexture.height),
                new Vector2(0.5f, 0.5f) // 中心点
            );
        }

        #endregion

        #region 分享

        public void OpenShare()
        {
            TT.ShareAppMessage(new JsonData()
            {
                ["title"] = _shareTitle,
                ["query"] = ""
            }, (res) => { print("分享成功！"); });
        }

        #endregion

        #region 排行榜

        /// <summary>
        /// 上传排行榜的数据
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="priority">权重</param>
        public void UploadRankData(string score, int priority = 0)
        {
            var paramJson = new JsonData
            {
                // 数据类型
                ["dataType"] = 0,
                // 玩家分数
                ["value"] = score,
                // 权重
                ["priority"] = priority,
            };
            TT.SetImRankData(paramJson);
        }

        /// <summary>
        /// 打开排行榜
        /// </summary>
        public void OpenRank()
        {
            // 检查登录状态
            TT.CheckSession(() =>
            {
                var paramJson = new JsonData
                {
                    ["rankType"] = nameof(RankType.day),
                    ["dataType"] = 0,
                    ["relationType"] = nameof(RelationType.all),
                    ["suffix"] = "分",
                    ["rankTitle"] = "每天刷新的排行榜",
                };
                // 打开排行榜
                TT.GetImRankList(paramJson);
            }, (failed) =>
            {
                // 未登录，跳转到登录界面
                TT.Login((code, anony, isLogin) =>
                {
                    // 登录成功，重新打开排行榜
                    OpenRank();
                }, (err) => { Debug.Log("登录失败:" + err); });
            });

        }

        // 关系类型
        enum RelationType
        {
            friend,
            all
        }

        // 排行榜类型
        enum RankType
        {
            day,
            week,
            month,
            all
        }


        // 获取排行榜数据
        public void GetRankData()
        {
            var paramJson = new JsonData
            {
                ["relationType"] = nameof(RelationType.all),
                ["dataType"] = 0,
                ["rankType"] = nameof(RankType.day),
                ["pageNum"] = 1,
                ["pageSize"] = 40,
            };
            TT.GetImRankData(paramJson,
                (ref TTRank.RankData data) =>
                {
                    Debug.Log(data.ToString());
                    Debug.Log($"data: {data}");
                    Debug.Log($"data: {data.Items.Count}");
                    for (var i = 0; i < data.Items.Count; i++)
                    {
                        Debug.Log($"item-->nickName :{data.Items[i].Nickname}, openId:{data.Items[i].OpenId}");
                    }

                    Debug.Log("GetImRankDataNew true");
                }, msg => { Debug.Log("GetImRankData " + msg); });
        }

        #endregion

        #region 数据保存

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <param name="saveName">数据名 可空</param>
        /// <typeparam name="T">保存的游戏数据类型</typeparam>
        /// <returns></returns>
        public static bool Save<T>(T serializableObject, [CanBeNull] string saveName = "")
        {
            return TT.Save<T>(serializableObject);
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="saveName">数据名字 可空</param>
        /// <typeparam name="T">加载的数据类型</typeparam>
        /// <returns></returns>
        public static T LoadSaving<T>([CanBeNull] string saveName = "")
        {
            return TT.LoadSaving<T>(saveName);
        }


        /// <summary>
        /// 指定数据类型 - 删除数据
        /// </summary>
        /// <param name="saveName"></param>
        /// <typeparam name="T"></typeparam>
        public static void DeleteSaving<T>([CanBeNull] string saveName = "")
        {
            TT.DeleteSaving<T>(saveName);
        }

        /// <summary>
        /// 清除游戏数据
        /// </summary>
        public static void DeleteAllSavings()
        {
            TT.ClearAllSavings();
        }
        #endregion
        
        #region 埋点回传
        private void OnAppStart()
        {
            //获取小程序冷启动时的参数
            var query = TT.GetLaunchOptionsSync().Query;
            if (query.ContainsKey("ad_params"))
            {
                try
                {
                    var stringQuery = JsonMapper.ToJson(query);
                    // 解析外层JSON
                    var outerObj = JObject.Parse(stringQuery);
                    // 获取ad_params的值（这是一个URL编码的字符串）
                    string adParamsEncoded = outerObj["ad_params"].ToString();
        
                    // URL解码
                    string adParamsDecoded = Uri.UnescapeDataString(adParamsEncoded);
        
                    // 解析解码后的JSON
                    var innerObj = JObject.Parse(adParamsDecoded);
        
                    // 获取log_extra中的clickid
                    string clickId = innerObj["log_extra"]["clickid"].ToString();

                    // 赋值给ClickId
                    _clickId = clickId;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    throw;
                }
            }
            else
            {
                Debug.Log("没有获取到ad_params参数");
            }

            Request_Active();
        }

        //激活事件
        private void Request_Active()
        {
            var jsonData = new
            {
                event_type = "active",
                context = new
                {
                    ad = new
                    {
                        callback = _clickId
                    }
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            PostRequest(_apiUrl, JsonMapper.ToJson(jsonData));
        }
        //roi
        private void Request_active_pay()
        {
            var jsonData = new
            {
                event_type = "active_pay",
                context = new
                {
                    ad = new
                    {
                        callback = _clickId
                    }
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            PostRequest(_apiUrl, JsonMapper.ToJson(jsonData));
        }
          
        //游戏中看视频的事件
        private void Request_Watch()
        {
            var jsonData = new
            {
                event_type = "game_addiction",
                context = new
                {
                    ad = new
                    {
                        callback = _clickId
                    }
                },
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };
            PostRequest(_apiUrl, JsonMapper.ToJson(jsonData));
        }
        private async void PostRequest(string url, string jsonData)
        {
            // 创建请求
            UnityWebRequest request = new UnityWebRequest(url, "POST");
        
            // 设置请求头和body
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        
            // 发送请求
            await request.SendWebRequest();
        
            // 处理响应
            if (request.result == UnityWebRequest.Result.ConnectionError || 
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
        #endregion
        
        //预加载激励视频广告
        private void PreloadRewardVideoAd()
        {
            if (_mRewardAd != null)
            {
                _mRewardAd.Destroy();
            }

            var param = new CreateRewardedVideoAdParam { AdUnitId = _rewardVideoAdId };
            _mRewardAd = TT.CreateRewardedVideoAd(param);

            _mRewardAd.OnLoad += () =>
            {
                Debug.Log("激励视频广告预加载成功");
            };

            _mRewardAd.OnError += (code, msg) =>
            {
                Debug.Log($"激励视频广告预加载失败: {code}, {msg}");
            };

            _mRewardAd.Load();
        }
    
        //预加载插屏广告
        private void PreloadInterstitialAd()
        {
            if (_mInterstitialAd != null)
            {
                _mInterstitialAd.Destroy();
            }

            _mInterstitialAd = TT.CreateInterstitialAd(new CreateInterstitialAdParam
            {
                InterstitialAdId = _interstitialAdId
            });

            _mInterstitialAd.OnLoad += () =>
            {
                Debug.Log("插屏广告预加载成功");
            };

            _mInterstitialAd.OnError += (code, msg) =>
            {
                Debug.Log($"插屏广告预加载失败: {code}, {msg}");
            };

            _mInterstitialAd.Load();
        }

        // ------------------------- 自定义数据分析 -------------------------
        /*public void ReportRewardVideo()
    {
        ReportRewardVideoCount++;
        Dictionary<string, int> dict = new Dictionary<string, int>
        {
            { "Ad_ShowCount", ReportRewardVideoCount }
        };
        TT.ReportAnalytics("AdShowCount", dict);
    }

    private void ReportInsertVideo()
    {
        ReportInsertVideoCount++;
        Dictionary<string, int> dict = new Dictionary<string, int>
        {
            { "Insert_ShowCount", ReportInsertVideoCount }
        };
        TT.ReportAnalytics("InsertCount", dict);
    }*/
    }
}
