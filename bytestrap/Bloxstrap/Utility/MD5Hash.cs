using System.Security.Cryptography;

namespace Bloxstrap.Utility
{
    public static class MD5Hash
    {
        // Hashing is IO-bound on large packages - a big sequential buffer keeps
        // the disk queue full instead of dribbling 4KB reads.
        private const int StreamBufferSize = 131072; // 128 KB

        public static string FromBytes(byte[] data)
        {
            using MD5 md5 = MD5.Create();
            return Stringify(md5.ComputeHash(data));
        }

        public static string FromStream(Stream stream)
        {
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            using MD5 md5 = MD5.Create();
            return Stringify(md5.ComputeHash(stream));
        }

        public static string FromFile(string filename)
        {
            using FileStream stream = new(
                filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                StreamBufferSize,
                FileOptions.SequentialScan);

            using MD5 md5 = MD5.Create();
            return Stringify(md5.ComputeHash(stream));
        }

        public static string FromString(string str)
        {
            return FromBytes(Encoding.UTF8.GetBytes(str));
        }

        public static string Stringify(byte[] hash)
        {
            // Convert.ToHexString is allocation-cheaper than BitConverter + Replace.
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// Fast path for "are these two files identical?" - compares file lengths
        /// first so differing files never pay for a full hash of both files.
        /// </summary>
        public static bool FilesEqual(string pathA, string pathB)
        {
            var infoA = new FileInfo(pathA);
            var infoB = new FileInfo(pathB);

            if (!infoA.Exists || !infoB.Exists)
                return false;

            if (infoA.Length != infoB.Length)
                return false;

            if (String.Equals(infoA.FullName, infoB.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            return String.Equals(FromFile(pathA), FromFile(pathB), StringComparison.Ordinal);
        }
    }
}
