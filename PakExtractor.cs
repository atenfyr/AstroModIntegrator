using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI.Unversioned;
using UAssetAPI;

namespace AstroModIntegrator
{
    public class PakExtractor : IDisposable
    {
        private Stream inner_stream;
        private PakBuilder builder;
        private PakReader pak_reader;

        public PakExtractor(Stream stream)
        {
            inner_stream = stream;
            builder = new PakBuilder();
            pak_reader = builder.Reader(stream);
        }

        public IReadOnlyList<string> GetAllPaths()
        {
            return pak_reader.Files().ToList().AsReadOnly();
        }

        public bool HasPath(string searchPath)
        {
            return GetAllPaths().Contains(searchPath);
        }

        public byte[] ReadRaw(string searchPath)
        {
            return pak_reader.Get(inner_stream, searchPath);
        }

        public void Dispose()
        {
            inner_stream.Dispose();
        }

        private static Metadata ParseMetadata(string data)
        {
            JObject jobj = JObject.Parse(data);
            int schemaVersion = jobj.ContainsKey("schema_version") ? (int)jobj["schema_version"] : 1;
            switch (schemaVersion)
            {
                case 1: // legacy
                    return JsonConvert.DeserializeObject<MetadataSchema1>(data).Convert();
                case 2:
                    return JsonConvert.DeserializeObject<Metadata>(data);
                default:
                    throw new NotImplementedException("Unimplemented schema version " + schemaVersion);
            }
        }

        public Metadata ReadMetadata()
        {
            string data = Encoding.UTF8.GetString(ReadRaw("metadata.json"));
            if (string.IsNullOrEmpty(data)) return null;
            try
            {
                return ParseMetadata(data);
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }
}
