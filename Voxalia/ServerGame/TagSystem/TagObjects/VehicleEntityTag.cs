//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class VehicleEntityTag : TemplateObject
    {
        // <--[object]
        // @Type VehicleEntityTag
        // @SubType ModelEntityTag
        // @Group Entities
        // @Description Represents any VehicleEntity.
        // -->
        VehicleEntity Internal;

        public VehicleEntityTag(VehicleEntity ent)
        {
            Internal = ent;
        }

        public override TemplateObject Handle(TagData data)
        {
            if (data.Remaining == 0)
            {
                return this;
            }
            switch (data[0])
            {
                // <--[tag]
                // @Name VehicleEntityTag.has_wheels
                // @Group General Information
                // @ReturnType BooleanTag
                // @Returns whether the VehicleEntity has wheels.
                // @Example "5" .has_wheels could return "true".
                // -->
                case "has_wheels":
                    return new BooleanTag(Internal.hasWheels).Handle(data.Shrink());

                default:
                    return new ModelEntityTag((ModelEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
