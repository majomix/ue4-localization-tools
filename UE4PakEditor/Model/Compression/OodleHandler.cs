using System.Runtime.InteropServices;

namespace UE4PakEditor.Model.Compression
{
    public static class OodleHandler
    {
        [DllImport("oo2core_8_win64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern long OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] result, long outputBufferSize, int a, int b, int c, long d, long e, long f, long g, long h, long i, int threadModule);

        public static byte[] Decompress(byte[] input, long decompressedLength)
        {
            byte[] result = new byte[decompressedLength];
            long decodedSize = OodleLZ_Decompress(input, input.Length, result, decompressedLength, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
            if (decodedSize == 0) return null;
            return result;
        }
    }
}
