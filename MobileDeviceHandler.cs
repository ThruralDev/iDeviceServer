
interface MobileDeviceHandler
{
    void listDevices();
    void beginListening();
    void sendToDevice(byte[] dataToSend);
}
