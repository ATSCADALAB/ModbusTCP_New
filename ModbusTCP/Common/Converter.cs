using System;

namespace ModbusTCP
{
    /// <summary>
    /// Ham ep, chuyen kieu du lieu
    /// Tu byte -> kieu du lieu va nguoc lai
    /// </summary>
    public static class Converter
    {

        #region Get/Set the bit at Pos.Bit

        public static bool GetBitAt(this byte[] buffer, int pos, int bit)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            if (bit < 0) bit = 0;
            if (bit > 7) bit = 7;
            return (buffer[pos] & Mask[bit]) != 0;
        }

        public static void SetBitAt(this byte[] buffer, int pos, int bit, bool value)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            if (bit < 0)
            {
                bit = 0;
            }

            if (bit > 7)
            {
                bit = 7;
            }

            if (value)
            {
                buffer[pos] = (byte)(buffer[pos] | Mask[bit]);
            }
            else
            {
                buffer[pos] = (byte)(buffer[pos] & ~Mask[bit]);
            }

        }

        #endregion      

        #region Get/Set 16 bit signed value (S7 int) -32768..32767

        public static short GetIntAt(this byte[] buffer, int pos)
        {
            return (short)((buffer[pos] << 8) | buffer[pos + 1]);
        }

        public static void SetIntAt(this byte[] buffer, int pos, Int16 value)
        {
            buffer[pos] = (byte)(value >> 8);
            buffer[pos + 1] = (byte)(value & 0x00FF);
        }

        #endregion

        #region Get/Set 32 bit signed value (S7 DInt) -2147483648..2147483647

        public static int GetDIntAt(this byte[] buffer, int pos, OrderBy orderBy = OrderBy.High)
        {
            int result;
            if (orderBy == OrderBy.High)
            {
                result = buffer[pos];
                result <<= 8;
                result += buffer[pos + 1];
                result <<= 8;
                result += buffer[pos + 2];
                result <<= 8;
                result += buffer[pos + 3];
                return result;
            }

            result = buffer[pos + 2];
            result <<= 8;
            result += buffer[pos + 3];
            result <<= 8;
            result += buffer[pos + 0];
            result <<= 8;
            result += buffer[pos + 1];
            return result;
        }

        public static void SetDIntAt(this byte[] buffer, int pos, int value, OrderBy orderBy = OrderBy.High)
        {
            if(orderBy == OrderBy.High)
            {
                buffer[pos + 3] = (byte)(value & 0xFF);
                buffer[pos + 2] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 1] = (byte)((value >> 16) & 0xFF);
                buffer[pos] = (byte)((value >> 24) & 0xFF);
            }
            else
            {
                buffer[pos + 1] = (byte)(value & 0xFF);
                buffer[pos + 0] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 3] = (byte)((value >> 16) & 0xFF);
                buffer[pos + 2] = (byte)((value >> 24) & 0xFF);
            }            
        }

        #endregion              

        #region Get/Set 16 bit unsigned value (S7 UInt) 0..65535

        public static UInt16 GetUIntAt(this byte[] buffer, int pos)
        {
            return (UInt16)((buffer[pos] << 8) | buffer[pos + 1]);
        }

        public static void SetUIntAt(this byte[] buffer, int pos, UInt16 value)
        {
            buffer[pos] = (byte)(value >> 8);
            buffer[pos + 1] = (byte)(value & 0x00FF);
        }

        #endregion

        #region Get/Set 32 bit unsigned value (S7 UDInt) 0..4294967296

        public static UInt32 GetUDIntAt(this byte[] buffer, int pos, OrderBy orderBy = OrderBy.High)
        {
            UInt32 result;

            if (orderBy == OrderBy.High)
            {
                result = buffer[pos];
                result <<= 8;
                result |= buffer[pos + 1];
                result <<= 8;
                result |= buffer[pos + 2];
                result <<= 8;
                result |= buffer[pos + 3];
                return result;
            }

            result = buffer[pos + 2];
            result <<= 8;
            result |= buffer[pos + 3];
            result <<= 8;
            result |= buffer[pos];
            result <<= 8;
            result |= buffer[pos + 1];
            return result;
        }

        public static void SetUDIntAt(this byte[] buffer, int pos, UInt32 value, OrderBy orderBy = OrderBy.High)
        {
            if (orderBy == OrderBy.High)
            {
                buffer[pos + 3] = (byte)(value & 0xFF);
                buffer[pos + 2] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 1] = (byte)((value >> 16) & 0xFF);
                buffer[pos] = (byte)((value >> 24) & 0xFF);
            }
            else
            {
                buffer[pos + 1] = (byte)(value & 0xFF);
                buffer[pos + 0] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 3] = (byte)((value >> 16) & 0xFF);
                buffer[pos + 2] = (byte)((value >> 24) & 0xFF);
            }
        }

        #endregion

        #region Get/Set 64 bit unsigned value (S7 ULint) 0..18446744073709551616

        public static UInt64 GetULIntAt(this byte[] buffer, int pos, OrderBy order = OrderBy.High)
        {
            UInt64 result;
            if (order == OrderBy.High)
            {
                result = buffer[pos];
                result <<= 8;
                result |= buffer[pos + 1];
                result <<= 8;
                result |= buffer[pos + 2];
                result <<= 8;
                result |= buffer[pos + 3];
                result <<= 8;
                result |= buffer[pos + 4];
                result <<= 8;
                result |= buffer[pos + 5];
                result <<= 8;
                result |= buffer[pos + 6];
                result <<= 8;
                result |= buffer[pos + 7];
                return result;
            }

            result = buffer[pos + 6];
            result <<= 8;
            result |= buffer[pos + 7];
            result <<= 8;
            result |= buffer[pos + 4];
            result <<= 8;
            result |= buffer[pos + 5];
            result <<= 8;
            result |= buffer[pos + 2];
            result <<= 8;
            result |= buffer[pos + 3];
            result <<= 8;
            result |= buffer[pos + 0];
            result <<= 8;
            result |= buffer[pos + 1];
            return result;
        }

        public static void SetULintAt(this byte[] buffer, int pos, UInt64 value, OrderBy orderBy = OrderBy.High)
        {
            if (orderBy == OrderBy.High)
            {
                buffer[pos + 7] = (byte)(value & 0xFF);
                buffer[pos + 6] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 5] = (byte)((value >> 16) & 0xFF);
                buffer[pos + 4] = (byte)((value >> 24) & 0xFF);
                buffer[pos + 3] = (byte)((value >> 32) & 0xFF);
                buffer[pos + 2] = (byte)((value >> 40) & 0xFF);
                buffer[pos + 1] = (byte)((value >> 48) & 0xFF);
                buffer[pos] = (byte)((value >> 56) & 0xFF);
            }
            else
            {
                buffer[pos + 7] = (byte)(value & 0xFF);
                buffer[pos + 6] = (byte)((value >> 8) & 0xFF);
                buffer[pos + 5] = (byte)((value >> 16) & 0xFF);
                buffer[pos + 4] = (byte)((value >> 24) & 0xFF);
                buffer[pos + 3] = (byte)((value >> 32) & 0xFF);
                buffer[pos + 2] = (byte)((value >> 40) & 0xFF);
                buffer[pos + 1] = (byte)((value >> 48) & 0xFF);
                buffer[pos] = (byte)((value >> 56) & 0xFF);
            }
        }

        #endregion       

        #region Get/Set 16 bit word (S7 Word) 16#0000..16#FFFF

        public static UInt16 GetWordAt(this byte[] buffer, int pos)
        {
            return GetUIntAt(buffer, pos);
        }

        public static void SetWordAt(this byte[] buffer, int pos, UInt16 value)
        {
            SetUIntAt(buffer, pos, value);
        }

        #endregion

        #region Get/Set 32 bit word (S7 DWord) 16#00000000..16#FFFFFFFF

        public static UInt32 GetDWordAt(this byte[] buffer, int pos, OrderBy orderBy = OrderBy.High)
        {
            return GetUDIntAt(buffer, pos, orderBy);
        }

        public static void SetDWordAt(this byte[] buffer, int pos, UInt32 value, OrderBy orderBy = OrderBy.High)
        {
            SetUDIntAt(buffer, pos, value, orderBy);
        }

        #endregion       

        #region Get/Set 32 bit floating point number (S7 Real) (Range of Single)

        public static Single GetRealAt(this byte[] buffer, int pos, OrderBy orderBy = OrderBy.High)
        {
            UInt32 value = GetUDIntAt(buffer, pos, orderBy);
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static void SetRealAt(this byte[] buffer, int pos, Single value, OrderBy orderBy = OrderBy.High)
        {
            byte[] FloatArray = BitConverter.GetBytes(value);
            if(orderBy == OrderBy.High)
            {
                buffer[pos] = FloatArray[3];
                buffer[pos + 1] = FloatArray[2];
                buffer[pos + 2] = FloatArray[1];
                buffer[pos + 3] = FloatArray[0];
            }
            else
            {
                buffer[pos + 2] = FloatArray[3];
                buffer[pos + 3] = FloatArray[2];
                buffer[pos + 0] = FloatArray[1];
                buffer[pos + 1] = FloatArray[0];
            }
        }

        #endregion

        #region Get/Set 64 bit floating point number (S7 LReal) (Range of Double)

        public static Double GetLRealAt(this byte[] buffer, int pos, OrderBy orderBy = OrderBy.High)
        {
            UInt64 value = GetULIntAt(buffer, pos, orderBy);
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToDouble(bytes, 0);
        }

        public static void SetLRealAt(this byte[] buffer, int pos, Double value, OrderBy orderBy = OrderBy.High)
        {
            byte[] FloatArray = BitConverter.GetBytes(value);
            if(orderBy == OrderBy.High)
            {
                buffer[pos] = FloatArray[7];
                buffer[pos + 1] = FloatArray[6];
                buffer[pos + 2] = FloatArray[5];
                buffer[pos + 3] = FloatArray[4];
                buffer[pos + 4] = FloatArray[3];
                buffer[pos + 5] = FloatArray[2];
                buffer[pos + 6] = FloatArray[1];
                buffer[pos + 7] = FloatArray[0];
            }
            else
            {
                buffer[pos + 6] = FloatArray[7];
                buffer[pos + 7] = FloatArray[6];
                buffer[pos + 4] = FloatArray[5];
                buffer[pos + 5] = FloatArray[4];
                buffer[pos + 2] = FloatArray[3];
                buffer[pos + 3] = FloatArray[2];
                buffer[pos + 0] = FloatArray[1];
                buffer[pos + 1] = FloatArray[0];
            }
        }

        #endregion
    }
}
