using System.IO;
using System.Text;

namespace UE4TextConverter.Model
{
    public class LocBinaryReader : BinaryReader
    {
        public LocBinaryReader(FileStream fileStream)
            : base(fileStream) { }

        private int DecryptSize(int size)
        {
            return (int)(size ^ 0xFFFFFFFF);
        }

        public override string ReadString()
        {
            var length = ReadInt32();

            if (length == 0)
                return string.Empty;

            var encoding = Encoding.UTF8;

            if (length < 0)
            {
                length = DecryptSize(length) + 1;
                encoding = Encoding.Unicode;
            }

            if (Equals(encoding, Encoding.Unicode))
            {
                length *= 2;
            }

            return encoding.GetString(ReadBytes(length)).Trim('\0');
        }
    }
}
