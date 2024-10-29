using System;
using System.Runtime.InteropServices;

namespace AirplaneModeManager
{
    class Program
    {
        // Define GUIDs
        private static readonly Guid CLSID_RadioManagementAPI = new Guid("581333F6-28DB-41BE-BC7A-FF201F12F3F6");
        private static readonly Guid IID_IRadioManager = new Guid("DB3AFBFB-08E6-46C6-AA70-BF9A34C30AB7");

        // Define HRESULT for error checking
        private const int S_OK = 0;
        private const int FAILED_FLAG = unchecked((int)0x80000000);

        // Define _RADIO_CHANGE_REASON as DWORD (uint)
        [StructLayout(LayoutKind.Sequential)]
        public struct RADIO_CHANGE_REASON
        {
            public uint Reason;
        }

        // Define IUIRadioInstanceCollection as IUnknown
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("00000000-0000-0000-C000-000000000046")] // IUnknown GUID
        public interface IUIRadioInstanceCollection
        {
            // Methods are not defined as they are not used in this context
        }

        // Define the IRadioManager interface
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("DB3AFBFB-08E6-46C6-AA70-BF9A34C30AB7")]
        public interface IRadioManager
        {
            // HRESULT IsRMSupported(DWORD* pdwState);
            int IsRMSupported(out uint pdwState);

            // HRESULT GetUIRadioInstances(IUIRadioInstanceCollection** param_1);
            int GetUIRadioInstances(out IUIRadioInstanceCollection param1);

            // HRESULT GetSystemRadioState(int* pbEnabled, int* param_2, _RADIO_CHANGE_REASON* param_3);
            int GetSystemRadioState(out int pbEnabled, out int param2, out RADIO_CHANGE_REASON param3);

            // HRESULT SetSystemRadioState(int bEnabled);
            int SetSystemRadioState(int bEnabled);

            // HRESULT Refresh();
            int Refresh();

            // HRESULT OnHardwareSliderChange(int param_1, int param_2);
            int OnHardwareSliderChange(int param1, int param2);
        }

        static void Main(string[] args)
        {
            try
            {
                // Create an instance of the RadioManager COM object
                IRadioManager radioManager = (IRadioManager)Activator.CreateInstance(
                    Type.GetTypeFromCLSID(CLSID_RadioManagementAPI));

                if (radioManager == null)
                {
                    Console.WriteLine("Failed to create RadioManager instance.");
                    return;
                }

                // Get the current system radio (flight mode) state
                int hr = radioManager.GetSystemRadioState(out int bEnabled, out int param2, out RADIO_CHANGE_REASON changeReason);
                if (hr < 0)
                {
                    Console.WriteLine($"GetSystemRadioState failed with HRESULT: 0x{hr:X}");
                    Marshal.ReleaseComObject(radioManager);
                    return;
                }

                Console.WriteLine($"Old flight-mode state was: {(bEnabled == 0 ? "on" : "off")}");

                // Set flight mode to the opposite state
                int newState = (bEnabled == 0) ? 1 : 0;
                hr = radioManager.SetSystemRadioState(newState);
                if (hr < 0)
                {
                    Console.WriteLine($"SetSystemRadioState failed with HRESULT: 0x{hr:X}");
                }
                else
                {
                    Console.WriteLine($"Flight mode has been turned {(newState == 0 ? "on" : "off")}.");
                }

                // Optionally, refresh the radio state
                hr = radioManager.Refresh();
                if (hr < 0)
                {
                    Console.WriteLine($"Refresh failed with HRESULT: 0x{hr:X}");
                }

                // Release the COM object
                Marshal.ReleaseComObject(radioManager);
            }
            catch (COMException comEx)
            {
                Console.WriteLine($"COM Exception: {comEx.Message} (HRESULT: 0x{comEx.ErrorCode:X})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Console.ReadLine();
        }
    }
}