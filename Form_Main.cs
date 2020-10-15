using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


using StackExchange.Redis;

namespace BBRate_To_Redis
{
    public partial class Form_Main : Form
    {
        #region 宣告

        Class_IPush IPush = null;                                                       //  Ipush
        Class_Redis Redis = null;                                                       //  Redis

        #endregion

        public Form_Main()
        {
            InitializeComponent();

            chkbtn_ShowLog.Checked = false;
            chkbtn_ShowLog_Click(chkbtn_ShowLog, new EventArgs());
        }

        private void Form_Main_Load(object sender, EventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Main_Form_Load", ex.ToString());
            }
        }

        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                chkbtn_Connect.Checked = false;
                chkbtn_Connect_Click(chkbtn_Connect, new EventArgs());
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Main_Form_FormClosing", ex.ToString());
            }
        }

        private void Initialize()
        {
            try
            {
                Appand_Log("[Initialize]");
                this.Text = string.Format("{0}", Class_Global.Title);

                Class_Global.Dic_Current_BBRate_Data = new Dictionary<string, BBRate_Data>();
                Class_Global.Dic_First_BBRate_Data = new Dictionary<string, BBRate_Data>() 
                {
                    {"iGKS2.TxRpt.IB1", null},  //  本點
                    {"iGKS2.TxRpt.IB2", null},  //  非本點
                    {"iGKS2.TxRpt.IB0", null}   //  全部
                };

                #region Check Directory
                //  建立所需資料夾
                foreach (string folder in Class_Global.List_Folder)
                {
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                }
                #endregion

                #region IPush Initialize
                IPush = new Class_IPush();
                IPush.Action_Appand_Log = Appand_Log;
                IPush.Action_StateChangeNotify = IPush_StateChangeNotify;
                IPush.Action_Receive_BBRate_Data = IPush_Receive_BBRate_Data;

                //  Show IPush Setting
                txt_IPushIP.Text = IPush.IPush_Setting.IP;
                txt_IPushPort.Text = IPush.IPush_Setting.Port.ToString();
                txt_IPushCompany.Text = IPush.IPush_Setting.Company;
                txt_IPushProduct.Text = IPush.IPush_Setting.Product;
                txt_IPushUser.Text = IPush.IPush_Setting.Username;
                txt_IPushPassword.Text = IPush.IPush_Setting.Password;
                txt_IPushDelay.Text = IPush.IPush_Setting.PacketDelay.ToString();
                #endregion

                #region Redis Initialize
                Redis = new Class_Redis();
                Redis.Action_Appand_Log = Appand_Log;
                Redis.Action_StateChangeNotify = Redis_StateChangeNotify;
                Redis.Action_Get_Redis_Subscribe = Redis_Get_Redis_Subscribe;
                Redis.Connect();

                //  Show Redis Setting
                lb_Redis_IP.Text = string.Format("IP : {0}", Redis.Redis_Setting.IP);
                lb_Redis_Port.Text = string.Format("Port : {0}", Redis.Redis_Setting.Port);
                lb_Redis_Channel.Text = string.Format("Channel : {0}", Redis.Redis_Setting.Channel);
                lb_Redis_DB.Text = string.Format("DB : {0}", Redis.Redis_Setting.DB);
                #endregion

                #region IPush訂閱資料的第一筆
                IniFile ini_Setting = new IniFile(Class_Global.Path_Setting);
                Class_Global.SetInfo_First_BBRate_Data = ini_Setting.GetSectionValues("First_BBRate");
                if (Class_Global.SetInfo_First_BBRate_Data.Count == 0)
                {
                    //  無訂閱資料的第一筆
                    Appand_Log("[Config中不存在IPush訂閱之第一筆資料(前日)]");
                    foreach (KeyValuePair<string, BBRate_Data> obj in Class_Global.Dic_First_BBRate_Data)
                    {
                        ini_Setting.WriteValue("First_BBRate", obj.Key, "");
                    }
                }
                else
                {
                    //  存在訂閱資料的第一筆
                    Appand_Log("[Config中存在IPush訂閱之第一筆資料(前日)]");
                    string str_value = string.Empty;
                    Dictionary<string, BBRate_Data> Temp_Dic_First_BBRate_Data = new Dictionary<string, BBRate_Data>(); //  建立Class_Global.Dic_First_BBRate_Data副本
                    Temp_Dic_First_BBRate_Data = Class_Global.Dic_First_BBRate_Data.ToDictionary(entry => entry.Key, entry => entry.Value);

                    foreach (KeyValuePair<string, BBRate_Data> obj in Class_Global.Dic_First_BBRate_Data)
                    {
                        str_value = string.Empty;
                        str_value = Class_Global.SetInfo_First_BBRate_Data[obj.Key];
                        if (!string.IsNullOrEmpty(str_value))
                        {
                            Appand_Log(string.Format("[Config First_BBRate:{0}]", str_value));
                            Temp_Dic_First_BBRate_Data[obj.Key] = IPush.Convert_2_BBRate_Data(str_value);
                        }
                    }
                    Class_Global.Dic_First_BBRate_Data = Temp_Dic_First_BBRate_Data;
                }
                #endregion

                #region 此App是否為純粹接收Redis Publis,依據命令引數中加入"Sub"關鍵字
                if (Class_Global.Redis_Client_Subscribe == true)
                {
                    chkbtn_Connect.Enabled = false;
                    gb_IPush.Enabled = false;
                }
                else
                {
                    //  直接IPush連線
                    chkbtn_Connect.Checked = true;
                    chkbtn_Connect_Click(chkbtn_Connect, new EventArgs());
                    //
                }
                #endregion
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Initialize", ex.ToString());
            }
        }

        //==================================================================================================================Controller

        private void chkbtn_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                if (chkbtn_Connect.Checked == true)
                {
                    //  檢查IPush連線資訊
                    if (IPushConnection_Check() == false)
                    {
                        MessageBox.Show("請確認IPush連線資訊","錯誤",  MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //

                    //  儲存連線資訊
                    Save_IPushConnection(IPush.IPush_Setting);
                    //

                    chkbtn_Connect.Text = "IPush中斷連線";
                    Controller_Enable(false);
                    IPush.Connect();
                }
                else
                {
                    chkbtn_Connect.Text = "IPush連線";
                    Controller_Enable(true);
                    IPush.DisConnect();
                }
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "chkbtn_Connect_Click", ex.ToString());
            }
        }

        private void chkbtn_ShowLog_Click(object sender, EventArgs e)
        {
            try
            {
                this.WindowState = FormWindowState.Normal;
                if (chkbtn_ShowLog.Checked == true)
                {
                    chkbtn_ShowLog.Text = "隱藏Log";

                    this.MaximizeBox = true;
                    this.MinimumSize = new System.Drawing.Size(400, 500);
                    this.Width = 1390;
                    this.Height = 500;
                    gb_IPush.Height = 267;
                    gb_Redis.Height = 121;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                }
                else
                {
                    chkbtn_ShowLog.Text = "顯示Log";

                    this.MaximizeBox = false;
                    this.MinimumSize = new System.Drawing.Size(276, 100);
                    this.Width = 276;
                    this.Height = 100;
                    gb_IPush.Height = 0;
                    gb_Redis.Height = 0;
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                }

                this.Refresh();
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "chkbtn_ShowLog_Click", ex.ToString());
            }
        }

        //==================================================================================================================IPush Event

        /// <summary>
        /// IPush委派函式-連線狀態變更
        /// </summary>
        /// <param name="val_status"></param>
        /// <param name="state"></param>
        private void IPush_StateChangeNotify(int val_status, IpushState state)
        {
            try
            {
                if (state == IpushState.Ready)
                {
                    //  IPush:Connect
                    foreach (KeyValuePair<string, BBRate_Data> obj in Class_Global.Dic_First_BBRate_Data)
                    {
                        IPush.SubSubject(obj.Key);
                    }
                }
                else
                {
                    //  IPush:UnConnect
                }

                StateChangeNotify(tsslb_IPushStatus, "IPush", state.ToString());
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "IPush_Connect_Status", ex.ToString());
            }
        }

        /// <summary>
        /// IPush委派函式-BBRate_Data發送前台
        /// </summary>
        /// <param name="_Data"></param>
        private void IPush_Receive_BBRate_Data(BBRate_Data _Data)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    //  紀錄當前IPush訂閱資料
                    lock (Class_Global.Dic_Current_BBRate_Data)
                    {
                        Class_Global.Dic_Current_BBRate_Data[_Data.Subject_Str] = _Data;
                    }

                    //  清盤時(BBINDEXS=0 & BBINDEXF=0)第一筆當作前日資料 , 寫入First_BBRate
                    lock (Class_Global.Dic_First_BBRate_Data)
                    {
                        if (_Data.BBINDEXS == 0 && _Data.BBINDEXF == 0)
                        {
                            //  清盤時第一筆當作前日資料,寫入First_BBRate
                            Class_Global.Dic_First_BBRate_Data[_Data.Subject_Str] = _Data;

                            IniFile ini_Setting = new IniFile(Class_Global.Path_Setting);
                            ini_Setting.WriteValue("First_BBRate", _Data.Subject_Str, _Data.ToString());
                            Appand_Log("IPush_Receive_BBRate_Data", string.Format("[清盤第一筆當作前日資料:{0}]", _Data.ToString()));
                        }
                    }

                    //  Publish To Redis Server
                    Redis.Add_Redis_Data(_Data);
                }));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "IPush_Receive_BBRate_Data", ex.ToString());
            }
        }

        //==================================================================================================================Redis Event

        /// <summary>
        /// Redis委派函式-連線狀態變更
        /// </summary>
        /// <param name="val_status"></param>
        /// <param name="state"></param>
        private void Redis_StateChangeNotify(int val_status, RedisState state)
        {
            try
            {
                if (state == RedisState.Ready)
                {
                    //  Redis:Connect
                }
                else
                {
                    //  Redis:UnConnect
                }

                StateChangeNotify(tsslb_RedisStatus, "Redis", state.ToString());
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_StateChangeNotify", ex.ToString());
            }
        }

        /// <summary>
        /// 委派函式-接收Publish之內容(此為Debug用)
        /// </summary>
        /// <param name="_Data"></param>
        private void Redis_Get_Redis_Subscribe(Class_Redis_Subscribe _Data)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    Appand_Log("Redis Subscribe", string.Format("[Redis Subscribe Channel:{0}][mag:{1}]", _Data.CHANNEL, _Data.MESSAGE));
                }));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Redis_Get_Redis_Subscribe", ex.ToString());
            }
        }

        //==================================================================================================================Function

        /// <summary>
        /// Log顯示
        /// </summary>
        /// <param name="str_appand_msg"></param>
        private void Appand_Log(string str_appand_msg)
        {
            try
            {
                if (this.IsDisposed == true)
                {
                    return;
                }

                if (rtb_Log.TextLength >= 4600)
                {
                    rtb_Log.Clear();
                }

                rtb_Log.AppendText(string.Format("[{0}]{1}", DateTime.Now.ToString(Class_Log.Time_Format), str_appand_msg) + "\r");
                rtb_Log.AppendText(Environment.NewLine);
                rtb_Log.SelectionStart = rtb_Log.Text.Length;
                rtb_Log.ScrollToCaret();

                Class_Log.Write_Log(Log_Type.Normal, "Appand_Log", str_appand_msg);
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Appand_Log", ex.ToString());
            }
        }

        /// <summary>
        /// 委派函式-Log for IPush
        /// </summary>
        /// <param name="_from"></param>
        /// <param name="_msg"></param>
        private void Appand_Log(string _from, string _msg)
        {
            try
            {
                if (this.IsDisposed == true)
                {
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    if (rtb_Log.TextLength >= 4600)
                    {
                        rtb_Log.Clear();
                    }

                    int lenth = rtb_Log.Text.Length;

                    rtb_Log.AppendText(string.Format("[{0}][{1}][{2}]", DateTime.Now.ToString(Class_Log.Time_Format), _from, _msg) + "\r");

                    if (rtb_Log.Lines.Length >= 2)
                    {
                        if (rtb_Log.Lines[rtb_Log.Lines.Length - 2].Contains("IPush"))
                        {
                            //rtb_Log.Select(lenth, rtb_Log.Lines[rtb_Log.Lines.Length - 2].Length);
                            //rtb_Log.SelectionBackColor = Color.Pink;
                        }
                        else if (rtb_Log.Lines[rtb_Log.Lines.Length - 2].Contains("[Redis Publish]"))
                        {
                            rtb_Log.Select(lenth, rtb_Log.Lines[rtb_Log.Lines.Length - 2].Length);
                            rtb_Log.SelectionBackColor = Color.LightBlue;

                            lb_LogTimeLast.Text = string.Format("Publish Log時間 : {0}", DateTime.Now.ToString("HH:mm:ss.ffff"));
                        }
                        else if (rtb_Log.Lines[rtb_Log.Lines.Length - 2].Contains("[Redis Subscribe]"))
                        {
                            rtb_Log.Select(lenth, rtb_Log.Lines[rtb_Log.Lines.Length - 2].Length);
                            rtb_Log.SelectionBackColor = Color.LightGreen;
                        }
                    }

                    rtb_Log.AppendText(Environment.NewLine);
                    rtb_Log.SelectionStart = rtb_Log.Text.Length;
                    rtb_Log.ScrollToCaret();

                    Class_Log.Write_Log(Log_Type.Normal, _from, _msg);
                }));
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "IPush_Appand_Log", ex.ToString());
            }
        }

        /// <summary>
        /// 連線狀態變更
        /// </summary>
        /// <param name="tssl">ToolStripStatusLabel</param>
        /// <param name="str_obj">連線物件名稱</param>
        /// <param name="str_status">連線狀態</param>
        private void StateChangeNotify(ToolStripStatusLabel tssl, string str_obj, string str_status)
        {
            try
            {
                switch (str_status)
                {
                    case "Ready":
                        tssl.Text = string.Format("{0}:Connect", str_obj);
                        tssl.ForeColor = Color.Blue;
                        break;

                    default:
                        tssl.Text = string.Format("{0}:UnConnect", str_obj);
                        tssl.ForeColor = Color.Red;
                        break;
                }
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "StateChangeNotify", ex.ToString());
            }
        }

        /// <summary>
        /// 控制項Enable/Disable
        /// </summary>
        /// <param name="enable">true:Enable false:Disable</param>
        private void Controller_Enable(bool enable)
        {
            try
            {
                txt_IPushIP.Enabled = enable;
                txt_IPushPort.Enabled = enable;
                txt_IPushCompany.Enabled = enable;
                txt_IPushProduct.Enabled = enable;
                txt_IPushUser.Enabled = enable;
                txt_IPushPassword.Enabled = enable;
                txt_IPushDelay.Enabled = enable;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Controller_Enable", ex.ToString());
            }
        }

        /// <summary>
        /// 檢查IPush連線資訊
        /// </summary>
        /// <returns></returns>
        private bool IPushConnection_Check()
        {
            try
            {
                if (string.IsNullOrEmpty(txt_IPushIP.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushPort.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushCompany.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushProduct.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushUser.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushPassword.Text))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(txt_IPushDelay.Text))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "IPushConnection_Check", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 儲存IPush連線資訊
        /// </summary>
        /// <param name="_Setting"></param>
        private void Save_IPushConnection(IPushSetting _Setting)
        {
            try
            {
                _Setting.IP = txt_IPushIP.Text;
                _Setting.Port = int.Parse(txt_IPushPort.Text);
                _Setting.Company = txt_IPushCompany.Text;
                _Setting.Product = txt_IPushProduct.Text;
                _Setting.Username = txt_IPushUser.Text;
                _Setting.Password = txt_IPushPassword.Text;
                _Setting.PacketDelay = int.Parse(txt_IPushDelay.Text);

                IniFile ini = new IniFile(Class_Global.Path_Setting);
                ini.WriteValue("IPush", "iPushIP", _Setting.IP);
                ini.WriteValue("IPush", "iPushPort", _Setting.Port);
                ini.WriteValue("IPush", "Company", _Setting.Company);
                ini.WriteValue("IPush", "Product", _Setting.Product);
                ini.WriteValue("IPush", "User", _Setting.Username);
                ini.WriteValue("IPush", "Password", _Setting.Password);
                ini.WriteValue("IPush", "PacketDelay", _Setting.PacketDelay);
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Save_IPushConnection", ex.ToString());
            }
        }

    }
}
