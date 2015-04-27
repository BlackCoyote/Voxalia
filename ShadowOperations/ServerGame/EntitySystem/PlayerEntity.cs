﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using ShadowOperations.ServerGame.ItemSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class PlayerEntity: EntityLiving
    {
        public Location HalfSize = new Location(0.5f, 0.5f, 1f);

        public Connection Network;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public bool Upward = false;
        public bool Downward = false;
        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;

        public bool Click = false;
        public bool AltClick = false;

        bool pkick = false;

        public List<ItemStack> Items = new List<ItemStack>();

        public int cItem = 0;

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted</param>
        /// <returns>A valid item</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            while (slot > Items.Count)
            {
                slot -= Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack("Air", TheServer, 1, "clear", "Air", "An empty slot.", Color.White.ToArgb());
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public void Kick(string message)
        {
            if (pkick)
            {
                return;
            }
            pkick = true;
            Network.SendMessage("Kicking you: " + message);
            // TODO: Broadcast kick message
            SysConsole.Output(OutputType.INFO, "Kicking " + this + ": " + message);
            if (Network.Alive)
            {
                Network.PrimarySocket.Close(5);
            }
            TheServer.DespawnEntity(this);
        }

        public Location Direction;

        bool pup = false;

        public PhysicsEntity Grabbed = null;

        public float GrabForce = 0;

        public PlayerEntity(Server tserver, Connection conn)
            : base(tserver, true, 100f)
        {
            Network = conn;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            GiveItem(new ItemStack("open_hand", TheServer, 1, "items/open_hand", "Open Hand", "Grab things!", Color.White.ToArgb()));
            GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/9mm_pistol_gun", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb()));
        }

        public void GiveItem(ItemStack item)
        {
            // TODO: stacking
            item.Info.PrepItem(this, item);
            Items.Add(item);
            Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, item));
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            return ((EntityCollidable)entry).Entity.Tag != this;
        }

        public override void Tick()
        {
            while (Direction.X < 0)
            {
                Direction.X += 360;
            }
            while (Direction.X > 360)
            {
                Direction.X -= 360;
            }
            if (Direction.Y > 89.9f)
            {
                Direction.Y = 89.9f;
            }
            if (Direction.Y < -89.9f)
            {
                Direction.Y = -89.9f;
            }
            bool fly = false;
            bool on_ground = TheServer.Collision.CuboidLineTrace(new Location(0.2f, 0.2f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis).Hit;
            if (Upward && !fly && !pup && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), (Location.UnitZ * 500f).ToBVector());
                Body.ActivityInformation.Activate();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            Location movement = new Location(0, 0, 0);
            if (Leftward)
            {
                movement.Y = -1;
            }
            if (Rightward)
            {
                movement.Y = 1;
            }
            if (Backward)
            {
                movement.X = 1;
            }
            if (Forward)
            {
                movement.X = -1;
            }
            bool Slow = false;
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.X * Utilities.PI180, fly ? Direction.Y * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > 4 * MoveSpeed * MoveSpeed)
            {
                pvel = pvel.Normalize() * 2 * MoveSpeed;
            }
            pvel *= MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            if (!fly && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            if (Grabbed != null)
            {
                if (Grabbed.IsSpawned && (Grabbed.GetPosition() - GetPosition()).LengthSquared() < 5 * 5 + Grabbed.Widest * Grabbed.Widest)
                {
                    Location pos = GetPosition() + new Location(0, 0, HalfSize.Z * 1.6f) + Utilities.ForwardVector_Deg(Direction.X, Direction.Y) * (2 + Grabbed.Widest);
                    if (GrabForce >= Grabbed.GetMass())
                    {
                        Grabbed.Body.LinearVelocity = new Vector3(0, 0, 0);
                    }
                    Location tvec = (pos - Grabbed.GetPosition());
                    double len = tvec.Length();
                    if (len == 0)
                    {
                        len = 1;
                    }
                    Vector3 push = ((-Grabbed.GetVelocity()).Normalize() * GrabForce + (tvec / len) * GrabForce).ToBVector() * Grabbed.Body.InverseMass;
                    if (push.LengthSquared() > len * len)
                    {
                        push /= (float)(push.Length() / len) / 10f;
                    }
                    Grabbed.Body.LinearVelocity += push;
                    Grabbed.Body.ActivityInformation.Activate();
                }
                else
                {
                    Grabbed = null;
                }
            }
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
            }
            ItemStack cit = GetItemForSlot(cItem);
            if (Click)
            {
                cit.Info.Click(this, cit);
            }
            if (AltClick)
            {
                cit.Info.AltClick(this, cit);
            }
        }

        public bool pclick = false;

        public float MoveSpeed = 10;

        public override Location GetAngles()
        {
            return Direction;
        }

        public override void SetAngles(Location rot)
        {
            Direction = rot;
        }

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.6f);
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
