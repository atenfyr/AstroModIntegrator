using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;

namespace AstroModIntegrator
{
    public class DataTableBaker
    {
        private ModIntegrator ParentIntegrator;

        public DataTableBaker(ModIntegrator ParentIntegrator)
        {
            this.ParentIntegrator = ParentIntegrator;
        }

        public UAsset Bake(Metadata[] allMods, List<string> optionalModIDs, byte[] superRawData)
        {
            UAsset y = new UAsset(IntegratorUtils.EngineVersion);
            y.UseSeparateBulkDataFiles = true;
            y.CustomSerializationFlags = CustomSerializationFlags.SkipParsingBytecode | CustomSerializationFlags.SkipPreloadDependencyLoading;
            y.Read(new AssetBinaryReader(new MemoryStream(superRawData), y));

            DataTableExport targetCategory = null;
            foreach (Export cat in y.Exports)
            {
                if (cat is DataTableExport)
                {
                    targetCategory = (DataTableExport)cat;
                    break;
                }
            }

            List<StructPropertyData> tab = targetCategory.Table.Data;
            FName[] columns = tab[0].Value.Select(x => x.Name).ToArray();

            List<StructPropertyData> newTable = new List<StructPropertyData>();
            foreach (Metadata mod in allMods)
            {
                if (mod == null) continue;

                y.AddNameReference(new FString(mod.ModID));

                string codedSyncMode = "SyncMode::NewEnumerator0";
                switch(mod.Sync)
                {
                    case SyncMode.None:
                        codedSyncMode = "SyncMode::NewEnumerator0";
                        break;
                    case SyncMode.ServerOnly:
                        codedSyncMode = "SyncMode::NewEnumerator1";
                        break;
                    case SyncMode.ClientOnly:
                        codedSyncMode = "SyncMode::NewEnumerator2";
                        break;
                    case SyncMode.ServerAndClient:
                        codedSyncMode = "SyncMode::NewEnumerator3";
                        break;
                }

                List<PropertyData> rows = new List<PropertyData>();
                rows.Add(new StrPropertyData(columns[0])
                {
                    Value = new FString(mod.Name ?? "", Encoding.ASCII),
                });
                rows.Add(new StrPropertyData(columns[1])
                {
                    Value = new FString(mod.Author ?? "", Encoding.ASCII),
                });
                rows.Add(new StrPropertyData(columns[2])
                {
                    Value = new FString(mod.Description ?? "", Encoding.ASCII),
                });
                rows.Add(new StrPropertyData(columns[3])
                {
                    Value = new FString(mod.ModVersion?.ToString() ?? "", Encoding.ASCII),
                });
                rows.Add(new StrPropertyData(columns[4])
                {
                    Value = new FString(mod.GameBuild?.ToString() ?? "", Encoding.ASCII),
                });
                y.AddNameReference(new FString("SyncMode"));
                y.AddNameReference(new FString(codedSyncMode));
                rows.Add(new BytePropertyData(columns[5])
                {
                    ByteType = BytePropertyType.FName,
                    EnumType = new FName(y, "SyncMode"),
                    EnumValue = new FName(y, codedSyncMode)
                });
                rows.Add(new StrPropertyData(columns[6])
                {
                    Value = new FString(mod.Homepage ?? "", Encoding.ASCII),
                });
                rows.Add(new BoolPropertyData(columns[7])
                {
                    Value = optionalModIDs.Contains(mod.ModID),
                });

                newTable.Add(new StructPropertyData(new FName(y, mod.ModID))
                {
                    StructType = tab[0].StructType,
                    Value = rows
                });
            }

            targetCategory.Table.Data = newTable;
            return y;
        }

        public UAsset Bake2(byte[] superRawData)
        {
            UAsset y = new UAsset(IntegratorUtils.EngineVersion);
            y.UseSeparateBulkDataFiles = true;
            y.Read(new AssetBinaryReader(new MemoryStream(superRawData), y));

            NormalExport cat1 = null;
            foreach (var exp in y.Exports)
            {
                if (exp.ObjectFlags.HasFlag(EObjectFlags.RF_ClassDefaultObject))
                {
                    cat1 = exp as NormalExport;
                    break;
                }
            }    
            if (cat1 == null) return null;

            cat1.Data = new List<PropertyData>()
            {
                new StrPropertyData(new FName(y, "IntegratorVersion"))
                {
                    Value = new FString(IntegratorUtils.CurrentVersion.ToString(), Encoding.ASCII)
                },
                new BoolPropertyData(new FName(y, "RefuseMismatchedConnections"))
                {
                    Value = ParentIntegrator.RefuseMismatchedConnections
                }
            };

            return y;
        }
    }
}
