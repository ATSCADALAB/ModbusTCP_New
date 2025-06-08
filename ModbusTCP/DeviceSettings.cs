namespace ModbusTCP
{
    public class DeviceSettings
    {
        public string IpAddress { get; set; }

        public int Port { get; set; }

        public int UnitID { get; set; }

        public int TimeOut { get; set; }
        
        public OrderBy OrderBy { get; set; }

        public int MaxReadTimes { get; set; }

        public string BlockSettings { get; set; }

        public string ClientID => $"{IpAddress}-{Port}";

        public void Update(DeviceSettings settings)
        {
            IpAddress = settings.IpAddress;
            Port = settings.Port;
            UnitID = settings.UnitID;
            TimeOut = settings.TimeOut;
            OrderBy = settings.OrderBy;
            MaxReadTimes = settings.MaxReadTimes;
            BlockSettings = settings.BlockSettings;
        }

        public static DeviceSettings Initialize(string deviceID)
        {
            if (string.IsNullOrWhiteSpace(deviceID)) return default;

            var deviceIDSplit = deviceID.Split('|');
            if (deviceIDSplit.Length != 7) return default;

            var ipAddress = deviceIDSplit[0];                    
            if (!int.TryParse(deviceIDSplit[1], out int port)) return default;
            if (!int.TryParse(deviceIDSplit[2], out int unitID)) return default;
            if (!int.TryParse(deviceIDSplit[3], out int timeOut)) return default;           
            if (!int.TryParse(deviceIDSplit[4], out int orderBy)) return default;
            if (!int.TryParse(deviceIDSplit[5], out int maxReadTimes)) return default;
            var blockSettings = deviceIDSplit[6];

            return new DeviceSettings()
            {
                IpAddress = ipAddress,
                Port = port,
                UnitID = unitID,
                TimeOut = timeOut,
                OrderBy = (OrderBy)orderBy,
                MaxReadTimes = maxReadTimes,
                BlockSettings = blockSettings
            };
        }
    }
}
