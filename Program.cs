using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using NativeWifi;

namespace LockOut
{
    class Program
    {
        private static WlanClient wlan;
        private static string[] safeSSIDs;
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Arguments: a list of safe SSIDs: e.g. home work friendsWlan");
                return;
            }
            safeSSIDs = args;
            wlan = new WlanClient();
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(PowerModeChanged);
            Console.WriteLine("The program will check the SSID of the connected WLAN after resume from standby and lock the computer if it is not in the list of safe SSIDs.");
            Console.WriteLine("Press any key to exit.\n");
            Console.ReadKey();
        }

        [DllImport("user32.dll")]
        public static extern void LockWorkStation();

        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        static void PowerModeChanged(Object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                Console.WriteLine("PowerMode: suspend");
            }
            if (e.Mode == PowerModes.Resume)
            {
                Console.WriteLine("PowerMode: resume");
                var connectedSsids = new List<string>();
                foreach (WlanClient.WlanInterface wlanInterface in wlan.Interfaces)
                {
                    if (wlanInterface.InterfaceState != Wlan.WlanInterfaceState.Connected)
                    {
                        Console.WriteLine(wlanInterface.InterfaceName + " is not connected!");
                    }
                    else
                    {
                        Wlan.Dot11Ssid ssid = wlanInterface.CurrentConnection.wlanAssociationAttributes.dot11Ssid;
                        connectedSsids.Add(new String(Encoding.ASCII.GetChars(ssid.SSID, 0, (int)ssid.SSIDLength)));
                    }
                }
                Console.WriteLine("Connected SSIDs:");
                foreach (var ssid in connectedSsids)
                    Console.WriteLine("\t" + ssid);
                if (connectedSsids.Intersect(safeSSIDs).Count() > 0)
                {
                    Console.WriteLine("-> save");
                }
                else
                {
                    Console.WriteLine("-> unsafe. Locking workstation now!");
                    LockWorkStation();
                }
            }
        }

    }
}
