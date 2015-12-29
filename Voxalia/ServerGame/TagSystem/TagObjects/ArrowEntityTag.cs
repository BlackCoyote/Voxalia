﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class ArrowEntityTag : TemplateObject
    {
        // <--[object]
        // @Type ArrowEntityTag
        // @SubType PrimitiveEntityTag
        // @Group Entities
        // @Description Represents any ArrowEntity.
        // -->
        ArrowEntity Internal;

        public ArrowEntityTag(ArrowEntity ent)
        {
            Internal = ent;
        }

        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name ArrowEntityTag.damage
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the amount of damage the ArrowEntity will do, not counting velocity-based damage.
                // @Example "10" .damage could return "1".
                // -->
                case "damage":
                    return new NumberTag(Internal.Damage).Handle(data.Shrink());

                default:
                    return new PrimitiveEntityTag((PrimitiveEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}