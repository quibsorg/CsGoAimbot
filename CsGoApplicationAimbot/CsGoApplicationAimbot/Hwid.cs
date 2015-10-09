using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Management;

namespace CsGoApplicationAimbot
{
    public class Hwid
    {
        public static string GetHwid()
        {
            if (string.IsNullOrEmpty(_fingerPrint))
            {
                _fingerPrint = GetHash("CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId() + "\nDISK >> " + DiskId() + "\nVIDEO >> " + VideoId() + "\nMAC >> " + MacId());
            }
            return _fingerPrint;
        }
        private static string _fingerPrint = string.Empty;
        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            byte[] bt = Encoding.ASCII.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(IList<byte> bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Count; i++)
            {
                byte b = bt[i];
                int n = b;
                int n1 = n & 15;
                int n2 = (n >> 4) % 15;
                if (n2 > 9)
                {
                    s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                }
                if (n1 > 9)
                {
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    s += n1.ToString(CultureInfo.InvariantCulture);
                }
                if ((i + 1) != bt.Count && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }
        private static string Identifier(string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string[] result = { "" };
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementBaseObject mo in from ManagementBaseObject mo in moc where mo[wmiMustBeTrue].ToString() == "True" where result[0] == "" select mo)
            {
                try
                {
                    result[0] = mo[wmiProperty].ToString();
                    break;
                }
                catch
                {
                    //Ignored
                }
            }
            return result[0];
        }
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            ManagementClass mc = new ManagementClass(wmiClass);
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementBaseObject mo in moc)
            {
                //Only get the first one
                if (result != "") continue;
                try
                {
                    result = mo[wmiProperty].ToString();
                    break;
                }
                catch
                {
                    //ignored
                }
            }
            return result;
        }

        private static string CpuId()
        {
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "ProcessorId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "Name");
            if (retVal == "") //If no Name, use Manufacturer
            {
                retVal = Identifier("Win32_Processor", "Manufacturer");
            }
            //Add clock speed for extra security
            retVal += Identifier("Win32_Processor", "MaxClockSpeed");
            return retVal;
        }
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") + Identifier("Win32_BIOS", "IdentificationCode") + Identifier("Win32_BIOS", "SerialNumber") + Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
        }
        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model") + Identifier("Win32_DiskDrive", "Manufacturer") + Identifier("Win32_DiskDrive", "Signature") + Identifier("Win32_DiskDrive", "TotalHeads");
        }
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model") + Identifier("Win32_BaseBoard", "Manufacturer") + Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
        }
        private static string VideoId()
        {
            return Identifier("Win32_VideoController", "DriverVersion") + Identifier("Win32_VideoController", "Name");
        }
        private static string MacId()
        {
            return Identifier("Win32_NetworkAdapterConfiguration", "MACAddress", "IPEnabled");
        }
        public static string PcName()
        {
            return Environment.MachineName;
            //Computer computer = new Computer();
            //return computer.Name;
        }
        public static string IpAdress()
        {
            WebClient client = new WebClient();
            return client.DownloadString("http://icanhazip.com/");
        }
    }

}
