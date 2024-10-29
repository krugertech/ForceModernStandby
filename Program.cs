using StandbyMe;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            FlightModeState newState = FlightModeState.Unknown;
            using (RadioManager radioManager = new RadioManager())
            {
                // Get the current flight mode state
                FlightModeState currentState = radioManager.GetFlightModeState();
                Console.WriteLine($"Old flight-mode state was: {(currentState == FlightModeState.Enabled ? "on" : "off")}");

                // Toggle the flight mode state
                newState = currentState == FlightModeState.Enabled ? FlightModeState.Disabled : FlightModeState.Enabled;
                radioManager.SetFlightModeState(newState);
                Console.WriteLine($"Flight mode has been turned {(newState == FlightModeState.Enabled ? "on" : "off")}.");
            }

            if (newState == FlightModeState.Enabled)
                SleepManager.ModernStandbySleepWorkaround();

            // TODO: Detect modern standby resuming turn off flight mode for convenience.
            // Solution: https://stackoverflow.com/questions/70272575/how-can-i-catch-an-event-when-the-computer-resume-from-sleep-hibernate-mode

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