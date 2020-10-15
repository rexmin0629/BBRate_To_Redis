using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace BBRate_To_Redis
{
    [Serializable]
    public class Class_Redis_Subscribe : INotifyPropertyChanged, ICloneable
    {
         public event PropertyChangedEventHandler PropertyChanged;
         private void NotifyPropertyChanged(string name)
         {
                 if (PropertyChanged != null)
                     PropertyChanged(this, new PropertyChangedEventArgs(name));
         }
    
         private string _Channel = default(string);
         [Display(Name = "CHANNEL", Description = " ")]
         public string CHANNEL
         {
              get { return _Channel; }
              set 
              {
                        _Channel = value;
                        this.NotifyPropertyChanged("CHANNEL");
              }
         }

         private string _Message = default(string);
         [Display(Name = "MESSAGE", Description = " ")]
         public string MESSAGE
         {
              get { return _Message; }
              set 
              {
                        _Message = value;
                        this.NotifyPropertyChanged("MESSAGE");
              }
         }

         public Class_Redis_Subscribe()
         {
              _Channel = default(string);
              _Message = default(string);
         }
    
         public static Dictionary<string, string> Get_Prop_Display()
         {
              Dictionary<string, string> _Infos = new Dictionary<string, string>();
              try
              {
                     PropertyInfo[] propInfos = default(PropertyInfo[]);
                     propInfos = typeof(Class_Redis_Subscribe).GetProperties();
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
              return new object[] { _Channel,_Message };
         }
    
         public override string ToString()
         {
              return string.Format("|{0}|{1}|", _Channel,_Message);
         }
    
    }
}
