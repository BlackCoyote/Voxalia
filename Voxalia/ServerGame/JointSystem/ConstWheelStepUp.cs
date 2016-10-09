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
using Voxalia.ServerGame.EntitySystem;
using BEPUphysics.Constraints;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.JointSystem
{
    public class ConstWheelStepUp: BaseJoint
    {
        public double Height;

        public ConstWheelStepUp(PhysicsEntity ent, double height)
        {
            One = ent;
            Two = ent;
            Height = height;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new WheelStepUpConstraint(Ent1.Body, Ent1.TheRegion.Collision, Height);
        }
    }
}
