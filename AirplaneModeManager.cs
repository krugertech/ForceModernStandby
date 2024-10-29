//// AirplaneModeLib/AirplaneModeManager.cs
//using System;
//using System.Management;
//using System.Runtime.InteropServices;

//namespace StandbyMe
//{
//    public class AirplaneModeManager
//    {
//        /// <summary>
//        /// Disables all wireless network adapters to simulate Airplane Mode.
//        /// </summary>
//        public void EnableAirplaneMode()
//        {
//            ToggleNetworkAdapters(enable: false);
//        }

//        /// <summary>
//        /// Enables all wireless network adapters to disable Airplane Mode.
//        /// </summary>
//        public void DisableAirplaneMode()
//        {
//            ToggleNetworkAdapters(enable: true);
//        }

//        /// <summary>
//        /// Initiates Modern Standby (Sleep Mode).
//        /// </summary>
//        public void InitiateModernStandby()
//        {
//            Application.SetSuspendState(PowerState.Suspend, true, true);
//        }

//        private void ToggleNetworkAdapters(bool enable)
//        {
//            try
//            {
//                string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled != NULL AND AdapterTypeID = 9";
//                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
//                using (ManagementObjectCollection adapters = searcher.Get())
//                {
//                    foreach (ManagementObject adapter in adapters)
//                    {
//                        try
//                        {
//                            if (enable)
//                            {
//                                adapter.InvokeMethod("Enable", null);
//                                Console.WriteLine($"Enabled adapter: {adapter["Name"]}");
//                            }
//                            else
//                            {
//                                adapter.InvokeMethod("Disable", null);
//                                Console.WriteLine($"Disabled adapter: {adapter["Name"]}");
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            Console.WriteLine($"Failed to toggle adapter {adapter["Name"]}: {ex.Message}");
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error toggling network adapters: {ex.Message}");
//            }
//        }
//    }

//    internal static class Application
//    {
//        [DllImport("powrprof.dll", SetLastError = true)]
//        public static extern bool SetSuspendState(PowerState state, bool force, bool disableWakeEvent);
//    }

//    public enum PowerState
//    {
//        Suspend = 0,
//        Hibernate = 1
//    }
//}