using System;
using System.Management;

namespace ForceModernStandby
{
    class NetworkAdapterManager
    {
        /// <summary>
        /// Lists all network adapters with their names and statuses.
        /// </summary>
        public static void ListNetworkAdapters()
        {
            Console.WriteLine("Listing all network adapters:\n");

            // Query WMI for physical network adapters
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    string name = adapter["Name"]?.ToString() ?? "Unknown";
                    string status = adapter["NetConnectionStatus"]?.ToString() ?? "Unknown";

                    // Interpret the NetConnectionStatus code
                    string statusDescription = GetConnectionStatusDescription(status);

                    Console.WriteLine($"Name: {name}");
                    Console.WriteLine($"Status: {statusDescription}");
                    Console.WriteLine(new string('-', 40));
                }
            }
        }

        /// <summary>
        /// Disables all active (enabled) physical network adapters.
        /// </summary>
        public static void DisableAllNetworkAdapters()
        {
            // Query WMI for physical network adapters
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    string name = adapter["Name"]?.ToString() ?? "Unknown";
                    bool isEnabled = Convert.ToBoolean(adapter["NetEnabled"] ?? false);

                    if (isEnabled)
                    {
                        Console.WriteLine($"Disabling adapter: {name}");

                        try
                        {
                            // Invoke the Disable method
                            ManagementBaseObject outParams = adapter.InvokeMethod("Disable", null, null);

                            // Check the return value for success/failure
                            uint returnValue = (uint)(outParams.Properties["ReturnValue"]?.Value ?? 1);
                            if (returnValue == 0)
                            {
                                Console.WriteLine($"Successfully disabled: {name}");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to disable: {name}. Return code: {returnValue}");
                            }
                        }
                        catch (ManagementException mex)
                        {
                            Console.WriteLine($"Error disabling adapter {name}: {mex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error disabling adapter {name}: {ex.Message}");
                        }
                    }
                    //else
                    //{
                    //    Console.WriteLine($"Adapter already disabled: {name}");
                    //}

                    //Console.WriteLine(new string('-', 40));
                }
            }
        }

        /// <summary>
        /// Enables all physical network adapters.
        /// </summary>
        public static void EnableAllNetworkAdapters()
        {
            // Query WMI for physical network adapters
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    string name = adapter["Name"]?.ToString() ?? "Unknown";
                    bool isEnabled = Convert.ToBoolean(adapter["NetEnabled"] ?? false);

                    if (!isEnabled)
                    {
                        Console.WriteLine($"Enabling adapter: {name}");

                        try
                        {
                            // Invoke the Enable method
                            ManagementBaseObject outParams = adapter.InvokeMethod("Enable", null, null);

                            // Check the return value for success/failure
                            uint returnValue = (uint)(outParams.Properties["ReturnValue"]?.Value ?? 1);
                            if (returnValue == 0)
                            {
                                Console.WriteLine($"Successfully enabled: {name}");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to enable: {name}. Return code: {returnValue}");
                            }
                        }
                        catch (ManagementException mex)
                        {
                            Console.WriteLine($"Error enabling adapter {name}: {mex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unexpected error enabling adapter {name}: {ex.Message}");
                        }
                    }
                    //else
                    //{
                    //    Console.WriteLine($"Adapter already enabled: {name}");
                    //}

                    //Console.WriteLine(new string('-', 40));
                }
            }
        }

        /// <summary>
        /// Translates the NetConnectionStatus code to a human-readable description.
        /// </summary>
        /// <param name="statusCode">The NetConnectionStatus code.</param>
        /// <returns>Description of the connection status.</returns>
        private static string GetConnectionStatusDescription(string statusCode)
        {
            return statusCode switch
            {
                "0" => "Disconnected",
                "1" => "Connecting",
                "2" => "Connected",
                "3" => "Disconnecting",
                "4" => "Hardware not present",
                "5" => "Hardware disabled",
                "6" => "Hardware malfunction",
                "7" => "Media disconnected",
                "8" => "Authenticating",
                "9" => "Authentication succeeded",
                "10" => "Authentication failed",
                "11" => "Invalid Address",
                "12" => "Credentials Required",
                _ => "Unknown",
            };
        }
    }
}
