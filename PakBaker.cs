using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;

namespace AstroModIntegrator
{
    public static class PakBaker
    {
        public static byte[] Bake(Dictionary<string, byte[]> data)
        {
            var stream = new MemoryStream();

            var builder = new PakBuilder();
            builder.Compression([PakCompression.Zlib]);
            var pak_writer = builder.Writer(stream, PakVersion.V4, "../../../");
            foreach (KeyValuePair<string, byte[]> entry in data)
            {
                pak_writer.WriteFile(entry.Key, entry.Value);
            }
            pak_writer.WriteIndex();

            return stream.ToArray();
        }
    }
}
