using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StandbyMe
{
    /// <summary>
    /// Provides method to control the computer's sleep state.
    /// </summary>
    public class SleepManager
    {
        // Import the SendMessage function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Constants for the SendMessage function
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint SC_MONITORPOWER = 0xF170;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(-1);
        private static readonly IntPtr MONITOR_OFF = new IntPtr(2);

        /// <summary>
        /// Turns off the monitor by sending a system command message.
        /// This is workaround to using SetSuspendState(false, true, true) which
        /// does not work in Windows with modern standby enabled.
        /// </summary>
        public static void ModernStandbySleepWorkaround()
        {
            SendMessage(HWND_BROADCAST, WM_SYSCOMMAND, new IntPtr(SC_MONITORPOWER), MONITOR_OFF);
        }
    }
}
