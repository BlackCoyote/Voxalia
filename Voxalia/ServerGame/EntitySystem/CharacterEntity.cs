﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using BEPUphysics.Character;
using BEPUutilities;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class CharacterEntity: LivingEntity
    {
        public CharacterEntity(Region tregion, float maxhealth)
            : base(tregion, maxhealth)
        {
        }

        public bool Upward = false;

        public bool Forward = false;

        public bool Backward = false;

        public bool Leftward = false;

        public bool Rightward = false;

        public bool Walk = false;

        public bool Sprint = false;

        public bool Downward = false;

        public bool Click = false;

        public bool AltClick = false;

        public bool Use = false;

        /// <summary>
        /// The direction the character is currently facing, as Yaw/Pitch.
        /// </summary>
        public Location Direction;

        public ModelEntity CursorMarker = null;

        public bool pup = false;

        public JointBallSocket GrabJoint = null;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public double ItemStartClickTime = -1;

        public bool WaitingForClickRelease = false;

        public bool FlashLightOn = false;

        /// <summary>
        /// Returns whether this character is currently crouching.
        /// </summary>
        public bool IsCrouching
        {
            get
            {
                return Downward || DesiredStance == Stance.Crouching;
            }
        }

        /// <summary>
        /// The animation of the character's head.
        /// </summary>
        public SingleAnimation hAnim = null;

        /// <summary>
        /// The animation of the character's torso.
        /// </summary>
        public SingleAnimation tAnim = null;

        /// <summary>
        /// The animation of the character's legs.
        /// </summary>
        public SingleAnimation lAnim = null;

        /// <summary>
        /// The internal physics character body.
        /// </summary>
        public CharacterController CBody;

        public override void SpawnBody()
        {
            if (CBody != null)
            {
                DestroyBody();
            }
            // TODO: Better variable control! (Server should command every detail!)
            CBody = new CharacterController(WorldTransform.Translation, CBHHeight * 2f, CBHHeight * 1.1f,
                CBHHeight * 1f, CBRadius, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
                CBTractionForce * Mass, CBSlideSpeed, CBSlideForce * Mass, CBAirSpeed, CBAirForce * Mass, CBJumpSpeed, CBSlideJumpSpeed, CBGlueForce * Mass);
            CBody.StanceManager.DesiredStance = Stance.Standing;
            CBody.ViewDirection = new Vector3(1f, 0f, 0f);
            CBody.Down = new Vector3(0f, 0f, -1f);
            CBody.Tag = this;
            Body = CBody.Body;
            Body.Tag = this;
            Body.AngularDamping = 1.0f;
            Shape = CBody.Body.CollisionInformation.Shape;
            ConvexEntityShape = CBody.Body.CollisionInformation.Shape;
            Body.CollisionInformation.CollisionRules.Group = CollisionUtil.Player;
            CBody.StepManager.MaximumStepHeight = CBStepHeight;
            CBody.StepManager.MinimumDownStepHeight = CBDownStepHeight;
            TheRegion.PhysicsWorld.Add(CBody);
            if (CursorMarker == null)
            {
                CursorMarker = new ModelEntity("cube", TheRegion);
                CursorMarker.scale = new Location(0.1f, 0.1f, 0.1f);
                CursorMarker.mode = ModelCollisionMode.AABB;
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                CursorMarker.CanSave = false;
                TheRegion.SpawnEntity(CursorMarker);
            }
        }

        public float CBHHeight = 1.3f;

        public float CBProneSpeed = 1f;

        public float CBMargin = 0.01f;

        public float CBStepHeight = 0.6f;

        public float CBDownStepHeight = 0.6f;

        public float CBRadius = 0.75f;

        public float CBMaxTractionSlope = 1.0f;

        public float CBMaxSupportSlope = 1.3f;

        public float CBStandSpeed = 5.0f;

        public float CBCrouchSpeed = 2.5f;

        public float CBSlideSpeed = 3f;

        public float CBAirSpeed = 1f;

        public float CBTractionForce = 100f;

        public float CBSlideForce = 70f;

        public float CBAirForce = 50f;

        public float CBJumpSpeed = 8f;

        public float CBSlideJumpSpeed = 3.5f;

        public float CBGlueForce = 500f;

        public override void DestroyBody()
        {
            if (CBody == null)
            {
                return;
            }
            TheRegion.PhysicsWorld.Remove(CBody);
            CBody = null;
            Body = null;
            if (CursorMarker.IsSpawned && !CursorMarker.Removed)
            {
                CursorMarker.RemoveMe();
                CursorMarker = null;
            }
        }

        public float ItemSpeedMod = 1;
        public bool ItemDoSpeedMod = false;

        public bool IsFlying = false;
        public float PreFlyMass = 0;

        public abstract void Fly();

        public abstract void Unfly();

        public Stance DesiredStance = Stance.Standing;

        public EntityUseable UsedNow = null;
        public double LastClick = 0;

        public double LastGunShot = 0;

        public double LastBlockBreak = 0;

        public double LastBlockPlace = 0;

        public bool WasClicking = false;

        public double LastAltClick = 0;

        public bool WasAltClicking = false;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public Location GetEyePosition()
        {
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("special06.r");
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>();
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)(Direction.Pitch / 1.75f * Utilities.PI180)));
                adjs["spine05"] = rotforw;
                Matrix m4 = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 270) * Utilities.PI180) % 360f))
                    * head.GetBoneTotalMatrix(0, adjs) * (rotforw * Matrix.CreateTranslation(new Vector3(0, 0, 0.2f)));
                m4.Transpose();
                return GetPosition() + new Location(m4.Translation) * 1.5f;// TODO: Match clientside ray trace?
            }
            else
            {
                return GetPosition() + new Location(0, 0, CBHHeight * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8 : 1.5));
            }
        }

        public override Location GetPosition()
        {
            if (Body != null)
            {
                RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
                BoundingBox box;
                Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
                return base.GetPosition() + new Location(0, 0, box.Min.Z);
            }
            return base.GetPosition() - new Location(0, 0, CBHHeight);
        }

        public override void SetPosition(Location pos)
        {
            if (Body != null)
            {
                RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
                BoundingBox box;
                Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
                base.SetPosition(pos + new Location(0, 0, -box.Min.Z));
            }
            else
            {
                base.SetPosition(pos + new Location(0, 0, CBHHeight));
            }
        }

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override Quaternion GetOrientation()
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Direction.Pitch)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Direction.Yaw);
        }

        public override void SetOrientation(Quaternion rot)
        {
            Matrix trot = Matrix.CreateFromQuaternion(rot);
            Location ang = Utilities.MatrixToAngles(trot);
            Direction.Yaw = ang.Yaw;
            Direction.Pitch = ang.Pitch;
        }
        
        public Location BlockBreakTarget;

        public double BlockBreakStarted = 0;
    }
}