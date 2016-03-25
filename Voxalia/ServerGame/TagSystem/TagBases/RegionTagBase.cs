﻿using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class RegionTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base region[<RegionTag>]
        // @Group World
        // @ReturnType RegionTag
        // @Returns the region with the given name.
        // -->
        Server TheServer;

        public RegionTagBase(Server tserver)
        {
            Name = "region";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            string rname = data.GetModifier(0).ToLowerFast();
            foreach (Region r in TheServer.LoadedRegions)
            {
                if (r.Name.ToLowerFast() == rname)
                {
                    return new RegionTag(r).Handle(data.Shrink());
                }
            }
            data.Error("Invalid region '" + TagParser.Escape(rname) + "'!");
            return new NullTag();
        }
    }
}
