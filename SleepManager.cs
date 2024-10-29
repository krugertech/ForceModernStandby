using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace StandbyMe
{
    /// <summary>
    /// Provides methods to control the computer's sleep state.
    /// </summary>
    public class SleepManager
    {
        // Import the SendMessageTimeout function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags fuFlags,
            uint uTimeout,
            out IntPtr lpdwResult);

        // Import the PostMessage function from user32.dll
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool PostMessage(
            IntPtr hWnd,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam);

        // Constants for the SendMessage functions
        private const uint WM_SYSCOMMAND = 0x0112;
        private const uint SC_MONITORPOWER = 0xF170;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(-1);
        private static readonly IntPtr MONITOR_OFF = new IntPtr(2);
        private const uint TIMEOUT = 5000; // Timeout in milliseconds (5 seconds)

        /// <summary>
        /// Turns off the monitor by sending a system command message.
        /// This is a workaround for using SetSuspendState(false, true, true) which
        /// does not work in Windows with modern standby enabled.
        /// </summary>
        public static void ModernStandbySleepWorkaround()
        {
            try
            {
                IntPtr result;
                IntPtr sendResult = SendMessageTimeout(
                    HWND_BROADCAST,
                    WM_SYSCOMMAND,
                    new IntPtr(SC_MONITORPOWER),
                    MONITOR_OFF,
                    SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                    TIMEOUT,
                    out result);

                if (sendResult == IntPtr.Zero)
                {
                    // SendMessageTimeout failed or timed out
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"SendMessageTimeout failed with error code: {error}");
                    // Optionally, log the error or handle it as needed
                }
                else
                {
                    // Optionally, you can process the result if needed
                    Debug.WriteLine("SendMessageTimeout succeeded.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Debug.WriteLine($"Exception in ModernStandbySleepWorkaround: {ex.Message}");
                // Optionally, rethrow or handle the exception as needed
            }
        }

        /// <summary>
        /// Alternative method using PostMessage to turn off the monitor asynchronously.
        /// </summary>
        public static void ModernStandbySleepWorkaroundAsync()
        {
            try
            {
                bool postResult = PostMessage(
                    HWND_BROADCAST,
                    WM_SYSCOMMAND,
                    new IntPtr(SC_MONITORPOWER),
                    MONITOR_OFF);

                if (!postResult)
                {
                    int error = Marshal.GetLastWin32Error();
                    Debug.WriteLine($"PostMessage failed with error code: {error}");
                    // Optionally, log the error or handle it as needed
                }
                else
                {
                    Debug.WriteLine("PostMessage succeeded.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in ModernStandbySleepWorkaroundAsync: {ex.Message}");
                // Optionally, rethrow or handle the exception as needed
            }
        }

        /// <summary>
        /// Flags for the SendMessageTimeout function.
        /// </summary>
        [Flags]
        private enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0000,
            SMTO_BLOCK = 0x0001,
            SMTO_ABORTIFHUNG = 0x0002,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008
        }
    }
}
