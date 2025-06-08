using System;
using System.Threading;

namespace ModbusTCP
{
    /// <summary>
    /// Ho tro cac ham lien quan den ket noi cua Client
    /// </summary>
    public class ClientAdapter : IDisposable
    {
        #region FIELDS

        private const int MaxTryConnection = 2;

        private const int MaxTryRead = 2;

        private const int MaxTryWrite = 2;

        private readonly object keyLock = new object();

        private DateTime lastConnected;

        #endregion

        #region PROPERTIES

        public string Name { get; set; }
        /// <summary>
        /// Chu ky song cua ket noi
        /// Neu qua thoi gian tren. 
        /// Client se ket noi lai voi Server (PLC) bat ke trang thai ket noi
        /// </summary>
        public uint Lifetime { get; set; } = 3600;

        public ModbusTCPClient Client { get; }
        /// <summary>
        /// Trang thai ket noi
        /// </summary>
        public bool Connected => Client != null && Client.Connected;

        #endregion

        #region CONSTRUCTORS

        public ClientAdapter(string name, ModbusTCPClient client)
        {
            Name = name;
            Client = client;
            this.lastConnected = DateTime.MinValue;
        }

        #endregion

        #region CONNECTION FUNCTION

        /// <summary>
        /// Kiem tra thoi gian ket noi cua Client
        /// Neu = 0. Duy tri trang thai ket noi hien tai
        /// Other. Thoi gian ket noi phai < LifeTime, neu khong se ket noi lai
        /// </summary>
        /// <returns></returns>
        public bool CheckLifeTime()
        {           
            var durationConnection = DateTime.Now - this.lastConnected;
            return
                Lifetime <= 0 ||
                durationConnection.TotalSeconds <= Lifetime ||
                Reconnect();
        }

        public bool CheckConnection()
        {
            CheckLifeTime();
            return Connected || Reconnect();
        }

        /// <summary>
        /// Ham Ket noi
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            for (var index = 0; index < MaxTryConnection; index++)
            {
                try
                {
                    lock (this.keyLock)
                        Client.Connect();
                }
                catch { }
                if (Client.Connected)
                {
                    this.lastConnected = DateTime.Now;
                    return true;
                }
                Thread.Sleep(1000);
            }
            return false;
        }
        /// <summary>
        /// Ham huy ket noi -> ket noi lai
        /// </summary>
        /// <returns></returns>
        public bool Reconnect()
        {
            for (var index = 0; index < MaxTryConnection; index++)
            {
                try
                {
                    lock (this.keyLock)
                    {
                        Client?.Disconnect();
                        Client?.Connect();
                    }
                }
                catch { }
                if (Client.Connected)
                {
                    this.lastConnected = DateTime.Now;
                    Thread.Sleep(100);
                    return true;
                }
                Thread.Sleep(1000);
            }
            return false;
        }
        /// <summary>
        /// Ham huy ket noi
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            for (var index = 0; index < MaxTryConnection; index++)
            {
                try
                {
                    lock (this.keyLock)
                        Client?.Disconnect();
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
            return false;
        }

        #endregion

        #region DATA FUNCTITON
        /// <summary>
        /// Doc du lieu Multi
        /// </summary>
        /// <param name="blockReader"></param>
        /// <returns></returns>
        public bool Read(int unitID, BlockReader blockReader)
        {
            if (!CheckConnection()) return false;
            int errorCode;
            for (int i = 0; i < MaxTryRead; i++)
            {
                lock (this.keyLock)
                {
                    errorCode = Client.ReadBytes(
                       unitID,
                       blockReader.From,
                       (ushort)blockReader.Count,
                       (byte)blockReader.Area,
                       blockReader.Buffer);
                }
                if (CheckErrorCode(errorCode)) return true;
                Thread.Sleep(15);
            }
            return false;
        }
       /// <summary>
       /// Doc du lueu single
       /// </summary>
       /// <param name="unitID"></param>
       /// <param name="address"></param>
       /// <param name="buffer"></param>
       /// <returns></returns>
        public bool Read(int unitID, Address address, byte[] buffer)
        {
            if (!CheckConnection()) return false;
            int errorCode;
            for (int i = 0; i < MaxTryRead; i++)
            {
                lock (this.keyLock)
                {
                    errorCode = Client.ReadBytes(
                        unitID,
                        address.Start,
                        (ushort)address.Size,
                        (byte)address.Area,
                        buffer);
                }
                if (CheckErrorCode(errorCode)) return true;
                Thread.Sleep(15);
            }
            return false;
        }

        public bool WriteCoil(int unitID, Address address, string value)
        {
            if (!CheckConnection()) return false;
            if (!value.Equals("0") && !value.Equals("1")) return false;
            int errorCode;
            var bitValue = value.Equals("1");
            for (int i = 0; i < MaxTryWrite; i++)
            {
                lock (this.keyLock)
                {
                    errorCode = Client.WriteSingleCoils(
                        unitID,
                        address.Start,
                        bitValue);
                }
                if (CheckErrorCode(errorCode)) return true;
                Thread.Sleep(10);
            }
            return false;
        }

        public bool WriteHolding(int unitID, Address address, byte[] buffer)
        {
            if (!CheckConnection()) return false;
            int errorCode;
            for (var i = 0; i < MaxTryWrite; i++)
            {
                lock (this.keyLock)
                {
                    errorCode = address.Size == 1 ?
                        Client.WriteSingleRegister(unitID, address.Start, buffer) :
                        Client.WriteMultipleRegister(unitID, address.Start, buffer);
                }
                if (CheckErrorCode(errorCode)) return true;
                Thread.Sleep(10);
            }
            return false;
        }
        /// <summary>
        /// Kiem tra loi khi read/write
        /// Neu la loi lien quan den TCP Error...
        /// Phai huy ket noi, ket noi lai
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool CheckErrorCode(int errorCode)
        {
            if (errorCode == ModbusConstants.ResultOK) return true;
            if ((errorCode & 0x0000FF00) != 0) // TCP error. Must re-establish the connection 
            {
                Reconnect();
            }

            return false;
        }

        #endregion


        public void Dispose()
        {
            try
            {
                Client?.Disconnect();
                Client?.Dispose();
            }
            catch { }
        }
    }
}
