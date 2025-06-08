using System.Collections.Generic;

namespace ModbusTCP
{
    /// <summary>
    /// Block khai bao doc Multi
    /// </summary>
    public class BlockReader
    {
        /// <summary>
        /// Vung nho 
        /// </summary>
        public ModbusArea Area { get; private set; }

        /// <summary>
        /// Kiem tra kieu roi rac (Coils, Input)
        /// </summary>
        public bool IsDiscrete { get; set; }

        /// <summary>
        /// Thanh ghi bat dau
        /// </summary>
        public int From { get; private set; }

        /// <summary>
        /// Thanh ghi ket thuc
        /// </summary>
        public int To { get; private set; }

        /// <summary>
        /// So thanh ghi
        /// </summary>
        public int Count
        {
            get
            {
                var count = To - From + 1;
                return count < 0 ? 0 : count;
            }
        }

        /// <summary>
        /// Gia tri buffer duoc luu sau moi lan doc Block
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Khai bao settings block co hop le hay khong
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Ket qua tra ve sau moi lan doc (Good: True, Bad: False)
        /// </summary>
        public bool IsReadSuccess { get; set; } = true;

        public BlockReader(ModbusArea area)
        {
            Area = area;
        }

        public BlockReader(string blockSetting)
        {
            Init(blockSetting);
        }

        /// <summary>
        /// Cau hinh settings tu blockSettings
        /// Format: Area-indexUI-From-To...
        /// </summary>
        /// <param name="blockSetting"></param>
        private void Init(string blockSetting)
        {
            IsValid = false;
            var blockSettingSplit = blockSetting.Split('-');
            if (blockSettingSplit.Length == 4)
            {
                var area = blockSettingSplit[0];
                if (!int.TryParse(blockSettingSplit[2], out int from)) return;
                if (!int.TryParse(blockSettingSplit[3], out int to)) return;
                if (from > to) return;

                Area = area.ToArea();
                From = from - 1;
                To = to - 1;
                switch (Area)
                {
                    case ModbusArea.Coil:
                    case ModbusArea.InputContact:
                        IsDiscrete = true;
                        Buffer = new byte[(Count + 7) / 8];
                        IsValid = true;
                        break;
                    case ModbusArea.InputRegister:
                    case ModbusArea.HoldingRegister:
                        IsDiscrete = false;
                        // 1 word = 2 byte
                        Buffer = new byte[2 * Count];
                        IsValid = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Kiem tra address co nam trong Block hay khong
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool IsInBlock(Address address)
        {
            return
                address.Area == Area &&
                address.IsDiscrete == IsDiscrete &&
                address.Start >= From &&
                address.Start + address.Size - 1 <= To;
        }

        public void Refresh()
        {
            if (Buffer is null) return;
            for (var i = 0; i < Buffer.Length; i++)
                Buffer[i] = 0;
        }
        
        /// <summary>
        /// Modbus quy dinh so luong thanh ghi toi da doc ve. 
        /// Khi qua so luong thanh ghi can doc. Se tu cach tach thanh 2 hoac nhieu Block con
        /// </summary>
        /// <param name="blockSetting"></param>
        /// <returns></returns>
        public static IEnumerable<BlockReader> Initialize(string blockSetting)
        {
            var blockSettingSplit = blockSetting.Split('-');
            if (blockSettingSplit.Length == 4)
            {
                var areaNumber = blockSettingSplit[0];
                if (!int.TryParse(blockSettingSplit[2], out int from)) yield break;
                if (!int.TryParse(blockSettingSplit[3], out int to)) yield break;
                if (from > to) yield break;

                from--;
                to--;
                var area = areaNumber.ToArea();
                switch (area)
                {
                    // Driver quy dinh toi da 1920 Cois, InputContact
                    case ModbusArea.Coil:
                    case ModbusArea.InputContact:
                        while (from < to)
                        {
                            var block = new BlockReader(area)
                            {
                                IsValid = true,
                                IsDiscrete = true,
                                From = from
                            };

                            var index = from + 1920;
                            if (index < to)
                            {
                                block.To = index - 1;
                                block.Buffer = new byte[240];
                            }
                            else
                            {
                                var count = to - from + 1;
                                block.To = to;
                                block.Buffer = new byte[(count + 7) / 8];
                            }

                            from = index;
                            yield return block;
                        }
                        yield break;

                    // Driver quy trinh toi da 120 thanh ghi
                    case ModbusArea.InputRegister:
                    case ModbusArea.HoldingRegister:
                        while (from < to)
                        {
                            var block = new BlockReader(area)
                            {
                                IsValid = true,
                                IsDiscrete = false,
                                From = from
                            };

                            var index = from + 120;
                            if (index < to)
                            {
                                block.To = index - 1;
                                block.Buffer = new byte[240];
                            }
                            else
                            {
                                var count = to - from + 1;
                                block.To = to;
                                block.Buffer = new byte[2 * count];
                            }

                            from = index;
                            yield return block;
                        }
                        yield break;
                }
            }
        }
    }
}
