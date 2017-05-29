using System.IO;
using System.Text;

namespace UE4TextConverter.Model
{
    internal class LocBinaryWriter : BinaryWriter
    {
        public LocBinaryWriter(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding) { }

        private int encryptSize(int size)
        {
            return (int)((size - 1) ^ 0xFFFFFFFF);
        }

        public override void Write(string value)
        {
            value = value + '\0';
            Write(encryptSize(value.Length));
            byte[] buffer = Encoding.Unicode.GetBytes(value);
            Write(buffer);
        }
    }
}
