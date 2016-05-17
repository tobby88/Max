using System.Collections;

namespace Tobby.Max
{
    class WallThermostatPlus
    {
        // class properties:
        private string rfAddress;
        public string RfAddress { get { return rfAddress; } }
        private string serial;
        public string Serial { get { return serial; } }
        private string name;
        public string Name { get { return name; } }
        private byte unknown;
        public byte Unknown { get { return unknown; } }
        private byte valve;
        public byte Valve { get { return valve; } }
        private double setTemp;
        public double SetTemp { get { return setTemp; } }
        private double actualTemp;
        public double ActualTemp { get { return actualTemp; } }
        private string dateUntil;
        public string DateUntil { get { return dateUntil; } }
        private string timeUntil;
        public string TimeUntil { get { return timeUntil; } }

        // constructor:
        public WallThermostatPlus(string rfAddress, string serial, string name)
        {
            this.rfAddress = rfAddress;
            this.serial = serial;
            this.name = name;
        }

        public void Update(byte unknown, BitArray flags, byte valve, double setTemp, string dateUntil, string timeUntil, double actualTemp)
        {
            this.unknown = unknown;
            // todo: flags!
            this.valve = valve;
            this.setTemp = setTemp;
            this.dateUntil = dateUntil;
            this.timeUntil = timeUntil;
            this.actualTemp = actualTemp;
        }
    }
}
