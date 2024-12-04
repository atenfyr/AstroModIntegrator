using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroModIntegrator
{
    // This source code is adapted from astro_mod_loader
    // https://github.com/AstroTechies/astro_modloader/blob/87145c9d5e5ba2914e576012820d662558619372/astro_mod_integrator/src/handlers/biome_placement_modifiers.rs#L35
    // while astro_mod_loader is unlicensed, both pieces of software are products of AstroTechies, which holds all copyright to the software
    // (in the same vein as how astro_mod_loader is not required to follow any license agreement for AstroModLoader as it is a product of the same organization)

    public enum BiomeType
    {
        Surface,
        Crust,
    }

    public struct PlacementModifier : ICloneable
    {
        [JsonProperty("planet_type")]
        public string PlanetType;

        [JsonProperty("biome_type")]
        public BiomeType BiomeType;

        [JsonProperty("biome_name")]
        public string BiomeName;

        [JsonProperty("layer_name")]
        public string LayerName;

        [JsonProperty("placements")]
        public List<string> Placements;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class BiomePlacementModifiersBaker
    {
    }
}
