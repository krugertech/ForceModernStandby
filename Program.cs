using ForceModernStandby;
using PowerNotificationMonitor;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{

    static bool _powerRestored = false;
    [STAThread]
    static async Task Main(string[] args)
    {
        object objLock = new object();
        try
        {
            using RadioManager radioManager = new RadioManager();
            using var powerMonitor = new PowerMonitor();
            powerMonitor.MonitorPowerChanged += (sender, e) =>
            {
                lock (objLock)
                {
                    //Console.Beep();
                    //Console.WriteLine($"Monitor power setting changed: {e.State}");

                    FlightModeState currentState = radioManager.GetFlightModeState();
                    //Console.WriteLine($"Current flight mode is: {(currentState == FlightModeState.Enabled ? "on" : "off")}");
                    if (currentState == FlightModeState.Enabled)
                    {
                        NetworkAdapterManager.EnableAllNetworkAdapters();
                        radioManager.SetFlightModeState(FlightModeState.Disabled);
                        Console.WriteLine($"Flight mode has been turned {(radioManager.GetFlightModeState() == FlightModeState.Enabled ? "on" : "off")}.");
                        Console.WriteLine("All operations complete.");
                        _powerRestored = true;
                        //Console.ReadLine();
                        //Environment.Exit(0);
                    }
                    else
                    {

                    }
                }
            };

            radioManager.SetFlightModeState(FlightModeState.Enabled);
            Console.WriteLine($"Flight mode has been turned {(radioManager.GetFlightModeState() == FlightModeState.Enabled ? "on" : "off")}.");
            NetworkAdapterManager.DisableAllNetworkAdapters();

            Console.WriteLine("Putting computer into modern standby. Please wait.");
            SleepManager.ModernStandbySleepWorkaround();

            //await Task.Delay(2000);
            Console.WriteLine("Press Ctrl+C to exit if no response.");
            while (!_powerRestored)
            {
                await Task.Delay(500);
            }

            //Console.WriteLine("Press return to exit.");
            //Console.ReadLine();

        }
        catch (RadioManagerException ex)
        {
            Console.WriteLine($"RadioManager Error: {ex.Message} (HRESULT: 0x{ex.HResultCode:X})");
            Console.WriteLine("Press return key to exit");
            Console.ReadLine();
        }
        catch (COMException comEx)
        {
            Console.WriteLine($"COM Exception: {comEx.Message} (HRESULT: 0x{comEx.ErrorCode:X})");
            Console.WriteLine("Press return key to exit");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine("Press return key to exit");
            Console.ReadLine();
        }
    }
}