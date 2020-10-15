using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq.Expressions;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.ObjectModel;

namespace BBRate_To_Redis
{
    /// <summary>
    /// 共用方法類別
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// 設定檔存取元件
        /// </summary>
        public class IniFile   // revision 11
        {
            string Path;
            string EXE = Assembly.GetExecutingAssembly().GetName().Name;

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

            public IniFile(string IniPath = null)
            {
                Path = new FileInfo(IniPath ?? EXE + ".ini").FullName.ToString();
            }

            public string Read(string Key, string Section = null)
            {
                var RetVal = new StringBuilder(255);
                GetPrivateProfileString(Section ?? EXE, Key, "", RetVal, 255, Path);
                return RetVal.ToString();
            }

            public void Write(string Key, string Value, string Section = null)
            {
                WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
            }

            public void DeleteKey(string Key, string Section = null)
            {
                Write(Key, null, Section ?? EXE);
            }

            public void DeleteSection(string Section = null)
            {
                Write(null, null, Section ?? EXE);
            }

            public bool KeyExists(string Key, string Section = null)
            {
                return Read(Key, Section).Length > 0;
            }
        }
        public static int[] GetRandomRunningNumberList(int start, int end)
        {
            List<int> serialList = new List<int>();
            for (int i = start; i <= end; i++)
            {
                serialList.Add(i);
            }
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            List<int> randomList = new List<int>();
            while (serialList.Count > 0)
            {
                int tempIndex = rnd.Next(serialList.Count);
                randomList.Add(serialList[tempIndex]);
                serialList.RemoveAt(tempIndex);
            }
            return randomList.ToArray();

        }
        public string[] DivideParamWithVerticalLine(string source)
        {
            return source.Split('|');
        }


        /// <summary>
        /// 2019.02.12 Rex_Modify 寫檔
        /// </summary>
        /// <param name="PATH"></param>
        /// <param name="str_datas"></param>
        public static void WriteToFile(string PATH, string str_datas)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(PATH);
                if (!System.IO.Directory.Exists(directoryName))
                {
                    System.IO.Directory.CreateDirectory(directoryName);
                }

                using (StreamWriter sw = new StreamWriter(PATH, true, Encoding.UTF8))
                {
                    TextWriter SW = TextWriter.Synchronized(sw);
                    SW.WriteLine(str_datas);
                    SW.Flush();
                    SW.Dispose();
                }
            }
            catch (Exception exp)
            {
            }
        }
        /// <summary>
        /// 2019.02.12 Rex_Modify 寫檔
        /// </summary>
        /// <param name="PATH"></param>
        /// <param name="str_datas"></param>
        public static void WriteToFile(string PATH, List<string> str_datas)
        {
            try
            {
                string directoryName = Path.GetDirectoryName(PATH);
                if (!System.IO.Directory.Exists(directoryName))
                {
                    System.IO.Directory.CreateDirectory(directoryName);
                }

                using (StreamWriter sw = new StreamWriter(PATH, true, Encoding.UTF8))
                {
                    TextWriter SW = TextWriter.Synchronized(sw);
                    for (int i = 0; i < str_datas.Count(); i++)
                    {
                        SW.WriteLine(str_datas[i]);
                    }
                    SW.Flush();
                    SW.Dispose();
                }
            }
            catch (Exception exp)
            {
            }
        }
        /// <summary>
        /// 2019.02.12 Rex_Modify 讀檔
        /// </summary>
        /// <param name="PATH"></param>
        /// <returns></returns>
        public static string[] ReadFile(string PATH)
        {
            string[] tmp = null;
            if (File.Exists(PATH))
            {
                tmp = File.ReadAllLines(PATH);
            }
            return tmp;
        }

        public static string String_Encode_UTF8(string myString, Encoding encode)
        {
            byte[] bytes = encode.GetBytes(myString);
            return encode.GetString(bytes);
        }

        public static int String_2_Integer(string str)
        {
            try
            {
                int result = 0;

                if (int.TryParse(str, out result) == false)
                {
                    result = 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static double String_2_Double(string str)
        {
            try
            {
                double result = 0;

                if (double.TryParse(str, out result) == false)
                {
                    result = 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static decimal String_2_Decimal(string str)
        {
            try
            {
                decimal result = 0;

                if (decimal.TryParse(str, out result) == false)
                {
                    result = 0;
                }

                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// 字串 轉 Big5 16進位
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] Big5_2_Hex(string str)
        {
            try
            {
                return Encoding.GetEncoding("Big5").GetBytes(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string Decimal_2_String(decimal dec)
        {
            try
            {
                return dec.ToString("0.########");
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
