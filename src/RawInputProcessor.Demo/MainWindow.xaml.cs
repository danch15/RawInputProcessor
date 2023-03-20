using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace RawInputProcessor.Demo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private RawPresentationInput _rawInput;
        private int _deviceCount;
        private RawInputEventArgs _event;

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public int DeviceCount
        {
            get { return _deviceCount; }
            set
            {
                _deviceCount = value;
                OnPropertyChanged();
            }
        }

        public RawInputEventArgs Event
        {
            get { return _event; }
            set
            {
                _event = value;
                OnPropertyChanged();
            }
        }

        private void OnKeyPressed(object sender, RawInputEventArgs e)
        {
            Event = e;
            DeviceCount = _rawInput.NumberOfKeyboards;
            //e.Handled = (ShouldHandle.IsChecked == true);
            if (ShouldHandle.IsChecked == true)
            {
                if (/*e.Device.Name.Contains("") &&*/
                    e.Key == System.Windows.Input.Key.Space)
                {
                    e.Handled = true;
                    return;
                }
            }
            e.Handled = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            StartWndProcHandler();
            base.OnSourceInitialized(e);
        }

        private void StartWndProcHandler()
        {
            MessageBox.Show(Environment.OSVersion.Version.ToString());
            bool addMessageFilter = Environment.OSVersion.Version.Major >= 10; //https://blog.csdn.net/Qsir/article/details/75095893
            _rawInput = new RawPresentationInput(this, RawInputCaptureMode.ForegroundAndBackground, addMessageFilter);
            _rawInput.KeyPressed += OnKeyPressed;
            DeviceCount = _rawInput.NumberOfKeyboards;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}