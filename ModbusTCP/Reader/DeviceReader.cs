using System.Collections.Generic;

namespace ModbusTCP
{
    public class DeviceReader
    {
        #region FILEDS 

        private int readTimes;

        private readonly ATDriver driver;

        private readonly List<BlockReader> blockReaders;

        private ClientAdapter clientAdapter;

        #endregion

        #region PROPERTIES

        public string DeviceName { get; set; }

        public string DeviceID { get; set; }
       
        public DeviceSettings Settings { get; set; }

        /// <summary>
        /// Trang thai ket noi
        /// </summary>
        public bool ConnectionStatus { get; private set; }

        #endregion

        public DeviceReader(ATDriver driver)
        {
            this.driver = driver;
            this.blockReaders = new List<BlockReader>();
        }

        /// <summary>
        /// Khoi tao ban dau. Tao danh sach cac Block can doc Multi
        /// </summary>
        public void Initialize()
        {
            if (Settings is null) return;
            this.clientAdapter = this.driver.TryGetClientAdapter(Settings.ClientID, out ClientAdapter adapter) ?
                adapter : this.driver.AddClientAdapter(Settings);

            if (string.IsNullOrEmpty(Settings.BlockSettings)) return;
            this.blockReaders.Clear();
            foreach (var blockSetting in Settings.BlockSettings.Split('/'))
                if (!string.IsNullOrEmpty(blockSetting.Trim()))
                    this.blockReaders.AddRange(BlockReader.Initialize(blockSetting));

            this.readTimes = Settings.MaxReadTimes;
        }

        public bool CheckConnection()
        {
            ConnectionStatus = this.clientAdapter != null && this.clientAdapter.CheckConnection();
            return ConnectionStatus;
        }

        /// <summary>
        /// Ham doc multi
        /// </summary>
        public void ReadMulti()
        {
            if (this.blockReaders.Count == 0) return;
            this.readTimes++;
            // Ap dung voi co che doc Multi
            // Khi doc Multi. Du lieu se duoc lu vao trong Buffer
            // Moix lan read Tag. Se duoc lay data tu Buffer (neu co).
            // Sau MaxReadTimes lan. Se doc lai Multi 1 lan de cap nhat gia tri moi
            // 1. ReadMulti -> read Tag1
            // 2. read Tag2
            // 3. read Tag3
            // ...
            // Max. read TagMax
            // Max + 1: ReadMulti -> read TagMax+1
            // * Neu khai bao Max = so Tag trong 1 Device. Thi moi luot doc Device moi, se tien hanh doc Multi
            if (this.readTimes <= Settings.MaxReadTimes) return;
            this.readTimes = 1;
            // Doc tuan tu cac Block
            // Ket qua tra ve se duoc luu vao Buffer. Kem theo trang thai Status
            foreach (var blockReader in this.blockReaders)
            {
                if (!blockReader.IsValid) continue;
                blockReader.IsReadSuccess = ConnectionStatus && this.clientAdapter.Read(Settings.UnitID, blockReader);
            }
        }

        /// <summary>
        /// Ham doc single
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value">Gia tri tra ve</param>
        /// <returns>Trang thai read</returns>
        public bool Read(Address address, out string value)
        {
            // Kiem tra xem dia chi khai bao co nam trong 1 block nao hay khong
            // Neu co thi doc du lieu tu Buffer cua Block
            value = string.Empty;
            var blockReaderIndex = blockReaders.FindIndex(x => x.IsValid && x.IsInBlock(address));
            if (blockReaderIndex > -1)
            {
                var blockReader = this.blockReaders[blockReaderIndex];
                if (!blockReader.IsValid || !blockReader.IsReadSuccess) return false;
                if (blockReader.IsDiscrete)
                {
                    var bufferIndex = ((address.Start - blockReader.From) / 8);
                    var bitIndex = (address.Start - blockReader.From) % 8;
                    value = blockReader.Buffer.GetBitAt(bufferIndex, bitIndex) ? "1" : "0";
                    return true;
                }
                var position = (address.Start - blockReader.From) * 2;
                return address.GetValue(blockReader.Buffer, position, Settings.OrderBy, ref value);
            }

            // ... Neu khong tu doc truc tiep Single tu Device 
            if (!ConnectionStatus) return false;
            var buffer = new byte[(address.IsDiscrete ? 1 : 2) * address.Size];
            return
                this.clientAdapter.Read(Settings.UnitID, address, buffer) &&
                address.GetValue(buffer, 0, Settings.OrderBy, ref value);

        }

        /// <summary>
        /// Ham ghi gia tri
        /// </summary>
        /// <param name="address"></param>
        /// <param name="value">Gia tri can ghi</param>
        /// <returns>Trang thai ghi</returns>
        public bool Write(Address address, string value)
        {
            if (!ConnectionStatus) return false;
            switch (address.Area)
            {
                case ModbusArea.Coil:
                    return this.clientAdapter.WriteCoil(Settings.UnitID, address, value);
                case ModbusArea.HoldingRegister:
                    var buffer = new byte[2 * address.Size];
                    return
                        (address.DataType != DataType.Bool || this.clientAdapter.Read(Settings.UnitID, address, buffer)) &&
                        address.SetValue(buffer, value) &&
                        this.clientAdapter.WriteHolding(Settings.UnitID, address, buffer);
                default:
                    return false;
            }
        }       
    }
}
