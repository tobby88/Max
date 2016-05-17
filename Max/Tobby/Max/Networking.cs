using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Tobby.Max
{
    static class Networking
    {
        // class properties:
        private const int cubeUdpPort = 23272;
        private const int cubeTcpPort = 62910;
        private const string broadcastString = "eQ3Max*\0**********I";
        private const byte maxTries = 20;
        private const short msToWait = 500;
        private static Cube tempCube;

        // static class - no constructor

        // class methods:

        // sends UDP-broadcast over all network interfaces
        public static void SendBroadcast(byte[] broadcast, int port)
        {
            // get all installed network interfaces
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            // loop through all installed network interfaces
            foreach (NetworkInterface adapter in nics)
            {
                // if adapter is off or not connected -> start over with the next nic
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;
                // if adapter does not support IPv4 -> start over with next nix
                if (!adapter.Supports(NetworkInterfaceComponent.IPv4))
                    continue;

                // loop through all IPv4 and IPv6 addresses of this nic
                foreach (UnicastIPAddressInformation ipAddr in adapter.GetIPProperties().UnicastAddresses)
                {
                    // only use IPv4-addresses of the nic, except for loopback device
                    if (ipAddr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ipAddr.Address.Equals(IPAddress.Loopback))
                    {
                        // send out broadcast:
                        //Console.WriteLine("Sending Broadcast from {0}:{1} (via {2})", ipAddr.Address.ToString(), port, adapter.Name);
                        // define broadcast target
                        IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
                        // define local endpoint so the correct network interface will be used
                        IPEndPoint localEndpoint = new IPEndPoint(ipAddr.Address, port);
                        // open socket on local network interface
                        UdpClient broadcaster = new UdpClient(localEndpoint);
                        broadcaster.EnableBroadcast = true;
                        // send the actual broadcast and close connection
                        broadcaster.Send(broadcast, broadcast.Length, broadcastEndpoint);
                        broadcaster.Close();
                    }
                }
            }
        }

        // receiver to listen for all packages (broadcast & unicast) on all network interfaces but only on one port
        // port has to be defined as "Object" instead of int because multithreading with parameters only allows "Objects"
        private static void udpCubeReceiver(Object port)
        {
            // create local endpoint - listen on all network interfaces for packages from all IPs but only on the given port
            // cast port from Object to int
            IPEndPoint receiverEndpoint = new IPEndPoint(IPAddress.Any, (int)port);
            // open the actual listener
            UdpClient receiver = new UdpClient(receiverEndpoint);
            // loop until correct cube-data is found
            while (!tempCube.Initialized)// && timerTries < maxTries)
            {
                // block thread and wait for package to receive
                byte[] rcvPacket = receiver.Receive(ref receiverEndpoint);
                // convert received package to string and compare to broadcast, so broadcast is not tried to be analyzed like an actual response
                string msg = Encoding.ASCII.GetString(rcvPacket);
                // it was not the broadcast? -> analyze the response
                if (msg != broadcastString)
                {
                    Console.WriteLine("Received respone from {0}", receiverEndpoint);
                    // response should be 26 bytes of data - if not: something's wrong
                    if (rcvPacket.Length != 26)
                        Console.WriteLine("Bad package");
                    // package seems to be ok -> save the data of the cube
                    else
                    {
                        IPAddress ip;
                        string name, serial, unknown, rfAddress, firmwareVersion;
                        ip = receiverEndpoint.Address;
                        name = msg.Substring(0, 8);
                        serial = msg.Substring(8, 10);
                        unknown = msg.Substring(18, 3);
                        rfAddress = BitConverter.ToString(rcvPacket, 21, 3);
                        rfAddress = rfAddress.Replace("-", "");
                        rfAddress = rfAddress.ToLower();
                        firmwareVersion = rcvPacket[24].ToString() + "." + ((rcvPacket[25] & 0xF0) / 16).ToString() + "." + (rcvPacket[25] & 0x0F).ToString();
                        Cube foundCube = new Cube(ip, name, serial, unknown, rfAddress, firmwareVersion);
                        tempCube = foundCube;
                    }
                    
                }
                /*else
                    Console.WriteLine("Received MAX!-Broadcast from {0}", receiverEndpoint);*/
            }
        }

        // finds a cube on all network interfaces
        public static Cube InitCube()
        {
            tempCube = new Cube();
            byte[] broadcastMsg = Encoding.ASCII.GetBytes(broadcastString);
            byte timerTries = 0;
            // Thread-delegate for a second thread (with a parameter)
            ParameterizedThreadStart receiverDel = new ParameterizedThreadStart(udpCubeReceiver);
            // create second thread
            Thread receiverThread = new Thread(receiverDel);
            //start the second thread
            receiverThread.Start(cubeUdpPort);
            Console.WriteLine("Trying to find cube");
            timerTries = 0;
            // send broadcast every x ms and wait y tries for answers, break if cube responded correctly
            while (!tempCube.Initialized && timerTries < maxTries)
            {
                Networking.SendBroadcast(broadcastMsg, cubeUdpPort);
                Thread.Sleep(msToWait);
                timerTries++;
            }
            // try to stop second thread
            receiverThread.Abort();

            // if the timer ended (maxTries*msToWait, maxTries broadcasts) withoud finding a cube print message
            if (timerTries >= maxTries)
                Console.WriteLine("No cube found. Please check connection and firewalls.");
            // if cube was found print out data of the cube
            else
            {
                Console.WriteLine("Cube found & cube data initialized");
                Console.WriteLine("Name: {0}, Serial: {1}, RF-Address: {2}, Firmware Version: {3}", tempCube.Name, tempCube.Serial, tempCube.RfAddress, tempCube.FirmwareVersion);
            }
            // could the second thread be stopped?
            if (receiverThread.IsAlive)
                Console.WriteLine("Could not terminate receiver-thread :-(");
            return tempCube;
        }

        // connect to cube and receive status
        public static List<byte[]> GetCubeData(Cube cube)
        {
            // break if the cube is not initializied (no cube found or didn't run InitCube() before running this)
            if (cube == null || !cube.Initialized)
            {
                Console.WriteLine("Cube not initialized!");
                return null;
            }
            else
            {
                // create TCP connection
                TcpClient client = new TcpClient();
                client.Connect(cube.IP, cubeTcpPort);

                /* calculate maximum buffer with the size of the biggest message:
                   H: 70 Bytes
                   C: max. 216 Bytes (but how long can a cube c message get?)
                   M: 14 fixed, 5 fixed per room (max. 10 rooms) = 50 max., 44 max. per room name = 440 max.,
                      16 fixed per device (max. 50 devices) = 800 max., 44 max. per device name = 2200 max. -> 3504 Bytes max.
                   L: 4 fixed + max. 12 (?) per submessage/device = 604 Bytes max.
                   S: ? but not relevant at the moment
                   = 4394 Bytes altogether */
                const int buffer = 8192;
                // standard buffer size is 8192 Bytes so already bigger than the biggest message
                //client.ReceiveBufferSize = buffer;

                // get the data stream of the TCP connection
                NetworkStream stream = client.GetStream();
                //Console.WriteLine("Successfully connected to the cube - fetching data now");
                // create buffer for every received package/message
                byte[] readBuffer = new byte[buffer];
                // store the size of the received message
                int bytesReceived = 0;
                int lastReceived = 0;
                // create a list for all received messages
                List<byte[]> messages = new List<byte[]>();
                // loop condition, will be resetted by a timer, a closed connection or a finished data transfer
                bool readMore = true;
                do
                {
                    // set timeout condition to abort a broken data connection
                    stream.ReadTimeout = 5000;
                    // try-catch-block to react to read timeout
                    try
                    {
                        // read received package to read buffer and store the number of bytes received
                        bytesReceived = stream.Read(readBuffer, lastReceived, readBuffer.Length - lastReceived);
                        // 0 bytes received means the connection was closed so stop trying to receive
                        if (bytesReceived == 0)
                        {
                            readMore = false;
                            Console.WriteLine("Connection was closed by the cube");
                        }
                        bytesReceived += lastReceived;
                        // chech if message is complete (ends with \r\n) or if it has been splitted because of the MTU
                        if (readBuffer[bytesReceived - 2] == (byte)'\r' && readBuffer[bytesReceived - 1] == (byte)'\n')
                        {
                            // add message to the list of all received messages, copy it so not only the reference is stored but the actual data, copy only the received bytes and not the following zeroes
                            byte[] temp = new byte[bytesReceived];
                            Array.Copy(readBuffer, temp, bytesReceived);
                            messages.Add(temp);
                            lastReceived = 0;
                        }
                        else
                            lastReceived = bytesReceived;
                        // message is an L-message which is usually the last message so stop receiving
                        if (readBuffer[0] == (byte)'L')
                        {
                            readMore = false;
                            //Console.WriteLine("Finished. Closing connection");
                        }
                    }
                    // catch the exception when the readtimeout is reached and stop receiving
                    catch (IOException)
                    {
                        readMore = false;
                        Console.WriteLine("Connection interrupted, maybe not all data could be received?");
                    }
                } while (readMore);
                // close connection
                client.Close();
                return messages;
            }
        }
    }
}
