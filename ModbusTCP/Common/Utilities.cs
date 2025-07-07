using System;
using System.Text.RegularExpressions;

namespace ModbusTCP
{
    /// <summary>
    /// Cac ham ho tro
    /// </summary>
    public static class RegexExtensions
    {
        public const string IpAddressPattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

        public const string AddressPattern = @"^(\d{1,5}|(0|1)\d{1,5}|(3|4)\d{1,5}(.([0-9]|1[0-5]))?)$";

        public static bool IsMatchIPV4Pattern(this string ipAddress)
        {
            return Regex.IsMatch(ipAddress, IpAddressPattern);
        }

        public static bool IsMatchAddressPattern(this string tagAddress)
        {
            return Regex.IsMatch(tagAddress, AddressPattern);
        }
    } 

    public static class TagExtensions
    {
        public static ModbusArea ToArea(this string area)
        {
            switch (area)
            {
                case "0":
                    return ModbusArea.Coil;
                case "1":
                    return ModbusArea.InputContact;
                case "3":
                    return ModbusArea.InputRegister;
                case "4":
                    return ModbusArea.HoldingRegister;
                default:
                    return ModbusArea.None;
            }
        }

        public static DataType GetDataType(this string tagType)
        {
            tagType = tagType.ToUpper();
            switch (tagType)
            {
                case "BOOL":
                    return DataType.Bool;
                case "WORD":
                    return DataType.Word;
                case "SHORT":
                    return DataType.Short;
                case "DWORD":
                    return DataType.DWord;
                case "LONG":
                    return DataType.Long;
                case "FLOAT":
                    return DataType.Float;
                case "DLONG":
                    return DataType.DLong;
                case "DOUBLE":
                    return DataType.Double;
                default:
                    return DataType.Default;
            }
        }

        public static string ToDisplayName(this AccessRight accessRight)
        {
            switch (accessRight)
            {
                case AccessRight.ReadOnly:
                    return "ReadOnly";
                case AccessRight.ReadWrite:
                    return "ReadWrite";
                default:
                    return string.Empty;
            }
        }

        public static string ToDisplayName(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Bool:
                    return "Bool";
                case DataType.Word:
                    return "Word";
                case DataType.Short:
                    return "Short";
                case DataType.DWord:
                    return "DWord";
                case DataType.Long:
                    return "Long";
                case DataType.Float:
                    return "Float";
                case DataType.DLong:
                    return "DLong";
                case DataType.Double:
                    return "Double";
                default:
                    return "Default";
            }
        }

        /// <summary>
        /// Tra ve so thanh ghi Word (2 byte)
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static int GetSize(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Bool:
                    return 1;
                case DataType.Word:
                    return 1;
                case DataType.Short:
                    return 1;
                case DataType.DWord:
                    return 2;
                case DataType.Long:
                    return 2;
                case DataType.Float:
                    return 2;
                case DataType.DLong:
                    return 4;
                case DataType.Double:
                    return 4;
                default:
                    return 0;
            }
        }

        public static Address GetAddress(this string tagAddress, DataType dataType, out string description)
        {
            description = string.Empty;
            tagAddress = tagAddress.ToUpper();
            if (!tagAddress.IsMatchAddressPattern())
            {
                description = $"The tag address wasn't in correct format.";
                return default;
            }

            var registerStr = tagAddress;
            var bitStr = "0";
            var dotIndex = tagAddress.IndexOf('.');
            if (dotIndex > 0)
            {
                bitStr = tagAddress.Substring(dotIndex + 1);
                registerStr = tagAddress.Substring(0, dotIndex);
            }
            registerStr = registerStr.PadLeft(6, '0');

            if (!int.TryParse(registerStr, out int register) ||
                !byte.TryParse(bitStr, out byte bit) ||
                bit > 15)
            {
                description = $"The tag address wasn't in correct format.";
                return default;
            }

            var tagType = dataType;
            var address = new Address();
            switch (registerStr[0])
            {
                case '0':
                    if (!CheckRange(register, 1, 99999, ref description)) return default;
                    if (!CheckCoil(tagType, ref description)) return default;
                    address.Area = ModbusArea.Coil;
                    address.IsDiscrete = true;
                    address.AccessRight = AccessRight.ReadWrite;
                    address.DataType = DataType.Bool;
                    address.Start = register - 1;
                    address.Size = 1;
                    break;
                case '1':
                    if (!CheckRange(register, 100001, 199999, ref description)) return default;
                    if (!CheckInputContact(tagType, ref description)) return default;
                    address.Area = ModbusArea.InputContact;
                    address.IsDiscrete = true;
                    address.AccessRight = AccessRight.ReadOnly;
                    address.DataType = DataType.Bool;
                    address.Start = register - 100001;
                    address.Size = 1;
                    break;
                case '3':
                    if (!CheckRange(register, 300001, 399999, ref description)) return default;
                    if (!CheckInputRegister(dotIndex > 0, ref tagType, ref description)) return default;
                    address.Area = ModbusArea.InputRegister;
                    address.IsDiscrete = false;
                    address.AccessRight = AccessRight.ReadOnly;
                    address.DataType = tagType;
                    address.Start = register - 300001;
                    address.Size = tagType.GetSize();
                    address.Bit = bit;
                    break;
                case '4':
                    if (!CheckRange(register, 400001, 499999, ref description)) return default;
                    if (!CheckHoldingRegister(dotIndex > 0, ref tagType, ref description)) return default;
                    address.Area = ModbusArea.HoldingRegister;
                    address.IsDiscrete = false;
                    address.AccessRight = AccessRight.ReadWrite;
                    address.DataType = tagType;
                    address.Start = register - 400001;
                    address.Size = tagType.GetSize();
                    address.Bit = bit;
                    break;
                default:
                    return default;
            }

            return address;
        }

        public static bool CheckRange(int address, int min, int max, ref string description)
        {
            if (address < min || address > max)
            {
                description = $"The tag address wasn't in correct format.";
                return false;
            }

            return true;
        }

        public static bool CheckCoil(DataType dataType, ref string description)
        {
            if ((int)dataType > 1)
            {
                description = $"Invalid address. Only Bool type is for Coil memory addresses.";
                return false;
            }
            else
            {
                description = $"Only Bool type is for Coil memory addresses. Default type is Bool. This is Read - Write Tag";
                return true;
            }
        }

        public static bool CheckInputContact(DataType dataType, ref string description)
        {
            if ((int)dataType > 1)
            {
                description = $"Invalid address. Only Bool type is for Input Contact memory addresses.";
                return false;
            }
            else
            {
                description = $"Only Bool Type is for Input Contact memory addresses. Default type is Bool. This is Read Only Tag";
                return true;
            }
        }

        public static bool CheckInputRegister(bool isBool, ref DataType dataType, ref string description)
        {
            if (isBool)
            {
                if (dataType != DataType.Bool)
                {
                    description = $"Invalid address or type. Bool type is for Input Register Bit memory addresses";
                    return false;
                }
                return true;
            }
            else
            {
                if (dataType == DataType.Bool)
                {
                    description = $"Invalid address. Word, Short, Dword, Long, Float, Double types are for Input Register memory addresses.";
                    return false;
                }
                else
                {
                    description = $"Word, Short, Dword, Long, Float, Double types are for Input Register memory addresses. Default type is Word. This is Read Only Tag";
                    if (dataType == DataType.Default)
                        dataType = DataType.Word;
                    return true;
                }
            }
        }

        public static bool CheckHoldingRegister(bool isBool, ref DataType dataType, ref string description)
        {
            if (isBool)
            {
                if (dataType != DataType.Bool)
                {
                    description = $"Invalid address or type. Bool type is for Holding Register Bit memory addresses";
                    return false;
                }
                return true;
            }
            else
            {
                if (dataType == DataType.Bool)
                {
                    description = $"Invalid address. Word, Short, Dword, Long, Float, Double types are for Holding Register memory addresses.";
                    return false;
                }
                else
                {
                    description = $"Word, Short, Dword, Long, Float, Double types are for Holding Register memory addresses. Default type is Word. This is Read - Write Tag";
                    if (dataType == DataType.Default)
                        dataType = DataType.Word;
                    return true;
                }
            }
        }
    }

    public static class AddressExtensions
    {
        public static bool GetValue(this Address address, byte[] buffer, int position, OrderBy orderBy, ref string value)
        {
            switch (address.DataType)
            {
                case DataType.Bool:
                    value = buffer.GetBitAt(position, address.Bit) ? "1" : "0";
                    return true;
                case DataType.Word:
                    value = buffer.GetWordAt(position).ToString();
                    return true;
                case DataType.Short:
                    value = buffer.GetIntAt(position).ToString();
                    return true;
                case DataType.DWord:
                    value = buffer.GetDWordAt(position, orderBy).ToString();
                    return true;
                case DataType.Long:
                    value = buffer.GetDIntAt(position, orderBy).ToString();
                    return true;
                case DataType.Float:
                    value = buffer.GetRealAt(position, orderBy).ToString();
                    return true;
                case DataType.DLong:
                    value = buffer.GetULIntAt(position, orderBy).ToString();
                    return true;
                case DataType.Double:
                    value = buffer.GetLRealAt(position, orderBy).ToString();
                    return true;
                default:
                    return false;
            }
        }

        public static bool SetValue(this Address address, byte[] buffer, string value, OrderBy orderBy = OrderBy.High)
        {
            switch (address.DataType)
            {
                case DataType.Bool:
                    if (value.Equals("1") || value.Equals("0"))
                    {
                        buffer.SetBitAt(0, address.Bit, value.Equals("1"));
                        return true;
                    }
                    return false;
                case DataType.Word:
                    if (ushort.TryParse(value, out ushort ushortValue))
                    {
                        buffer.SetWordAt(0, ushortValue);
                        return true;
                    }
                    return false;
                case DataType.Short:
                    if (short.TryParse(value, out short shortValue))
                    {
                        buffer.SetIntAt(0, shortValue);
                        return true;
                    }
                    return false;
                case DataType.DWord:
                    if (uint.TryParse(value, out uint uintValue))
                    {
                        buffer.SetDWordAt(0, uintValue, orderBy);
                        return true;
                    }
                    break;
                case DataType.Long:
                    if (int.TryParse(value, out int intValue))
                    {
                        buffer.SetDIntAt(0, intValue, orderBy);
                        return true;
                    }
                    return false;
                case DataType.Float:
                    if (float.TryParse(value, out float floatValue))
                    {
                        buffer.SetRealAt(0, floatValue, orderBy);
                        return true;
                    }
                    return false;
                case DataType.DLong:
                    if (ulong.TryParse(value, out ulong dlongValue))
                    {
                        buffer.SetULintAt(0, dlongValue, orderBy);
                        return true;
                    }
                    return false;
                case DataType.Double:
                    if (double.TryParse(value, out double doubleValue))
                    {
                        buffer.SetLRealAt(0, doubleValue, orderBy);
                        return true;
                    }
                    return false;
                default:
                    break;

            }

            return false;
        }
    }
}
