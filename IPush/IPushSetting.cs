using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BBRate_To_Redis
{
    // 0.未連線, 10.準備連線, 20已連線, 30.準備斷線
    public enum IpushState
    {
        None,
        Success,
        Fail,
        Lost,
        Ready,
    }

    public class IPushSetting : INotifyPropertyChanged
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

        private int _Port = 0;
        public int Port
        {
            get { return _Port; }
            set
            {
                _Port = value;
                this.NotifyPropertyChanged("Port");
            }
        }

        private string _Company = string.Empty;
        public string Company
        {
            get { return _Company; }
            set
            {
                _Company = value;
                this.NotifyPropertyChanged("Company");
            }
        }

        private string _Product = string.Empty;
        public string Product
        {
            get { return _Product; }
            set
            {
                _Product = value;
                this.NotifyPropertyChanged("Product");
            }
        }

        private string _Username = string.Empty;
        public string Username
        {
            get { return _Username; }
            set
            {
                _Username = value;
                this.NotifyPropertyChanged("Username");
            }
        }

        private string _Password = string.Empty;
        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                this.NotifyPropertyChanged("Password");
            }
        }

        private string _Broker_ID = string.Empty;
        public string Broker_ID
        {
            get { return _Broker_ID; }
            set
            {
                _Broker_ID = value;
                this.NotifyPropertyChanged("Broker_ID");
            }
        }

        private string _Account = string.Empty;
        public string Account
        {
            get { return _Account; }
            set
            {
                _Account = value;
                this.NotifyPropertyChanged("Account");
            }
        }

        private int _PacketDelay = 0;
        public int PacketDelay
        {
            get { return _PacketDelay; }
            set
            {
                _PacketDelay = value;
                this.NotifyPropertyChanged("PacketDelay");
            }
        }

        public IPushSetting()
        {
            _IP = string.Empty;
            _Port = 0;
            _Company = string.Empty;
            _Product = string.Empty;
            _Username = string.Empty;
            _Password = string.Empty;
            _Broker_ID = string.Empty;
            _Account = string.Empty;
            _PacketDelay = 0;
        }
    }
}
