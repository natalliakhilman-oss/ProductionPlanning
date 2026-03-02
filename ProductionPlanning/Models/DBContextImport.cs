using System.ComponentModel;
using System.Security.Cryptography;

namespace ProductionPlanning.Models
{

    // Создает Guid ключи для использования их в качестве первичных ключей в БД
    public static class DbGuid
    {
        public enum DbGuidType
        {
            AsString,
            AsBinary,
            AtEnd
        }

        private static RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        /// <summary>
        /// Returns sequential GUID values.
        /// You must specify the GUID type that you want.
        /// If you are not sure what type to use please visit https://git.io/vS1hL
        /// </summary>

        public static Guid NewGuid(DbGuidType type = DbGuidType.AsString)
        {
            byte[] randomBytes = new byte[10];
            _rng.GetBytes(randomBytes);
            long timestamp = DateTime.UtcNow.Ticks / 10000L;
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            switch (type)
            {
                case DbGuidType.AsString:
                case DbGuidType.AsBinary:
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    if (type == DbGuidType.AsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }
                    break;

                case DbGuidType.AtEnd:
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }
            return new Guid(guidBytes);
        }
    }
}
