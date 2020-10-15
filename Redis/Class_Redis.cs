using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

using ServiceStack;
using ServiceStack.Text;
using ServiceStack.Redis;
using ServiceStack.DataAnnotations;
using StackExchange.Redis;
using System.Threading;

namespace BBRate_To_Redis
{
    /// <summary>
    /// 建立Redis單一連線
    /// </summary>
    public sealed class RedisConnection
    {
        private static Lazy<RedisConnection> lazy = new Lazy<RedisConnection>(() =>
        {
            if (_settingOption == null)
                throw new InvalidOperationException("Please call Init() first.");
            return new RedisConnection();
        });

        private static ConfigurationOptions _settingOption = null;

        public readonly ConnectionMultiplexer ConnectionMultiplexer;

        public static RedisConnection Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private RedisConnection()
        {
            ConnectionMultiplexer = ConnectionMultiplexer.Connect(_settingOption);
        }

        public static void Init(ConfigurationOptions settingOption)
        {
            _settingOption = settingOption;
        }
    }

    //==================================================================================================================

    public class Class_Redis : IDisposable
    {
        #region 宣告

        private ConnectionMultiplexer Redis = null;
        private ConfigurationOptions Redis_Config = null;

        private static Class_Redis _Instance = null;
        public static Class_Redis Instance //  靜態宣告 only one
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Class_Redis(); //  獨體模式，只會有一個實體
                }
                return _Instance;
            }
        }

        public RedisSetting Redis_Setting = null;                                                           //  Redis設定

        public int stateInt = 0;                                                                            //  連線狀態
        private RedisState _ConnectStatus = RedisState.None;
        public RedisState ConnectStatus
        {
            get
            {
                return _ConnectStatus;
            }
            set
            {
                // 0.未連線, 10.準備連線, 20已連線, 30.準備斷線
                if (_ConnectStatus != value)
                {
                    _ConnectStatus = value;
                    switch (_ConnectStatus)
                    {
                        case RedisState.None:
                        case RedisState.Fail:
                            stateInt = 0;
                            break;
                        case RedisState.Ready:
                            stateInt = 20;
                            break;
                        case RedisState.Success:
                            stateInt = 10;
                            break;
                        case RedisState.Lost:
                            stateInt = 30;
                            break;
                    }
                    if (Action_StateChangeNotify != null)
                    {
                        Action_StateChangeNotify(stateInt, _ConnectStatus);
                    }

                }
            }

        }

        public bool isConnected
        {
            get
            {
                if (ConnectStatus == RedisState.Ready)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Action<string, string> Action_Appand_Log;                                                    //  委派函式-Log
        public Action<int, RedisState> Action_StateChangeNotify;                                            //  委派函式-連線狀態變更
        public Action<Class_Redis_Subscribe> Action_Get_Redis_Subscribe;                                    //  委派函式-接收Publish之內容(此為Debug用)

        //  Thread為接收IPush轉為BBRate_Data物件之資料並Publish
        private ManualResetEvent MRE_Redis;
        private Thread Thread_Redis = null;
        private List<BBRate_Data> List_Redis = null;

        //  Thread為接收Publish之內容(此為Debug用)
        private ManualResetEvent MRE_Redis_Subscribe;
        private Thread Thread_Redis_Subscribe = null;
        private List<Class_Redis_Subscribe> List_Redis_Subscribe = null;

        private bool Is_CloseThread = false;

        private System.Timers.Timer Timer_To_Active = new System.Timers.Timer();                            //  開盤檢查(08:45)
        public delegate void Delegate_Timer_To_Active(object sender, System.Timers.ElapsedEventArgs e);     //  Timer_To_Active.Elapsed之委派
        public Delegate_Timer_To_Active DelegateObj;                                                        //  Timer_To_Active.Elapsed之委派

        #endregion

        public Class_Redis()
        {
            Is_CloseThread = false;
            Event_Report_IsOpen += new EventHandler<CustomEventArgs>(Class_Redis_Event_Report_IsOpen);      //  建立開盤/未開盤觸發事件
            
            #region Timer To Active 開盤檢查(08:45)
            DelegateObj = Action_Timer_To_Active;

            Timer_To_Active.Interval = 1000;
            Timer_To_Active.Elapsed += new System.Timers.ElapsedEventHandler((object sender, System.Timers.ElapsedEventArgs e) => 
            {
                //  因為這裡屬於System.Timers.Timer的執行緒 所以使用委派
                DelegateObj(sender, e);
            });
            Timer_To_Active.Start();
            #endregion

            List_Redis = new List<BBRate_Data>();
            MRE_Redis = new ManualResetEvent(false);
            Thread_Redis = new Thread(() => { Publish_BBRate_Datas(); });
            Thread_Redis.IsBackground = true;
            Thread_Redis.Name = "Thread_Redis_" + this.GetHashCode();
            Thread_Redis.Start();

            List_Redis_Subscribe = new List<Class_Redis_Subscribe>();
            MRE_Redis_Subscribe = new ManualResetEvent(false);
            Thread_Redis_Subscribe = new Thread(() => { Subscribe_Datas(); });
            Thread_Redis_Subscribe.IsBackground = true;
            Thread_Redis_Subscribe.Name = "Thread_Redis_Subscribe_" + this.GetHashCode();
            Thread_Redis_Subscribe.Start();

            Initialize();
        }

        ~Class_Redis()
        {
            Dispose();
        }

        /// <summary>
        /// Timer To Active.Elapsed委派 開盤檢查之Action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Action_Timer_To_Active(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int _diff = -2;
                DateTime _dt_now = DateTime.Now;

                DateTime _dt_comp_Mo_Start = new DateTime(_dt_now.Year, _dt_now.Month, _dt_now.Day, 08, 44, 55);    //  日盤開-是否到了今天08:45:00
                DateTime _dt_comp_Mo_End = new DateTime(_dt_now.Year, _dt_now.Month, _dt_now.Day, 13, 50, 00);      //  日盤收-是否到了今天13:50:00
                DateTime _dt_comp_AF_Start = new DateTime(_dt_now.Year, _dt_now.Month, _dt_now.Day, 14, 59, 55);    //  夜盤開-是否到了今天15:00:00
                DateTime _dt_comp_AF_End = new DateTime(_dt_now.Year, _dt_now.Month, _dt_now.Day, 05, 00, 00);      //  夜盤收-是否到了今天05:00:00

                #region 日-開盤
                if (Class_Global.Market_Status == MarketStatus.Close &&
                    (((_dt_now - _dt_comp_Mo_Start.AddSeconds(_diff)).TotalSeconds >= 0) && ((_dt_now - _dt_comp_Mo_Start).TotalSeconds < 0)))
                {
                    //  開盤前2秒
                    Appand_Log("Redis", string.Format("[日-開盤前{0}秒再次紀錄當前最後一筆]", _diff));
                    Record_First_BBRate_Data();

                    //  清空Redis某資料庫
                    Appand_Log("Redis", string.Format("[日-開盤前清空Redis資料庫]"));
                    Redis_FlushDatabase();
                }

                if (((_dt_now - _dt_comp_Mo_Start).TotalSeconds >= 0) && ((_dt_now - _dt_comp_Mo_End).TotalSeconds < 0))
                {
                    //  日-開盤
                    if (Class_Global.Market_Status == MarketStatus.Close)
                    {
                        Appand_Log("Redis", string.Format("[日-開盤]"));
                        Class_Global.Market_Status = MarketStatus.Open;
                        if (Event_Report_IsOpen != null)
                        {
                            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                        }
                    }
                }
                #endregion

                #region 日-收盤
                if (((_dt_now - _dt_comp_Mo_End).TotalSeconds >= 0) && ((_dt_now - _dt_comp_AF_Start).TotalSeconds < 0))
                {
                    //  日-收盤
                    if (Class_Global.Market_Status == MarketStatus.Open)
                    {
                        Appand_Log("Redis", string.Format("[日-收盤]"));
                        Class_Global.Market_Status = MarketStatus.Close;
                        if (Event_Report_IsOpen != null)
                        {
                            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                        }
                    }
                }
                #endregion

                #region 夜-開盤
                if (Class_Global.Market_Status == MarketStatus.Close &&
                    (((_dt_now - _dt_comp_AF_Start.AddSeconds(_diff)).TotalSeconds >= 0) && ((_dt_now - _dt_comp_AF_Start).TotalSeconds < 0)))
                {
                    //  開盤前2秒
                    //Appand_Log("Redis", string.Format("[夜-開盤前{0}秒再次紀錄當前最後一筆]", _diff));
                    //Record_First_BBRate_Data();

                    //  清空Redis某資料庫
                    Appand_Log("Redis", string.Format("[夜-開盤前清空Redis資料庫]"));
                    Redis_FlushDatabase();
                }

                if (((_dt_now - _dt_comp_AF_Start).TotalSeconds >= 0) && ((_dt_now - _dt_comp_AF_End.AddDays(1)).TotalSeconds < 0))
                {
                    //  夜-開盤
                    if (Class_Global.Market_Status == MarketStatus.Close)
                    {
                        Appand_Log("Redis", string.Format("[夜-開盤]"));
                        Class_Global.Market_Status = MarketStatus.Open;
                        if (Event_Report_IsOpen != null)
                        {
                            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                        }
                    }
                }
                #endregion

                #region 夜-收盤
                if (((_dt_now - _dt_comp_AF_End).TotalSeconds >= 0) && ((_dt_now - _dt_comp_Mo_Start).TotalSeconds < 0))
                {
                    //  夜-收盤
                    if (Class_Global.Market_Status == MarketStatus.Open)
                    {
                        Appand_Log("Redis", string.Format("[夜-收盤]"));
                        Class_Global.Market_Status = MarketStatus.Close;
                        if (Event_Report_IsOpen != null)
                        {
                            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                        }
                    }
                }
                #endregion

                #region 舊寫法
                //DateTime _dt_comp = new DateTime(_dt_now.Year, _dt_now.Month, _dt_now.Day, 08, 45, 00);             //  是否到了今天08:45:00(開盤時間)
                //if ((_dt_now - _dt_comp).TotalSeconds >= 0)
                //{
                //    //  開盤
                //    if (Class_Global.Market_Status == MarketStatus.Close)
                //    {
                //        Class_Global.Market_Status = MarketStatus.Open;
                //        if (Event_Report_IsOpen != null)
                //        {
                //            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                //        }
                //    }
                //}
                //else
                //{
                //    //  未開盤
                //    if (Class_Global.Market_Status == MarketStatus.Open)
                //    {
                //        Class_Global.Market_Status = MarketStatus.Close;
                //        if (Event_Report_IsOpen != null)
                //        {
                //            Event_Report_IsOpen(this, new CustomEventArgs(_dt_now.ToString(Class_Log.Time_Format), Class_Global.Market_Status.ToString()));
                //        }
                //    }
                //}

                //if (Class_Global.Market_Status == MarketStatus.Close && ((_dt_now - _dt_comp).TotalSeconds - 1) >= 0)
                //{
                //    //  開盤前1秒
                //    Appand_Log("Redis", string.Format("[開盤前1秒再次紀錄當前最後一筆]"));
                //    Record_First_BBRate_Data();

                //    //  清空Redis某資料庫
                //    Redis_FlushDatabase();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Action_Timer_To_Active", ex.ToString());
            }
        }

        /// <summary>
        /// Thread_Redis DoWork
        /// </summary>
        private void Publish_BBRate_Datas()
        {
            while (Is_CloseThread == false)
            {
                if (MRE_Redis.WaitOne(100) == false)
                {
                    continue;
                }

                BBRate_Data _BD = null;
                lock (List_Redis)
                {
                    if (List_Redis.Count > 0)
                    {
                        _BD = List_Redis[0];
                    }
                    else
                    {
                        MRE_Redis.Reset();

                        _BD = null;
                        continue;
                    }
                }

                try
                {
                    if (_BD != null)
                    {
                        //  Publish (只Publish非本點=iGKS2.TxRpt.IB2)
                        if (!string.IsNullOrEmpty(_BD.Subject_Str) && _BD.Subject_Str == "iGKS2.TxRpt.IB2")
                        {
                            string str_KeyTime = DateTime.Now.ToString("HHmmss.ffffff");
                            string str_BD = _BD.Redis_Publish_String(str_KeyTime);
                            //Redis_Publish(str_BD);
                            //Redis_StringSet(str_KeyTime, str_BD);
                        }

                        lock (List_Redis)
                        {
                            List_Redis.RemoveAt(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Class_Log.Write_Log(Log_Type.Error, "Publish_BBRate_Datas_1", ex.ToString());
                }
            }

            try
            {
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Publish_BBRate_Datas_2", ex.ToString());
            }
            finally
            {
                if (Thread_Redis != null && Thread_Redis.IsAlive)
                {
                    Thread_Redis.Join(1000);
                    Thread_Redis = null;
                }
            }
        }

        /// <summary>
        /// Thread_Redis_Subscribe DoWork
        /// </summary>
        private void Subscribe_Datas()
        {
            while (Is_CloseThread == false)
            {
                if (MRE_Redis_Subscribe.WaitOne(100) == false)
                {
                    continue;
                }

                Class_Redis_Subscribe _RS = null;
                lock (List_Redis_Subscribe)
                {
                    if (List_Redis_Subscribe.Count > 0)
                    {
                        _RS = List_Redis_Subscribe[0];
                    }
                    else
                    {
                        MRE_Redis_Subscribe.Reset();

                        _RS = null;
                        continue;
                    }
                }

                try
                {
                    if (_RS != null)
                    {
                        //  傳到前台顯示
                        if (Action_Get_Redis_Subscribe != null)
                        {
                            Action_Get_Redis_Subscribe(_RS);
                        }
                        //

                        //  Debu用-從Redis DB StringGet(可以Subscribe代表Publish時也已經一起寫入DB)
                        if (Class_Global.Redis_Client_Subscribe == true)
                        {
                            string[] arr = _RS.MESSAGE.Split('|');
                            Redis_StringGet(arr[33]);
                        }
                        //

                        lock (List_Redis)
                        {
                            List_Redis_Subscribe.RemoveAt(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Class_Log.Write_Log(Log_Type.Error, "Subscribe_Datas_1", ex.ToString());
                }
            }

            try
            {
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Subscribe_Datas_2", ex.ToString());
            }
            finally
            {
                if (Thread_Redis_Subscribe != null && Thread_Redis_Subscribe.IsAlive)
                {
                    Thread_Redis_Subscribe.Join(1000);
                    Thread_Redis_Subscribe = null;
                }
            }
        }

        public void Initialize()
        {
            ConnectStatus = RedisState.None;

            #region Redis Config
            IniFile ini_Setting = new IniFile(Class_Global.Path_Setting);
            Dictionary<string, string> setInfo = ini_Setting.GetSectionValues("Redis");
            Redis_Setting = new RedisSetting()
            {
                IP = setInfo["IP"],
                Port = setInfo["Port"],
                Channel = setInfo["channel"],
                DB = setInfo["DB"]
            };

            Redis_Config = new ConfigurationOptions()
            {
                EndPoints = { { Redis_Setting.IP, int.Parse(Redis_Setting.Port) } },
                ConnectTimeout = 10000,
                //SyncTimeout = 1500,
                ConnectRetry = 3,
                AbortOnConnectFail = false, //  if the first connection attempt fails, ConnectionMultiplexer will retry in the background rather than throwing an exception.
                AllowAdmin = true,          //  清除資料庫需要Admin權限,所以要打開
            };
            #endregion
        }

        public void Dispose()
        {
            try
            {
                Is_CloseThread = true;
                Timer_To_Active.Stop();
                Timer_To_Active.Dispose();
                DisConnect();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Dispose", ex.ToString());
            }
        }

        public void Connect()
        {
            #region Redis Connection Sample
            //RedisConnection.Init(string.Format("{0}:{1}", Redis_Setting.IP, Redis_Setting.Port));
            //Redis = RedisConnection.Instance.ConnectionMultiplexer;
            //Appand_Log("Redis", string.Format("[Redis Connect to Redis Server: {0}:{1}]", Redis_Setting.IP, Redis_Setting.Port));

            //ISubscriber sub = Redis.GetSubscriber();
            //sub.Subscribe(Redis_Setting.Channel, (channel, message) =>
            //{
            //    Console.WriteLine(string.Format("[{0}] channel:{1} message:{2}", DateTime.Now.ToString(Class_Log.Time_Format), channel, message));
            //});
            #endregion

            //  建立連線
            RedisConnection.Init(Redis_Config);
            Redis = RedisConnection.Instance.ConnectionMultiplexer;
            Redis.IncludeDetailInExceptions = true;
            Redis.ConnectionFailed += new EventHandler<ConnectionFailedEventArgs>(Redis_ConnectionFailed);
            Redis.ConnectionRestored += new EventHandler<ConnectionFailedEventArgs>(Redis_ConnectionRestored);
            Redis.ErrorMessage += new EventHandler<RedisErrorEventArgs>(Redis_ErrorMessage);
            Redis.InternalError += new EventHandler<InternalErrorEventArgs>(Redis_InternalError);

            Appand_Log("Redis", string.Format("[Redis Connect to Redis Server: {0}:{1}]", Redis_Setting.IP, Redis_Setting.Port));

            if (Redis.IsConnected == true)
            {
                ConnectStatus = RedisState.Ready;
                Appand_Log("Redis", string.Format("[Redis Connection Ready]"));
            }
            else
            {
                ConnectStatus = RedisState.Fail;
                Appand_Log("Redis", string.Format("[Redis Connection Fail]"));
            }

            //  建立Subscribe(Debug用)
            ISubscriber _Sub = Redis.GetSubscriber();
            _Sub.Subscribe(Redis_Setting.Channel, (channel, message) =>
            {
                Class_Redis_Subscribe _RS = new Class_Redis_Subscribe() { CHANNEL = channel, MESSAGE = message };

                lock (List_Redis_Subscribe)
                {
                    List_Redis_Subscribe.Add(_RS);
                }
                MRE_Redis_Subscribe.Set();
            });
        }

        public void DisConnect()
        {
            Redis.Close();
            Redis.CloseAsync();
            Redis.Dispose();
            ConnectStatus = RedisState.Lost;
            Appand_Log("Redis", string.Format("[Redis DisConnect To Server]"));
        }

        //==================================================================================================================Event Object

        public class CustomEventArgs : EventArgs
        {
            private string _Event_Time;
            public string Event_Time
            {
                get { return _Event_Time; }
                set { _Event_Time = value; }
            }

            private string _Event_Msg;
            public string Event_Msg
            {
                get { return _Event_Msg; }
                set { _Event_Msg = value; }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="event_type">0:Run Report 1:Error Report</param>
            /// <param name="s">Report Message</param>
            public CustomEventArgs(string event_time, string event_msg)
            {
                _Event_Time = event_time;
                _Event_Msg = event_msg;
            }
        }

        private event EventHandler<CustomEventArgs> Event_Report_IsOpen;
        private void Class_Redis_Event_Report_IsOpen(object sender, Class_Redis.CustomEventArgs e)
        {
            try
            {
                MarketStatus b_msg = MarketStatus.Close;
                if (Enum.TryParse(e.Event_Msg, out b_msg) == true)
                {
                    if (b_msg == MarketStatus.Open)
                    {
                        //  開盤
                    }
                    else
                    {
                        //  收盤
                        //Appand_Log("Redis", string.Format("[收盤紀錄當前最後一筆]"));
                        //Record_First_BBRate_Data();
                    }
                }
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Class_Redis_Event_Report", ex.ToString());
            }
        }

        //==================================================================================================================Event

        /// <summary>
        /// 連接失敗 ， 如果重新連接成功你將不會收到這個通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redis_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            ConnectStatus = RedisState.Fail;
            Appand_Log("Redis", string.Format("[Redis Connection Fail Endpoint:{0} FailureType:{1} Msg:{2}]", e.EndPoint, e.FailureType, (e.Exception == null ? "" : e.Exception.Message)));
        }

        /// <summary>
        /// 重新創建連接之前的錯誤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redis_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            Appand_Log("Redis", string.Format("[Redis Connection Restored:{0}]", e.EndPoint));
        }

        /// <summary>
        /// 發生錯誤時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redis_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            Appand_Log("Redis", string.Format("[Redis ErrorMessage:{0}]", e.Message));
        }

        /// <summary>
        /// redis類庫錯誤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redis_InternalError(object sender, InternalErrorEventArgs e)
        {
            Appand_Log("Redis", string.Format("[Redis InternalError:{0}]", e.Exception.Message));
        }

        //==================================================================================================================Function

        private void Appand_Log(string _from, string _msg)
        {
            if (Action_Appand_Log != null)
            {
                Action_Appand_Log(_from, _msg);
            }
        }

        /// <summary>
        /// 前台收到IPush轉換成BBRate_Data物件之資料 , 新增到Publish Queue中
        /// </summary>
        /// <param name="_BD"></param>
        public void Add_Redis_Data(BBRate_Data _BD)
        {
            try
            {
                lock (List_Redis)
                {
                    List_Redis.Add(_BD);
                    Appand_Log("Redis", string.Format("[Add Redis Data]"));
                }
                MRE_Redis.Set();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Add_Redis_Data", ex.ToString());
            }
        }

        /// <summary>
        /// Redis Publish
        /// </summary>
        /// <param name="str_msg"></param>
        private void Redis_Publish(string str_msg)
        {
            try
            {
                if (string.IsNullOrEmpty(str_msg))
                {
                    return;
                }

                ISubscriber sub = Redis.GetSubscriber();
                sub.Publish(Redis_Setting.Channel, str_msg);
                Appand_Log("Redis Publish", string.Format("[Redis Publish Channel:{0}][msg:{1}]", Redis_Setting.Channel, str_msg));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_Publish", ex.ToString());
            }
        }

        /// <summary>
        /// 資料存入Redis某資料庫
        /// </summary>
        /// <param name="str_key"></param>
        /// <param name="str_msg"></param>
        private void Redis_StringSet(string str_key, string str_msg)
        {
            try
            {
                if (string.IsNullOrEmpty(str_key) || string.IsNullOrEmpty(str_msg))
                {
                    return;
                }
                
                IDatabase db = Redis.GetDatabase(int.Parse(Redis_Setting.DB));
                db.StringSet(str_key, str_msg);
                Appand_Log("Redis StringSet", string.Format("[Redis StringSet Key:{0}]", str_key));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_StringSet", ex.ToString());
            }
        }

        /// <summary>
        /// Redis某資料庫取得資料
        /// </summary>
        /// <param name="str_key"></param>
        /// <returns></returns>
        private string Redis_StringGet(string str_key)
        {
            try
            {
                if (string.IsNullOrEmpty(str_key))
                {
                    return string.Empty;
                }

                IDatabase db = Redis.GetDatabase(int.Parse(Redis_Setting.DB));
                RedisValue r_value = db.StringGet(str_key);
                Appand_Log("Redis StringGet", string.Format("[Redis StringGet value:{0}]", r_value.ToString()));

                return r_value.ToString();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_StringGet", ex.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// 清空Redis某資料庫
        /// </summary>
        private void Redis_FlushDatabase()
        {
            try
            {
                IServer server = Redis.GetServer(string.Format("{0}:{1}", Redis_Setting.IP, Redis_Setting.Port));
                server.FlushDatabase(int.Parse(Redis_Setting.DB));
                Appand_Log("Redis FlushDatabase", string.Format("[Redis FlushDatabase Server:{0}:{1} DB:{2}]", Redis_Setting.IP, Redis_Setting.Port, Redis_Setting.DB));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_FlushDatabase", ex.ToString());
            }
        }

        /// <summary>
        /// 將當前最後一筆寫入ini , 並記錄於Class_Global.Dic_First_BBRate_Data
        /// </summary>
        private void Record_First_BBRate_Data()
        {
            try
            {
                IniFile ini_Setting = new IniFile(Class_Global.Path_Setting);

                lock (Class_Global.Dic_Current_BBRate_Data)
                {
                    //  將當前最後一筆寫入ini , 並記錄於Class_Global.Dic_First_BBRate_Data
                    foreach (KeyValuePair<string, BBRate_Data> obj in Class_Global.Dic_Current_BBRate_Data)
                    {
                        Class_Global.Dic_First_BBRate_Data[obj.Key] = obj.Value;
                        ini_Setting.WriteValue("First_BBRate", obj.Key, obj.Value.ToString());
                        Appand_Log("Redis", string.Format("[收盤:{0}]", obj.Value.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Record_First_BBRate_Data", ex.ToString());
            }
        }
    }
}
