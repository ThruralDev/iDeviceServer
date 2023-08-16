using iMobileDevice.iDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iMobileDevice;
using iMobileDevice.Lockdown;
using System.Collections.ObjectModel;

class IMobileDeviceHandler : MobileDeviceHandler
{
    private iDeviceEventCallBack callback;
    private iDeviceApi iDeviceApi;
    private String uuid;
    private iDeviceConnectionHandle currentConnectionHandle;

    public IMobileDeviceHandler()
    {
        NativeLibraries.Load();
    }

    public void listDevices() {

        ReadOnlyCollection<string> udids;
        int count = 0;

        var idevice = LibiMobileDevice.Instance.iDevice;
        var lockdown = LibiMobileDevice.Instance.Lockdown;

        var ret = idevice.idevice_get_device_list(out udids, ref count);

        if (ret == iDeviceError.NoDevice)
        {
            // Not actually an error in our case
            Console.WriteLine("No devices connected.");
            return;
        }

        ret.ThrowOnError();

            // Get the device name
            foreach (var udid in udids)
            {
                iDeviceHandle deviceHandle;
                idevice.idevice_new(out deviceHandle, udid).ThrowOnError();

                LockdownClientHandle lockdownHandle;
                lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "Quamotion").ThrowOnError();

                string deviceName;
                lockdown.lockdownd_get_device_name(lockdownHandle, out deviceName).ThrowOnError();

                deviceHandle.Dispose();
                lockdownHandle.Dispose();
            }
        }

    public void beginListening()
    {
        callback = createEventCallback();
        iDeviceError iDeviceError = iDeviceApi.idevice_event_subscribe(callback, new IntPtr());
    }

    private iDeviceEventCallBack createEventCallback()
    {
        return (ref iDeviceEvent devEvent, IntPtr data) =>
        {
            uuid = devEvent.udidString;
            switch (devEvent.@event)
            {
                case iDeviceEventType.DeviceAdd:
                    Connect(uuid);
                    // do whatever you want when connected.
                    receiveDataFromDevice();
                    sendToDevice(new byte[] { 0x10, 0x20, 0x33, 0x11 });
                    break;
                case iDeviceEventType.DeviceRemove:
                    break;
                default:
                    return;
            }
        };
    }

    private void Connect(string newUdid)
    {
        iDeviceApi.idevice_new(out iDeviceHandle deviceHandle, newUdid).ThrowOnError();
        var error = iDeviceApi.idevice_connect(deviceHandle, 5050, out iDeviceConnectionHandle connection);
        if (error != iDeviceError.Success) return;
        currentConnectionHandle = connection;
    }

    private void receiveDataFromDevice()
    {
        Task.Run(() =>
        {
            while (true)
            {
                uint receivedBytes = 0;
                byte[] buffer = new byte[1024];
                iDeviceApi.idevice_connection_receive(currentConnectionHandle, buffer, (uint)buffer.Length,
                    ref receivedBytes);
                if (receivedBytes <= 0) continue;
                // Do something with your received bytes
                Console.WriteLine("Received data from device...");
                Console.WriteLine(buffer.ToString());
            }
        });
    }

    public void sendToDevice(byte[] dataToSend)
    {
        uint sentBytes = 0;
        iDeviceApi.idevice_connection_send(currentConnectionHandle, dataToSend, (uint)dataToSend.Length, ref sentBytes);
    }
}
