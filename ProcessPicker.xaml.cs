using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UXDowngrader
{
    public partial class ProcessPicker : UserControl
    {
        public Process SelectedProcess
        {
            get { return (Process) GetValue(SelectedProcessProperty); }
            set { SetValue(SelectedProcessProperty, value); }
        }

        public static readonly DependencyProperty SelectedProcessProperty =
            DependencyProperty.Register(
                "SelectedProcess",
                typeof(Process),
                typeof(ProcessPicker),
                new UIPropertyMetadata(null));

        private bool crosshairActive = false;
        private Process candidateProcess;
        private Cursor crosshairCursor;
        private OverrideCursor overrideCursor;

        public ProcessPicker()
        {
            InitializeComponent();

            crosshairCursor = new Cursor(new MemoryStream(Properties.Resources.Crosshair));
        }

        private void crosshair_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(crosshair);
        }

        private void crosshair_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);

            if (candidateProcess != null)
                SelectedProcess = candidateProcess;
            else
                SelectedProcess = null;
        }

        private void crosshair_GotMouseCapture(object sender, MouseEventArgs e)
        {
            crosshairActive = true;
            candidateProcess = null;

            overrideCursor = new OverrideCursor(crosshairCursor);
        }

        private void crosshair_LostMouseCapture(object sender, MouseEventArgs e)
        {
            overrideCursor.Dispose();
            overrideCursor = null;

            crosshairActive = false;
            infoTextBlock.Text = "";
        }

        private void crosshair_MouseMove(object sender, MouseEventArgs e)
        {
            if (!crosshairActive)
                return;

            candidateProcess = null;

            var point = this.PointToScreen(e.GetPosition(this));

            var hwnd = WindowFromPoint(point);
            if (hwnd != IntPtr.Zero)
            {
                uint processId;
                GetWindowThreadProcessId(hwnd, out processId);

                var process = Process.GetProcessById((int)processId);
                if (processId != Process.GetCurrentProcess().Id)
                    candidateProcess = process;
            }

            if (candidateProcess != null)
                infoTextBlock.Text = candidateProcess.ProcessName;
            else
                infoTextBlock.Text = "";
        }

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator Point(POINT p)
            {
                return new Point(p.X, p.Y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT((int)p.X, (int)p.Y);
            }
        }
    }
}
