﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.OtherSystems;
using System.Drawing;
using Voxalia.ClientGame.JointSystem;
using System.Threading;
using Frenetic;
using Frenetic.TagHandlers.Common;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public List<ItemStack> Items = new List<ItemStack>();

        public int QuickBarPos = 0;

        public Object TickLock = new Object();

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
                return new ItemStack(this, "Air")
                {
                    DrawColor = Color.White.ToArgb(),
                    Tex = Textures.Clear,
                    Description = "An empty slot.",
                    Count = 0,
                    Datum = 0,
                    DisplayName = "Air"
                };
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public double opsat = 0;

        string SaveStr = null;

        public void OncePerSecondActions()
        {
            gFPS = gTicks;
            gTicks = 0;
            if (CVars.system.Modified)
            {
                CVars.system.Modified = false;
                StringBuilder cvarsave = new StringBuilder(CVars.system.CVarList.Count * 100);
                cvarsave.Append("// THIS FILE IS AUTOMATICALLY GENERATED.\n");
                cvarsave.Append("// This file is run very early in startup, be careful with it!\n");
                cvarsave.Append("debug minimal;\n");
                for (int i = 0; i < CVars.system.CVarList.Count; i++)
                {
                    if (!CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ServerControl)
                        && !CVars.system.CVarList[i].Flags.HasFlag(CVarFlag.ReadOnly))
                    {
                        string val = CVars.system.CVarList[i].Value;
                        if (val.Contains('\"'))
                        {
                            val = "<{unescape[" + EscapeTags.Escape(val) + "]}>";
                        }
                        cvarsave.Append("set \"" + CVars.system.CVarList[i].Name + "\" \"" + val + "\";\n");
                    }
                }
                SaveStr = cvarsave.ToString();
                Thread thread = new Thread(new ThreadStart(SaveCFG));
                thread.Start();
            }
        }

        public void SaveCFG()
        {
            try
            {
                Program.Files.WriteText("clientdefaultsettings.cfg", SaveStr);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving settings: " + ex.ToString());
            }
        }

        public double GlobalTickTimeLocal = 0;

        public double Delta;

        void ScheduleCheck()
        {
            for (int i = 0; i < RunImmediately.Count; i++)
            {
                RunImmediately[i].RunSynchronously();
            }
            RunImmediately.Clear();
        }

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            lock (TickLock)
            {
                Delta = e.Time * CVars.g_timescale.ValueD;
                GlobalTickTimeLocal += Delta;
                try
                {
                    opsat += Delta;
                    if (opsat >= 1)
                    {
                        opsat -= 1;
                        OncePerSecondActions();
                    }
                    ScheduleCheck();
                    Textures.Update(GlobalTickTimeLocal);
                    Shaders.Update(GlobalTickTimeLocal);
                    Models.Update(GlobalTickTimeLocal);
                    KeyHandler.Tick();
                    MouseHandler.Tick();
                    UIConsole.Tick();
                    Commands.Tick();
                    Network.Tick();
                    TickWorld(Delta);
                    Sounds.Update(CameraPos, CameraTarget - CameraPos, CameraUp, Player.GetVelocity(), Window.Focused);
                    ScheduleCheck();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Ticking: " + ex.ToString());
                }
                PlayerEyePosition = Player.GetEyePosition();
                CameraFinalTarget = PlayerEyePosition + Player.ForwardVector() * 100f;
                CollisionResult cr = TheWorld.Collision.RayTrace(PlayerEyePosition, CameraFinalTarget, IgnorePlayer);
                CameraFinalTarget = cr.Position;
                CameraImpactNormal = cr.Hit ? cr.Normal.Normalize() : Location.Zero;
                CameraDistance = (PlayerEyePosition - CameraFinalTarget).Length();
            }
        }

        public Location PlayerEyePosition;

        public Location CameraImpactNormal;

        bool IgnorePlayer(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            if (entry is BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable
                && ((BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)entry).Entity.Tag == Player)
            {
                return false;
            }
            return TheWorld.Collision.ShouldCollide(entry);
        }

        public Location CameraFinalTarget;

        public double CameraDistance;

        public void ResetWorld()
        {
            Items.Clear();
            QuickBarPos = 0;
            BuildWorld();
            TheWorld.SpawnEntity(Player);
        }
    }
}