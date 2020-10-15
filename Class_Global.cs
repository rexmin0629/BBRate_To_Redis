using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace BBRate_To_Redis
{
    /// <summary>
    /// 市場狀態
    /// </summary>
    public enum MarketStatus
    {
        Open,   //  開盤
        Close,  //  收盤
    }

    class Class_Global
    {
        public static AssemblyTitleAttribute Assembly_title = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0];
        public static AssemblyProductAttribute Assembly_product = (AssemblyProductAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
        public static AssemblyCopyrightAttribute Assembly_copyright = (AssemblyCopyrightAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false)[0];
        public static AssemblyCompanyAttribute Assembly_company = (AssemblyCompanyAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false)[0];
        public static AssemblyDescriptionAttribute Assembly_description = (AssemblyDescriptionAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)[0];
        public static Version Assembly_version = Assembly.GetExecutingAssembly().GetName().Version;

        public static string[] Proc_Args = default(string[]);                                                                               //  App命令引數內容

        public static string Title = string.Format("{0} v.{1}", Assembly_title.Title, Assembly_version.ToString());

        public static string DateFormat_yyyyMMdd = "yyyyMMdd";
        
        public static string Path_Setting = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"configs\setting.ini");             //  設定檔(預設)
        public static string Folder_Log = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Folder_Log");                         //  資料夾Log
        public static List<string> List_Folder = new List<string>() { Folder_Log };

        public static bool Redis_Client_Subscribe = false;                                                                                  //  App為Redis Subscribe狀態的Client

        public static Dictionary<string, BBRate_Data> Dic_First_BBRate_Data = null;                                                         //  IPush訂閱之第一筆資料(前日)
        public static Dictionary<string, BBRate_Data> Dic_Current_BBRate_Data = null;                                                       //  IPush訂閱之當前資料
        public static Dictionary<string, string> SetInfo_First_BBRate_Data = null;                                                          //  取得Config檔之IPush訂閱之第一筆資料(前日)

        public static MarketStatus Market_Status = MarketStatus.Close;                                                                      //  市場狀態:開盤/收盤
    }
}
