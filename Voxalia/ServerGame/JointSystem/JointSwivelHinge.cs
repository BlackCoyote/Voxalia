﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointSwivelHinge : BaseJoint
    {
        public JointSwivelHinge(PhysicsEntity e1, PhysicsEntity e2, Location hinge, Location twist)
        {
            Ent1 = e1;
            Ent2 = e2;
            WorldHinge = hinge;
            WorldTwist = twist;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new SwivelHingeAngularJoint(Ent1.Body, Ent2.Body, WorldHinge.ToBVector(), WorldTwist.ToBVector());
        }

        public Location WorldHinge;

        public Location WorldTwist;
    }
}