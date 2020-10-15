using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace BBRate_To_Redis
{
    public static class StackExchangeRedisHelper
    {
        //建立連線字串
        private static readonly string Coonstr = "10.14.106.53:16380";

        //鎖定
        private static object _locker = new Object();

        //建立連線物件
        private static ConnectionMultiplexer _instance = null;

        //建構子
        static StackExchangeRedisHelper()
        {
        }

        /// <summary>
        /// 使用一個靜態屬性來返回已連接的實例，如下列中所示。這樣，一旦 ConnectionMultiplexer 斷開連接，便可以初始化新的連接實例。
        /// </summary>
        public static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            _instance = ConnectionMultiplexer.Connect(Coonstr);
                        }
                    }
                }
                //註冊如下事件
                _instance.ConnectionFailed += MuxerConnectionFailed;
                _instance.ConnectionRestored += MuxerConnectionRestored;
                _instance.ErrorMessage += MuxerErrorMessage;
                _instance.ConfigurationChanged += MuxerConfigurationChanged;
                _instance.HashSlotMoved += MuxerHashSlotMoved;
                _instance.InternalError += MuxerInternalError;
                return _instance;
            }
        }

        /// <summary>
        /// 取得Redis連線
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase();
        }

        /// <summary>
        /// 根據key獲取緩存對象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            key = MergeKey(key);
            if (Exists(key))
            {
                return Deserialize<T>(GetDatabase().StringGet(key));
            }

            throw new Exception();
        }

        /// <summary>
        /// 設置緩存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Set<T>(string key, T value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            key = MergeKey(key);
            GetDatabase().StringSet(key, Serialize(value), expiry, when, flags);
        }

        /// <summary>
        /// 根據key獲取緩存對象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            key = MergeKey(key);
            if (Exists(key))
            {
                return GetDatabase().StringGet(key);
            }

            return string.Empty;
        }

        /// <summary>
        /// 設置緩存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetString(string key, string value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always, CommandFlags flags = CommandFlags.None)
        {
            key = MergeKey(key);
            GetDatabase().StringSet(key, value, expiry, when, flags);
        }

        /// <summary>
        /// 判斷在緩存中是否存在該key的緩存數據
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exists(string key)
        {
            return GetDatabase().KeyExists(key);  //可直接調用
        }

        /// <summary>
        /// 移除指定key的緩存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Remove(string key)
        {
            key = MergeKey(key);
            return GetDatabase().KeyDelete(key);
        }

        /// <summary>
        /// 異步設置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        //public static async Task SetAsync(string key, object value)
        //{
        //    key = MergeKey(key);
        //    await GetDatabase().StringSetAsync(key, Serialize(value));
        //}

        /// <summary>
        /// 根據key獲取緩存對象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        //public static async Task<RedisValue> GetAsync(string key)
        //{
        //    key = MergeKey(key);
        //    var value = await GetDatabase().StringGetAsync(key);
        //    return value;
        //}

        /// <summary>
        /// 實現遞增
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long Increment(string key)
        {
            key = MergeKey(key);
            //三種命令模式
            //Sync,同步模式會直接阻塞調用者，但是顯然不會阻塞其他線程。
            //Async,異步模式直接走的是Task模型。
            //Fire - and - Forget,就是發送命令，然後完全不關心最終什麼時候完成命令操作。
            //即發即棄：通過配置 CommandFlags 來實現即發即棄功能，在該實例中該方法會立即返回，如果是string則返回null 如果是int則返回0.這個操作將會繼續在後台運行，一個典型的用法頁面計數器的實現：
            return GetDatabase().StringIncrement(key, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// 實現遞減
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static long Decrement(string key, string value)
        {
            key = MergeKey(key);
            return GetDatabase().HashDecrement(key, value, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// GetServer方法會接收一個EndPoint類或者一個唯一標識一台服務器的鍵值對
        /// 有時候需要為單個服務器指定特定的命令
        /// 使用IServer可以使用所有的shell命令，比如：
        /// DateTime lastSave = server.LastSave();
        /// ClientInfo[] clients = server.ClientList();
        /// 如果報錯在連接字符串後加 ,allowAdmin=true;
        /// </summary>
        /// <returns></returns>
        public static IServer GetServer(string host, int port)
        {
            IServer server = Instance.GetServer(host, port);
            return server;
        }

        /// <summary>
        /// 獲取全部終結點
        /// </summary>
        /// <returns></returns>
        public static EndPoint[] GetEndPoints()
        {
            EndPoint[] endpoints = Instance.GetEndPoints();
            return endpoints;
        }

        /// <summary>
        /// 這裏的 MergeKey 用來拼接 Key 的前綴，具體不同的業務模塊使用不同的前綴。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string MergeKey(string key)
        {
            return key;
        }

        /// <summary>
        /// 串行化對象
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        /// <summary>
        /// 反串行化對象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        /// <summary>
        /// 配置更改時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            Console.WriteLine("Configuration changed: " + e.EndPoint);
        }

        /// <summary>
        /// 發生錯誤時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Console.WriteLine("ErrorMessage: " + e.Message);
        }

        /// <summary>
        /// 重新創建連接之前的錯誤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine("ConnectionRestored: " + e.EndPoint);
        }

        /// <summary>
        /// 連接失敗 ， 如果重新連接成功你將不會收到這個通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Console.WriteLine("重新連接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }

        /// <summary>
        /// 更改集羣
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            Console.WriteLine("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }

        /// <summary>
        /// redis類庫錯誤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            Console.WriteLine("InternalError:Message" + e.Exception.Message);
        }

        //場景不一樣，選擇的模式便會不一樣，大家可以按照自己系統架構情況合理選擇長連接還是Lazy。
        //創建連接後，通過調用ConnectionMultiplexer.GetDatabase 方法返回對 Redis Cache 數據庫的引用。從 GetDatabase 方法返回的對象是一個輕量級直通對象，不需要進行存儲。

        /// <summary>
        /// 使用的是Lazy，在真正需要連接時創建連接。
        /// 延遲加載技術
        /// 微軟azure中的配置 連接模板
        /// </summary>
        //private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        //{
        //    //var options = ConfigurationOptions.Parse(constr);
        //    ////options.ClientName = GetAppName(); // only known at runtime
        //    //options.AllowAdmin = true;
        //    //return ConnectionMultiplexer.Connect(options);
        //    ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(Coonstr);
        //    muxer.ConnectionFailed += MuxerConnectionFailed;
        //    muxer.ConnectionRestored += MuxerConnectionRestored;
        //    muxer.ErrorMessage += MuxerErrorMessage;
        //    muxer.ConfigurationChanged += MuxerConfigurationChanged;
        //    muxer.HashSlotMoved += MuxerHashSlotMoved;
        //    muxer.InternalError += MuxerInternalError;
        //    return muxer;
        //});

        #region 當作消息代理中間件使用 一般使用更專業的消息隊列來處理這種業務場景

        /// <summary>
        /// 當作消息代理中間件使用
        /// 消息組建中,重要的概念便是生產者,消費者,消息中間件。
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static long Publish(string channel, string message)
        {
            ISubscriber sub = Instance.GetSubscriber();
            //return sub.Publish("messages", "hello");
            return sub.Publish(channel, message);
        }

        /// <summary>
        /// 在消費者端得到該消息並輸出
        /// </summary>
        /// <param name="channelFrom"></param>
        /// <returns></returns>
        public static void Subscribe(string channelFrom)
        {
            ISubscriber sub = Instance.GetSubscriber();
            sub.Subscribe(channelFrom, (channel, message) =>
            {
                Console.WriteLine((string)message);
            });
        }

        #endregion
    }
}
