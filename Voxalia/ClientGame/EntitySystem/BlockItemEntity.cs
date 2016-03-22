﻿using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.OtherSystems;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionTests;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;
        public byte Dat;
        public byte Paint;
        public double soundmaxrate = 0.2;

        public BlockItemEntity(Region tregion, Material tmat, byte dat, byte tpaint, BlockDamage damage)
            : base(tregion, false, true)
        {
            Mat = tmat;
            Dat = dat;
            Paint = tpaint;
            Shape = BlockShapeRegistry.BSD[dat].GetShape(damage, out Offset);
            SetMass(5);
        }

        public Location Offset;

        public VBO vbo = null;

        public override void SpawnBody()
        {
            vbo = new VBO();
            List<BEPUutilities.Vector3> vecs = BlockShapeRegistry.BSD[Dat].GetVertices(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> norms = BlockShapeRegistry.BSD[Dat].GetNormals(new BEPUutilities.Vector3(0, 0, 0), false, false, false, false, false, false);
            List<BEPUutilities.Vector3> tcoord = BlockShapeRegistry.BSD[Dat].GetTCoords(new BEPUutilities.Vector3(0, 0, 0), Mat, false, false, false, false, false, false);
            vbo.Vertices = new List<OpenTK.Vector3>();
            vbo.Normals = new List<OpenTK.Vector3>();
            vbo.TexCoords = new List<OpenTK.Vector3>();
            vbo.Indices = new List<uint>();
            vbo.Colors = new List<Vector4>();
            vbo.TCOLs = new List<Vector4>();
            vbo.Tangents = new List<Vector3>();
            System.Drawing.Color tcol = Voxalia.Shared.Colors.ForByte(Paint);
            for (int i = 0; i < vecs.Count; i++)
            {
                vbo.Vertices.Add(new OpenTK.Vector3(vecs[i].X, vecs[i].Y, vecs[i].Z));
                vbo.Normals.Add(new OpenTK.Vector3(norms[i].X, norms[i].Y, norms[i].Z));
                vbo.TexCoords.Add(new OpenTK.Vector3(tcoord[i].X, tcoord[i].Y, tcoord[i].Z));
                vbo.Indices.Add((uint)i);
                vbo.Colors.Add(new Vector4(1, 1, 1, 1));
                if (tcol.A == 0)
                {
                    if (tcol.R == 127 && tcol.G == 0 && tcol.B == 127)
                    {
                        Random urand = new Random(1594124); // TODO: Track where the block came from?
                        vbo.TCOLs.Add(new OpenTK.Vector4((float)urand.NextDouble(), (float)urand.NextDouble(), (float)urand.NextDouble(), 1f));
                    }
                    else if (tcol.R == 127 && tcol.G == 0 && tcol.B == 0)
                    {
                        Random random = new Random((int)(vecs[i].X + vecs[i].Y + vecs[i].Z));
                        vbo.TCOLs.Add(new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1f));
                    }
                    else
                    {
                        vbo.TCOLs.Add(new Vector4(tcol.R / 255f, tcol.G / 255f, tcol.B / 255f, 0f));
                    }
                }
                else
                {
                    vbo.TCOLs.Add(new OpenTK.Vector4((tcol.R / 255f), (tcol.G / 255f), (tcol.B / 255f), 1f * (tcol.A / 255f)));
                }
            }
            for (int i = 0; i < vecs.Count; i += 3)
            {
                int basis = i;
                OpenTK.Vector3 v1 = vbo.Vertices[basis];
                OpenTK.Vector3 dv1 = vbo.Vertices[basis + 1] - v1;
                OpenTK.Vector3 dv2 = vbo.Vertices[basis + 2] - v1;
                OpenTK.Vector3 t1 = vbo.TexCoords[basis];
                OpenTK.Vector3 dt1 = vbo.TexCoords[basis + 1] - t1;
                OpenTK.Vector3 dt2 = vbo.TexCoords[basis + 2] - t1;
                OpenTK.Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) * 1f / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                OpenTK.Vector3 normal = vbo.Normals[basis];
                tangent = (tangent - normal * OpenTK.Vector3.Dot(normal, tangent)).Normalized();
                vbo.Tangents.Add(tangent);
                vbo.Tangents.Add(tangent);
                vbo.Tangents.Add(tangent);
            }
            vbo.GenerateVBO();
            base.SpawnBody();
            Body.CollisionInformation.Events.ContactCreated += Events_ContactCreated; // TODO: Perhaps better more direct event?
        }

        public double lastSoundTime;
        
        void Events_ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            if (TheRegion.GlobalTickTimeLocal - lastSoundTime < soundmaxrate)
            {
                return;
            }
            lastSoundTime = TheRegion.GlobalTickTimeLocal;
            if (other is FullChunkObject)
            {
                ContactInformation info;
                ((ConvexFCOPairHandler)pair).ContactInfo(/*contact.Id*/0, out info);
                float vellen = Math.Abs(info.RelativeVelocity.X) + Math.Abs(info.RelativeVelocity.Y) + Math.Abs(info.RelativeVelocity.Z);
                float mod = vellen / 5;
                if (mod > 2)
                {
                    mod = 2;
                }
                Location block = new Location(contact.Position - contact.Normal * 0.01f);
                BlockInternal bi = TheRegion.GetBlockInternal(block);
                MaterialSound sound = ((Material)bi.BlockMaterial).Sound();
                if (sound != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound, mod, 0.5f * mod);
                }
                MaterialSound sound2 = Mat.Sound();
                if (sound2 != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(block, sound2, mod, 0.5f * mod);
                }
            }
            else if (other is EntityCollidable)
            {
                BEPUphysics.Entities.Entity e = ((EntityCollidable)other).Entity;
                BEPUutilities.Vector3 relvel = Body.LinearVelocity - e.LinearVelocity;
                float vellen = Math.Abs(relvel.X) + Math.Abs(relvel.Y) + Math.Abs(relvel.Z);
                float mod = vellen / 5;
                if (mod > 2)
                {
                    mod = 2;
                }
                MaterialSound sound = Mat.Sound();
                if (sound != MaterialSound.NONE)
                {
                    new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(new Location(contact.Position), sound, mod, 0.5f * mod);
                }
            }
        }

        public override void DestroyBody()
        {
            if (vbo != null)
            {
                vbo.Destroy();
            }
            base.DestroyBody();
        }

        public override void Render()
        {
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            { 
                TheClient.s_fbov.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 3)
            {
                TheClient.s_transponlyvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 7)
            {
                TheClient.s_transponlyvoxlit.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 7)
            {
                TheClient.s_transponlyvoxlitsh.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 4)
            {
                TheClient.s_shadowvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
            }
            Matrix4 mat = Matrix4.CreateTranslation(-ClientUtilities.Convert(Offset)) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            vbo.Render(false);
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_fbo.Bind();
            }
            else if (TheClient.FBOid == 3)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_transponly.Bind();
            }
            else if (TheClient.FBOid == 7)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_transponlylit.Bind();
            }
            else if (TheClient.FBOid == 8)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_transponlylitsh.Bind();
            }
            else if (TheClient.FBOid == 4)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_shadow.Bind();
            }
        }
    }
}
