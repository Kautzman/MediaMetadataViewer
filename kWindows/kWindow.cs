using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace kWindows.Core
{
    public class kWindow : Window, IDisposable
    {
        // IDisposable vars
        protected bool disposed = false;
        protected SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        private WINDOWPLACEMENT m_WindowPlacement;
        public WINDOWPLACEMENT WindowPlacement
        {
            get
            {
                IntPtr handle = new WindowInteropHelper(this).Handle;
                if (handle == null)
                {
                    return default(WINDOWPLACEMENT);
                }

                GetWindowPlacement(handle, out m_WindowPlacement);
                return m_WindowPlacement;
            }

            set
            {
                if (m_WindowPlacement != value)
                {
                    m_WindowPlacement = value;
                    m_WindowPlacement.Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    m_WindowPlacement.Flags = 0;
                    m_WindowPlacement.ShowCmd = (m_WindowPlacement.ShowCmd == (int)ShowWindowsCommands.SW_SHOWMINIMIZED ? (int)ShowWindowsCommands.SW_SHOWNORMAL : m_WindowPlacement.ShowCmd);
                    IntPtr handle = new WindowInteropHelper(this).Handle;
                    SetWindowPlacement(handle, ref m_WindowPlacement);
                }
            }
        }

        public kWindow()
        {
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        protected static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = false; // A false value allows the default resizing logic of WPF to fall through and actually respect the XAML.
                    break;
            }
            return (IntPtr)0;
        }

        // I'm unsure how well this is going to work with more exotic DPI settings, but we'll keep an eye on it.
        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);

                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;

                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        // The default windowing controls -- Pay attention to method names when calling them from buttons!
        #region Window Controls
        protected void WindowMiminizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected void WindowMaximizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        protected void WindowCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected void WindowCollapseClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        protected void EnableDrag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Override this on the extended class if you have stuff you need to dispose of in the window.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // func goes here
            }

            disposed = true;
        }
        #endregion

        #region Helper Utilities

        protected void ShowClickable_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Hand;
        }

        protected void ShowClickable_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.Cursor != Cursors.Wait)
                Mouse.OverrideCursor = Cursors.Arrow;
        }

        public static IEnumerable<T> FindWindowChildren<T>(DependencyObject dObj) where T : DependencyObject
        {
            if (dObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dObj, i);

                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }
                    foreach (T nestedChild in FindWindowChildren<T>(child))
                    {
                        yield return nestedChild;
                    }
                }
            }
        }
        #endregion

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
    }
}
