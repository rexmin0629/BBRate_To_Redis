using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBRate_To_Redis
{
    // 0.未連線, 10.準備連線, 20已連線, 30.準備斷線
    public enum RedisState
    {
        None,
        Success,
        Fail,
        Lost,
        Ready,
    }

    public class RedisSetting : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string _IP = string.Empty;
        public string IP
        {
            get { return _IP; }
            set
            {
                _IP = value;
                this.NotifyPropertyChanged("IP");
            }
        }

        private string _Port = string.Empty;
        public string Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
                this.NotifyPropertyChanged("Port");
            }
        }

        private string _Channel = string.Empty;
        public string Channel
        {
            get { return _Channel; }
            set
            {
                _Channel = value;
                this.NotifyPropertyChanged("Channel");
            }
        }

        private string _DB = string.Empty;
        public string DB
        {
            get { return _DB; }
            set
            {
                _DB = value;
                this.NotifyPropertyChanged("DB");
            }
        }

        public RedisSetting()
        {
            _IP = string.Empty;
            _Port = string.Empty;
            _Channel = string.Empty;
            _DB = string.Empty;
        }
    }
}
