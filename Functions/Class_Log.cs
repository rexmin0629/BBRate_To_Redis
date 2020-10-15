using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace BBRate_To_Redis
{
    public enum Log_Type { Normal = 0, Error = 1 }

    class Class_Log
    {
        private static readonly object Locker = new object();
        private static Encoding _Log_Encoding = Encoding.UTF8;
        private static string Date_Format = "yyyyMMdd";
        public static string Time_Format = "HH:mm:ss.ffffff";
        private static string _Log_Full_Path = Path.Combine(Class_Global.Folder_Log, string.Format("{0}.txt", DateTime.Now.ToString(Date_Format)));

        public static void Write_Log(Log_Type _type, string _title, string _msg)
        {
            Log_Object log = new Log_Object(_type, DateTime.Now, _title, _msg, false);

            string str_log = log.Print(true);
            Console.WriteLine(str_log);

            try
            {
                try
                {
                    _Log_Full_Path = Path.Combine(Class_Global.Folder_Log, string.Format("{0}.txt", DateTime.Now.ToString(Date_Format)));
                    if (Monitor.TryEnter(Locker, 500))
                    {
                        using (StreamWriter sw = new StreamWriter(_Log_Full_Path, true, _Log_Encoding))
                        {
                            sw.WriteLine(str_log);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(Locker);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Log物件
    /// </summary>
    public class Log_Object
    {
        public static string Time_Format_Short = "HH:mm:ss.fff";
        public static string Time_Format_Long = "yyyy-MM-dd HH:mm:ss.fff";

        public Log_Object(Log_Type _type, DateTime dt_log, string _title, string _msg, bool is_LogTime_HaveDate)
        {
            Log_type = _type;
            Time_Log = (is_LogTime_HaveDate) ? dt_log.ToString(Time_Format_Long) : dt_log.ToString(Time_Format_Short);
            Title = _title;
            Msg = _msg;
        }

        public Log_Object(Log_Type _type, DateTime dt_log, string _msg, bool is_LogTime_HaveDate)
        {
            Log_type = _type;
            Time_Log = (is_LogTime_HaveDate) ? dt_log.ToString(Time_Format_Long) : dt_log.ToString(Time_Format_Short);
            Msg = _msg;
        }

        public Log_Type Log_type { get; set; }

        public string Time_Log { get; set; }

        public string Title { get; set; }

        public string Msg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="is_show_type">顯示Log_Type</param>
        /// <returns></returns>
        public virtual string Print(bool is_show_type)
        {
            string _log_type = string.Empty;
            string _log_title = string.Empty;
            string str_Format_1 = "[{0}][Msg_Type:{1}][Title:{2}] Msg:{3}";
            string str_Format_2 = "[{0}][Title:{1}] Msg:{2}";
            switch (Log_type)
            {
                case Log_Type.Normal:
                    _log_type = Log_Type.Normal.ToString();
                    break;

                case Log_Type.Error:
                    _log_type = Log_Type.Error.ToString();
                    _log_title = Title;
                    break;
            }

            try
            {
                return (is_show_type == true) ?
                    string.Format(str_Format_1, Time_Log, _log_type, _log_title, Msg) :
                    string.Format(str_Format_2, Time_Log, _log_title, Msg);
            }
            catch
            {
                return (is_show_type == true) ?
                    string.Format(str_Format_1, "", "", "", "") :
                    string.Format(str_Format_2, "", "", "");
            }
        }
    }
}
