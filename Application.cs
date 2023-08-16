using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Application
{
    static public void Main(String[] args)
    {
        MobileDeviceHandler serverCode = new IMobileDeviceHandler();

        Console.WriteLine("Listing devices...");
        serverCode.listDevices();

        // Keep program 'alive'
        var line = Console.ReadLine().ToString();
        Console.WriteLine(line);
    }
}
