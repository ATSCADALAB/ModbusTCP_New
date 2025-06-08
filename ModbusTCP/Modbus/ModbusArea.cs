namespace ModbusTCP
{    
    /// <summary>
    /// Vung nho Modbus
    /// </summary>
    public enum ModbusArea : byte
    {
        None = 0,
        Coil = 1,
        InputContact = 2,
        InputRegister = 4,
        HoldingRegister = 3
    }
}
