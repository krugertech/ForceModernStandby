using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace StandbyMe
{
    /// <summary>
    /// Represents the state of the flight mode.
    /// </summary>
    public enum FlightModeState
    {
        Unknown = -1,
        Enabled = 0,
        Disabled = 1
    }

    /// <summary>
    /// Represents the reasons for radio state changes.
    /// Extend this enum based on actual reasons if known.
    /// </summary>
    [Flags]
    public enum RadioChangeReason : uint
    {
        Unknown = 0,
        UserInitiated = 1,
        SystemUpdate = 2,
        // Add other reasons as per API documentation
    }

    /// <summary>
    /// Represents specific errors related to RadioManager operations.
    /// </summary>
    public class RadioManagerException : Exception
    {
        /// <summary>
        /// Gets the HRESULT error code.
        /// </summary>
        public int HResultCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioManagerException"/> class with a specified error message and HRESULT.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="hresult">The HRESULT error code.</param>
        public RadioManagerException(string message, int hresult) : base(message)
        {
            HResultCode = hresult;
        }
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
        int GetSystemRadioState(out int pbEnabled, out int param2, out RadioChangeReason param3);

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
    public sealed class RadioManager : IDisposable
    {
        // Define GUIDs
        private static readonly Guid CLSID_RadioManagementAPI = new Guid("581333F6-28DB-41BE-BC7A-FF201F12F3F6");
        private static readonly Guid IID_IRadioManager = new Guid("DB3AFBFB-08E6-46C6-AA70-BF9A34C30AB7");

        private IRadioManager _radioManager;
        private bool _disposed = false;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioManager"/> class.
        /// </summary>
        public RadioManager()
        {
            Log("Initializing RadioManager.");

            // Create an instance of the RadioManager COM object
            Type radioManagerType = Type.GetTypeFromCLSID(CLSID_RadioManagementAPI);
            if (radioManagerType == null)
            {
                Log("Failed to get Type from CLSID_RadioManagementAPI.");
                throw new RadioManagerException("Failed to get the Type from CLSID_RadioManagementAPI.", unchecked((int)0x80004005)); // E_FAIL
            }

            object obj = Activator.CreateInstance(radioManagerType);
            if (obj == null)
            {
                Log("Failed to create an instance of RadioManager.");
                throw new RadioManagerException("Failed to create an instance of RadioManager.", unchecked((int)0x80004005)); // E_FAIL
            }

            _radioManager = obj as IRadioManager;
            if (_radioManager == null)
            {
                Marshal.ReleaseComObject(obj);
                Log("The created object does not implement IRadioManager.");
                throw new RadioManagerException("The created object does not implement IRadioManager.", unchecked((int)0x80004002)); // E_NOINTERFACE
            }

            Log("RadioManager initialized successfully.");
        }

        /// <summary>
        /// Gets the current state of the flight mode.
        /// </summary>
        /// <returns><see cref="FlightModeState.Enabled"/> if flight mode is enabled; otherwise, <see cref="FlightModeState.Disabled"/>.</returns>
        /// <exception cref="RadioManagerException">Thrown when the COM call fails.</exception>
        public FlightModeState GetFlightModeState()
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                Log("Calling GetSystemRadioState.");

                int hr = _radioManager.GetSystemRadioState(out int bEnabled, out int param2, out RadioChangeReason changeReason);
                if (hr < 0)
                {
                    Log($"GetSystemRadioState failed with HRESULT: 0x{hr:X}");
                    throw new RadioManagerException($"GetSystemRadioState failed with HRESULT: 0x{hr:X}", hr);
                }

                Log($"GetSystemRadioState succeeded. bEnabled: {bEnabled}, ChangeReason: {changeReason}");
                return (FlightModeState)bEnabled;
            }
        }

        /// <summary>
        /// Sets the flight mode state.
        /// </summary>
        /// <param name="state">The desired flight mode state.</param>
        /// <exception cref="RadioManagerException">Thrown when the COM call fails.</exception>
        public void SetFlightModeState(FlightModeState state)
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                Log($"Setting flight mode state to: {state}.");

                int newState = (int)state;
                int hr = _radioManager.SetSystemRadioState(newState);
                if (hr < 0)
                {
                    Log($"SetSystemRadioState failed with HRESULT: 0x{hr:X}");
                    throw new RadioManagerException($"SetSystemRadioState failed with HRESULT: 0x{hr:X}", hr);
                }

                Log("SetSystemRadioState succeeded. Refreshing state.");
                // Optionally, refresh the radio state
                hr = _radioManager.Refresh();
                if (hr < 0)
                {
                    Log($"Refresh failed with HRESULT: 0x{hr:X}");
                    throw new RadioManagerException($"Refresh failed with HRESULT: 0x{hr:X}", hr);
                }

                Log("Refresh succeeded.");
            }
        }

        /// <summary>
        /// Releases the COM object and other resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor to ensure resources are released.
        /// </summary>
        ~RadioManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases the COM object and other resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    if (_radioManager != null)
                    {
                        Log("Releasing COM object.");
                        Marshal.ReleaseComObject(_radioManager);
                        _radioManager = null;
                        Log("COM object released.");
                    }
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Ensures that the object has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the object has been disposed.</exception>
        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RadioManager));
            }
        }

        /// <summary>
        /// Logs a message for diagnostics.
        /// </summary>
        /// <param name="message">The message to log.</param>
        private void Log(string message)
        {
            // Example using Debug. Replace with a logging framework as needed.
            Debug.WriteLine($"[{DateTime.Now}] [RadioManager] {message}");
            // TODO: integrate with a logging framework like NLog or Serilog.
        }
    }
}
