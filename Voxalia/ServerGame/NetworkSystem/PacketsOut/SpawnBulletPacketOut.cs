﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SpawnBulletPacketOut: AbstractPacketOut
    {
        public SpawnBulletPacketOut(BulletEntity e)
        {
            ID = 7;
            Data = new byte[8 + 4 + 12 + 12];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Utilities.FloatToBytes(e.Size).CopyTo(Data, 8);
            e.GetPosition().ToBytes().CopyTo(Data, 8 + 4);
            e.GetVelocity().ToBytes().CopyTo(Data, 8 + 4 + 12);
        }
    }
}