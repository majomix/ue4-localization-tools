using System.IO;
using System.Text;

namespace UE4TextConverter.Model
{
    internal class LocBinaryReader : BinaryReader
    {
        public LocBinaryReader(FileStream fileStream, Encoding encoding)
            : base(fileStream, encoding) { }

        private int decryptSize(uint size)
        {
            return (int)((size ^ 0xFFFFFFFF) + 1);
        }

        private int readEncryptedUInt32()
        {
            return decryptSize(ReadUInt32());
        }

        public override string ReadString()
        {
            int length = readEncryptedUInt32();
            return new string(ReadChars(length)).TrimEnd('\0');
        }
    }
}
