using System;

namespace Zepheus.InterLib.Encryption
{
    public sealed class InterCrypto
    {

        public static byte[] EncryptData(byte[] iv, byte[] data)
        {
            byte[] ret = new byte[data.Length];
            Buffer.BlockCopy(data, 0, ret, 0, data.Length);
            // Some simple encryption...
            for (int i = 0; i < ret.Length; i++)
            {
                ret[ret.Length - 1 - i] ^= iv[i % 16];
                ret[i] ^= iv[i % 16];
            }

            return ret;
        }

        public static byte[] DecryptData(byte[] iv, byte[] data)
        {
            byte[] ret = new byte[data.Length];
            Buffer.BlockCopy(data, 0, ret, 0, data.Length);
            // Some simple encryption...
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] ^= iv[i % 16];
                ret[ret.Length - 1 - i] ^= iv[i % 16];
            }

            return ret;
        }
    }
}
