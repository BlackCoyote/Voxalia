//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using System.Linq;
using Voxalia.ClientGame.OtherSystems;
using FreneticGameCore;
using FreneticGameGraphics;

namespace Voxalia.ClientGame.GraphicsSystems
{
    /// <summary>
    /// Rendering utility.
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Prepare the renderer.
        /// </summary>
        public void Init()
        {
            GenerateSquareVBO();
            GenerateLineVBO();
            GenerateBoxVBO();
        }

        public VBO Square;
        VBO Line;
        VBO Box;

        void GenerateSquareVBO()
        {
            Vector3[] vecs = new Vector3[6];
            uint[] inds = new uint[6];
            Vector3[] norms = new Vector3[6];
            Vector3[] texs = new Vector3[6];
            Vector4[] cols = new Vector4[6];
            Vector4[] BoneIDs = new Vector4[6];
            Vector4[] BoneWeights = new Vector4[6];
            Vector4[] BoneIDs2 = new Vector4[6];
            Vector4[] BoneWeights2 = new Vector4[6];
            for (uint n = 0; n < 6; n++)
            {
                inds[n] = n;
                norms[n] = new Vector3(0, 0, 1);
                cols[n] = new Vector4(1, 1, 1, 1);
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(1, 0, 0);
            texs[0] = new Vector3(1, 0, 0);
            vecs[1] = new Vector3(1, 1, 0);
            texs[1] = new Vector3(1, 1, 0);
            vecs[2] = new Vector3(0, 1, 0);
            texs[2] = new Vector3(0, 1, 0);
            vecs[3] = new Vector3(1, 0, 0);
            texs[3] = new Vector3(1, 0, 0);
            vecs[4] = new Vector3(0, 1, 0);
            texs[4] = new Vector3(0, 1, 0);
            vecs[5] = new Vector3(0, 0, 0);
            texs[5] = new Vector3(0, 0, 0);
            Square = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                Normals = norms.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                BoneIDs = BoneIDs.ToList(),
                BoneWeights = BoneWeights.ToList(),
                BoneIDs2 = BoneIDs2.ToList(),
                BoneWeights2 = BoneWeights2.ToList()
            };
            Square.GenerateVBO();
        }
        
        void GenerateLineVBO()
        {
            Vector3[] vecs = new Vector3[2];
            uint[] inds = new uint[2];
            Vector3[] norms = new Vector3[2];
            Vector3[] texs = new Vector3[2];
            Vector4[] cols = new Vector4[2];
            Vector3[] tangs = new Vector3[2];
            for (uint u = 0; u < 2; u++)
            {
                inds[u] = u;
                tangs[u] = new Vector3(0f, 0f, 0f);
            }
            for (int n = 0; n < 2; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 2; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[2];
            Vector4[] BoneWeights = new Vector4[2];
            Vector4[] BoneIDs2 = new Vector4[2];
            Vector4[] BoneWeights2 = new Vector4[2];
            for (int n = 0; n < 2; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(0, 0, 0);
            texs[0] = new Vector3(0, 0, 0);
            vecs[1] = new Vector3(1, 0, 0);
            texs[1] = new Vector3(1, 0, 0);
            Line = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                Normals = norms.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                BoneIDs = BoneIDs.ToList(),
                BoneWeights = BoneWeights.ToList(),
                BoneIDs2 = BoneIDs2.ToList(),
                BoneWeights2 = BoneWeights2.ToList()
            };
            Line.GenerateVBO();
        }

        void GenerateBoxVBO()
        {
            // TODO: Optimize?
            Vector3[] vecs = new Vector3[24];
            uint[] inds = new uint[24];
            Vector3[] norms = new Vector3[24];
            Vector3[] texs = new Vector3[24];
            Vector4[] cols = new Vector4[24];
            Vector3[] tangs = new Vector3[24];
            for (uint u = 0; u < 24; u++)
            {
                inds[u] = u;
                tangs[u] = new Vector3(1f, 0f, 0f);
            }
            for (int t = 0; t < 24; t++)
            {
                texs[t] = new Vector3(0, 0, 0);
            }
            for (int n = 0; n < 24; n++)
            {
                norms[n] = new Vector3(0, 0, 1); // TODO: Accurate normals somehow? Do lines even have normals?
            }
            for (int c = 0; c < 24; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[24];
            Vector4[] BoneWeights = new Vector4[24];
            Vector4[] BoneIDs2 = new Vector4[24];
            Vector4[] BoneWeights2 = new Vector4[24];
            for (int n = 0; n < 24; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            int i = 0;
            int zero = -1; // Ssh.
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            Box = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                Normals = norms.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                BoneIDs = BoneIDs.ToList(),
                BoneWeights = BoneWeights.ToList(),
                BoneIDs2 = BoneIDs2.ToList(),
                BoneWeights2 = BoneWeights2.ToList()
            };
            Box.GenerateVBO();
        }

        public Renderer(TextureEngine tengine, ShaderEngine shaderdet)
        {
            Engine = tengine;
            Shaders = shaderdet;
        }

        public TextureEngine Engine;
        public ShaderEngine Shaders;

        public bool bbreak = true;

        /// <summary>
        /// Renders a line box.
        /// </summary>
        public void RenderLineBox(Location min, Location max, Matrix4d? rot = null)
        {
            if (min.IsNaN() || min.IsInfinite() || max.IsNaN() || max.IsInfinite())
            {
                SysConsole.Output(OutputType.WARNING, "Invalid line box from " + min + " to " + max);
                return;
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            Engine.White.Bind();
            View3D.CheckError("RenderLineBox: BindTexture");
            Location halfsize = (max - min) / 2;
            if ((min + halfsize) == Location.Zero)
            {
                return; // ???
            }
            if (Math.Abs(min.X + halfsize.X) < 1 || Math.Abs(min.Y + halfsize.Y) < 1 || Math.Abs(min.Z + halfsize.Z) < 1)
            {
                return; // ???
            }
            if (Math.Abs(min.X) < 1 || Math.Abs(min.Y) < 1 || Math.Abs(min.Z) < 1)
            {
                return; // ???
            }
            if (bbreak)
            {
                return; // TODO: Fix!
            }
            Matrix4d mat = Matrix4d.Scale(ClientUtilities.ConvertD(halfsize))
            * (rot != null && rot.HasValue ? rot.Value : Matrix4d.Identity)
            * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(min + halfsize));
            Client.Central.MainWorldView.SetMatrix(2, mat); // TODO: Client reference!
            View3D.CheckError("RenderLineBox: SetMatrix");
            GL.BindVertexArray(Box._VAO);
            View3D.CheckError("RenderLineBox: Bind VAO");
            GL.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedInt, IntPtr.Zero);
            if (View3D.CheckError("RenderLineBox: Pass"))
            {
                SysConsole.Output(OutputType.DEBUG, "Caught: " + Box._VAO + ", " + min + ", " + max + ", " + halfsize);
            }
        }

        /// <summary>
        /// Render a line between two points.
        /// </summary>
        /// <param name="start">The initial point.</param>
        /// <param name="end">The ending point.</param>
        public void RenderLine(Location start, Location end)
        {
            // TODO: Efficiency!
            float len = (float)(end - start).Length();
            Location vecang = Utilities.VectorToAngles(start - end);
            Matrix4d mat = Matrix4d.Scale(len, 1, 1)
                * Matrix4d.CreateRotationY((float)(vecang.Y * Utilities.PI180))
                * Matrix4d.CreateRotationZ((float)(vecang.Z * Utilities.PI180))
                * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(start));
            Client.Central.MainWorldView.SetMatrix(2, mat); // TODO: Client reference!
            GL.BindVertexArray(Line._VAO);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        /// <summary>
        /// Render a cylinder between two points.
        /// </summary>
        /// <param name="start">The initial point.</param>
        /// <param name="end">The ending point.</param>
        /// <param name="width">The width of the cylinder.</param>
        public void RenderCylinder(Location start, Location end, float width)
        {
            float len = (float)(end - start).Length();
            Location vecang = Utilities.VectorToAngles(start - end);
            Matrix4d mat = Matrix4d.CreateRotationY((float)(90 * Utilities.PI180))
                * Matrix4d.Scale(len, width, width)
                * Matrix4d.CreateRotationY((float)(vecang.Y * Utilities.PI180))
                * Matrix4d.CreateRotationZ((float)(vecang.Z * Utilities.PI180))
                 * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(start));
            Client.Central.MainWorldView.SetMatrix(2, mat);
            Client.Central.Models.Cylinder.Draw(); // TODO: Models reference in constructor - or client reference?
        }

        public Vector4 AdaptColor(Vector3 vt, System.Drawing.Color tcol)
        {
            return AdaptColor(ClientUtilities.ConvertToD(vt), tcol);
        }

        public Vector4 AdaptColor(Vector3d vt, System.Drawing.Color tcol)
        {
            if (tcol.A == 0)
            {
                if (tcol.R == 127 && tcol.G == 0 && tcol.B == 127)
                {
                    float r = (float)SimplexNoise.Generate(vt.X / 10f, vt.Y / 10f, vt.Z / 10f);
                    float g = (float)SimplexNoise.Generate((vt.X + 50f) / 10f, (vt.Y + 127f) / 10f, (vt.Z + 10f) / 10f);
                    float b = (float)SimplexNoise.Generate((vt.X - 150f) / 10f, (vt.Y - 65f) / 10f, (vt.Z + 73f) / 10f);
                    return new Vector4(r, g, b, 1f);
                }
                else if (tcol.R == 127 && tcol.G == 0 && tcol.B == 0)
                {
                    Random random = new Random((int)(vt.X + vt.Y + vt.Z));
                    return new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1f);
                }
                else
                {
                    return new Vector4(tcol.R / 255f, tcol.G / 255f, tcol.B / 255f, 0f);
                }
            }
            else
            {
                return new Vector4(tcol.R / 255f, tcol.G / 255f, tcol.B / 255f, tcol.A / 255f);
            }
        }

        public void SetColor(Vector4 col)
        {
            if (!Client.Central.MainWorldView.RenderingShadows)
            {
                GL.Uniform4(3, ref col);
            }
        }

        public void SetColor(Color4 c)
        {
            SetColor(new Vector4(c.R, c.G, c.B, c.A));
        }
        
        public void SetMinimumLight(float min)
        {
            if (Client.Central.MainWorldView.RenderLights) // TODO: Pass client reference!
            {
                GL.Uniform1(16, min);
            }
        }

        public void EnableShine(bool defaultColor = true)
        {
            if (defaultColor)
            {
                SetColor(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            }
            GL.Disable(EnableCap.DepthTest);
            GL.DepthMask(false);
            GL.Uniform1(8, 1.0f);
        }

        public void DisableShine()
        {
            SetColor(Vector4.One);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Uniform1(8, 0.0f);
        }

        /// <summary>
        /// Renders a 3D rectangle.
        /// </summary>
        /// <param name="mat">The matrix.</param>
        public void RenderRectangle3D(Matrix4 mat)
        {
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Renders a 2D rectangle.
        /// </summary>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
        /// <param name="rot">The rotation matrix, if any.</param>
        public void RenderRectangle(float xmin, float ymin, float xmax, float ymax, Matrix4? rot = null)
        {
            Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * (rot != null && rot.HasValue ? rot.Value : Matrix4.Identity) * Matrix4.CreateTranslation(xmin, ymin, 0);
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Renders a 2D rectangle, with centered rotation.
        /// </summary>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
        /// <param name="cx">The rotation offset X.</param>
        /// <param name="cy">The rotation offset Y.</param>
        /// <param name="rot">The rotation matrix.</param>
        public void RenderRectangleCentered(float xmin, float ymin, float xmax, float ymax, float cx, float cy, Matrix4 rot)
        {
            Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * Matrix4.CreateTranslation(-cx, -cy, 0) * rot * Matrix4.CreateTranslation(xmin + cx, ymin + cy, 0);
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        public void RenderBillboard(Location center, Location scale, Location facing, float pzr = 0f)
        {
            Location lookdir = (facing - center).Normalize();
            Location right = lookdir.CrossProduct(Location.UnitZ); // TODO: Camera up vector!
            Location updir = right.CrossProduct(lookdir);
            Matrix4d mat = Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f) * Matrix4d.Scale((float)scale.X, (float)scale.Y, (float)scale.Z);
            Matrix4d m2 = new Matrix4d(right.X, updir.X, lookdir.X, center.X,
                right.Y, updir.Y, lookdir.Y, center.Y,
                right.Z, updir.Z, lookdir.Z, center.Z,
                0, 0, 0, 1);
            m2.Transpose();
            mat *= m2;
            Client.Central.MainWorldView.SetMatrix(2, mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            /*
            // TODO: Quaternion magic?
            Location relang = Utilities.VectorToAngles(pos - facing);
            if (relang.IsInfinite() || relang.IsNaN())
            {
                throw new Exception("Unable to handle billboard: relang=" + relang);
            }
            Matrix4d mat = 
                Matrix4d.Scale(ClientUtilities.ConvertD(scale))
                * Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f)
                * Matrix4d.CreateRotationY((float)((relang.Y - 90) * Utilities.PI180))
                * Matrix4d.CreateRotationZ((float)(relang.Z * Utilities.PI180))
                * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(pos + new Location(scale.X * 0.5, scale.Y * 0.5, 0.0)));
            Client.Central.MainWorldView.SetMatrix(2, mat); // TODO: Client reference!
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            */
        }
        
        public void RenderBilboardLine(Location pos, Location p2, float width, Location facing)
        {
            Location center = (pos + p2) * 0.5;
            double len = (center - facing).Length();
            Location lookdir = (center - facing) / len;
            double len2 = (p2 - pos).Length();
            if (len < 0.001 || len2 < 0.001)
            {
                return;
            }
            Location updir = (p2 - pos) / len2;
            Location right = updir.CrossProduct(lookdir);
            Matrix4d mat = Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f) * Matrix4d.Scale((float)len2 * 0.5f, width, 1f);
            Matrix4d m2 = new Matrix4d(right.X, updir.X, lookdir.X, center.X,
                right.Y, updir.Y, lookdir.Y, center.Y,
                right.Z, updir.Z, lookdir.Z, center.Z,
                0, 0, 0, 1);
            m2.Transpose();
            mat *= m2;
            Client.Central.MainWorldView.SetMatrix(2, mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }
    }
}
