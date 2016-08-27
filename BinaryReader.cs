using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DLPTool
{
    public static class BinaryReaderExtensions
    {
        public static double ReadFloat(this BinaryReader @this)
        {
            return @this.ReadSingle();
        }

        public static string ReadString(this BinaryReader @this, int length)
        {
            var buf = @this.ReadBytes(length);
            var strLen = length;

            if (buf.Last() == '\0')
                strLen--;

            return Encoding.UTF8.GetString(buf, 0, strLen);
        }
    }

    public class BigEndianBinaryReader : BinaryReader
    {
        private byte[] ReadBigEndianBytes(int length)
        {
            var bytes = new byte[length];

            for (int i = (length - 1); i >= 0; i--)
                bytes[i] = ReadByte();

            return bytes;
        }

        public override decimal ReadDecimal()
        {
            throw new NotImplementedException();
        }

        public override double ReadDouble()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToDouble(buf, 0);
        }

        public override float ReadSingle()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToSingle(buf, 0);
        }

        public override short ReadInt16()
        {
            var buf = ReadBigEndianBytes(2);
            return BitConverter.ToInt16(buf, 0);
        }

        public override int ReadInt32()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToInt32(buf, 0);
        }

        public override long ReadInt64()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToInt64(buf, 0);
        }

        public override ushort ReadUInt16()
        {
            var buf = ReadBigEndianBytes(2);
            return BitConverter.ToUInt16(buf, 0);
        }

        public override uint ReadUInt32()
        {
            var buf = ReadBigEndianBytes(4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public override ulong ReadUInt64()
        {
            var buf = ReadBigEndianBytes(8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public BigEndianBinaryReader(Stream input) : base(input) { }
        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }
    }
}
