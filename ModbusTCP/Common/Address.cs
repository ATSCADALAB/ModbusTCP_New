namespace ModbusTCP
{
    /// <summary>
    /// Quy dinh thong tin dai chi khai bao cua Tag
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Vung du lieu thanh ghi
        /// </summary>
        public ModbusArea Area { get; set; }

        /// <summary>
        /// Co phai la kieu Bool (roi rac hay khong)
        /// </summary>
        public bool IsDiscrete { get; set; }

        /// <summary>
        /// Quyen access truy cap
        /// </summary>
        public AccessRight AccessRight { get; set; }

        /// <summary>
        /// Lieu du lieu
        /// </summary>
        public DataType DataType { get; set; }
       
        /// <summary>
        /// Dia chi thanh ghi bat dau
        /// </summary>
        public int Start { get; set; }
        
        /// <summary>
        /// So luong thanh ghi
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Vi tri bit tren thanh ghi
        /// Dung cho kieu Bool
        /// </summary>
        public int Bit { get; set; }

        public override string ToString()
        {
            return $"{Area}-{IsDiscrete}-{DataType}-{Start}-{Size}";
        }
    }
}
