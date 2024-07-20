using RawInputProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace RawFormInputProcessor.Demo
{
    public partial class Form1 : Form
    {
        private RawFormsInput _rawInput;
        private int _deviceCount;
        private RawInputEventArgs _event;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show(Environment.OSVersion.Version.ToString());
            bool addMessageFilter = Environment.OSVersion.Version.Major >= 10; //https://blog.csdn.net/Qsir/article/details/75095893
            _rawInput = new RawFormsInput(this, RawInputCaptureMode.Foreground, addMessageFilter);
            _rawInput.KeyPressed += OnKeyPressed;
            //DeviceCount = _rawInput.NumberOfKeyboards;
        }

        private void OnKeyPressed(object sender, RawInputEventArgs e)
        {
            textBox2.Text = e.Key.ToString();
            textBox3.Text = e.Device.Name;
            //Event = e;
            //DeviceCount = _rawInput.NumberOfKeyboards;
            //e.Handled = (ShouldHandle.IsChecked == true);
            //if (ShouldHandle.IsChecked == true)
            {
                if (e.Key == Keys.Space)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }
    }
}
