﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.NetworkSystem;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlayerEntity: EntityLiving
    {
        public Location HalfSize = new Location(0.55f, 0.55f, 1.3f);

        public Connection Network;

        public Connection ChunkNetwork;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public byte LastCPingByte = 0;

        public bool Upward = false;
        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Walk = false;

        public bool Click = false;
        public bool AltClick = false;

        bool pkick = false;

        public bool FlashLightOn = false;

        public List<ItemStack> Items = new List<ItemStack>();

        public int cItem = 0;

        public SingleAnimation hAnim = null;
        public SingleAnimation tAnim = null;
        public SingleAnimation lAnim = null;

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
                return new ItemStack("Air", TheServer, 1, "clear", "Air", "An empty slot.", Color.White.ToArgb(), "blank.dae", true);
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public int GetSlotForItem(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == item)
                {
                    return i + 1;
                }
            }
            return -1;
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
            if (IsSpawned)
            {
                ItemStack it = GetItemForSlot(cItem);
                it.Info.SwitchFrom(this, it);
                HookItem.RemoveHook(this);
                TheWorld.DespawnEntity(this);
            }
        }

        public Location Direction;

        public CubeEntity CursorMarker = null;

        bool pup = false;

        public JointBallSocket GrabJoint = null;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public bool WaitingForClickRelease = false;

        public PlayerEntity(World tworld, Connection conn)
            : base(tworld, true, 100f)
        {
            Network = conn;
            SetMass(100);
            Shape = new BoxShape((float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White.ToArgb(), "items/common/hand.dae", true));
            GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White.ToArgb(), "items/common/fist.dae", true));
            GiveItem(new ItemStack("block", TheServer, 10, "blocks/solid/grass_side", "Grass", "Grassy!", Color.White.ToArgb(), "items/block.dae", false) { Datum = 2 });
            GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/weapons/9mm_pistol_ico", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb(), "items/weapons/silenced_pistol.dae", false));
            GiveItem(new ItemStack("shotgun_gun", TheServer, 1, "items/weapons/shotgun_ico", "Shotgun", "It shoots many bullets!", Color.White.ToArgb(), "items/weapons/shotgun.dae", false));
            GiveItem(new ItemStack("bow", TheServer, 1, "items/weapons/bow_ico", "Bow", "It shoots arrows!", Color.White.ToArgb(), "items/weapons/bow.dae", false));
            GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White.ToArgb(), "items/common/hook.dae", true));
            GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            GiveItem(new ItemStack("rifle_gun", TheServer, 1, "items/weapons/rifle_ico", "Assault Rifle", "It shoots rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/m4a1.dae", false));
            GiveItem(new ItemStack("minigun_gun", TheServer, 1, "items/weapons/minigun_ico", "Minigun", "It shoots ^ivery^r rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/minigun.dae", false));
            GiveItem(new ItemStack("bullet", "9mm_ammo", TheServer, 100, "items/weapons/ammo/9mm_round_ico", "9mm Ammo", "Nine whole millimeters!", Color.White.ToArgb(), "items/weapons/ammo/9mm_round.dae", false));
            GiveItem(new ItemStack("bullet", "shotgun_ammo", TheServer, 100, "items/weapons/ammo/shotgun_shell_ico", "Shotgun Ammo", "Always travels in packs!", Color.White.ToArgb(), "items/weapons/ammo/shotgun_shell.dae", false));
            GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 100, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", Color.White.ToArgb(), "items/weapons/ammo/rifle_round.dae", false));
            GiveItem(new ItemStack("bullet", "minigun_ammo", TheServer, 2000, "items/weapons/ammo/minigun_round_ico", "Minigun Ammo", "Very very rapid!", Color.White.ToArgb(), "items/weapons/ammo/minigun_round.dae", false));
            SetHealth(Health);
            CGroup = CollisionUtil.Player;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            if (CursorMarker == null)
            {
                CursorMarker = new CubeEntity(new Location(0.01, 0.01, 0.01), TheWorld, 0);
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                TheWorld.SpawnEntity(CursorMarker);
            }
        }

        public override void DestroyBody()
        {
            base.DestroyBody();
            if (CursorMarker.IsSpawned)
            {
                TheWorld.DespawnEntity(CursorMarker);
                CursorMarker = null;
            }
        }

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                if (hAnim != null && hAnim.Name == anim)
                {
                    return;
                }
                hAnim = TheServer.Animations.GetAnimation(anim);
            }
            else if (mode == 1)
            {
                if (tAnim != null && tAnim.Name == anim)
                {
                    return;
                }
                tAnim = TheServer.Animations.GetAnimation(anim);
            }
            else
            {
                if (lAnim != null && lAnim.Name == anim)
                {
                    return;
                }
                lAnim = TheServer.Animations.GetAnimation(anim);
            }
            TheWorld.SendToAll(new AnimationPacketOut(this, anim, mode));
        }

        public void GiveItem(ItemStack item)
        {
            // TODO: stacking
            item.Info.PrepItem(this, item);
            Items.Add(item);
            Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, item));
        }

        public void RemoveItem(int item)
        {
            while (item < 0)
            {
                item += Items.Count + 1;
            }
            while (item > Items.Count)
            {
                item -= Items.Count + 1;
            }
            ItemStack its = GetItemForSlot(item);
            if (item == cItem) // TODO: ensure cItem is wrapped
            {
                its.Info.SwitchFrom(this, its);
            }
            Items.RemoveAt(item - 1);
            Network.SendPacket(new RemoveItemPacketOut(item - 1));
            if (item <= cItem)
            {
                cItem--;
                Network.SendPacket(new SetHeldItemPacketOut(cItem));
            }
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheWorld.Collision.ShouldCollide(entry);
        }

        public bool IgnorePlayers(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Player)
            {
                return false;
            }
            return TheWorld.Collision.ShouldCollide(entry);
        }

        public override void Tick()
        {
            if (!IsSpawned)
            {
                return;
            }
            while (Direction.Yaw < 0)
            {
                Direction.Yaw += 360;
            }
            while (Direction.Yaw > 360)
            {
                Direction.Yaw -= 360;
            }
            if (Direction.Pitch > 89.9f)
            {
                Direction.Pitch = 89.9f;
            }
            if (Direction.Pitch < -89.9f)
            {
                Direction.Pitch = -89.9f;
            }
            bool fly = false;
            CollisionResult crGround = TheWorld.Collision.CuboidLineTrace(new Location(HalfSize.X - 0.01f, HalfSize.Y - 0.01f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnorePlayers);
            if (Upward && !fly && !pup && crGround.Hit && GetVelocity().Z < 1f)
            {
                Vector3 imp = (Location.UnitZ * GetMass() * 7f).ToBVector();
                Body.ApplyLinearImpulse(ref imp);
                Body.ActivityInformation.Activate();
                imp = -imp;
                if (crGround.HitEnt != null)
                {
                    crGround.HitEnt.ApplyLinearImpulse(ref imp);
                    crGround.HitEnt.ActivityInformation.Activate();
                }
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
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.Yaw * Utilities.PI180, fly ? Direction.Pitch * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Walk ? 0.7f : 1f);
            if (Stance == PlayerStance.CROUCH)
            {
                intent_vel *= 0.5f;
            }
            else if (Stance == PlayerStance.CRAWL)
            {
                intent_vel *= 0.3f;
            }
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > 4 * MoveSpeed * MoveSpeed)
            {
                pvel = pvel.Normalize() * 2 * MoveSpeed;
            }
            pvel *= MoveSpeed * (Walk ? 0.7f : 1f);
            if (!fly)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0) * (crGround.Hit ? 1f : 0.1f));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            CursorMarker.SetPosition(GetPosition() + ForwardVector() * 0.5f);
            CursorMarker.SetOrientation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(Direction.Pitch * Utilities.PI180)) * // TODO: is the pitch really needed for this?
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(Direction.Yaw * Utilities.PI180)));
            base.SetOrientation(Quaternion.Identity);
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
            }
            if (GetVelocity().LengthSquared() > 1)
            {
                SetAnimation("human/" + StanceName() +  "/walk_lowquality", 1);
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 2);
            }
            else
            {
                SetAnimation("human/" + StanceName() + "/idle01", 1);
                SetAnimation("human/" + StanceName() + "/idle01", 2);
            }
            ItemStack cit = GetItemForSlot(cItem);
            if (Click)
            {
                cit.Info.Click(this, cit);
                LastClick = TheWorld.GlobalTickTime;
                WasClicking = true;
            }
            else if (WasClicking)
            {
                cit.Info.ReleaseClick(this, cit);
                WasClicking = false;
            }
            if (AltClick)
            {
                cit.Info.AltClick(this, cit);
                LastAltClick = TheWorld.GlobalTickTime;
            }
            cit.Info.Tick(this, cit);
            // TODO: Better system
            Location pos = GetPosition();
            TryChunk(pos);
            for (int x = -2; x < 3; x++)
            {
                for (int y = -2; y < 3; y++)
                {
                    for (int z = -2; z < 3; z++)
                    {
                        TryChunk(pos + new Location(30 * x, 30 * y, 30 * z));
                    }
                }
            }
            base.Tick();
        }

        public void TryChunk(Location worldPos)
        {
            worldPos = TheWorld.ChunkLocFor(worldPos);
            if (!ChunksAwareOf.Contains(worldPos))
            {
                Chunk chk = TheWorld.LoadChunk(worldPos);
                ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk));
                ChunksAwareOf.Add(worldPos);
            }
        }

        public HashSet<Location> ChunksAwareOf = new HashSet<Location>();

        public double LastClick = 0;

        public double LastGunShot = 0;

        public bool WasClicking = false;

        public double LastAltClick = 0;

        public float MoveSpeed = 10;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public Location GetEyePosition()
        {
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("head");
                Matrix m4 = head.GetBoneTotalMatrix(0);
                m4.Transpose();
                return GetPosition() + Location.FromBVector(m4.Translation);
            }
            else
            {
                return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8f);
            }
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override string ToString()
        {
            return Name;
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

        public YourStatusFlags Flags = YourStatusFlags.NONE;

        public override void SetHealth(float health)
        {
            base.SetHealth(health);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth(), Flags));
        }

        public override void SetMaxHealth(float maxhealth)
        {
            base.SetMaxHealth(maxhealth);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth(), Flags));
        }

        public override void Die()
        {
            SetHealth(MaxHealth);
            if (TheWorld.SpawnPoints.Count == 0)
            {
                SysConsole.Output(OutputType.WARNING, "No spawn points... generating one!");
                TheWorld.SpawnEntity(new SpawnPointEntity(TheWorld));
            }
            SpawnPointEntity spe = null;
            for (int i = 0; i < 10; i++)
            {
                spe = TheWorld.SpawnPoints[Utilities.UtilRandom.Next(TheWorld.SpawnPoints.Count)];
                if (!TheWorld.Collision.CuboidLineTrace(HalfSize, spe.GetPosition(), spe.GetPosition() + new Location(0, 0, 0.01f)).Hit)
                {
                    break;
                }
            }
            SetPosition(spe.GetPosition());
        }

        public PlayerStance Stance = PlayerStance.STAND;

        public string StanceName()
        {
            return Stance.ToString().ToLower();
        }
    }
}