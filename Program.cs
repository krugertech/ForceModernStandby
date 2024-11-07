

//class Program
//{
//    static void Main(string[] args)
//    {
//        using (var powerMonitor = new PowerMonitor())
//        {
//            // Subscribe to events
//            powerMonitor.MonitorPowerChanged += (sender, e) =>
//            {
//                Console.Beep();
//                Console.WriteLine($"Monitor power setting changed: {e.State}");
//            };

//            //powerMonitor.ConsoleDisplayStateChanged += (sender, e) =>
//            //{
//            //    Console.Beep();
//            //    Console.WriteLine($"Console display state changed: {e.State}");
//            //};

//            //powerMonitor.OtherPowerSettingChanged += (sender, e) =>
//            //{
//            //    Console.Beep();
//            //    Console.WriteLine($"Other power setting changed: {e.PowerSetting}, Data: {e.Data}");
//            //};

//            Console.WriteLine("Monitoring power notifications. Press Ctrl+C to exit.");
//            powerMonitor.StartMonitoring();
//        }
//    }
//}



//using System;
//using System.Runtime.InteropServices;

//namespace PowerNotificationMonitor
//{
//    class Program
//    {
//        // Import necessary Windows API functions and constants
//        [DllImport("user32.dll", SetLastError = true)]
//        static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern bool UnregisterClass(IntPtr lpClassName, IntPtr hInstance);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern IntPtr CreateWindowEx(
//            uint dwExStyle,
//            [MarshalAs(UnmanagedType.LPStr)] string lpClassName,
//            [MarshalAs(UnmanagedType.LPStr)] string lpWindowName,
//            uint dwStyle,
//            int x,
//            int y,
//            int nWidth,
//            int nHeight,
//            IntPtr hWndParent,
//            IntPtr hMenu,
//            IntPtr hInstance,
//            IntPtr lpParam);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern bool DestroyWindow(IntPtr hWnd);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern bool TranslateMessage([In] ref MSG lpMsg);

//        [DllImport("user32.dll", SetLastError = true)]
//        static extern IntPtr DispatchMessage([In] ref MSG lpMsg);

//        [DllImport("user32.dll")]
//        static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

//        [DllImport("kernel32.dll")]
//        static extern IntPtr GetModuleHandle(string lpModuleName);

//        [DllImport("User32.dll", CharSet = CharSet.Auto)]
//        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

//        [DllImport("User32.dll", CharSet = CharSet.Auto)]
//        public static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

//        [StructLayout(LayoutKind.Sequential)]
//        public struct WNDCLASS
//        {
//            public uint style;
//            public IntPtr lpfnWndProc;
//            public int cbClsExtra;
//            public int cbWndExtra;
//            public IntPtr hInstance;
//            public IntPtr hIcon;
//            public IntPtr hCursor;
//            public IntPtr hbrBackground;
//            [MarshalAs(UnmanagedType.LPStr)]
//            public string lpszMenuName;
//            [MarshalAs(UnmanagedType.LPStr)]
//            public string lpszClassName;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct MSG
//        {
//            public IntPtr hwnd;
//            public uint message;
//            public IntPtr wParam;
//            public IntPtr lParam;
//            public uint time;
//            public POINT pt;
//        }

//        [StructLayout(LayoutKind.Sequential)]
//        public struct POINT
//        {
//            public int x;
//            public int y;
//        }

//        [StructLayout(LayoutKind.Sequential, Pack = 4)]
//        public struct POWERBROADCAST_SETTING
//        {
//            public Guid PowerSetting;
//            public uint DataLength;
//            // Variable-length data follows
//        }

//        // Constants
//        const int WM_POWERBROADCAST = 0x0218;
//        const int PBT_POWERSETTINGCHANGE = 0x8013;
//        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

//        // GUIDs for power settings
//        static Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");
//        static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6FE69556-704A-47A0-8F24-C28D936FDA47");

//        // Delegate for window procedure
//        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

//        static void Main(string[] args)
//        {
//            // Register window class
//            WNDCLASS wndClass = new WNDCLASS();
//            wndClass.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(new WndProcDelegate(WndProc));
//            wndClass.hInstance = GetModuleHandle(null);
//            wndClass.lpszClassName = "PowerNotificationWindowClass";

//            ushort classAtom = RegisterClass(ref wndClass);
//            if (classAtom == 0)
//            {
//                Console.WriteLine("Failed to register window class");
//                return;
//            }

//            // Create message-only window
//            IntPtr hWnd = CreateWindowEx(0, wndClass.lpszClassName, "Power Notification Window", 0, 0, 0, 0, 0, (IntPtr)(-3), IntPtr.Zero, wndClass.hInstance, IntPtr.Zero);
//            if (hWnd == IntPtr.Zero)
//            {
//                Console.WriteLine("Failed to create window");
//                return;
//            }

//            // Register for power setting notifications
//            IntPtr hPowerNotify1 = RegisterPowerSettingNotification(hWnd, ref GUID_MONITOR_POWER_ON, DEVICE_NOTIFY_WINDOW_HANDLE);
//            if (hPowerNotify1 == IntPtr.Zero)
//            {
//                Console.WriteLine("Failed to register power setting notification for monitor power");
//                return;
//            }

//            IntPtr hPowerNotify2 = RegisterPowerSettingNotification(hWnd, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);
//            if (hPowerNotify2 == IntPtr.Zero)
//            {
//                Console.WriteLine("Failed to register power setting notification for console display state");
//                return;
//            }

//            Console.WriteLine("Monitoring power notifications. Press Ctrl+C to exit.");

//            // Message loop
//            MSG msg;
//            while (GetMessage(out msg, IntPtr.Zero, 0, 0))
//            {
//                TranslateMessage(ref msg);
//                DispatchMessage(ref msg);
//            }

//            // Cleanup
//            UnregisterPowerSettingNotification(hPowerNotify1);
//            UnregisterPowerSettingNotification(hPowerNotify2);
//            DestroyWindow(hWnd);
//            UnregisterClass((IntPtr)classAtom, wndClass.hInstance);
//        }

//        static IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
//        {
//            if (msg == WM_POWERBROADCAST)
//            {
//                if ((int)wParam == PBT_POWERSETTINGCHANGE)
//                {
//                    // Marshal the POWERBROADCAST_SETTING structure
//                    POWERBROADCAST_SETTING powerSetting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));

//                    // Calculate the data pointer
//                    int dataOffset = Marshal.OffsetOf(typeof(POWERBROADCAST_SETTING), "DataLength").ToInt32() + Marshal.SizeOf(typeof(uint));
//                    IntPtr pData = (IntPtr)((long)lParam + dataOffset);

//                    // Read the data value
//                    int data = Marshal.ReadInt32(pData);

//                    if (powerSetting.PowerSetting == GUID_MONITOR_POWER_ON)
//                    {
//                        Console.Beep();
//                        Console.WriteLine($"Monitor power setting changed: {data}");
//                    }
//                    else if (powerSetting.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
//                    {
//                        Console.Beep();
//                        Console.WriteLine($"Console display state changed: {data}");
//                    }
//                    else
//                    {
//                        // Handle other power settings if needed
//                        Console.Beep();
//                        Console.WriteLine($"Other power setting changed: {powerSetting.PowerSetting}, Data: {data}");
//                    }
//                }
//            }
//            return DefWindowProc(hWnd, msg, wParam, lParam);
//        }
//    }
//}




using ForceModernStandby;
using PowerNotificationMonitor;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    [STAThread]
    static async Task Main(string[] args)
    {
        try
        {
            using RadioManager radioManager = new RadioManager();
            // Get the current flight mode state.

            using var powerMonitor = new PowerMonitor();
            powerMonitor.MonitorPowerChanged += (sender, e) =>
            {
                Console.Beep();
                Console.WriteLine($"Monitor power setting changed: {e.State}");

                FlightModeState currentState = radioManager.GetFlightModeState();
                Console.WriteLine($"Current flight mode is: {(currentState == FlightModeState.Enabled ? "on" : "off")}");
                if (currentState == FlightModeState.Enabled)
                {
                    NetworkAdapterManager.EnableAllNetworkAdapters();
                    radioManager.SetFlightModeState(FlightModeState.Disabled);
                    Console.WriteLine($"Flight mode has been turned {(radioManager.GetFlightModeState() == FlightModeState.Enabled ? "on" : "off")}.");

                    Console.WriteLine("All operations compelte. Press any key to exit.");
                    Console.ReadKey(true);
                    Environment.Exit(0);
                }
                else
                {

                }
            };

            var t = Task.Run(() => {
                Console.WriteLine("Monitoring power notifications. Press Ctrl+C to exit.");
                powerMonitor.StartMonitoring();
            });
            await Task.Delay(250);
            // Subscribe to events


            //powerMonitor.ConsoleDisplayStateChanged += (sender, e) =>
            //{
            //    Console.Beep();
            //    Console.WriteLine($"Console display state changed: {e.State}");
            //};

            //powerMonitor.OtherPowerSettingChanged += (sender, e) =>
            //{
            //    Console.Beep();
            //    Console.WriteLine($"Other power setting changed: {e.PowerSetting}, Data: {e.Data}");
            //};

            // Toggle the flight mode state
            radioManager.SetFlightModeState(FlightModeState.Enabled);
            Console.WriteLine($"Flight mode has been turned {(radioManager.GetFlightModeState() == FlightModeState.Enabled ? "on" : "off")}.");
            NetworkAdapterManager.DisableAllNetworkAdapters();

            Console.WriteLine("Putting computer into modern standby. Please wait.");
            SleepManager.ModernStandbySleepWorkaround();

            t.Wait();

            //Console.WriteLine("Waiting for computer to resume form standby. Press any key to exit.");
            //Console.ReadKey();
        }
        catch (RadioManagerException ex)
        {
            Console.WriteLine($"RadioManager Error: {ex.Message} (HRESULT: 0x{ex.HResultCode:X})");
            // Optionally, log the error
            Debug.WriteLine($"Error: {ex.Message} (HRESULT: 0x{ex.HResultCode:X})");
        }
        catch (COMException comEx)
        {
            Console.WriteLine($"COM Exception: {comEx.Message} (HRESULT: 0x{comEx.ErrorCode:X})");
            Debug.WriteLine($"COM Exception: {comEx.Message} (HRESULT: 0x{comEx.ErrorCode:X})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Debug.WriteLine($"Exception: {ex.Message}");
        }

    }

}