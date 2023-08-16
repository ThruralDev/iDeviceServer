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
}
