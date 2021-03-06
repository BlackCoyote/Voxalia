//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
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
    class LivingEntityTag : TemplateObject
    {
        // <--[object]
        // @Type LivingEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any LivingEntity.
        // -->
        LivingEntity Internal;

        public LivingEntityTag(LivingEntity ent)
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
                // @Name LivingEntityTag.health
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's health.
                // @Example "5" .health could return "100".
                // -->
                case "health":
                    return new NumberTag(Internal.Health).Handle(data.Shrink());
                // <--[tag]
                // @Name LivingEntityTag.max_health
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's maximum health.
                // @Example "5" .max_health could return "100".
                // -->
                case "max_health":
                    return new NumberTag(Internal.MaxHealth).Handle(data.Shrink());
                // <--[tag]
                // @Name LivingEntityTag.health_percentage
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's health as a percentage of its maximum health.
                // @Example "5" .health_percentage could return "70" - indicating 70%.
                // -->
                case "health_percentage":
                    return new NumberTag((Internal.MaxHealth / Internal.Health) * 100).Handle(data.Shrink());

                default:
                    return new PhysicsEntityTag((PhysicsEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            if (Internal is PlayerEntity)
            {
                return ((PlayerEntity)Internal).Name;
            }
            else
            {
                return Internal.EID.ToString();
            }
        }
    }
}
