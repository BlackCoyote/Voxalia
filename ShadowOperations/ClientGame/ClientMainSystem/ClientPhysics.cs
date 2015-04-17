﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;
using BEPUphysics;
using BEPUutilities;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        /// <summary>
        /// The physics world in which all physics-related activity takes place.
        /// </summary>
        public Space PhysicsWorld;

        public CollisionUtil Collision;

        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
            PhysicsWorld = new Space();
               // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new Vector3(0, 0, -9.8f);
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
        }

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
        }
    }
}
