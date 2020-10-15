using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

namespace BBRate_To_Redis
{
    static class Program
    {
        #region 宣告
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int WS_SHOWNORMAL = 1; // 1.Normal   2.Minimized   3.Maximized
        public static string Str_UNIQUENAME = string.Empty;
        #endregion

        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            #region 命令引數
            Class_Global.Proc_Args = Environment.GetCommandLineArgs();//取得命令引數

            if (Class_Global.Proc_Args.Length >= 2 && Class_Global.Proc_Args[1].ToLower() == "sub")
            {
                Class_Global.Redis_Client_Subscribe = true;
            }
            #endregion

            Str_UNIQUENAME = "Global\\" + Class_Global.Title;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);//设置应用程序处理异常方式：ThreadException处理
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);//發生Thread錯誤時記錄下來
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionEventHandler);//發生錯誤時記錄下來

            //using (Mutex m = new Mutex(true, Str_UNIQUENAME))
            //{
            //    if (!m.WaitOne(0, true))
            //    {
            //        Class_Log.Write_Log(Log_Type.Normal, "Main", "Process is exists");

            //        Process instance = RunningInstance();
            //        if (instance != null)
            //        {
            //            HandleRunningInstance(instance);
            //        }
            //        return;
            //    }
            //    else
            //    {
            //        Class_Log.Write_Log(Log_Type.Normal, "Main", "Start Process");

            //        //  Dll Regist
            //        //

            //        Application.EnableVisualStyles();
            //        Application.SetCompatibleTextRenderingDefault(false);
            //        Application.Run(new Form_Main());
            //    }
            //}

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //  設定Redis Cofig
            //Dialog_RedisSetting _RedisSetting = new Dialog_RedisSetting();
            //_RedisSetting.ShowDialog();
            //_RedisSetting.Close();
            //_RedisSetting = null;
            //

            Application.Run(new Form_Main());
        }


        //==========================================================================================================================Function

        private static void HandleRunningInstance(Process instance)
        {
            //Make sure the window is not minimized or maximized    
            ShowWindowAsync(instance.MainWindowHandle, WS_SHOWNORMAL);
            //Set the real intance to foreground window
            SetForegroundWindow(instance.MainWindowHandle);
        }

        private static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();  // 取得目前作用中的處理序
            Process[] processes = Process.GetProcessesByName(current.ProcessName);  // 取得指定的處理緒名稱的所有處理序
            //Loop through the running processes in with the same name    
            foreach (Process process in processes)
            {
                //Ignore the current process    
                if (process.Id != current.Id)
                {
                    //Make sure that the process is running from the exe file.    
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        //Return the other process instance.    
                        return process;
                    }
                }
            }
            //No other instance was found, return null.  
            return null;
        }

        private static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            //Exception exception = e.ExceptionObject as Exception;
            //string message = exception == null ? "null" : exception.Message;

            Class_Log.Write_Log(Log_Type.Error, "Application_ThreadException", e.ExceptionObject.ToString());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Class_Log.Write_Log(Log_Type.Error, "Application_ThreadException", e.Exception.ToString());
        }
    }
}
