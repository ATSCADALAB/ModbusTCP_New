namespace ModbusTCP
{
    public class ModbusConstants
    {
        public const byte FuncReadCoil = 1;

        public const byte FuncReadDiscreteInputs = 2;

        public const byte FuncReadHoldingRegister = 3;

        public const byte FuncReadInputRegister = 4;

        public const byte FuncWriteSingleCoil = 5;

        public const byte FuncWriteSingleRegister = 6;

        public const byte FuncWriteMultipleCoils = 15;

        public const byte FuncWriteMultipleRegister = 16;

        public const byte FuncReadWriteMultipleRegister = 23;

        public const byte ResultOK = 0;

        public const byte ResultBad = 7;

        /// <summary>Constant for exception illegal function.</summary>
        public const byte ExcIllegalFunction = 1;
        /// <summary>Constant for exception illegal data address.</summary>
        public const byte ExcIllegalDataAdr = 2;
        /// <summary>Constant for exception illegal data value.</summary>
        public const byte ExcIllegalDataVal = 3;
        /// <summary>Constant for exception slave device failure.</summary>
        public const byte ExcSlaveDeviceFailure = 4;
        /// <summary>Constant for exception acknowledge.</summary>
        public const byte ExcAck = 5;
        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte ExcSlaveIsBusy = 6;
        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte ExcGatePathUnavailable = 10;
        /// <summary>Constant for exception not connected.</summary>
        public const byte ExcExceptionNotConnected = 253;
        /// <summary>Constant for exception connection lost.</summary>
        public const byte ExcExceptionConnectionLost = 254;
        /// <summary>Constant for exception response timeout.</summary>
        public const byte ExcExceptionTimeout = 255;
        /// <summary>Constant for exception wrong offset.</summary>
        public const byte ExcExceptionOffset = 128;
        /// <summary>Constant for exception send failt.</summary>
        public const byte ExcSendFailt = 100;

        public const int ExcTCPSocketCreation = 0x00000100;
        public const int ExcTCPConnectionTimeout = 0x00000200;
        public const int ExcTCPConnectionFailed = 0x00000300;
        public const int ExcTCPReceiveTimeout = 0x00000400;
        public const int ExcTCPDataReceive = 0x00000500;
        public const int ExcTCPSendTimeout = 0x00000600;
        public const int ExcTCPDataSend = 0x00000700;
        public const int ExcTCPConnectionReset = 0x00000800;
        public const int ExcTCPNotConnected = 0x00000900;
    }    
}
