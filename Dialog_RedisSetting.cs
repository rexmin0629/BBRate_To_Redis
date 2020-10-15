using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BBRate_To_Redis
{
    public partial class Dialog_RedisSetting : Form
    {
        IniFile Redis_Setting = null;
        Dictionary<string, string> Redis_setInfo = null;

        public Dialog_RedisSetting()
        {
            InitializeComponent();
        }

        private void Dialog_RedisSetting_Load(object sender, EventArgs e)
        {
            //  Read Redis Config
            Redis_Setting = new IniFile(Class_Global.Path_Setting);
            Redis_setInfo = Redis_Setting.GetSectionValues("Redis");

            txt_IP.Text = Redis_setInfo["IP"];
            txt_Port.Text = Redis_setInfo["Port"];
            txt_Channel.Text = Redis_setInfo["channel"];
        }

        private void Dialog_RedisSetting_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                bool check_result = true;
                if (string.IsNullOrEmpty(txt_IP.Text))
                {
                    check_result = false;
                }
                if (string.IsNullOrEmpty(txt_Port.Text))
                {
                    check_result = false;
                }
                if (string.IsNullOrEmpty(txt_Channel.Text))
                {
                    check_result = false;
                }

                if (check_result == false)
                {
                    MessageBox.Show("請確認Redis連線資訊", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    Redis_Setting.WriteValue("Redis", "IP", txt_IP.Text);
                    Redis_Setting.WriteValue("Redis", "Port", txt_Port.Text);
                    Redis_Setting.WriteValue("Redis", "channel", txt_Channel.Text);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Dialog_RedisSetting_btn_OK_Click", ex.ToString());
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                Class_Log.Write_Log(Log_Type.Error, "Dialog_RedisSetting_btn_Cancel_Click", ex.ToString());
            }
        }
    }
}
