using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Tobby.Max
{
    class Messaging
    {
        // class properties:
        private List<byte[]> messages;
        private List<byte[]> splittedMessages;
        private string cubeDate;
        public string CubeDate { get { return cubeDate; } }
        private string cubeTime;
        public string CubeTime { get { return cubeTime; } }

        // constructor:
        public Messaging(List<byte[]> messages)
        {
            this.messages = messages;
            splittedMessages = new List<byte[]>();
            // first split all messages so one list entry is only one message
            splitAllMessages();
        }

        // class methods:

        // decode all messages
        // not necessary at the moment?
        /*public void DecodeAll(ref Cube cube, ref Room[] rooms)
        {
            // first update timestamp
            foreach (byte[] message in splittedMessages)
            {
                if ((char)message[0] == 'H')
                {
                    updateTimestamp(message);
                    break;
                }
            }
            // then go through every message and decode depending on the message type
            foreach (byte[] message in splittedMessages)
            {
                switch((char)message[0])
                {
                    case 'H':
                        decodeHMessage(message, ref cube);
                        break;
                    case 'M':
                        decodeMMessage(message, ref rooms);
                        break;
                    case 'C':
                        //add decoding of C-messages here
                        break;
                    case 'L':
                        decodeLMessage(message);
                        break;
                }
            }
        }*/

        // decode only H message
        public void DecodeH(ref Cube cube)
        {
            updateTimestamp(ref cube);
            foreach (byte[] message in splittedMessages)
            {
                if ((char)message[0] == 'H')
                {
                    decodeHMessage(message, ref cube);
                    break;
                }
            }
        }

        // decode only M message
        public void DecodeM(ref Cube cube, ref List<Room> rooms)
        {
            updateTimestamp(ref cube);
            foreach (byte[] message in splittedMessages)
            {
                if ((char)message[0] == 'M')
                {
                    decodeMMessage(message, ref rooms);
                    break;
                }
            }
        }

        // decode only L message
        public void DecodeL(ref Cube cube, ref List<Room> rooms)
        {
            updateTimestamp(ref cube);
            foreach (byte[] message in splittedMessages)
            {
                if ((char)message[0] == 'L')
                {
                    decodeLMessage(message, ref rooms);
                    break;
                }
            }
        }

        // split all messages
        private void splitAllMessages()
        {
            // loop through all messages and give them to the actual split function
            foreach (byte[] message in messages)
            {
                // gets back a list of all messages in the originial message, now loop through them and add them to the splittedMessages-list
                List<byte[]> temp = new List<byte[]>();
                temp = splitMessage(message);
                foreach (byte[] splitMessage in temp)
                {
                    splittedMessages.Add(splitMessage);
                }
            }
        }

        // split message - if more than one message is currently stored in one message entry
        private List<byte[]> splitMessage(byte[] message)
        {
            List<byte[]> splittedMessage = new List<byte[]>();
            int start = 0;
            int end = message.GetLength(0);
            // loop through all letters of an (unsplitted) message and search for the end (\r\n) of an old message and a beginning (X:) of a new message
            for (int i = 3; i < message.GetLength(0); i++)
            {
                if (message[i - 3] == (byte)'\r' && message[i - 2] == (byte)'\n' && message[i] == (byte)':')
                {
                    end = i - 2;
                    byte[] temp = new byte[end - start];
                    Array.Copy(message, start, temp, 0, end - start);
                    splittedMessage.Add(temp);
                    start = end + 1;
                    end = message.GetLength(0);
                }
            }
            // for the last message in a given message-block the check above will fail because there is no new message starting, so just copy the rest of the message as a new message
            byte[] temp2 = new byte[end - start];
            Array.Copy(message, start, temp2, 0, end - start);
            splittedMessage.Add(temp2);
            return splittedMessage;
        }

        // update timestamps
        private void updateTimestamp(ref Cube cube)
        {
            // first update timestamp
            foreach (byte[] message in splittedMessages)
            {
                if ((char)message[0] == 'H')
                {
                    string msg = Encoding.ASCII.GetString(message);
                    int cubeYear, cubeMonth, cubeDay;
                    cubeYear = byte.Parse(msg.Substring(49, 2), System.Globalization.NumberStyles.HexNumber) + 2000;
                    cubeMonth = byte.Parse(msg.Substring(51, 2), System.Globalization.NumberStyles.HexNumber);
                    cubeDay = byte.Parse(msg.Substring(53, 2), System.Globalization.NumberStyles.HexNumber);
                    cubeDate = cubeDay.ToString("00") + "." + cubeMonth.ToString("00") + "." + cubeYear.ToString();
                    int cubeHours, cubeMinutes;
                    cubeHours = byte.Parse(msg.Substring(56, 2), System.Globalization.NumberStyles.HexNumber);
                    cubeMinutes = byte.Parse(msg.Substring(58, 2), System.Globalization.NumberStyles.HexNumber);
                    cubeTime = cubeHours.ToString("00") + ":" + cubeMinutes.ToString("00");
                    cube.RenewTimestamp(cubeDate, cubeTime);
                    break;
                }
            }
            
        }

        // decode H message
        private void decodeHMessage(byte[] message, ref Cube cube)
        {
            string serial, rfAddress, firmwareVersion, unknown, httpConnectionID, dutyCycle, freeMemorySlots, stateCubeTime, ntpCounter;
            string msg = Encoding.ASCII.GetString(message);
            serial = msg.Substring(2, 10);
            rfAddress = msg.Substring(13, 6);
            firmwareVersion = (char)message[21] + "." + (char)message[22] + "." + (char)message[23];
            unknown = msg.Substring(25, 8);
            httpConnectionID = msg.Substring(34, 8);
            dutyCycle = msg.Substring(43, 2);
            freeMemorySlots = msg.Substring(46, 2);
            // cube date and time are already encoded by updateTimestamp
            stateCubeTime = msg.Substring(61, 2);
            ntpCounter = msg.Substring(64, 4);
            cube.RenewData(serial, rfAddress, firmwareVersion, unknown, httpConnectionID, dutyCycle, freeMemorySlots, stateCubeTime, ntpCounter);
        }

        // decode M message
        private void decodeMMessage(byte[] message, ref List<Room> rooms)
        {
            int count, index;
            byte[] data;
            string msg = Encoding.ASCII.GetString(message);
            // don't know what index and count do...
            index = byte.Parse(msg.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            count = byte.Parse(msg.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            // convert the actual room data from Base64
            data = Convert.FromBase64String(msg.Substring(8, message.GetLength(0) - 10));
            // don't know what it's for
            byte[] tempUnknown = new byte[2];
            Array.Copy(data, tempUnknown, 2);
            string unknown = Encoding.ASCII.GetString(tempUnknown);
            // get the number of rooms
            byte roomCount = data[2];
            int offset = 3;
            // decoding the room
            for (byte i = 0; i < roomCount; i++)
            {
                byte id = data[offset];
                byte length = data[offset + 1];
                byte[] tempName = new byte[length];
                Array.Copy(data, offset + 2, tempName, 0, length);
                string name = Encoding.UTF8.GetString(tempName);
                byte[] tempAddress = new byte[3];
                string groupRfAddress = BitConverter.ToString(data, offset + length + 2, 3);
                groupRfAddress = groupRfAddress.Replace("-", "");
                groupRfAddress = groupRfAddress.ToLower();
                rooms.Add(new Room(id, name, groupRfAddress));
                offset = offset + length + 5;
            }
            // decoding the devices
            byte deviceCount = data[offset];
            offset++;
            byte heaters = 0, wallTs = 0, windows = 0;
            for (byte i = 0; i < deviceCount; i++)
            {
                byte type = data[offset];
                byte[] tempAddress = new byte[3];
                string rfAddress = BitConverter.ToString(data, offset+1, 3);
                rfAddress = rfAddress.Replace("-", "");
                rfAddress = rfAddress.ToLower();
                string serial = Encoding.ASCII.GetString(data, offset + 4, 10);
                byte length = data[offset + 14];
                byte[] tempName = new byte[length];
                Array.Copy(data, offset + 15, tempName, 0, length);
                string name = Encoding.UTF8.GetString(tempName);
                byte roomID = data[offset + length + 15];
                offset = offset + length + 16;
                switch (type)
                {
                    case 0:
                        Console.WriteLine("Cube in the M-Message???");
                        break;
                    case 1:
                        HeaterThermostat tempHeater = new HeaterThermostat(rfAddress, serial, name);
                        foreach (Room room in rooms)
                            if (room.ID == roomID)
                            {
                                room.HeaterThermostats.Add(tempHeater);
                                break;
                            }
                            //Console.WriteLine("Added HeaterThermostat with\nRF-Addres: {0}, Serial: {1}, Name: \"{2}\"", rfAddress, serial, name);
                            heaters++;
                        break;
                    case 3:
                        WallThermostatPlus tempWallT = new WallThermostatPlus(rfAddress, serial, name);
                        foreach (Room room in rooms)
                            if (room.ID == roomID)
                            {
                                room.WallThermostatsPlus.Add(tempWallT);
                                break;
                            }
                        //Console.WriteLine("Added WallThermostat+ with\nRF-Addres: {0}, Serial: {1}, Name: \"{2}\"", rfAddress, serial, name);
                        wallTs++;
                        break;
                    case 4:
                        WindowContact tempWindowC = new WindowContact(rfAddress, serial, name);
                        foreach (Room room in rooms)
                            if (room.ID == roomID)
                            {
                                room.WindowContacts.Add(tempWindowC);
                                break;
                            }
                        //Console.WriteLine("Added WindowContact with\nRF-Addres: {0}, Serial: {1}, Name: \"{2}\"", rfAddress, serial, name);
                        windows++;
                        break;
                }
            }
            if (windows + wallTs + heaters < deviceCount)
                Console.WriteLine("Not all devices could be added - unsupported devices or errors when decoding?");
            Console.WriteLine("Added {0} HeaterThermostats, {1} WallThermostats+ and {2} WindowContacts", heaters, wallTs, windows);
        }

        // decode C messages

        // decode L message
        private void decodeLMessage(byte[] message, ref List<Room> rooms)
        {
            // convert the actual data from Base64
            byte[] data = Convert.FromBase64String(Encoding.ASCII.GetString(message).Substring(2, message.GetLength(0) - 2));
            int offset = 0;
            while (offset < data.GetLength(0) && !(data[offset] == 0xce && data[offset + 1] == 0))
            {
                byte length = data[offset];
                string rfAddress = BitConverter.ToString(data, offset + 1, 3);
                rfAddress = rfAddress.Replace("-", "");
                rfAddress = rfAddress.ToLower();
                byte unknown = data[offset + 4];
                byte[] tmpFlags = new byte[2];
                Array.Copy(data, offset + 5, tmpFlags, 0, 2);
                BitArray flags = new BitArray(tmpFlags);
                //Console.WriteLine("Length {0} rfAddress {1} Unknown {2}", length, rfAddress, unknown);
                switch (length)
                {
                    case 6:
                        // what is this? Window Contact?
                        break;
                    case 8:
                        // Eco-Button
                        Console.WriteLine("Eco-buttons not yet supported!");
                        break;
                    case 11:
                        // HeaterThermostat (also HeaterThermostat+?)
                    case 12:
                        // WallThermostat+ (as it's very similiar to HeaterThermostats)
                        byte valve = data[offset + 7];
                        double setTemp = (data[offset + 8] & 0x3F)/2.0;
                        byte tempMSB = (byte)((data[offset + 8] & 0x80) >> 7);
                        bool found = false;
                        if (length == 12 || (length == 11 && (data[offset + 6] & 0x03) > 1))
                        {
                            string dateUntil;
                            byte dateUntilDay, dateUntilMonth;
                            int dateUntilYear;
                            dateUntilMonth = (byte)(((data[offset + 9] & 0xE0) >> 4) | ((data[offset + 10] & 0x40) >> 6));
                            dateUntilDay = (byte)(data[offset + 9] & 0x1F);
                            dateUntilYear = (data[offset + 10] & 0x1F) + 2000;
                            dateUntil = dateUntilDay.ToString("00") + "." + dateUntilMonth.ToString("00") + "." + dateUntilYear.ToString();
                            string timeUntil = (data[offset + 11] * 0.5).ToString("00:00");
                            if (length == 11)
                                foreach (Room room in rooms)
                                {
                                    foreach (HeaterThermostat heater in room.HeaterThermostats)
                                        if (heater.RfAddress == rfAddress)
                                        {
                                            room.UpdateHeater(rfAddress, unknown, flags, valve, setTemp, dateUntil, timeUntil);
                                            found = true;
                                            break;
                                        }
                                    if (found) break;
                                }
                            else
                            {
                                double actualTemp;
                                actualTemp = (data[offset + 12] + tempMSB * 256) / 10.0;
                                foreach (Room room in rooms)
                                {
                                    foreach (WallThermostatPlus wallT in room.WallThermostatsPlus)
                                        if (wallT.RfAddress == rfAddress)
                                        {
                                            room.UpdateThermostat(rfAddress, unknown, flags, valve, setTemp, dateUntil, timeUntil, actualTemp);
                                            found = true;
                                            break;
                                        }
                                    if (found) break;
                                }
                            }
                        }
                        else {
                            double actualTemp;
                            actualTemp = (data[offset + 10] + (data[offset + 9] & 0x01) * 256) / 10.0;
                            foreach (Room room in rooms)
                            {
                                foreach (HeaterThermostat heater in room.HeaterThermostats)
                                    if (heater.RfAddress == rfAddress)
                                    {
                                        room.UpdateHeater(rfAddress, unknown, flags, valve, setTemp, actualTemp);
                                        found = true;
                                        break;
                                    }
                                if (found) break;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Unsupported L-Submessage message length");
                        break;
                }
                offset = offset + length + 1;
            }
        }
    }
}