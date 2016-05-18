using RaspberryGPIOManager;
using System;
using System.Collections.Generic;

namespace Tobby.Max
{
    class House
    {
        // class properties:
        private Cube cube;
        public Cube Cube { get { return cube; } }
        private List<Room> rooms;
        public List<Room> Rooms { get { return rooms; } }
        private GPIOPinDriver gpio;
        private bool gpioState;

        // constructor:
        public House()
        {
            Console.WriteLine("No GPIO defined. Maybe the software was compiled with raspi=false?");
            rooms = new List<Room>();
        }

        public House(GPIOPinDriver.Pin pin)
        {
            gpio = new GPIOPinDriver(pin, GPIOPinDriver.GPIODirection.Out, GPIOPinDriver.GPIOState.Low);
            gpioState = false;
            rooms = new List<Room>();
        }

        // class methods:

        // adds a cube to the house (only one cube supported at the moment)
        public void AddCube()
        {
            cube = Networking.InitCube();
        }

        // connect to cube and receive status
        public void GetStructure()
        {
            List<byte[]> messages = Networking.GetCubeData(cube);
            Messaging decoder = new Messaging(messages);
            decoder.DecodeH(ref cube);
            decoder.DecodeM(ref cube, ref rooms);
        }

        public void Output()
        {
            // calculate how many lines are neccessary for output
            int maximumTemps = 0, maximumValves = 0;
            foreach (Room room in rooms)
            {
                if (room.WallThermostatsPlus.Count > 0 && room.WallThermostatsPlus.Count > maximumTemps)
                    maximumTemps = room.WallThermostatsPlus.Count;
                if (room.WallThermostatsPlus.Count == 0 && room.HeaterThermostats.Count > 0 && room.HeaterThermostats.Count > maximumTemps)
                    maximumTemps = room.HeaterThermostats.Count;
                if (room.HeaterThermostats.Count > maximumValves)
                    maximumValves = room.HeaterThermostats.Count;
            }

            // print date and first two letters of room names
            Console.Write("{0} {1} ", Cube.CubeDate.PadRight(10), Cube.CubeTime.PadRight(5));
            foreach (Room room in Rooms)
                Console.Write("  {0}  ", room.Name.Substring(0, 2));
            Console.Write(Environment.NewLine);

            // TODO:
            // print desired temperatures
            for (int i = 0; i < maximumTemps; i++)
            {
                Console.Write("desired temp    ");
                foreach (Room room in Rooms)
                    if (room.WallThermostatsPlus.Count > 0 && room.WallThermostatsPlus.Count >= i)
                        Console.Write(" " + room.WallThermostatsPlus[i].SetTemp.ToString("F1").PadLeft(4) + " ");
                    else if (room.WallThermostatsPlus.Count == 0 && room.HeaterThermostats.Count >= i)
                        Console.Write(" " + room.HeaterThermostats[i].SetTemp.ToString("F1").PadLeft(4) + " ");
                    else
                        Console.Write("      ");
                Console.Write(Environment.NewLine);
            }

            // print actual temperatures
            for (int i = 0; i < maximumTemps; i++)
            {
                Console.Write("actual temp     ");
                foreach (Room room in Rooms)
                    if (room.WallThermostatsPlus.Count > 0 && room.WallThermostatsPlus.Count >= i)
                        Console.Write(" " + room.WallThermostatsPlus[i].ActualTemp.ToString("F1").PadLeft(4) + " ");
                    else if (room.WallThermostatsPlus.Count == 0 && room.HeaterThermostats.Count >= i)
                        Console.Write(" " + room.HeaterThermostats[i].ActualTemp.ToString("F1").PadLeft(4) + " ");
                    else
                        Console.Write("      ");
                Console.Write(Environment.NewLine);
            }

            // print valve positions
            for (int i = 0; i < maximumValves; i++)
            {
                Console.Write("valve position  ");
                foreach (Room room in Rooms)
                    if (room.HeaterThermostats.Count > 0 && room.HeaterThermostats.Count >= i)
                        Console.Write(" " + room.HeaterThermostats[i].Valve.ToString().PadLeft(3) + "% ");
                    else
                        Console.Write("      ");
                Console.Write(Environment.NewLine);
            }
        }

        // renew status of all sensors
        public void RenewSensorData()
        {
            List<byte[]> messages = Networking.GetCubeData(cube);
            Messaging decoder = new Messaging(messages);
            decoder.DecodeL(ref cube, ref rooms);
        }

        // switch on central heater if necessary - and switch of if not necessary
        public void SetHeatingControlSystem()
        {
            bool switchOn = false;
            foreach (Room room in rooms)
            {
                foreach (HeaterThermostat heater in room.HeaterThermostats)
                    if (heater.Valve >= 10)
                    {
                        switchOn = true;
                        break;
                    }
                if (switchOn)
                    break;
            }
            if (gpio != null)
                if (switchOn)
                {
                    gpio.State = GPIOPinDriver.GPIOState.High;
                    if (gpioState == false)
                        Console.WriteLine("Switched ON central heater");
                    gpioState = true;
                }
                else
                {
                    gpio.State = GPIOPinDriver.GPIOState.Low;
                    if (gpioState == true)
                        Console.WriteLine("Switched OFF central heater");
                    gpioState = false;
                }
            else
                Console.WriteLine("Could not access the GPIO. Maybe the GPIO is in use. Or the software has not been compiled for the Pi (raspi=false).");
        }
    }
}
