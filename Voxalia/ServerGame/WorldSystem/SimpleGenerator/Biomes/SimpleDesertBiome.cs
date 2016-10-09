//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleDesertBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Desert";
        }

        public override Material SurfaceBlock()
        {
            return Material.SAND;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SAND;
        }

        public override Material BaseBlock()
        {
            return base.BaseBlock();
        }
    }
}
