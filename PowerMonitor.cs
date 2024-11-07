using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PowerNotificationMonitor
{
    public class PowerMonitor : IDisposable
    {
        #region Win32 API Imports
        [DllImport("user32.dll", SetLastError = true)]
        private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterClass(IntPtr lpClassName, IntPtr hInstance);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            [MarshalAs(UnmanagedType.LPStr)] string lpClassName,
            [MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Win32 Structures and Delegates
        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct WNDCLASS
        {
            public uint style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszClassName;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            // Variable-length data follows
        }
        #endregion

        #region Constants
        private const int WM_POWERBROADCAST = 0x0218;
        private const int PBT_POWERSETTINGCHANGE = 0x8013;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        private static Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");
        private static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6FE69556-704A-47A0-8F24-C28D936FDA47");
        private const int WM_QUIT = 0x0012;
        #endregion

        #region Private Fields
        private Thread _messageThread;
        private IntPtr _hWnd;
        private IntPtr _hPowerNotify1;
        private IntPtr _hPowerNotify2;
        private ushort _classAtom;
        private WNDCLASS _wndClass;
        private WndProcDelegate _wndProcDelegate;
        private bool _isDisposed;
        private readonly ManualResetEvent _threadStarted = new ManualResetEvent(false);
        #endregion

        #region Events
        public event EventHandler<PowerStateChangedEventArgs> MonitorPowerChanged;
        public event EventHandler<PowerStateChangedEventArgs> ConsoleDisplayStateChanged;
        public event EventHandler<PowerSettingChangedEventArgs> OtherPowerSettingChanged;
        #endregion

        public PowerMonitor()
        {
            _wndProcDelegate = WndProc;
            _wndClass = new WNDCLASS
            {
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
                hInstance = GetModuleHandle(null),
                lpszClassName = "PowerNotificationWindowClass"
            };

            _classAtom = RegisterClass(ref _wndClass);
            if (_classAtom == 0)
                throw new InvalidOperationException("Failed to register window class");

            // Start the message loop on a dedicated thread
            _messageThread = new Thread(MessageLoop)
            {
                IsBackground = true,
                Name = "PowerMonitorMessageLoopThread"
            };
            _messageThread.SetApartmentState(ApartmentState.STA);
            _messageThread.Start();

            // Wait until the thread has initialized
            _threadStarted.WaitOne();
        }

        private void MessageLoop()
        {
            try
            {
                // Create message-only window
                _hWnd = CreateWindowEx(0, _wndClass.lpszClassName, "Power Notification Window", 0, 0, 0, 0, 0, (IntPtr)(-3), IntPtr.Zero, _wndClass.hInstance, IntPtr.Zero);
                if (_hWnd == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to create window");

                // Register for power setting notifications
                _hPowerNotify1 = RegisterPowerSettingNotification(_hWnd, ref GUID_MONITOR_POWER_ON, DEVICE_NOTIFY_WINDOW_HANDLE);
                if (_hPowerNotify1 == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to register power setting notification for monitor power");

                _hPowerNotify2 = RegisterPowerSettingNotification(_hWnd, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
                if (_hPowerNotify2 == IntPtr.Zero)
                    throw new InvalidOperationException("Failed to register power setting notification for console display state");

                // Signal that the thread has started successfully
                _threadStarted.Set();

                // Message loop
                MSG msg;
                while (GetMessage(out msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PowerMonitor MessageLoop Exception: {ex.Message}");
                // Optionally, handle logging or rethrow
            }
            finally
            {
                Cleanup();
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_POWERBROADCAST && (int)wParam == PBT_POWERSETTINGCHANGE)
            {
                try
                {
                    POWERBROADCAST_SETTING powerSetting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(lParam);
                    int dataOffset = Marshal.OffsetOf<POWERBROADCAST_SETTING>("DataLength").ToInt32() + Marshal.SizeOf<uint>();
                    IntPtr pData = IntPtr.Add(lParam, dataOffset);
                    int data = Marshal.ReadInt32(pData);

                    if (powerSetting.PowerSetting == GUID_MONITOR_POWER_ON)
                    {
                        MonitorPowerChanged?.Invoke(this, new PowerStateChangedEventArgs(data));
                    }
                    else if (powerSetting.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
                    {
                        ConsoleDisplayStateChanged?.Invoke(this, new PowerStateChangedEventArgs(data));
                    }
                    else
                    {
                        OtherPowerSettingChanged?.Invoke(this, new PowerSettingChangedEventArgs(powerSetting.PowerSetting, data));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WndProc Exception: {ex.Message}");
                    // Optionally, handle logging
                }
            }
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private void Cleanup()
        {
            if (_hPowerNotify1 != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_hPowerNotify1);
                _hPowerNotify1 = IntPtr.Zero;
            }

            if (_hPowerNotify2 != IntPtr.Zero)
            {
                UnregisterPowerSettingNotification(_hPowerNotify2);
                _hPowerNotify2 = IntPtr.Zero;
            }

            if (_hWnd != IntPtr.Zero)
            {
                DestroyWindow(_hWnd);
                _hWnd = IntPtr.Zero;
            }

            if (_classAtom != 0)
            {
                UnregisterClass((IntPtr)_classAtom, _wndClass.hInstance);
                _classAtom = 0;
            }
        }

        public void StartMonitoring()
        {
            // The monitoring starts automatically upon instantiation and thread start.
            // This method can be used to ensure the monitoring has started, or to perform additional setup if needed.
            // Currently, it's a placeholder and does not perform any actions.
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Signal the message loop to exit
                    if (_hWnd != IntPtr.Zero)
                    {
                        PostMessage(_hWnd, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
                    }
                }

                // Wait for the thread to finish
                if (_messageThread != null && _messageThread.IsAlive)
                {
                    _messageThread.Join(2000); // Wait for 2 seconds
                }

                _isDisposed = true;
            }
        }

        ~PowerMonitor()
        {
            Dispose(false);
        }
    }

    public class PowerStateChangedEventArgs : EventArgs
    {
        public int State { get; }

        public PowerStateChangedEventArgs(int state)
        {
            State = state;
        }
    }

    public class PowerSettingChangedEventArgs : EventArgs
    {
        public Guid PowerSetting { get; }
        public int Data { get; }

        public PowerSettingChangedEventArgs(Guid powerSetting, int data)
        {
            PowerSetting = powerSetting;
            Data = data;
        }
    }
}
