using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BBRate_To_Redis
{
    public partial class Component_Redis : Component, IDisposable
    {
        public Component_Redis()
        {
            InitializeComponent();
        }

        public Component_Redis(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        ~Component_Redis()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.Redis_Client != null)
            {
                this.Redis_Client.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        //===================================================================================================參數

        private R_Client Redis_Client { get; set; }
        public bool Connected { get; set; }

        //===================================================================================================物件

        /// <summary>
        /// Redis Config
        /// </summary>
        public class R_Config
        {
            public string Host { get; set; }        //  Redis Server
            public int Port { get; set; }           //  Redis Port
            public string DB_Index { get; set; }    //  Redis DB Index(0-16)

            public R_Config()
            {
                Host = "127.0.0.1";
                Port = 6379;
                DB_Index = "0";
            }
        }

        /// <summary>
        /// 封包架構
        /// </summary>
        public class Protocal_Structe
        {
            public byte[] Header;
            public byte[] Body;
            public byte[] CRC;

            public Protocal_Structe(int len_header, int len_body, int len_crc)
            {
                Header = new byte[len_header];
                Body = new byte[len_body];
                CRC = new byte[len_crc];
            }
        }
        
        /// <summary>
        /// Redis 命令
        /// </summary>
        public enum RedisCommand
        {
            Non,                //  Nothing
            INFO,               //  Redis資訊  
            SELECT,             //  選擇資料庫(0-16)
            EXPIRE,             //  設定逾時時間
            DEL,                //  刪除key

            #region String
            GET,                //  取得key之值
            SET,                //  設定key之值
            #endregion

            #region Hash
            HMSET,              //  將多個 field-value (域-值)對設定到Hash表key中
            HGETALL,            //  取得Hash表中指定key的所有字段和值
            #endregion

            #region List
            RPUSH,              //  將一個或多個值插入到列表的尾部(最右邊)
            LPOP,               //  移除並返回列表的第一個元素
            LLEN,               //  取得列表的長度
            LRANGE,             //  取得列表中指定區間的元素
            #endregion

            #region Set
            SADD,               //  將一個或多個成員元素加入到集合中，已經存在於集合的成員元素將被忽略
            SMEMBERS,           //  返回集合中的所有的成員
            SREM,               //  移除集合中一個或多個成員
            #endregion

            #region Sorted Set
            ZADD,               //  將一個或多個成員元素及其分數值加入到有序集中
            ZRANGE,             //  返回有序集中，指定區間內的成員。其中成員的位置按分數值遞增(從小到大)來排序
            ZREMRANGEBYRANK,    //  移除有序集中，指定排名(rank)區間內的所有成員
            #endregion


            #region 發佈
            SUBSCRIBE,          //  訂閱一個或多個频道的信息
            UNSUBSCRIBE,        //  退訂一個或多個频道的信息
            PUBLISH,            //  信息發送到指定的频道
            #endregion
            
            #region 事務
            MULTI,              //  事務開始
            EXEC,               //  執行事務
            #endregion
        }

        /// <summary>
        /// Command & argument
        /// </summary>
        public class R_Cmd_arg
        {
            public RedisCommand Cmd { get; set; }               //  Cmd
            public string[] arg_data { get; set; }              //  argument('Key','Value')

            public R_Cmd_arg()
            {
                Cmd = RedisCommand.Non;
                arg_data = new string[0];
            }
        }

        //===================================================================================================Event

        public event EventHandler<CustomEventArgs> Event_Msg_Report;
        public event EventHandler<EventArgs_Receive_Report> Event_Receive_Report;
        public event EventHandler Event_List_R_Push_Alarm;//  觸發事件 List 新增一筆資料

        //===================================================================================================Event_Object

        public class CustomEventArgs : EventArgs
        {

            private SockteLog_Object log;
            private int e_type;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="event_type">0:Run Report 1:Error Report</param>
            /// <param name="s">Report Message</param>
            public CustomEventArgs(int event_type, SockteLog_Object log_object)
            {
                log = log_object;
                e_type = event_type;
            }

            public SockteLog_Object Log
            {
                get { return log; }
                set { log = value; }
            }

            public int E_type
            {
                get { return e_type; }
                set { e_type = value; }
            }
        }

        public class EventArgs_Receive_Report : EventArgs
        {
            private Protocal_Structe receive;
            private DateTime time;

            public EventArgs_Receive_Report(Protocal_Structe _receive, DateTime _time)
            {
                receive = _receive;
                time = _time;
            }

            public Protocal_Structe Receive
            {
                get { return receive; }
                set { receive = value; }
            }

            public DateTime Time
            {
                get { return time; }
                set { time = value; }
            }
        }

        //===================================================================================================Client Start

        public void Start_Connect(R_Config config)
        {
            Redis_Client = null;
            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress add = IPAddress.Parse(config.Host);
                IPEndPoint clientEndpoint = new IPEndPoint(add, config.Port);
                clientSocket.Connect(clientEndpoint);

                Redis_Client = new R_Client(clientSocket);
                Redis_Client.remote_Name = string.Format("{0}:{1}[{2}]>", config.Host, config.Port, config.DB_Index);
                Redis_Client.remote_MAC = Guid.NewGuid().ToString();//   序號當MAC
                Redis_Client.Event_Msg_Report += Redis_Client_Event_Msg_Report;
                Redis_Client.Event_Receive_Report += Redis_Client_Event_Receive_Report;
                Redis_Client.Event_Close += Redis_Client_Event_Close;

                //  Cmds 一開始先指定資料庫Index
                SelectDB_Async(config.DB_Index);

                Connected = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Err_10 \r\n{0}", ex.ToString()));
                Connected = false;
            }
        }

        private void Redis_Client_Event_Msg_Report(object sender, CustomEventArgs e)
        {
            if(Event_Msg_Report!=null)
            {
                Event_Msg_Report(sender, new CustomEventArgs(e.E_type, e.Log));
            }
        }

        private void Redis_Client_Event_Receive_Report(object sender, EventArgs_Receive_Report e)
        {
            if(Event_Receive_Report!=null)
            {
                Event_Receive_Report(sender, new EventArgs_Receive_Report(e.Receive, e.Time));
            }
        }

        private void Redis_Client_Event_Close(object sender, EventArgs e)
        {
            Redis_Client = null;
            Connected = false;
        }

        //===================================================================================================Redis Client

        public class R_Client : IDisposable
        {
            public Socket work_Socket;
            protected NetworkStream ns = null;
            protected BinaryReader br = null;
            public byte[] buffer;
            public List<byte> buff_receive_Temp = new List<byte>();

            Thread _thread = null;
            public List<R_Cmd_arg> List_Command = new List<R_Cmd_arg>();
            public List<Protocal_Structe> List_Complate = new List<Protocal_Structe>();
            public int Connect_Ticktime = Environment.TickCount;                //  記錄每次的通訊時間

            #region 資訊參數

            public string remote_Name { get; set; }
            public string remote_MAC { get; set; }

            private bool is_Stop = false;
            public bool Is_Stop
            {
                get { return is_Stop; }
                set
                {
                    is_Stop = value;

                    if (is_Stop == true)
                    {
                        if (Event_Close != null)
                        {
                            Event_Close(this, new EventArgs());
                        }
                    }
                }
            }

            #endregion

            public R_Client(Socket _socket)
            {
                buffer = new byte[1024];

                this.work_Socket = _socket;
                ns = new NetworkStream(this.work_Socket);
                br = new BinaryReader(this.ns);

                _thread = new Thread(() => { DoWork(); });
                _thread.IsBackground = true;
                _thread.Start();
            }

            ~R_Client()
            {
                Dispose();
            }

            public void Dispose()
            {
                try
                {
                    if (Is_Stop == false)
                        Is_Stop = true;
                    
                    //  Socket關閉
                    if (this.work_Socket != null)
                    {
                        this.work_Socket.Close();
                        this.work_Socket = null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Err_18 \r\n{0}", ex.ToString()));
                }

                GC.SuppressFinalize(this);
            }

            private void DoWork()
            {
                while (Is_Stop == false)
                {
                    if (Read() == false)
                    {
                        Is_Stop = true;
                    }

                    if (Send_Command() == false)
                    {
                        Is_Stop = true;
                    }

                    Thread.Sleep(10);
                }
            }
            
            private bool Send_Command()
            {
                try
                {
                    R_Cmd_arg cmd = null;
                    lock (List_Command)
                    {
                        if (List_Command.Count > 0)
                        {
                            cmd = List_Command[0];
                        }
                        else
                        {
                            cmd = null;
                            return true;
                        }
                    }

                    if (cmd != null)
                    {
                        string str_cmd = Create_Cmd(cmd);
                        
                        if (Send_Data(Utility.Big5_2_Hex(str_cmd)) == false)
                        {
                            return false;
                        }

                        //if (Event_Msg_Report != null)
                        //{
                        //    Event_Msg_Report(this, new CustomEventArgs(0,
                        //        new SockteLog_Object(SockteLog_Object.Log_Type.Socket, DateTime.Now, this.remote_Name, "", cmd.Cmd.ToString(), str_cmd.Replace("\r\n", ""), "", false)));
                        //}
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Err_16 \r\n{0}", ex.ToString()));
                    return false;
                }
            }

            private bool Send_Data(byte[] byte_send)
            {
                try
                {
                    lock (this)
                    {
                        //  每次的通訊時間
                        this.Connect_Ticktime = Environment.TickCount;

                        ns.Write(byte_send, 0, byte_send.Length);
                        ns.Flush();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Err_12 \r\n{0}", ex.ToString()));
                    return false;
                }
            }

            private bool Read()
            {
                try
                {
                    int bytesRead = this.work_Socket.Available;

                    if (bytesRead > 0)
                    {
                        //  每次的通訊時間
                        this.Connect_Ticktime = Environment.TickCount;

                        //  此處得到的是 Hex 之資料
                        byte[] bye_temp = this.br.ReadBytes(bytesRead);

                        lock (this.buff_receive_Temp)
                        {
                            //  把得到的資料先放到暫存
                            this.buff_receive_Temp.AddRange(bye_temp);
                        }

                        byte[] byte_Receive_Data = this.buff_receive_Temp.GetRange(0, this.buff_receive_Temp.Count).ToArray();

                        lock (this.buff_receive_Temp)
                        {
                            this.buff_receive_Temp.RemoveRange(0, this.buff_receive_Temp.Count);
                        }

                        //  收到完整封包(byte_Receive_Data)
                        Protocal_Structe _ps = new Protocal_Structe(0, byte_Receive_Data.Length, 0);
                        Buffer.BlockCopy(byte_Receive_Data, 0, _ps.Body, 0, byte_Receive_Data.Length);

                        lock (List_Command)
                        {
                            if (List_Command.Count > 0)
                            {
                                List_Command.RemoveAt(0);
                            }
                        }

                        lock (List_Complate)
                        {
                            List_Complate.Add(_ps);
                        }

                        //  觸發Receive事件
                        if (Event_Receive_Report != null)
                        {
                            Event_Receive_Report(this, new EventArgs_Receive_Report(_ps, DateTime.Now));
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Err_17 \r\n{0}", ex.ToString()));
                    return false;
                }
            }

            //  Header格式(*<number of arguments>\r\n)
            private const string Str_Header = "*{0}\r\n";
            //  參數訊息($<number of bytes of argument N>\r\n<argument data>\r\n)
            private const string Str_Body = "${0}\r\n{1}\r\n";
            private string Create_Cmd(R_Cmd_arg _cmd_arg)
            {
                /*      Redis Protocol
                 *      *<number of arguments>\r\n$<number of bytes of argument 1>\r\n<argument data>\r\n
                 *      *參數數量\r\n$參數byte長度\r\n參數資料\r\n
                 *      例：*1\r\n$4\r\nINFO\r\n
                 *      例：*3\r\n$3\r\nSET\r\n$5\r\nmykey\r\n$7\r\nmyvalue\r\n
                 */

                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat(Str_Header, _cmd_arg.arg_data.Length + 1);

                    string cmd = _cmd_arg.Cmd.ToString();
                    sb.AppendFormat(Str_Body, cmd.Length, cmd);

                    foreach (string d in _cmd_arg.arg_data)
                    {
                        sb.AppendFormat(Str_Body, d.Length, d);
                    }

                    return sb.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Err_19 \r\n{0}", ex.ToString()));
                    return string.Empty;
                }
            }

            //===================================================================================================Event

            public event EventHandler<CustomEventArgs> Event_Msg_Report;
            public event EventHandler<EventArgs_Receive_Report> Event_Receive_Report;
            public event EventHandler Event_Close;
        }

        //===================================================================================================Function

        int _TimeOut = 1000;

        /// <summary>
        /// 信息發送到指定的频道
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public string Publish_Async(string ch, string msg)
        {
            try
            {
                if (Redis_Client != null)
                {
                    string result = string.Empty;
                    lock (Redis_Client.List_Command)
                    {
                        Redis_Client.List_Command.Add(new R_Cmd_arg()
                        {
                            Cmd = RedisCommand.PUBLISH,
                            arg_data = new string[] { ch, msg },
                        });
                    }

                    Task<string>[] list_task = new Task<string>[]
                    {
                        Async_Result(Redis_Client)
                    };
                    Task.WaitAny(list_task, _TimeOut);
                    result = list_task[0].Result;

                    return result;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Err_38 \r\n{0}", ex.ToString()));
                return string.Empty;
            }
        }
    }
}
