using System;
using System.Net;

namespace Tobby.Max
{
    class Cube
    {
        // class properties:
        private bool initialized;
        public bool Initialized { get { return initialized; } }
        private IPAddress ip;
        public IPAddress IP { get { return ip; }  }
        private string name;
        public string Name { get { return name; } }
        private string serial;
        public string Serial { get { return serial; } }
        private string unknown1;
        public string Unknown1 { get { return unknown1; } }
        private string unknown2;
        public string Unknown2 { get { return unknown2; } }

        private string rfAddress;
        public string RfAddress { get { return rfAddress; } }
        private string firmwareVersion;
        public string FirmwareVersion { get { return firmwareVersion; } }
        private string httpConnectionID;
        public string HttpConnectionID { get { return httpConnectionID; } }
        private string dutyCycle;
        public string DutyCycle { get { return dutyCycle; } }
        private string freeMemorySlots;
        public string FreeMemorySlots { get { return freeMemorySlots; } }
        private string cubeDate;
        public string CubeDate { get { return cubeDate; } }
        private string cubeTime;
        public string CubeTime { get { return cubeTime; } }
        private string stateCubeTime;
        public string StateCubeTimer { get { return stateCubeTime; } }
        private string ntpCounter;
        public string NtpCounter { get { return ntpCounter; } }
        private bool first;

        // constructor:
        public Cube() { }
        public Cube(IPAddress ip, string name, string serial, string unknown1, string rfAddress, string firmwareVersion)
        {
            this.ip = ip;
            this.name = name;
            this.serial = serial;
            this.unknown1 = unknown1;
            this.rfAddress = rfAddress;
            this.firmwareVersion = firmwareVersion;
            initialized = true;
            first = false;
        }

        // member functions:

        // renew timestamp of cubeData
        public void RenewTimestamp(string cubeDate, string cubeTime)
        {
            this.cubeDate = cubeDate;
            this.cubeTime = cubeTime;
        }

        // renew data of cube but cubeTime and cubeDate
        public void RenewData(string serial, string rfAddress, string firmwareVersion, string unknown2, string httpConnectionID, string dutyCycle, string freeMemorySlots, string stateCubeTime, string ntpCounter)
        {
            if (serial != this.serial || rfAddress != this.rfAddress)
                Console.WriteLine("Data does not fit to the cube, wrong cube chosen?");
            else
            {
                this.firmwareVersion = firmwareVersion;
                this.unknown2 = unknown2;
                this.httpConnectionID = httpConnectionID;
                this.dutyCycle = dutyCycle;
                this.freeMemorySlots = freeMemorySlots;
                this.stateCubeTime = stateCubeTime;
                this.ntpCounter = ntpCounter;
                if (first == false)
                {
                    Console.WriteLine("Data of cube with Serial {0} and RF-Adress {1} renewed: Firmware: {2}, HTTP-Connection ID: {3}, DutyCycle: {4}, Free Slots: {5}, Date: {6}, Time: {7}, StateCubeTimer: {8}, NTP Counter: {9}", this.serial, this.rfAddress, this.firmwareVersion, this.httpConnectionID, this.dutyCycle, this.freeMemorySlots, this.cubeDate, this.CubeTime, this.stateCubeTime, this.ntpCounter);
                    first = true;
                }
            }
        }
    }
}
 
 