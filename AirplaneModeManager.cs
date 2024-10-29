using System;
using System.Runtime.InteropServices;

namespace AirplaneModeManager
{
    /// <summary>
    /// Represents the reason for radio state changes.
    /// Adjust the structure based on actual implementation details if needed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RADIO_CHANGE_REASON
    {
        public uint Reason;
    }

    /// <summary>
    /// Represents the COM interface for radio instance collection.
    /// Methods are not defined as they are not used in this context.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("00000000-0000-0000-C000-000000000046")] // IUnknown GUID
    public interface IUIRadioInstanceCollection
    {
        // Methods can be defined here if needed
    }

    /// <summary>
    /// Represents the COM interface for Radio Manager.
    /// </summary>
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

    /// <summary>
    /// Manages the system's Airplane (Flight) mode using COM interop.
    /// </summary>
    public class RadioManager : IDisposable
    {
        // Define GUIDs
        private static readonly Guid CLSID_RadioManagementAPI = new Guid("581333F6-28DB-41BE-BC7A-FF201F12F3F6");
        private static readonly Guid IID_IRadioManager = new Guid("DB3AFBFB-08E6-46C6-AA70-BF9A34C30AB7");

        private IRadioManager _radioManager;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioManager"/> class.
        /// </summary>
        public RadioManager()
        {
            // Create an instance of the RadioManager COM object
            Type radioManagerType = Type.GetTypeFromCLSID(CLSID_RadioManagementAPI);
            if (radioManagerType == null)
            {
                throw new COMException("Failed to get the Type from CLSID_RadioManagementAPI.");
            }

            object obj = Activator.CreateInstance(radioManagerType);
            if (obj == null)
            {
                throw new COMException("Failed to create an instance of RadioManager.");
            }

            _radioManager = obj as IRadioManager;
            if (_radioManager == null)
            {
                Marshal.ReleaseComObject(obj);
                throw new COMException("The created object does not implement IRadioManager.");
            }
        }

        /// <summary>
        /// Gets the current state of the flight mode.
        /// </summary>
        /// <returns>True if flight mode is enabled; otherwise, false.</returns>
        public bool GetFlightModeState()
        {
            int hr = _radioManager.GetSystemRadioState(out int bEnabled, out int param2, out RADIO_CHANGE_REASON changeReason);
            if (hr < 0)
            {
                throw new COMException($"GetSystemRadioState failed with HRESULT: 0x{hr:X}", hr);
            }

            // Assuming bEnabled == 0 means flight mode is on
            return bEnabled == 0;
        }

        /// <summary>
        /// Sets the flight mode state.
        /// </summary>
        /// <param name="enable">True to enable flight mode; false to disable.</param>
        public void SetFlightModeState(bool enable)
        {
            int newState = enable ? 0 : 1; // Assuming 0 = enabled, 1 = disabled
            int hr = _radioManager.SetSystemRadioState(newState);
            if (hr < 0)
            {
                throw new COMException($"SetSystemRadioState failed with HRESULT: 0x{hr:X}", hr);
            }

            // Optionally, refresh the radio state
            hr = _radioManager.Refresh();
            if (hr < 0)
            {
                throw new COMException($"Refresh failed with HRESULT: 0x{hr:X}", hr);
            }
        }

        /// <summary>
        /// Releases the COM object and other resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_radioManager != null)
                {
                    Marshal.ReleaseComObject(_radioManager);
                    _radioManager = null;
                }
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Destructor to ensure resources are released.
        /// </summary>
        ~RadioManager()
        {
            Dispose();
        }
    }
}
