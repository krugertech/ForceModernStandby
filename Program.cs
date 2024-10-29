using AirplaneModeManager;
using System.Runtime.InteropServices;

class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            using (RadioManager radioManager = new RadioManager())
            {
                // Get the current flight mode state
                bool isFlightModeEnabled = radioManager.GetFlightModeState();
                Console.WriteLine($"Old flight-mode state was: {(isFlightModeEnabled ? "on" : "off")}");

                // Toggle the flight mode
                bool newState = !isFlightModeEnabled;
                radioManager.SetFlightModeState(newState);
                Console.WriteLine($"Flight mode has been turned {(newState ? "on" : "off")}.");
            }
        }
        catch (COMException comEx)
        {
            Console.WriteLine($"COM Exception: {comEx.Message} (HRESULT: 0x{comEx.ErrorCode:X})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}