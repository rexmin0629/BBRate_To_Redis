using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BBRate_To_Redis
{
    //BBRate_Data

    [Serializable]
    public class BBRate_Data : INotifyPropertyChanged, ICloneable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string _Subject_Str = default(string);
        [Display(Name = "Subject_Str", Description = "訂閱字串")]
        public string Subject_Str
        {
            get { return _Subject_Str; }
            set
            {
                _Subject_Str = value;
                this.NotifyPropertyChanged("Subject_Str");
            }
        }

        private string _Date = default(string);
        [Display(Name = "DATE", Description = "日期")]
        public string DATE
        {
            get { return _Date; }
            set
            {
                _Date = value;
                this.NotifyPropertyChanged("DATE");
            }
        }

        private string _Time = default(string);
        [Display(Name = "TIME", Description = "時間")]
        public string TIME
        {
            get { return _Time; }
            set
            {
                _Time = value;
                this.NotifyPropertyChanged("TIME");
            }
        }

        private decimal _BBindexS = default(decimal);
        [Display(Name = "BBINDEXS", Description = "指數現貨")]
        public decimal BBINDEXS
        {
            get { return _BBindexS; }
            set
            {
                _BBindexS = value;
                this.NotifyPropertyChanged("BBINDEXS");
            }
        }

        private decimal _BBindexF = default(decimal);
        [Display(Name = "BBINDEXF", Description = "指數期貨")]
        public decimal BBINDEXF
        {
            get { return _BBindexF; }
            set
            {
                _BBindexF = value;
                this.NotifyPropertyChanged("BBINDEXF");
            }
        }

        private decimal _BBTXLongQ = default(decimal);
        [Display(Name = "BBTXLONGQ", Description = "大台多口數")]
        public decimal BBTXLONGQ
        {
            get { return _BBTXLongQ; }
            set
            {
                _BBTXLongQ = value;
                this.NotifyPropertyChanged("BBTXLONGQ");
            }
        }

        private decimal _BBTXShortQ = default(decimal);
        [Display(Name = "BBTXSHORTQ", Description = "大台空口數")]
        public decimal BBTXSHORTQ
        {
            get { return _BBTXShortQ; }
            set
            {
                _BBTXShortQ = value;
                this.NotifyPropertyChanged("BBTXSHORTQ");
            }
        }

        private decimal _BBTXQRate = default(decimal);
        [Display(Name = "BBTXQRATE", Description = "大台口數比")]
        public decimal BBTXQRATE
        {
            get { return _BBTXQRate; }
            set
            {
                _BBTXQRate = value;
                this.NotifyPropertyChanged("BBTXQRATE");
            }
        }

        private decimal _BBTXLongAQ = default(decimal);
        [Display(Name = "BBTXLONGAQ", Description = "大台多戶數")]
        public decimal BBTXLONGAQ
        {
            get { return _BBTXLongAQ; }
            set
            {
                _BBTXLongAQ = value;
                this.NotifyPropertyChanged("BBTXLONGAQ");
            }
        }

        private decimal _BBTXShortAQ = default(decimal);
        [Display(Name = "BBTXSHORTAQ", Description = "大台空戶數")]
        public decimal BBTXSHORTAQ
        {
            get { return _BBTXShortAQ; }
            set
            {
                _BBTXShortAQ = value;
                this.NotifyPropertyChanged("BBTXSHORTAQ");
            }
        }

        private decimal _BBTXAQRate = default(decimal);
        [Display(Name = "BBTXAQRATE", Description = "大台戶數比")]
        public decimal BBTXAQRATE
        {
            get { return _BBTXAQRate; }
            set
            {
                _BBTXAQRate = value;
                this.NotifyPropertyChanged("BBTXAQRATE");
            }
        }

        private decimal _BBMXLongQ = default(decimal);
        [Display(Name = "BBMXLONGQ", Description = "小台多口數")]
        public decimal BBMXLONGQ
        {
            get { return _BBMXLongQ; }
            set
            {
                _BBMXLongQ = value;
                this.NotifyPropertyChanged("BBMXLONGQ");
            }
        }

        private decimal _BBMXShortQ = default(decimal);
        [Display(Name = "BBMXSHORTQ", Description = "小台空口數")]
        public decimal BBMXSHORTQ
        {
            get { return _BBMXShortQ; }
            set
            {
                _BBMXShortQ = value;
                this.NotifyPropertyChanged("BBMXSHORTQ");
            }
        }

        private decimal _BBMXQRate = default(decimal);
        [Display(Name = "BBMXQRATE", Description = "小台口數比")]
        public decimal BBMXQRATE
        {
            get { return _BBMXQRate; }
            set
            {
                _BBMXQRate = value;
                this.NotifyPropertyChanged("BBMXQRATE");
            }
        }

        private decimal _BBMXLongAQ = default(decimal);
        [Display(Name = "BBMXLONGAQ", Description = "小台多戶數")]
        public decimal BBMXLONGAQ
        {
            get { return _BBMXLongAQ; }
            set
            {
                _BBMXLongAQ = value;
                this.NotifyPropertyChanged("BBMXLONGAQ");
            }
        }

        private decimal _BBMXShortAQ = default(decimal);
        [Display(Name = "BBMXSHORTAQ", Description = "小台空戶數")]
        public decimal BBMXSHORTAQ
        {
            get { return _BBMXShortAQ; }
            set
            {
                _BBMXShortAQ = value;
                this.NotifyPropertyChanged("BBMXSHORTAQ");
            }
        }

        private decimal _BBMXAQRate = default(decimal);
        [Display(Name = "BBMXAQRATE", Description = "小台戶數比")]
        public decimal BBMXAQRATE
        {
            get { return _BBMXAQRate; }
            set
            {
                _BBMXAQRate = value;
                this.NotifyPropertyChanged("BBMXAQRATE");
            }
        }

        private decimal _BBEXLongQ = default(decimal);
        [Display(Name = "BBEXLONGQ", Description = "電子多口數")]
        public decimal BBEXLONGQ
        {
            get { return _BBEXLongQ; }
            set
            {
                _BBEXLongQ = value;
                this.NotifyPropertyChanged("BBEXLONGQ");
            }
        }

        private decimal _BBEXShortQ = default(decimal);
        [Display(Name = "BBEXSHORTQ", Description = "電子空口數")]
        public decimal BBEXSHORTQ
        {
            get { return _BBEXShortQ; }
            set
            {
                _BBEXShortQ = value;
                this.NotifyPropertyChanged("BBEXSHORTQ");
            }
        }

        private decimal _BBEXQRate = default(decimal);
        [Display(Name = "BBEXQRATE", Description = "電子口數比")]
        public decimal BBEXQRATE
        {
            get { return _BBEXQRate; }
            set
            {
                _BBEXQRate = value;
                this.NotifyPropertyChanged("BBEXQRATE");
            }
        }

        private decimal _BBEXLongAQ = default(decimal);
        [Display(Name = "BBEXLONGAQ", Description = "電子多戶數")]
        public decimal BBEXLONGAQ
        {
            get { return _BBEXLongAQ; }
            set
            {
                _BBEXLongAQ = value;
                this.NotifyPropertyChanged("BBEXLONGAQ");
            }
        }

        private decimal _BBEXShortAQ = default(decimal);
        [Display(Name = "BBEXSHORTAQ", Description = "電子空戶數")]
        public decimal BBEXSHORTAQ
        {
            get { return _BBEXShortAQ; }
            set
            {
                _BBEXShortAQ = value;
                this.NotifyPropertyChanged("BBEXSHORTAQ");
            }
        }

        private decimal _BBEXAQRate = default(decimal);
        [Display(Name = "BBEXAQRATE", Description = "電子戶數比")]
        public decimal BBEXAQRATE
        {
            get { return _BBEXAQRate; }
            set
            {
                _BBEXAQRate = value;
                this.NotifyPropertyChanged("BBEXAQRATE");
            }
        }

        private decimal _BBFXLongQ = default(decimal);
        [Display(Name = "BBFXLONGQ", Description = "金融多口數")]
        public decimal BBFXLONGQ
        {
            get { return _BBFXLongQ; }
            set
            {
                _BBFXLongQ = value;
                this.NotifyPropertyChanged("BBFXLONGQ");
            }
        }

        private decimal _BBFXShortQ = default(decimal);
        [Display(Name = "BBFXSHORTQ", Description = "金融空口數")]
        public decimal BBFXSHORTQ
        {
            get { return _BBFXShortQ; }
            set
            {
                _BBFXShortQ = value;
                this.NotifyPropertyChanged("BBFXSHORTQ");
            }
        }

        private decimal _BBFXQRate = default(decimal);
        [Display(Name = "BBFXQRATE", Description = "金融口數比")]
        public decimal BBFXQRATE
        {
            get { return _BBFXQRate; }
            set
            {
                _BBFXQRate = value;
                this.NotifyPropertyChanged("BBFXQRATE");
            }
        }

        private decimal _BBFXLongAQ = default(decimal);
        [Display(Name = "BBFXLONGAQ", Description = "金融多戶數")]
        public decimal BBFXLONGAQ
        {
            get { return _BBFXLongAQ; }
            set
            {
                _BBFXLongAQ = value;
                this.NotifyPropertyChanged("BBFXLONGAQ");
            }
        }

        private decimal _BBFXShortAQ = default(decimal);
        [Display(Name = "BBFXSHORTAQ", Description = "金融空戶數")]
        public decimal BBFXSHORTAQ
        {
            get { return _BBFXShortAQ; }
            set
            {
                _BBFXShortAQ = value;
                this.NotifyPropertyChanged("BBFXSHORTAQ");
            }
        }

        private decimal _BBFXAQRate = default(decimal);
        [Display(Name = "BBFXAQRATE", Description = "金融戶數比")]
        public decimal BBFXAQRATE
        {
            get { return _BBFXAQRate; }
            set
            {
                _BBFXAQRate = value;
                this.NotifyPropertyChanged("BBFXAQRATE");
            }
        }

        private decimal _BBWXLongQ = default(decimal);
        [Display(Name = "BBWXLONGQ", Description = "週小台多口數")]
        public decimal BBWXLONGQ
        {
            get { return _BBWXLongQ; }
            set
            {
                _BBWXLongQ = value;
                this.NotifyPropertyChanged("BBWXLONGQ");
            }
        }

        private decimal _BBWXShortQ = default(decimal);
        [Display(Name = "BBWXSHORTQ", Description = "週小台空口數")]
        public decimal BBWXSHORTQ
        {
            get { return _BBWXShortQ; }
            set
            {
                _BBWXShortQ = value;
                this.NotifyPropertyChanged("BBWXSHORTQ");
            }
        }

        private decimal _BBWXQRate = default(decimal);
        [Display(Name = "BBWXQRATE", Description = "週小台口數比")]
        public decimal BBWXQRATE
        {
            get { return _BBWXQRate; }
            set
            {
                _BBWXQRate = value;
                this.NotifyPropertyChanged("BBWXQRATE");
            }
        }

        private decimal _BBWXLongAQ = default(decimal);
        [Display(Name = "BBWXLONGAQ", Description = "週小台多戶數")]
        public decimal BBWXLONGAQ
        {
            get { return _BBWXLongAQ; }
            set
            {
                _BBWXLongAQ = value;
                this.NotifyPropertyChanged("BBWXLONGAQ");
            }
        }

        private decimal _BBWXShortAQ = default(decimal);
        [Display(Name = "BBWXSHORTAQ", Description = "週小台空戶數")]
        public decimal BBWXSHORTAQ
        {
            get { return _BBWXShortAQ; }
            set
            {
                _BBWXShortAQ = value;
                this.NotifyPropertyChanged("BBWXSHORTAQ");
            }
        }

        private decimal _BBWXAQRate = default(decimal);
        [Display(Name = "BBWXAQRATE", Description = "週小台戶數比")]
        public decimal BBWXAQRATE
        {
            get { return _BBWXAQRate; }
            set
            {
                _BBWXAQRate = value;
                this.NotifyPropertyChanged("BBWXAQRATE");
            }
        }

        private decimal _BBMXQRate100 = default(decimal);
        [Display(Name = "BBMXQRATE100", Description = " ")]
        public decimal BBMXQRATE100
        {
            get { return _BBMXQRate100; }
            set
            {
                _BBMXQRate100 = value;
                this.NotifyPropertyChanged("BBMXQRATE100");
            }
        }

        private decimal _BBMXLSQ = default(decimal);
        [Display(Name = "BBMXLSQ", Description = " ")]
        public decimal BBMXLSQ
        {
            get { return _BBMXLSQ; }
            set
            {
                _BBMXLSQ = value;
                this.NotifyPropertyChanged("BBMXLSQ");
            }
        }

        private decimal _BBSpread = default(decimal);
        [Display(Name = "BBSPREAD", Description = " ")]
        public decimal BBSPREAD
        {
            get { return _BBSpread; }
            set
            {
                _BBSpread = value;
                this.NotifyPropertyChanged("BBSPREAD");
            }
        }

        private decimal _BBMXLongQNet = default(decimal);
        [Display(Name = "BBMXLONGQNET", Description = "前日小台多口數")]
        public decimal BBMXLONGQNET
        {
            get { return _BBMXLongQNet; }
            set
            {
                _BBMXLongQNet = value;
                this.NotifyPropertyChanged("BBMXLONGQNET");
            }
        }

        private decimal _BBMXShortQNet = default(decimal);
        [Display(Name = "BBMXSHORTQNET", Description = "前日小台多口數")]
        public decimal BBMXSHORTQNET
        {
            get { return _BBMXShortQNet; }
            set
            {
                _BBMXShortQNet = value;
                this.NotifyPropertyChanged("BBMXSHORTQNET");
            }
        }

        private decimal _BBMXQRate100Net = default(decimal);
        [Display(Name = "BBMXQRATE100NET", Description = " ")]
        public decimal BBMXQRATE100NET
        {
            get { return _BBMXQRate100Net; }
            set
            {
                _BBMXQRate100Net = value;
                this.NotifyPropertyChanged("BBMXQRATE100NET");
            }
        }

        private decimal _BBMXLSQNet = default(decimal);
        [Display(Name = "BBMXLSQNET", Description = " ")]
        public decimal BBMXLSQNET
        {
            get { return _BBMXLSQNet; }
            set
            {
                _BBMXLSQNet = value;
                this.NotifyPropertyChanged("BBMXLSQNET");
            }
        }

        private decimal _BBMXLongAQNet = default(decimal);
        [Display(Name = "BBMXLONGAQNET", Description = "前日小台多戶數")]
        public decimal BBMXLONGAQNET
        {
            get { return _BBMXLongAQNet; }
            set
            {
                _BBMXLongAQNet = value;
                this.NotifyPropertyChanged("BBMXLONGAQNET");
            }
        }

        private decimal _BBMXShortAQNet = default(decimal);
        [Display(Name = "BBMXSHORTAQNET", Description = "前日小台空戶數")]
        public decimal BBMXSHORTAQNET
        {
            get { return _BBMXShortAQNet; }
            set
            {
                _BBMXShortAQNet = value;
                this.NotifyPropertyChanged("BBMXSHORTAQNET");
            }
        }

        public BBRate_Data()
        {
            _Subject_Str = default(string);
            _Date = default(string);
            _Time = default(string);
            _BBindexS = default(decimal);
            _BBindexF = default(decimal);
            _BBTXLongQ = default(decimal);
            _BBTXShortQ = default(decimal);
            _BBTXQRate = default(decimal);
            _BBTXLongAQ = default(decimal);
            _BBTXShortAQ = default(decimal);
            _BBTXAQRate = default(decimal);
            _BBMXLongQ = default(decimal);
            _BBMXShortQ = default(decimal);
            _BBMXQRate = default(decimal);
            _BBMXLongAQ = default(decimal);
            _BBMXShortAQ = default(decimal);
            _BBMXAQRate = default(decimal);
            _BBEXLongQ = default(decimal);
            _BBEXShortQ = default(decimal);
            _BBEXQRate = default(decimal);
            _BBEXLongAQ = default(decimal);
            _BBEXShortAQ = default(decimal);
            _BBEXAQRate = default(decimal);
            _BBFXLongQ = default(decimal);
            _BBFXShortQ = default(decimal);
            _BBFXQRate = default(decimal);
            _BBFXLongAQ = default(decimal);
            _BBFXShortAQ = default(decimal);
            _BBFXAQRate = default(decimal);
            _BBWXLongQ = default(decimal);
            _BBWXShortQ = default(decimal);
            _BBWXQRate = default(decimal);
            _BBWXLongAQ = default(decimal);
            _BBWXShortAQ = default(decimal);
            _BBWXAQRate = default(decimal);
            _BBMXQRate100 = default(decimal);
            _BBMXLSQ = default(decimal);
            _BBSpread = default(decimal);
            _BBMXLongQNet = default(decimal);
            _BBMXShortQNet = default(decimal);
            _BBMXQRate100Net = default(decimal);
            _BBMXLSQNet = default(decimal);
            _BBMXLongAQNet = default(decimal);
            _BBMXShortAQNet = default(decimal);
        }

        public static Dictionary<string, string> Get_Prop_Display()
        {
            Dictionary<string, string> _Infos = new Dictionary<string, string>();
            try
            {
                PropertyInfo[] propInfos = default(PropertyInfo[]);
                propInfos = typeof(BBRate_Data).GetProperties();
                propInfos.ToList().ForEach(x =>
                {
                    IList<CustomAttributeData> cad = (x as System.Reflection.MemberInfo).GetCustomAttributesData();
                    _Infos.Add(x.Name, cad[0].NamedArguments[1].TypedValue.Value.ToString());
                });
                return _Infos;
            }
            catch (Exception ex)
            {
                return _Infos;
            }
        }

        /// <summary>
        /// 此物件轉為Dictionary狀態
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> Get_Prop_KeyValue()
        {
            Dictionary<string, object> dic_result = new Dictionary<string, object>();
            try
            {
                dic_result = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary(prop => prop.Name, prop => prop.GetValue(this, null));

                return dic_result;
            }
            catch (Exception ex)
            {
                return dic_result;
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public object Clone_Deep()
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            return formatter.Deserialize(stream);
        }

        public object[] ToArray()
        {
            return new object[] { _Subject_Str, _Date, _Time, _BBindexS, _BBindexF, _BBTXLongQ, _BBTXShortQ, _BBTXQRate, _BBTXLongAQ, _BBTXShortAQ, _BBTXAQRate, _BBMXLongQ, _BBMXShortQ, _BBMXQRate, _BBMXLongAQ, _BBMXShortAQ, _BBMXAQRate, _BBEXLongQ, _BBEXShortQ, _BBEXQRate, _BBEXLongAQ, _BBEXShortAQ, _BBEXAQRate, _BBFXLongQ, _BBFXShortQ, _BBFXQRate, _BBFXLongAQ, _BBFXShortAQ, _BBFXAQRate, _BBWXLongQ, _BBWXShortQ, _BBWXQRate, _BBWXLongAQ, _BBWXShortAQ, _BBWXAQRate, _BBMXQRate100, _BBMXLSQ, _BBSpread, _BBMXLongQNet, _BBMXShortQNet, _BBMXQRate100Net, _BBMXLSQNet, _BBMXLongAQNet, _BBMXShortAQNet };
        }

        public override string ToString()
        {
            string str_result = string.Format("|{0}|", string.Join("|", this.ToArray()));
            return str_result;
        }

        public string Redis_Publish_String(string str_KeyTime)
        {
            try
            {
                Dictionary<string, object> dic_obj = Get_Prop_KeyValue();
                List<string> list_result = new List<string>();

                foreach (KeyValuePair<string, object> obj in dic_obj)
                {
                    switch (obj.Key)
                    {
                        case "Subject_Str":
                        case "TIME":
                        case "BBMXQRATE100":
                        case "BBMXLSQ":
                        case "BBSPREAD":
                        case "BBMXLONGQNET":
                        case "BBMXSHORTQNET":
                        case "BBMXQRATE100NET":
                        case "BBMXLSQNET":
                            //  這些都是不需要Publish的欄位
                            break;

                        case "DATE":
                            list_result.Add((string)obj.Value);
                            break;

                        case "BBWXAQRATE":
                            list_result.Add(Utility.Decimal_2_String((decimal)obj.Value));
                            list_result.Add(str_KeyTime); //  因為依據文件Publish內容,所以"KeyTime"要在"BBWXAQRATE"後面才加入
                            break;

                        default:
                            list_result.Add(Utility.Decimal_2_String((decimal)obj.Value));
                            break;
                    }
                }

                return string.Join("|", list_result.ToArray());
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}