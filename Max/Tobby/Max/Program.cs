using RaspberryGPIOManager;
using System;
using System.Threading;

namespace Tobby.Max
{
    class Program
    {
        static void Main(string[] args)
        {


            // set to true if compiling for the RasPi heating control system
            bool raspi = true;


            // create new house, add "heating control system" if running on raspi
            House house;
            if (raspi == true)
                house = new House(GPIOPinDriver.Pin.GPIO22);
            else
                house = new House();
            // search for cube on the network (only one cube supported at the moment)
            house.AddCube();
            // add all data about cube and rooms (and what's inside that rooms) to the house
            house.GetStructure();
            // then loop and renew data every 60 seconds, switch on central heater if necessary
            while (true)
            {
                try
                {
                    house.RenewSensorData();
                    house.Output();
                    house.SetHeatingControlSystem();
                }
                catch
                {
                    Console.WriteLine("Something bad happened! Trying again in 60 seconds");
                }
                Thread.Sleep(60000);
                Console.Clear();
            }
        }
    }
}
