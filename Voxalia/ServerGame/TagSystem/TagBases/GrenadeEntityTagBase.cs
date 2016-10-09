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
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class GrenadeEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base grenade_entity[<GrenadeEntityTag>]
        // @Group Entities
        // @ReturnType GrenadeEntityTag
        // @Returns the grenade entity with the given entity ID.
        // -->
        Server TheServer;

        public GrenadeEntityTagBase(Server tserver)
        {
            Name = "grenade_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                Entity e = TheServer.GetEntity(eid);
                if (e != null && e is GrenadeEntity)
                {
                    return new GrenadeEntityTag((GrenadeEntity)e).Handle(data.Shrink());
                }
            }
            data.Error("Invalid grenade entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
