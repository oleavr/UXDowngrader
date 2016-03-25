using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Input;

namespace UXDowngrader
{
    public partial class MainWindow : Window
    {
        private Frida.DeviceManager deviceManager;
        private Frida.Device device;
        private LinkedList<Frida.Session> sessions = new LinkedList<Frida.Session>();
        private LinkedList<Frida.Script> scripts = new LinkedList<Frida.Script>();

        private BackgroundWorker goWorker;

        public MainWindow()
        {
            InitializeComponent();

            deviceManager = new Frida.DeviceManager(Dispatcher);

            foreach (var d in deviceManager.EnumerateDevices())
            {
                if (d.Type == Frida.DeviceType.Local)
                {
                    this.device = d;
                    break;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            deviceManager.Dispose();
            deviceManager = null;

            base.OnClosed(e);
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            uint processId;
            if (uint.TryParse(pidTextBox.Text, out processId))
            {
                pidTextBox.IsEnabled = false;
                goButton.IsEnabled = false;

                goWorker = new BackgroundWorker();
                goWorker.DoWork += (s1, e1) =>
                {
                    var session = device.Attach(processId);
                    var script = session.CreateScript(Properties.Resources.Agent);
                    script.Load();

                    sessions.AddLast(session);

                    e1.Result = script;
                };
                goWorker.RunWorkerCompleted += (s2, e2) =>
                {
                    pidTextBox.IsEnabled = true;
                    goButton.IsEnabled = true;

                    try
                    {
                        var script = e2.Result as Frida.Script;
                        scripts.AddLast(script);

                        processPicker.SelectedProcess = null;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.InnerException.Message.Split(new char[] { ' ' }, 2)[1], "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
                goWorker.RunWorkerAsync();
            }
        }
    }
}
