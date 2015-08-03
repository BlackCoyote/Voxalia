﻿using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class TargetEntity: PrimitiveEntity, EntityTargettable
    {
        public TargetEntity(Region tworld)
            : base(tworld)
        {
            network = false;
            NetworkMe = false;
        }

        public string Targetname = "";

        public string GetTargetName()
        {
            return Targetname;
        }

        public abstract void Trigger(Entity ent, Entity user);

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "targetname":
                    Targetname = data;
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }
        
        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("targetname", Targetname));
            return vars;
        }
    }
}
