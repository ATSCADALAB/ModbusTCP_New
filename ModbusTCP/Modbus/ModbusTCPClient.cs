using System;
using System.Net;

namespace ModbusTCP
{
    public class ModbusTCPClient : IDisposable
    {
        #region FIELDS

        private string ipAddress = "127.0.0.1";

        private int port = 502;

        private int timeOut = 1000;

        private readonly ModbusSocket modbusSocket;

        private readonly byte[] modbusBuffer = new byte[0xFF];

        private short transactionID = 0;

        #endregion

        #region PROPERTIES

        public string Name { get; }

        public string IpAddress
        {
            get => this.ipAddress;
            set => this.ipAddress = value;
        }

        public int Port
        {
            get => this.port;
            set => this.port = value;
        }

        public int TimeOut
        {
            get => this.timeOut;
            set
            {
                if (value < 1000) return;
                this.timeOut = value;
                if (this.modbusSocket != null)
                {
                    this.modbusSocket.ConnectTimeout = TimeOut;
                    this.modbusSocket.ReceiveTimeout = TimeOut;
                    this.modbusSocket.SendTimeout = TimeOut;
                }
            }
        }

        public bool Connected => this.modbusSocket != null && this.modbusSocket.Connected;

        #endregion

        public ModbusTCPClient()
        {
            this.modbusSocket = new ModbusSocket();
        }

        public ModbusTCPClient(string name)
        {
            Name = name;
            this.modbusSocket = new ModbusSocket();
        }

        ~ModbusTCPClient()
        {
            Dispose();
        }

        #region CONNECTION

        public int Connect()
        {
            int errorCode;
            try
            {
                errorCode = this.modbusSocket.Connect(this.ipAddress, this.port);
            }
            catch
            {
                errorCode = ModbusConstants.ExcTCPConnectionFailed;
            }
            return errorCode;
        }

        public int ConnectTo(string ipAddress, ushort port)
        {
            this.ipAddress = ipAddress;
            this.port = port;
            return Connect();
        }

        public int Disconnect()
        {
            this.modbusSocket?.Close();
            return ModbusConstants.ResultOK;
        }

        #endregion

        #region DATA FUNCTION

        private byte[] CreateReadHeader(int id, int startAddress, ushort length, byte function)
        {
            byte[] data = new byte[12];
            
            if (transactionID > 250) transactionID = 0;
            var idBytes = BitConverter.GetBytes(transactionID);
            transactionID++;

            data[0] = idBytes[1];				// Slave id high byte
            data[1] = idBytes[0];				// Slave id low byte
            data[5] = 6;					// Message size
            data[6] = (byte)id;					// Slave address
            data[7] = function;				// Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0];				// Start address
            data[9] = _adr[1];				// Start address
            byte[] _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));
            data[10] = _length[0];			// Number of data to read
            data[11] = _length[1];			// Number of data to read
            return data;
        }

        private byte[] CreateWriteHeader(int id, int startAddress, ushort numData, ushort numBytes, byte function)
        {
            byte[] data = new byte[numBytes + 11];
            //data[0] = 0;				// Slave id high byte
            //data[1] = 0;				// Slave id low byte+
            if (transactionID > 250) transactionID = 0;
            var idBytes = BitConverter.GetBytes(transactionID);
            transactionID++;

            data[0] = idBytes[1];				// Slave id high byte
            data[1] = idBytes[0];				// Slave id low bytev

            byte[] _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
            data[4] = _size[0];				// Complete message size in bytes
            data[5] = _size[1];				// Complete message size in bytes
            data[6] = (byte)id;					// Slave address
            data[7] = function;				// Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0];				// Start address
            data[9] = _adr[1];				// Start address
            if (function >= ModbusConstants.FuncWriteMultipleCoils)
            {
                byte[] _cnt = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numData));
                data[10] = _cnt[0];			// Number of bytes
                data[11] = _cnt[1];			// Number of bytes
                data[12] = (byte)(numBytes - 2);
            }
            return data;
        }

        public int ReadBytes(int id, int startAddress, ushort length, byte function, byte[] values)
        {
            try
            {
                var header = CreateReadHeader(id, startAddress, length, function);
                // 7 bytes header + 1 byte FC + 1 byte number of data + [Data length]
                var erorrCode = WriteSyncData(header, 9 + values.Length, out byte[] result);
                if (erorrCode != ModbusConstants.ResultOK) return erorrCode;
                if (result is null || result.Length != values.Length) return ModbusConstants.ExcTCPDataReceive;

                Array.ConstrainedCopy(result, 0, values, 0, values.Length);
                return erorrCode;
            }
            catch
            {
                return ModbusConstants.ResultBad;
            }
        }

        public int WriteSingleCoils(int id, int startAddress, bool value)
        {
            try
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, ModbusConstants.FuncWriteSingleCoil);
                if (value == true) data[10] = 255;
                else data[10] = 0;
                return WriteSyncData(data, 12, out _);
            }
            catch
            {
                return ModbusConstants.ResultBad;
            }
        }

        public int WriteSingleRegister(int id, int startAddress, byte[] values)
        {
            try
            {
                byte[] data;
                data = CreateWriteHeader(id, startAddress, 1, 1, ModbusConstants.FuncWriteSingleRegister);
                data[10] = values[0];
                data[11] = values[1];
                return WriteSyncData(data, 12, out _);
            }
            catch
            {
                return ModbusConstants.ResultBad;
            }
        }

        public int WriteMultipleRegister(int id, int startAddress, byte[] values)
        {
            try
            {
                ushort numBytes = Convert.ToUInt16(values.Length);
                if (numBytes % 2 > 0) numBytes++;
                byte[] data;

                data = CreateWriteHeader(id, startAddress, Convert.ToUInt16(numBytes / 2), Convert.ToUInt16(numBytes + 2), ModbusConstants.FuncWriteMultipleRegister);
                Array.ConstrainedCopy(values, 0, data, 13, values.Length);
                return WriteSyncData(data, 12, out _);
            }
            catch
            {
                return ModbusConstants.ResultBad;
            }
        }

        private int WriteSyncData(byte[] writeData, int responseSize, out byte[] result)
        {
            int errorCode;
            result = default;

            errorCode = this.modbusSocket.Send(writeData, 0, writeData.Length);
            if (errorCode != ModbusConstants.ResultOK) return errorCode;

            errorCode = this.modbusSocket.Receive(this.modbusBuffer, 0, responseSize);
            if (errorCode != ModbusConstants.ResultOK) return errorCode;

            byte function = modbusBuffer[7];

            // ------------------------------------------------------------
            // Response data is slave ModbusModbus.exception
            if (function > ModbusConstants.ExcExceptionOffset)
            {
                return function;
            }
            // ------------------------------------------------------------
            // Write response data
            else if ((function >= ModbusConstants.FuncWriteSingleCoil) && (function != ModbusConstants.FuncReadWriteMultipleRegister))
            {
                result = new byte[2];
                Array.ConstrainedCopy(modbusBuffer, 10, result, 0, 2);
            }
            // ------------------------------------------------------------
            // Read response data
            else
            {
                result = new byte[modbusBuffer[8]];
                Array.ConstrainedCopy(modbusBuffer, 9, result, 0, modbusBuffer[8]);
            }

            return ModbusConstants.ResultOK;
        }

        #endregion

        public void Dispose()
        {
            this.modbusSocket?.Close();
        }
    }
}
