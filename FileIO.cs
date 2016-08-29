using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ARTSManager
{
    public class asStream
    {
        private MemoryStream _stream;

        public asStream(int length)
        {
            _stream = new MemoryStream(length);
        }

        public asStream(string filename)
        {
            using (var fs = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                var length = (int)fs.Length;

                _stream = new MemoryStream(length);
                fs.CopyTo(_stream, length);

                // reset position
                _stream.Position = 0;
            }
        }
        
        private bool IsPositionInvalid(long lastPos, int length)
        {
            return ((int)(_stream.Position - lastPos) != length);
        }

        public byte[] Read(int length)
        {
            var buffer = new byte[length];
            if (_stream.Read(buffer, 0, length) != length)
                throw new EndOfStreamException("asStream::Read -- End of stream.");
            return buffer;
        }

        public int Write(byte[] buffer, int length = -1)
        {
            if (length == -1)
                length = buffer.Length;
            if (length == 0)
                return 0;

            var lastPos = _stream.Position;
            _stream.Write(buffer, 0, length);
            if (IsPositionInvalid(lastPos, length))
                throw new EndOfStreamException("asStream::Write -- End of stream.");
            return length;
        }

        protected int GetArray<T>(T[] values, int count, Func<T> enumerator)
        {
            for (int i = 0; i < count; i++)
                values[i] = enumerator();
            return count;
        }

        protected int Put<T>(T value, int size, Func<T, byte[]> getBytes)
            where T : struct
        {
            var buffer = getBytes(value);
            Array.Reverse(buffer);
            return Write(buffer, size);
        }

        protected int PutArray<T>(T[] values, int count, Func<T, int> enumerator)
        {
            if (count == -1)
                count = values.Length;

            var length = 0;

            if (count > 0)
            {
                foreach (var value in values)
                    length += enumerator(value);
            }

            return length;
        }
        
        public byte GetByte()
        {
            return Read(1)[0];
        }

        public short GetShort()
        {
            var buffer = Read(2);
            Array.Reverse(buffer);
            return BitConverter.ToInt16(buffer, 0);
        }
        
        public int GetInt()
        {
            var buffer = Read(4);
            Array.Reverse(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }
        
        public float GetFloat()
        {
            var buffer = Read(4);
            Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public string GetString()
        {
            // NOTE: this expects an int length before the actual string!
            var length = GetInt();
            return GetString(length);
        }
        
        public string GetString(int length)
        {
            // sounds like they don't want a string...ok then
            if (length < 1)
                return String.Empty;

            var input = Read(length);
            var str = "";

            for (int i = 0; i < length; i++)
            {
                var c = (char)input[i];

                // end of string?
                if (c == '\0')
                    break;

                str += c;
            }

            return str;
        }

        public int PutString(string value)
        {
            var strLen = value.Length;

            Put(strLen);
            return PutString(value, (value.Length + 1));
        }

        public int PutString(string value, int length)
        {
            var strLen = value.Length;
            var buffer = new byte[length];

            for (int i = 0; i < strLen; i++)
                buffer[i] = (byte)value[i];

            return Write(buffer);
        }

        public int Get(byte[] values, int count)
        {
            return GetArray(values, count, GetByte);
        }

        public int Get(short[] values, int count)
        {
            return GetArray(values, count, GetShort);
        }
        
        public int Get(int[] values, int count)
        {
            return GetArray(values, count, GetInt);
        }
        
        public int Get(float[] values, int count)
        {
            return GetArray(values, count, GetFloat);
        }

        public int Put(byte value)
        {
            return Write(new[] { value }, 1);
        }

        public int Put(short value)
        {
            return Put(value, 2, BitConverter.GetBytes);
        }
        
        public int Put(int value)
        {
            return Put(value, 4, BitConverter.GetBytes);
        }
        
        public int Put(float value)
        {
            return Put(value, 4, BitConverter.GetBytes);
        }

        public int Put(byte[] values, int count = -1)
        {
            if (count == -1)
                count = values.Length;

            return Write(values, count);
        }

        public int Put(short[] values, int count = -1)
        {
            return PutArray(values, count, Put);
        }
        
        public int Put(int[] values, int count = -1)
        {
            return PutArray(values, count, Put);
        }
        
        public int Put(float[] values, int count = -1)
        {
            return PutArray(values, count, Put);
        }

        public int Seek(int offset)
        {
            return (int)_stream.Seek(offset, SeekOrigin.Begin);
        }

        public int Skip(int count)
        {
            return (int)(_stream.Position += count);
        }

        public int Tell()
        {
            return (int)_stream.Position;
        }

        public int Size()
        {
            return (int)_stream.Length;
        }
    }
}
