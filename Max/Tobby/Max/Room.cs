using System;
using System.Collections;
using System.Collections.Generic;

namespace Tobby.Max
{
    class Room
    {
        // class properties
        private int id;
        public int ID { get { return id; } }
        private string name;
        public string Name { get { return name; } }
        private string groupRfAddress;
        public string GroupRfAddress { get { return groupRfAddress; } }
        private List<HeaterThermostat> heaterThermostats;
        public List<HeaterThermostat> HeaterThermostats { get { return heaterThermostats; } }
        private List<WallThermostatPlus> wallThermostatsPlus;
        public List<WallThermostatPlus> WallThermostatsPlus { get { return wallThermostatsPlus; } }
        private List<WindowContact> windowContacts;
        public List<WindowContact> WindowContacts { get { return windowContacts; } }

        // constructor
        public Room(int id, string name, string groupRfAddress)
        {
            heaterThermostats = new List<HeaterThermostat>();
            wallThermostatsPlus = new List<WallThermostatPlus>();
            windowContacts = new List<WindowContact>();
            this.id = id;
            this.name = name;
            this.groupRfAddress = groupRfAddress;
            Console.WriteLine("Room created with ID: {0}, Name: \"{1}\" and Group RF Address: {2}", this.id, this.name, this.groupRfAddress);
        }

        // class methods

        // update HeaterThermostat data
        public void UpdateHeater(string rfAddress, byte unknown, BitArray flags, byte valve, double setTemp, string dateUntil, string timeUntil)
        {
            foreach (HeaterThermostat heater in heaterThermostats)
            {
                if (heater.RfAddress == rfAddress)
                {
                    heater.Update(unknown, flags, valve, setTemp, dateUntil, timeUntil);
                }
            }
        }
        public void UpdateHeater(string rfAddress, byte unknown, BitArray flags, byte valve, double setTemp, double actualTemp)
        {
            foreach (HeaterThermostat heater in heaterThermostats)
            {
                if (heater.RfAddress == rfAddress)
                {
                    heater.Update(unknown, flags, valve, setTemp, actualTemp);
                }
            }
        }
        public void UpdateThermostat(string rfAddress, byte unknown, BitArray flags, byte valve, double setTemp, string dateUntil, string timeUntil, double actualTemp)
        {
            foreach (WallThermostatPlus wallT in wallThermostatsPlus)
            {
                if (wallT.RfAddress == rfAddress)
                {
                    wallT.Update(unknown, flags, valve, setTemp, dateUntil, timeUntil, actualTemp);
                }
            }
        }
    }
}
