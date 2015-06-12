﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using Frenetic;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class CVarSetPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            DataReader dr = new DataReader(new DataStream(data));
            int cvarname_id = dr.ReadInt();
            string cvarvalue = dr.ReadFullString();
            string cvarname = TheClient.Network.Strings.StringForIndex(cvarname_id);
            CVar cvar = TheClient.CVars.system.Get(cvarname);
            if (!cvar.Flags.HasFlag(CVarFlag.ServerControl))
            {
                return false;
            }
            cvar.Set(cvarvalue, true);
            return true;
        }
    }
}