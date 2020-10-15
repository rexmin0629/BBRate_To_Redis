using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxIPUSHXLib;
using System.Threading;

namespace BBRate_To_Redis
{
    public class Class_IPush : IDisposable
    {
        #region 宣告

        private AxiPushX axiPushX;

        private static Class_IPush _Instance = null;
        public static Class_IPush Instance //  靜態宣告 only one
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new Class_IPush(); //  獨體模式，只會有一個實體
                }
                return _Instance;
            }
        }

        public IPushSetting IPush_Setting = null;                                  //  IPush設定
        public List<string> ipushSubjectList = new List<string>();                  //  訂閱紀錄

        public int stateInt = 0;                                                    //  連線狀態
        private IpushState _ConnectStatus = IpushState.None;
        public IpushState ConnectStatus
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
                        case IpushState.None:
                        case IpushState.Fail:
                            stateInt = 0;
                            break;
                        case IpushState.Ready:
                            stateInt = 20;
                            break;
                        case IpushState.Success:
                            stateInt = 10;
                            break;
                        case IpushState.Lost:
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
                if (ConnectStatus == IpushState.Ready)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Action<string, string> Action_Appand_Log;                            //  委派函式-Log
        public Action<int, IpushState> Action_StateChangeNotify;                    //  委派函式-連線狀態變更
        public Action<BBRate_Data> Action_Receive_BBRate_Data;                      //  委派函式-BBRate_Data發送前台

        private ManualResetEvent MRE_BBRate;
        private Thread Thread_BBRate = null;
        private List<BBRate_Data> List_BBRate = null;
        private bool Is_CloseThread = false;

        #endregion

        public Class_IPush()
        {
            Is_CloseThread = false;
            List_BBRate = new List<BBRate_Data>();
            MRE_BBRate = new ManualResetEvent(false);
            Thread_BBRate = new Thread(() => { Get_BBRate_Datas(); });
            Thread_BBRate.IsBackground = true;
            Thread_BBRate.Name = "Thread_BBRate_" + this.GetHashCode();
            Thread_BBRate.Start();

            Initialize();
        }
        
        private void Get_BBRate_Datas()
        {
            while (Is_CloseThread == false)
            {
                if (MRE_BBRate.WaitOne(100) == false)
                {
                    continue;
                }

                BBRate_Data _BD = null;
                lock (List_BBRate)
                {
                    if (List_BBRate.Count > 0)
                    {
                        _BD = List_BBRate[0];
                    }
                    else
                    {
                        MRE_BBRate.Reset();

                        _BD = null;
                        continue;
                    }
                }

                try
                {
                    if (_BD != null)
                    {
                        //  傳前台
                        if (Action_Receive_BBRate_Data != null)
                        {
                            Action_Receive_BBRate_Data(_BD);
                        }

                        lock (List_BBRate)
                        {
                            List_BBRate.RemoveAt(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Class_Log.Write_Log(Log_Type.Error, "Get_BBRate_Datas_1", ex.ToString());
                }
            }

            try
            {
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Get_BBRate_Datas_2", ex.ToString());
            }
            finally
            {
                if (Thread_BBRate != null && Thread_BBRate.IsAlive)
                {
                    Thread_BBRate.Join(1000);
                    Thread_BBRate = null;
                }
            }
        }

        public void Initialize()
        {
            try
            {
                _ConnectStatus = IpushState.None;

                #region IPush Config
                IniFile ini_Setting = new IniFile(Class_Global.Path_Setting);
                Dictionary<string, string> setInfo = ini_Setting.GetSectionValues("IPush");
                IPush_Setting = new IPushSetting()
                {
                    IP = setInfo["iPushIP"],
                    Port = int.Parse(setInfo["iPushPort"]),
                    Company = setInfo["Company"],
                    Product = setInfo["Product"],
                    Username = setInfo["User"],
                    Password = setInfo["Password"],
                    PacketDelay = int.Parse(setInfo["PacketDelay"]),
                };
                #endregion

                axiPushX = new AxiPushX();
                axiPushX.CreateControl();
                axiPushX.ipuship = IPush_Setting.IP;
                axiPushX.ipushport = IPush_Setting.Port;
                axiPushX.company = IPush_Setting.Company;
                axiPushX.product = IPush_Setting.Product;
                axiPushX.username = IPush_Setting.Username;
                axiPushX.password = IPush_Setting.Password;
                axiPushX.delayTime = IPush_Setting.PacketDelay;

                axiPushX.ConnectReady += new _DiPushXEvents_ConnectReadyEventHandler(axiPushX_ConnectReady);
                axiPushX.ConnectLost += new EventHandler(axiPushX_ConnectLost);
                axiPushX.ConnectFail += new _DiPushXEvents_ConnectFailEventHandler(axiPushX_ConnectFail);
                axiPushX.CommandMsg += new _DiPushXEvents_CommandMsgEventHandler(axiPushX_CommandMsg);
                axiPushX.SubjectReceived += new _DiPushXEvents_SubjectReceivedEventHandler(axiPushX_SubjectReceived);
                axiPushX.SubjectBinReceived += new _DiPushXEvents_SubjectBinReceivedEventHandler(axiPushX_SubjectBinReceived);

                System.Windows.Forms.Control control = new System.Windows.Forms.Control();
                control.Controls.Add(axiPushX);
                axiPushX.Handle.GetHashCode();  //  must to add this line
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Initialize", ex.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                Is_CloseThread = true;
                DisConnect();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Dispose", ex.ToString());
            }
        }

        public void Connect()
        {
            int result = 0;
            result = axiPushX.ipushConnect();

            if (result <= 0)
            {
                ConnectStatus = IpushState.Fail;
                Appand_Log("IPush", string.Format("[IPush {0} Connect to IPush Server Fail {1}]", axiPushX.Tag, axiPushX.ipuship));
            }
            else
            {
                ConnectStatus = IpushState.Success;
                Appand_Log("IPush", string.Format("[IPush {0} Connect to IPush Server Success {1}]", axiPushX.Tag, axiPushX.ipuship));
            }
        }

        public void DisConnect()
        {
            UnSubSubjectAll();
            axiPushX.ipushDisconnect();
            Appand_Log("IPush", string.Format("[IPush DisConnect To Server]"));
        }

        public void SendSubject(string ipushSendSubject, string msg)
        {
            axiPushX.ipushSendSubject(ipushSendSubject, msg);
            Appand_Log("IPush", string.Format("[SendSubject:{0} msg:{1}]", ipushSendSubject, msg));
        }

        //==================================================================================================================Event

        private void axiPushX_ConnectReady(object sender, _DiPushXEvents_ConnectReadyEvent e)
        {
            ConnectStatus = IpushState.Ready;
            Appand_Log("IPush", string.Format("[IPush {0} Connection Ready]", axiPushX.Tag));
        }

        private void axiPushX_ConnectLost(object sender, EventArgs e)
        {
            ConnectStatus = IpushState.Lost;
            Appand_Log("IPush", string.Format("[IPush {0} Connection Lost]", axiPushX.Tag));
        }

        private void axiPushX_ConnectFail(object sender, _DiPushXEvents_ConnectFailEvent e)
        {
            ConnectStatus = IpushState.Fail;
            Appand_Log("IPush", string.Format("[IPush {0} Connection Fail {1}]", axiPushX.Tag, e.nStatus));
        }

        private void axiPushX_CommandMsg(object sender, _DiPushXEvents_CommandMsgEvent e)
        {
            Appand_Log("IPush CommandMsg", string.Format("[IPush CommandMsg code:{0}][Msg:{1}]", e.nCode, e.strMsg));
        }

        private void axiPushX_SubjectReceived(object sender, _DiPushXEvents_SubjectReceivedEvent e)
        {
            Appand_Log("IPush SubjectReceived", string.Format("[IPush SubjectReceived:{0}][data:{1}]", e.subject, e.data));

            try
            {
                BBRate_Data _BD = Convert_2_BBRate_Data(e.subject, e.data);
                if (_BD != null)
                {
                    lock (List_BBRate)
                    {
                        List_BBRate.Add(_BD);
                    }
                    MRE_BBRate.Set();
                }
                else
                {
                    Appand_Log("IPush SubjectReceived", string.Format("BBRate_Data 轉換失敗"));
                }
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "axiPushX_SubjectReceived", ex.ToString());
            }
        }

        private void axiPushX_SubjectBinReceived(object sender, _DiPushXEvents_SubjectBinReceivedEvent e)
        {
            Appand_Log("IPush SubjectBinReceived", string.Format("[IPush SubjectBinReceived:{0}][data-type:{1}]", e.subject, e.data.GetType()));
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
        /// 訂閱
        /// </summary>
        /// <param name="subjectStr"></param>
        public void SubSubject(string subjectStr)
        {
            if (!ipushSubjectList.Contains(subjectStr))
            {
                axiPushX.ipushSubSubject(subjectStr);
                ipushSubjectList.Add(subjectStr);
                Appand_Log("IPush SubSubject", string.Format("[IPush SubSubject:{0}]", subjectStr));
            }
        }

        /// <summary>
        /// 取消訂閱
        /// </summary>
        /// <param name="subjectStr"></param>
        public void UnSubSubject(string subjectStr)
        {
            if (ipushSubjectList.Contains(subjectStr))
            {
                axiPushX.ipushUnsubSubject(subjectStr);
                ipushSubjectList.Remove(subjectStr);
                Appand_Log("IPush UnSubSubject", string.Format("[IPush UnSubSubject:{0}]", subjectStr));
            }
        }

        /// <summary>
        /// 取消所有訂閱
        /// </summary>
        public void UnSubSubjectAll()
        {
            Appand_Log("IPush UnSubSubjectAll", string.Format("[IPush SubjectList Clear All]"));
            for (int i = 0; i < ipushSubjectList.Count(); i++)
            {
                axiPushX.ipushUnsubSubject(ipushSubjectList[i]);
                Appand_Log("IPush UnSubSubjectAll", string.Format("[IPush UnSubSubject:{0}]", ipushSubjectList[i]));
            }
            ipushSubjectList.Clear();
        }

        /// <summary>
        /// IPush SubjectReceived 取得之 RowData 轉 BBRate_Data 物件
        /// </summary>
        /// <param name="str_Subject">Subject</param>
        /// <param name="str_Data">Data</param>
        /// <returns></returns>
        public BBRate_Data Convert_2_BBRate_Data(string str_Data)
        {
            BBRate_Data bb_data = null;

            try
            {
                string[] arr_RawData = str_Data.Split('|');

                bb_data = new BBRate_Data()
                {
                    Subject_Str = arr_RawData[1],
                    DATE = arr_RawData[2],
                    TIME = arr_RawData[3],
                    BBINDEXS = Utility.String_2_Decimal(arr_RawData[4]),
                    BBINDEXF = Utility.String_2_Decimal(arr_RawData[5]),
                    BBTXLONGQ = Utility.String_2_Decimal(arr_RawData[6]),
                    BBTXSHORTQ = Utility.String_2_Decimal(arr_RawData[7]),
                    BBTXQRATE = Utility.String_2_Decimal(arr_RawData[8]),
                    BBTXLONGAQ = Utility.String_2_Decimal(arr_RawData[9]),
                    BBTXSHORTAQ = Utility.String_2_Decimal(arr_RawData[10]),
                    BBTXAQRATE = Utility.String_2_Decimal(arr_RawData[11]),
                    BBMXLONGQ = Utility.String_2_Decimal(arr_RawData[12]),
                    BBMXSHORTQ = Utility.String_2_Decimal(arr_RawData[13]),
                    BBMXQRATE = Utility.String_2_Decimal(arr_RawData[14]),
                    BBMXLONGAQ = Utility.String_2_Decimal(arr_RawData[15]),
                    BBMXSHORTAQ = Utility.String_2_Decimal(arr_RawData[16]),
                    BBMXAQRATE = Utility.String_2_Decimal(arr_RawData[17]),
                    BBEXLONGQ = Utility.String_2_Decimal(arr_RawData[18]),
                    BBEXSHORTQ = Utility.String_2_Decimal(arr_RawData[19]),
                    BBEXQRATE = Utility.String_2_Decimal(arr_RawData[20]),
                    BBEXLONGAQ = Utility.String_2_Decimal(arr_RawData[21]),
                    BBEXSHORTAQ = Utility.String_2_Decimal(arr_RawData[22]),
                    BBEXAQRATE = Utility.String_2_Decimal(arr_RawData[23]),
                    BBFXLONGQ = Utility.String_2_Decimal(arr_RawData[24]),
                    BBFXSHORTQ = Utility.String_2_Decimal(arr_RawData[25]),
                    BBFXQRATE = Utility.String_2_Decimal(arr_RawData[26]),
                    BBFXLONGAQ = Utility.String_2_Decimal(arr_RawData[27]),
                    BBFXSHORTAQ = Utility.String_2_Decimal(arr_RawData[28]),
                    BBFXAQRATE = Utility.String_2_Decimal(arr_RawData[29]),
                    BBWXLONGQ = Utility.String_2_Decimal(arr_RawData[30]),
                    BBWXSHORTQ = Utility.String_2_Decimal(arr_RawData[31]),
                    BBWXQRATE = Utility.String_2_Decimal(arr_RawData[32]),
                    BBWXLONGAQ = Utility.String_2_Decimal(arr_RawData[33]),
                    BBWXSHORTAQ = Utility.String_2_Decimal(arr_RawData[34]),
                    BBWXAQRATE = Utility.String_2_Decimal(arr_RawData[35]),
                };

                bb_data.BBMXQRATE100 = bb_data.BBMXQRATE * 100;
                bb_data.BBMXLSQ = bb_data.BBMXLONGQ - bb_data.BBMXSHORTQ;
                bb_data.BBSPREAD = bb_data.BBINDEXF - bb_data.BBINDEXS;

                //BBMXLONGQNET
                //BBMXSHORTQNET
                //BBMXQRATE100NET
                //BBMXLSQNET

                return bb_data;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Convert_2_BBRate_Data", ex.ToString());
                return bb_data;
            }
        }

        /// <summary>
        /// IPush SubjectReceived 取得之 RowData 轉 BBRate_Data 物件
        /// </summary>
        /// <param name="str_Subject">Subject</param>
        /// <param name="str_Data">Data</param>
        /// <returns></returns>
        public BBRate_Data Convert_2_BBRate_Data(string str_Subject, string str_Data)
        {
            BBRate_Data bb_data = null;

            try
            {
                string[] arr_RawData = str_Data.Split('|');

                bb_data = new BBRate_Data()
                {
                    Subject_Str = str_Subject,
                    DATE = arr_RawData[0],
                    TIME = arr_RawData[1],
                    BBINDEXS = Utility.String_2_Decimal(arr_RawData[2]),
                    BBINDEXF = Utility.String_2_Decimal(arr_RawData[3]),
                    BBTXLONGQ = Utility.String_2_Decimal(arr_RawData[4]),
                    BBTXSHORTQ = Utility.String_2_Decimal(arr_RawData[5]),
                    BBTXQRATE = Utility.String_2_Decimal(arr_RawData[6]),
                    BBTXLONGAQ = Utility.String_2_Decimal(arr_RawData[7]),
                    BBTXSHORTAQ = Utility.String_2_Decimal(arr_RawData[8]),
                    BBTXAQRATE = Utility.String_2_Decimal(arr_RawData[9]),
                    BBMXLONGQ = Utility.String_2_Decimal(arr_RawData[10]),
                    BBMXSHORTQ = Utility.String_2_Decimal(arr_RawData[11]),
                    BBMXQRATE = Utility.String_2_Decimal(arr_RawData[12]),
                    BBMXLONGAQ = Utility.String_2_Decimal(arr_RawData[13]),
                    BBMXSHORTAQ = Utility.String_2_Decimal(arr_RawData[14]),
                    BBMXAQRATE = Utility.String_2_Decimal(arr_RawData[15]),
                    BBEXLONGQ = Utility.String_2_Decimal(arr_RawData[16]),
                    BBEXSHORTQ = Utility.String_2_Decimal(arr_RawData[17]),
                    BBEXQRATE = Utility.String_2_Decimal(arr_RawData[18]),
                    BBEXLONGAQ = Utility.String_2_Decimal(arr_RawData[19]),
                    BBEXSHORTAQ = Utility.String_2_Decimal(arr_RawData[20]),
                    BBEXAQRATE = Utility.String_2_Decimal(arr_RawData[21]),
                    BBFXLONGQ = Utility.String_2_Decimal(arr_RawData[22]),
                    BBFXSHORTQ = Utility.String_2_Decimal(arr_RawData[23]),
                    BBFXQRATE = Utility.String_2_Decimal(arr_RawData[24]),
                    BBFXLONGAQ = Utility.String_2_Decimal(arr_RawData[25]),
                    BBFXSHORTAQ = Utility.String_2_Decimal(arr_RawData[26]),
                    BBFXAQRATE = Utility.String_2_Decimal(arr_RawData[27]),
                    BBWXLONGQ = Utility.String_2_Decimal(arr_RawData[28]),
                    BBWXSHORTQ = Utility.String_2_Decimal(arr_RawData[29]),
                    BBWXQRATE = Utility.String_2_Decimal(arr_RawData[30]),
                    BBWXLONGAQ = Utility.String_2_Decimal(arr_RawData[31]),
                    BBWXSHORTAQ = Utility.String_2_Decimal(arr_RawData[32]),
                    BBWXAQRATE = Utility.String_2_Decimal(arr_RawData[33]),
                };

                bb_data.BBMXQRATE100 = bb_data.BBMXQRATE * 100;
                bb_data.BBMXLSQ = bb_data.BBMXLONGQ - bb_data.BBMXSHORTQ;
                bb_data.BBSPREAD = bb_data.BBINDEXF - bb_data.BBINDEXS;

                //BBMXLONGQNET
                //BBMXSHORTQNET
                //BBMXQRATE100NET
                //BBMXLSQNET

                BBRate_Data bb_temp = null;
                if (Class_Global.Dic_First_BBRate_Data.TryGetValue(str_Subject, out bb_temp))
                {
                    bb_data.BBMXLONGAQNET = bb_temp.BBMXLONGQ;
                    bb_data.BBMXSHORTAQNET = bb_temp.BBMXSHORTQ;
                }

                return bb_data;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Convert_2_BBRate_Data", ex.ToString());
                return bb_data;
            }
        }
    }
}
