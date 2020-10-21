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

        public void Write(string value, EngineVersion version)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (version == EngineVersion.Version4_2)
                {
                    Write(1);
                    Write((byte)0);
                    return;
                }

                Write(0);
                return;
            }

            value = value + '\0';
            Write(encryptSize(value.Length));
            byte[] buffer = Encoding.Unicode.GetBytes(value);
            Write(buffer);
        }
    }
}
