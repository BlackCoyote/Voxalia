﻿using System;
using System.Text;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.EntitySystem;
using BEPUutilities.Threading;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using System.Diagnostics;
using Priority_Queue;
using FreneticScript;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.OtherSystems;
using System.Threading.Tasks;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Region
    {
        /// <summary>
        /// The physics world in which all physics-related activity takes place.
        /// 
        /// </summary>
        public Space PhysicsWorld;

        public CollisionUtil Collision;

        public double Delta;

        public Location GravityNormal = new Location(0, 0, -1);

        public List<Entity> Entities = new List<Entity>();

        public List<Entity> Tickers = new List<Entity>();

        public List<Entity> ShadowCasters = new List<Entity>();

        public PhysicsEntity[] GenShadowCasters = new PhysicsEntity[0];
        
        public AABB[] Highlights = new AABB[0];

        /// <summary>
        /// Builds the physics world.
        /// </summary>
        public void BuildWorld()
        {
            ParallelLooper pl = new ParallelLooper();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                pl.AddThread();
            }
            CollisionDetectionSettings.AllowedPenetration = 0.01f;
            PhysicsWorld = new Space(pl);
            PhysicsWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            // Set the world's general default gravity
            PhysicsWorld.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, 0, -9.8f * 3f / 2f);
            PhysicsWorld.DuringForcesUpdateables.Add(new LiquidVolume(this));
            // Load a CollisionUtil instance
            Collision = new CollisionUtil(PhysicsWorld);
        }
        public void AddChunk(FullChunkObject mesh)
        {
            PhysicsWorld.Add(mesh);
        }

        public void RemoveChunkQuiet(FullChunkObject mesh)
        {
            PhysicsWorld.Remove(mesh);
        }

        public bool SpecialCaseRayTrace(Location start, Location dir, float len, MaterialSolidity considerSolid, Func<BroadPhaseEntry, bool> filter, out RayCastResult rayHit)
        {
            Ray ray = new Ray(start.ToBVector(), dir.ToBVector());
            RayCastResult best = new RayCastResult(new RayHit() { T = len }, null);
            bool hA = false;
            if (considerSolid.HasFlag(MaterialSolidity.FULLSOLID))
            {
                RayCastResult rcr;
                if (PhysicsWorld.RayCast(ray, len, filter, out rcr))
                {
                    best = rcr;
                    hA = true;
                }
            }
            AABB box = new AABB();
            box.Min = start;
            box.Max = start;
            box.Include(start + dir * len);
            foreach (KeyValuePair<Vector3i, Chunk> chunk in LoadedChunks)
            {
                if (chunk.Value == null || chunk.Value.FCO == null)
                {
                    continue;
                }
                if (!box.Intersects(new AABB() { Min = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, Max = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE) }))
                {
                    continue;
                }
                RayHit temp;
                if (chunk.Value.FCO.RayCast(ray, len, null, considerSolid, out temp))
                {
                    hA = true;
                    //temp.T *= len;
                    if (temp.T < best.HitData.T)
                    {
                        best.HitData = temp;
                        best.HitObject = chunk.Value.FCO;
                    }
                }
            }
            rayHit = best;
            return hA;
        }

        public bool SpecialCaseConvexTrace(ConvexShape shape, Location start, Location dir, float len, MaterialSolidity considerSolid, Func<BroadPhaseEntry, bool> filter, out RayCastResult rayHit)
        {
            RigidTransform rt = new RigidTransform(start.ToBVector(), BEPUutilities.Quaternion.Identity);
            BEPUutilities.Vector3 sweep = (dir * len).ToBVector();
            RayCastResult best = new RayCastResult(new RayHit() { T = len }, null);
            bool hA = false;
            if (considerSolid.HasFlag(MaterialSolidity.FULLSOLID))
            {
                RayCastResult rcr;
                if (PhysicsWorld.ConvexCast(shape, ref rt, ref sweep, filter, out rcr))
                {
                    best = rcr;
                    hA = true;
                }
            }
            sweep = dir.ToBVector();
            AABB box = new AABB();
            box.Min = start;
            box.Max = start;
            box.Include(start + dir * len);
            foreach (KeyValuePair<Vector3i, Chunk> chunk in LoadedChunks)
            {
                if (chunk.Value == null || chunk.Value.FCO == null)
                {
                    continue;
                }
                if (!box.Intersects(new AABB() { Min = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, Max = chunk.Value.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE) }))
                {
                    continue;
                }
                RayHit temp;
                if (chunk.Value.FCO.ConvexCast(shape, ref rt, ref sweep, len, considerSolid, out temp))
                {
                    hA = true;
                    //temp.T *= len;
                    if (temp.T < best.HitData.T)
                    {
                        best.HitData = temp;
                        best.HitObject = chunk.Value.FCO;
                    }
                }
            }
            rayHit = best;
            return hA;
        }

        public double PhysTime;

        /// <summary>
        /// Ticks the physics world.
        /// </summary>
        public void TickWorld(double delta)
        {
            Delta = delta;
            if (Delta <= 0)
            {
                return;
            }
            GlobalTickTimeLocal += Delta;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
            sw.Stop();
            PhysTime = (double)sw.ElapsedMilliseconds / 1000f;
            for (int i = 0; i < Tickers.Count; i++)
            {
                Tickers[i].Tick();
            }
            SolveJoints();
            TickClouds();
            CheckForRenderNeed();
        }

        public void SolveJoints()
        {
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i].Enabled && Joints[i] is BaseFJoint)
                {
                    ((BaseFJoint)Joints[i]).Solve();
                }
            }
        }

        /// <summary>
        /// Spawns an entity in the world.
        /// </summary>
        /// <param name="e">The entity to spawn.</param>
        public void SpawnEntity(Entity e)
        {
            Entities.Add(e);
            if (e.Ticks)
            {
                Tickers.Add(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Add(e);
            }
            if (e is PhysicsEntity)
            {
                PhysicsEntity pe = e as PhysicsEntity;
                pe.SpawnBody();
                if (pe.GenBlockShadows)
                {
                    // TODO: Effic?
                    PhysicsEntity[] neo = new PhysicsEntity[GenShadowCasters.Length + 1];
                    Array.Copy(GenShadowCasters, neo, GenShadowCasters.Length);
                    neo[neo.Length - 1] = pe;
                    GenShadowCasters = neo;
                }
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Spawn();
            }
        }

        public void Despawn(Entity e)
        {
            Entities.Remove(e);
            if (e.Ticks)
            {
                Tickers.Remove(e);
            }
            if (e.CastShadows)
            {
                ShadowCasters.Remove(e);
            }
            if (e is PhysicsEntity)
            {
                PhysicsEntity pe = e as PhysicsEntity;
                pe.DestroyBody();
                for (int i = 0; i < pe.Joints.Count; i++)
                {
                    DestroyJoint(pe.Joints[i]);
                }
                if (pe.GenBlockShadows)
                {
                    PhysicsEntity[] neo = new PhysicsEntity[GenShadowCasters.Length - 1];
                    int x = 0;
                    bool valid = true;
                    for (int i = 0; i < GenShadowCasters.Length; i++)
                    {
                        if (GenShadowCasters[i] != pe)
                        {
                            neo[x++] = GenShadowCasters[i];
                            if (x == GenShadowCasters.Length)
                            {
                                valid = false;
                                return;
                            }
                        }
                    }
                    if (valid)
                    {
                        GenShadowCasters = neo;
                    }
                }
            }
            else if (e is PrimitiveEntity)
            {
                ((PrimitiveEntity)e).Destroy();
            }
        }

        public InternalBaseJoint GetJoint(long JID)
        {
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i].JID == JID)
                {
                    return Joints[i];
                }
            }
            return null;
        }

        public Entity GetEntity(long EID)
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i].EID == EID)
                {
                    return Entities[i];
                }
            }
            return null;
        }

        public Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

        public Client TheClient;

        public Vector3i ChunkLocFor(Location pos)
        {
            Vector3i temp;
            temp.X = (int)Math.Floor(pos.X / Chunk.CHUNK_SIZE);
            temp.Y = (int)Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
            temp.Z = (int)Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
            return temp;
        }

        public Chunk LoadChunk(Vector3i pos, int posMult)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                // TODO: ?!?!?!?
                if (chunk.PosMultiplier != posMult)
                {
                    Chunk ch = chunk;
                    LoadedChunks.Remove(pos);
                    chunk = new Chunk(posMult);
                    chunk.OwningRegion = this;
                    chunk.adding = ch.adding;
                    chunk.rendering = ch.rendering;
                    chunk._VBO = ch._VBO;
                    chunk.WorldPosition = pos;
                    LoadedChunks.Add(pos, chunk);
                }
            }
            else
            {
                chunk = new Chunk(posMult);
                chunk.OwningRegion = this;
                chunk.WorldPosition = pos;
                LoadedChunks.Add(pos, chunk);
            }
            return chunk;
        }

        public Chunk GetChunk(Vector3i pos)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                return chunk;
            }
            return null;
        }

        public Material GetBlockMaterial(Location pos)
        {
            return (Material)GetBlockInternal(pos).BlockMaterial;
        }

        public BlockInternal GetBlockInternal(Location pos)
        {
            Chunk ch = GetChunk(ChunkLocFor(pos));
            if (ch == null)
            {
                return new BlockInternal((ushort)Material.AIR, 0, 0, 128);
            }
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            return ch.GetBlockAt(x, y, z);
        }

        public void Regen(Location pos, Chunk ch, int x = 1, int y = 1, int z = 1)
        {
            UpdateChunk(ch);
            if (x == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos) + new Vector3i(-1, 0, 0));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (y == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos) + new Vector3i(0, -1, 0));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (z == 0 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos + new Location(0, 0, -1)));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (x == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos) + new Vector3i(1, 0, 0));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (y == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos) + new Vector3i(0, 1, 0));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            if (z == ch.CSize - 1 || TheClient.CVars.r_chunkoverrender.ValueB)
            {
                ch = GetChunk(ChunkLocFor(pos) + new Vector3i(0, 0, 1));
                if (ch != null)
                {
                    UpdateChunk(ch);
                }
            }
            // TODO: Cascade downward for lighting updates?
        }

        public void SetBlockMaterial(Location pos, ushort mat, byte dat = 0, byte paint = 0, bool regen = true)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos), 1);
            int x = (int)Math.Floor(((int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int y = (int)Math.Floor(((int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            int z = (int)Math.Floor(((int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE) / (float)ch.PosMultiplier);
            ch.SetBlockAt(x, y, z, new BlockInternal(mat, dat, paint, 0));
            ch.Edited = true;
            if (regen)
            {
                Regen(pos, ch, x, y, z);
            }
        }

        public void UpdateChunk(Chunk ch)
        {
            /*TheClient.Schedule.StartASyncTask(() =>
            {
                ch.CalculateLighting();
            });*/
            TheClient.Schedule.ScheduleSyncTask(() =>
            {
                ch.AddToWorld();
                ch.CreateVBO();
            });
        }

        public Dictionary<Vector3i, Tuple<Matrix4d, Model, Model, Location, Texture>> AxisAlignedModels = new Dictionary<Vector3i, Tuple<Matrix4d, Model, Model, Location, Texture>>();

        const double MAX_GRASS_DIST = 9; // TODO: CVar?

        const double mgd_sq = MAX_GRASS_DIST * MAX_GRASS_DIST;

        public void RenderPlants()
        {
            if (TheClient.CVars.r_plants.ValueB)
            {
                TheClient.SetEnts();
                RenderPlants(1);
                RenderPlants(3);
                RenderPlants(7);
                RenderGrass();
            }
        }

        public void RenderGrass()
        {
            int ts_Arr = GL.GenVertexArray();
            int ts_Buff = GL.GenBuffer();
            int ts_Inds = GL.GenBuffer();
            OpenTK.Vector3[] pos = new OpenTK.Vector3[60];
            uint[] inds = new uint[60];
            for (uint i = 0; i < 60; i++)
            {
                pos[i] = new OpenTK.Vector3((float)Utilities.UtilRandom.NextDouble() * 4 - 2, (float)Utilities.UtilRandom.NextDouble() * 4 - 2, (float)Utilities.UtilRandom.NextDouble() * 4 - 2);
                inds[i] = i;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, ts_Buff);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(pos.Length * OpenTK.Vector3.SizeInBytes), pos, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ts_Inds);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(uint)), inds, BufferUsageHint.StaticDraw);
            GL.BindVertexArray(ts_Arr);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ts_Buff);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ts_Inds);
            if (TheClient.MainWorldView.FBOid == FBOID.FORWARD_SOLID)
            {
                TheClient.s_forw_grass = TheClient.s_forw_grass.Bind();
            }
            else
            {
                return;
            }
            GL.BindVertexArray(ts_Arr);
            TheClient.Textures.GetTexture("blocks/transparent/tallgrass").Bind(); // TODO: Cache!
            GL.UniformMatrix4(1, false, ref TheClient.MainWorldView.PrimaryMatrix);
            Matrix4 ident = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref ident);
            GL.DrawElements(PrimitiveType.Points, inds.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            TheClient.isVox = true;
            TheClient.SetEnts();
            GL.DeleteVertexArray(ts_Arr);
            GL.DeleteBuffer(ts_Buff);
            GL.DeleteBuffer(ts_Inds);
        }

        public void RenderPlants(int distmod)
        {
            // TODO: Clean this method
            OpenTK.Vector3 wind = ClientUtilities.Convert(ActualWind);
            float len = wind.Length;
            Matrix4 windtransf = Matrix4.CreateFromAxisAngle(wind / len, (float)Math.Min(len, 1.0));
            HashSet<Model> mods = new HashSet<Model>();
            Model prev = null;
            bool matchable = false;
            Location playerPos = TheClient.Player.GetPosition();
            foreach (KeyValuePair<Vector3i, Tuple<Matrix4d, Model, Model, Location, Texture>> mod in AxisAlignedModels)
            {
                double dist = mod.Key.ToLocation().DistanceSquared(TheClient.MainWorldView.CameraPos);
                if (dist > mgd_sq * distmod)
                {
                    continue;
                }
                double dist2 = mod.Key.ToLocation().DistanceSquared(playerPos);
                float rot = 0;
                if (dist2 < 3 * 3)
                {
                    rot = (3 * 3 - (float)dist2) * 0.5f;
                    matchable = false;
                }
                bool upd = false;
                if (distmod <= 4)
                {
                    Model mt = distmod <= 2 ? mod.Value.Item2 : mod.Value.Item3;
                    if (!mods.Contains(mt) || !matchable)
                    {
                        upd = true;
                        mt.ForceBoneNoOffset = true;
                        if (rot > 0.001)
                        {
                            OpenTK.Vector3 ppt = ClientUtilities.Convert(playerPos);
                            ppt.Z = mod.Key.Z;
                            OpenTK.Vector3 rel = ppt - ClientUtilities.Convert(mod.Key.ToLocation());
                            Matrix4 rotter = Matrix4.CreateFromAxisAngle(rel.Normalized(), rot);
                            mt.CustomAnimationAdjustments["b_bottom"] = rotter;
                            mt.CustomAnimationAdjustments["b_top"] = rotter;
                        }
                        else
                        {
                            mt.CustomAnimationAdjustments["b_bottom"] = windtransf;
                            mt.CustomAnimationAdjustments["b_top"] = windtransf;
                            matchable = true;
                        }
                        mt.UpdateTransforms(mt.RootNode, Matrix4.Identity);
                        mt.LoadSkin(TheClient.Textures);
                    }
                    if (mt != prev || upd) // TODO: Sort so we don't need to update very often?
                    {
                        Matrix4[] mats = new Matrix4[mt.Meshes[0].Bones.Count];
                        for (int x = 0; x < mt.Meshes[0].Bones.Count; x++)
                        {
                            mats[x] = mt.Meshes[0].Bones[x].Transform;
                        }
                        mt.SetBones(mats);
                        prev = mt;
                    }
                    TheClient.Rendering.SetColor(new OpenTK.Vector4((float)mod.Value.Item4.X, (float)mod.Value.Item4.Y, (float)mod.Value.Item4.Z, 1f));
                    Matrix4d transf = /* Matrix4d.Scale(((mgd_sq * 4 - dist) / (mgd_sq * 4))) * */ mod.Value.Item1;
                    TheClient.MainWorldView.SetMatrix(2, transf);
                    mt.Draw();
                }
                else if (mod.Value.Item5.Name != null && dist >= mgd_sq * 3)
                {
                    Texture t = mod.Value.Item5;
                    if (!t.LoadedProperly)
                    {
                        t.Internal_Texture = TheClient.Textures.GetTexture(t.Name).Original_InternalID;
                        t.LoadedProperly = true;
                    }
                    t.Bind();
                    Location start = ClientUtilities.ConvertD(mod.Value.Item1.ExtractTranslation());
                    Location goal = TheClient.MainWorldView.CameraPos;
                    goal.Z = start.Z;
                    TheClient.Rendering.RenderBillboard(start, Location.One * (2 * (mgd_sq * distmod - dist) / (mgd_sq * distmod)), goal);
                }
            }
            TheClient.Rendering.SetColor(Color4.White);
        }

        public void RenderEffects()
        {
            GL.LineWidth(5);
            TheClient.Rendering.SetColor(Color4.White);
            for (int i = 0; i < Highlights.Length; i++)
            {
                TheClient.Rendering.RenderLineBox(Highlights[i].Min, Highlights[i].Max);
            }
            GL.LineWidth(1);
        }

        public void Render()
        {
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.Rendering.SetMinimumLight(0f);
            if (TheClient.RenderTextures)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            /*foreach (Chunk chunk in LoadedChunks.Values)
            {
                if (TheClient.CFrust == null || TheClient.CFrust.ContainsBox(chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE,
                chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE)))
                {
                    chunk.Render();
                }
            }*/
            if (TheClient.MainWorldView.FBOid == FBOID.MAIN || TheClient.MainWorldView.FBOid == FBOID.NONE || TheClient.MainWorldView.FBOid == FBOID.FORWARD_SOLID)
            {
                chToRender.Clear();
                if (TheClient.CVars.r_chunkmarch.ValueB)
                {
                    ChunkMarchAndDraw();
                }
                else
                {
                    foreach (Chunk ch in LoadedChunks.Values)
                    {
                        BEPUutilities.Vector3 min = ch.WorldPosition.ToVector3() * Chunk.CHUNK_SIZE;
                        if (TheClient.MainWorldView.CFrust == null || TheClient.MainWorldView.CFrust.ContainsBox(min, min + new BEPUutilities.Vector3(Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE)))
                        {
                            ch.Render();
                            chToRender.Add(ch);
                        }
                    }
                }
            }
            else
            {
                foreach (Chunk ch in chToRender)
                {
                    ch.Render();
                }
            }
            if (TheClient.RenderTextures)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        public List<Chunk> chToRender = new List<Chunk>();

        static Vector3i[] MoveDirs = new Vector3i[] { new Vector3i(-1, 0, 0), new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, -1), new Vector3i(0, 0, 1) };

        public int MaxRenderDistanceChunks = 10;

        public void ChunkMarchAndDraw()
        {
            Vector3i start = ChunkLocFor(TheClient.MainWorldView.CameraPos);
            HashSet<Vector3i> seen = new HashSet<Vector3i>();
            Queue<Vector3i> toSee = new Queue<Vector3i>();
            HashSet<Vector3i> toSeeSet = new HashSet<Vector3i>();
            toSee.Enqueue(start);
            toSeeSet.Add(start);
            while (toSee.Count > 0)
            {
                Vector3i cur = toSee.Dequeue();
                toSeeSet.Remove(cur);
                if ((Math.Abs(cur.X - start.X) > MaxRenderDistanceChunks)
                    || (Math.Abs(cur.Y - start.Y) > MaxRenderDistanceChunks)
                    || (Math.Abs(cur.Z - start.Z) > MaxRenderDistanceChunks))
                {
                    continue;
                }
                seen.Add(cur);
                Chunk chcur = GetChunk(cur);
                if (chcur != null)
                {
                    chcur.Render();
                    chToRender.Add(chcur);
                }
                for (int i = 0; i < MoveDirs.Length; i++)
                {
                    Vector3i t = cur + MoveDirs[i];
                    if (!seen.Contains(t) && !toSeeSet.Contains(t))
                    {
                        for (int j = 0; j < MoveDirs.Length; j++)
                        {
                            if (BEPUutilities.Vector3.Dot(MoveDirs[j].ToVector3(), (TheClient.MainWorldView.CameraTarget - TheClient.MainWorldView.CameraPos).ToBVector()) < -0.8f) // TODO: Wut?
                            {
                                continue;
                            }
                            Vector3i nt = cur + MoveDirs[j];
                            if (!seen.Contains(nt) && !toSeeSet.Contains(nt))
                            {
                                BEPUutilities.Vector3 min = nt.ToVector3() * Chunk.CHUNK_SIZE;
                                if (TheClient.MainWorldView.CFrust == null || TheClient.MainWorldView.CFrust.ContainsBox(min, min + new BEPUutilities.Vector3(Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE)))
                                {
                                    toSee.Enqueue(nt);
                                    toSeeSet.Add(nt);
                                }
                                else
                                {
                                    seen.Add(nt);
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        public void AddJoint(InternalBaseJoint joint)
        {
            Joints.Add(joint);
            joint.One.Joints.Add(joint);
            joint.Two.Joints.Add(joint);
            joint.Enable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                pjoint.CurrentJoint = pjoint.GetBaseJoint();
                PhysicsWorld.Add(pjoint.CurrentJoint);
            }
        }

        public void DestroyJoint(InternalBaseJoint joint)
        {
            if (!Joints.Remove(joint))
            {
                SysConsole.Output(OutputType.WARNING, "Destroyed non-existent joint?!");
            }
            joint.One.Joints.Remove(joint);
            joint.Two.Joints.Remove(joint);
            joint.Disable();
            if (joint is BaseJoint)
            {
                BaseJoint pjoint = (BaseJoint)joint;
                PhysicsWorld.Remove(pjoint.CurrentJoint);
            }
        }

        public double GlobalTickTimeLocal = 0;

        public void ForgetChunk(Vector3i cpos)
        {
            Chunk ch;
            if (LoadedChunks.TryGetValue(cpos, out ch))
            {
                ch.Destroy();
                LoadedChunks.Remove(cpos);
            }
        }

        public bool InWater(Location min, Location max)
        {
            // TODO: Efficiency!
            min = min.GetBlockLocation();
            max = max.GetUpperBlockBorder();
            for (int x = (int)min.X; x < max.X; x++)
            {
                for (int y = (int)min.Y; y < max.Y; y++)
                {
                    for (int z = (int)min.Z; z < max.Z; z++)
                    {
                        if (GetBlockMaterial(min + new Location(x, y, z)).GetSolidity() == MaterialSolidity.LIQUID)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public Location GetSkyLight(Location pos, Location norm)
        {
            // TODO: Optimize this method!
            if (norm.Z < -0.99)
            {
                return Location.Zero;
            }
            pos.Z = pos.Z + 1;
            int XP = (int)Math.Floor(pos.X / Chunk.CHUNK_SIZE);
            int YP = (int)Math.Floor(pos.Y / Chunk.CHUNK_SIZE);
            int ZP = (int)Math.Floor(pos.Z / Chunk.CHUNK_SIZE);
            int x = (int)(Math.Floor(pos.X) - (XP * Chunk.CHUNK_SIZE));
            int y = (int)(Math.Floor(pos.Y) - (YP * Chunk.CHUNK_SIZE));
            int z = (int)(Math.Floor(pos.Z) - (ZP * Chunk.CHUNK_SIZE));
            float light = 1f;
            while (true)
            {
                Chunk ch = GetChunk(new Vector3i(XP, YP, ZP));
                if (ch == null)
                {
                    break;
                }
                while (z < Chunk.CHUNK_SIZE)
                {
                    BlockInternal bi = ch.GetBlockAtLOD((int)x, (int)y, (int)z);
                    if (bi.IsOpaque())
                    {
                        return Location.Zero;
                    }
                    light -= (float)((Material)bi.BlockMaterial).GetLightDamage();
                    z++;
                }
                ZP++;
                z = 0;
            }
            if (light > 0 && TheClient.CVars.r_treeshadows.ValueB)
            {
                BoundingBox bb = new BoundingBox(pos.ToBVector(), (pos + new Location(1, 1, 500)).ToBVector());
                if (GenShadowCasters != null)
                {
                    for (int i = 0; i < GenShadowCasters.Length; i++)
                    {
                        PhysicsEntity pe = GenShadowCasters[i];
                        if (pe == null) // Shouldn't happen.
                        {
                            continue;
                        }
                        if (pe.GenBlockShadows)
                        {
                            if (pe.ShadowMainDupe.BoundingBox.Intersects(bb))
                            {
                                light = 0;
                                break;
                            }
                            if (pe.ShadowCastShape.BoundingBox.Intersects(bb))
                            {
                                light -= 0.3f;
                                if (light <= 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return Math.Max(norm.Dot(SunLightPathNegative), 0.5) * new Location(light) * SkyLightMod;
        }

        static Location SunLightPathNegative = new Location(0, 0, 1);

        const float SkyLightMod = 0.75f;

        public Location GetAmbient(Location pos)
        {
            return TheClient.BaseAmbient;
        }

        public Location Regularize(Location col)
        {
            if (col.X < 1.0 && col.Y < 1.0 && col.Z < 1.0)
            {
                return col;
            }
            return col / Math.Max(col.X, Math.Max(col.Y, col.Z));
        }

        public Location GetBlockLight(Location pos, Location norm, List<Chunk> potentials)
        {
            Location lit = Location.Zero;
            foreach (Chunk ch in potentials)
            {
                if (ch == null)
                {
                    continue;
                }
                lock (ch.Lits)
                {
                    foreach (KeyValuePair<Vector3i, Material> pot in ch.Lits)
                    {
                        double distsq = (pot.Key.ToLocation() + ch.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE).DistanceSquared(pos);
                        double range = pot.Value.GetLightEmitRange();
                        // TODO: Apply normal vector stuff?
                        if (distsq < range * range)
                        {
                            lit += pot.Value.GetLightEmit() * (range - Math.Sqrt(distsq));
                        }
                    }
                }
            }
            return lit;
        }

        public Location GetLightAmount(Location pos, Location norm, List<Chunk> potentials)
        {
            if (potentials == null)
            {
                potentials = new List<Chunk>();
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            potentials.Add(GetChunk(new Vector3i(x, y, z)));
                        }
                    }
                }
            }
            Location amb = GetAmbient(pos);
            Location sky = GetSkyLight(pos, norm);
            Location blk = GetBlockLight(pos, norm, potentials);
            return amb + sky + blk;
        }

        public SimplePriorityQueue<Action> PrepChunks = new SimplePriorityQueue<Action>();
        
        public SimplePriorityQueue<Vector3i> NeedsRendering = new SimplePriorityQueue<Vector3i>();

        public HashSet<Vector3i> RenderingNow = new HashSet<Vector3i>();

        public HashSet<Vector3i> PreppingNow = new HashSet<Vector3i>();

        public void CheckForRenderNeed()
        {
            lock (RenderingNow)
            {
                while (NeedsRendering.Count > 0 && RenderingNow.Count < TheClient.CVars.r_chunksatonce.ValueI)
                {
                    Vector3i temp = NeedsRendering.Dequeue();
                    Chunk ch = GetChunk(temp);
                    if (ch != null)
                    {
                        ch.MakeVBONow();
                        RenderingNow.Add(temp);
                    }
                }
            }
            lock (PreppingNow)
            {
                while (PrepChunks.Count > 0 && PreppingNow.Count < TheClient.CVars.r_chunksatonce.ValueI)
                {
                    Action temp = PrepChunks.Dequeue();
                    temp.Invoke();
                }
            }
        }

        public void DoneRendering(Chunk ch)
        {
            lock (RenderingNow)
            {
                RenderingNow.Remove(ch.WorldPosition);
            }
        }

        /// <summary>
        /// Do not call directly, use Chunk.CreateVBO().
        /// </summary>
        /// <param name="ch"></param>
        public void NeedToRender(Chunk ch)
        {
            lock (RenderingNow)
            {
                if (!NeedsRendering.Contains(ch.WorldPosition))
                {
                    NeedsRendering.Enqueue(ch.WorldPosition, (ch.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE).DistanceSquared(TheClient.Player.GetPosition()));
                }
            }
        }
    }
}
