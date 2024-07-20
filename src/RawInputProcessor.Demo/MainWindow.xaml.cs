using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

            foreach (var key in Enum.GetValues(typeof(Key)))
            {
                cbxPttKey.Items.Add(key.ToString());
            }

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
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
                if (e.Device.Name.Contains(keyboardName.Text) && e.Key.ToString() == cbxPttKey.Text)
                //e.Key == System.Windows.Input.Key.Space)
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
            this.Title += $"   系统版本：{Environment.OSVersion.Version},Major={Environment.OSVersion.Version.Major}";
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

        private void CatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (Event?.Device != null)
            {
                int i = Event.Device.Name.IndexOf("VID");
                if (i >= 0 && Event.Device.Name.Length >= i + 17)
                {
                    keyboardName.Text = Event.Device.Name.Substring(i, 17);
                }

                cbxPttKey.SelectedValue = Event.Key.ToString();
            }

            InputTextBox.Focus();
        }
    }
}