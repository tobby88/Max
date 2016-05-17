namespace Tobby.Max
{
    class WindowContact
    {
        // class properties:
        private string rfAddress;
        public string RfAddress { get { return rfAddress; } }
        private string serial;
        public string Serial { get { return serial; } }
        private string name;
        public string Name { get { return name; } }

        // constructor:
        public WindowContact(string rfAddress, string serial, string name)
        {
            this.rfAddress = rfAddress;
            this.serial = serial;
            this.name = name;
        }
    }
}
