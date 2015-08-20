﻿using System.Collections.Generic;
using BEPUutilities;
using System;
using BEPUphysics.CollisionShapes;
using Voxalia.Shared.Files;

namespace Voxalia.Shared
{
    public class ModelHandler
    {
        public Model3D LoadModel(byte[] data)
        {
            if (data.Length < 3 || data[0] != 'V' || data[1] != 'M' || data[2] != 'D')
            {
                throw new Exception("Model3D: Invalid header bits.");
            }
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            dr.ReadBytes("VMD001".Length);
            Model3D mod = new Model3D();
            mod.MatrixA = ReadMat(dr);
            int meshCount = dr.ReadInt();
            mod.Meshes = new List<Model3DMesh>(meshCount);
            for (int m = 0; m < meshCount; m++)
            {
                Model3DMesh mesh = new Model3DMesh();
                mod.Meshes.Add(mesh);
                mesh.Name = dr.ReadFullString();
                int vertexCount = dr.ReadInt() * 3;
                mesh.Vertices = new List<Vector3>(vertexCount);
                for (int v = 0; v < vertexCount; v++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    float f3 = dr.ReadFloat();
                    mesh.Vertices.Add(new Vector3(f1, f2, f3));
                }
                int tcCount = dr.ReadInt();
                mesh.TexCoords = new List<Vector2>(tcCount);
                for (int t = 0; t < tcCount; t++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    mesh.TexCoords.Add(new Vector2(f1, f2));
                }
                int normCount = dr.ReadInt();
                mesh.Normals = new List<Vector3>(normCount);
                for (int n = 0; n < normCount; n++)
                {
                    float f1 = dr.ReadFloat();
                    float f2 = dr.ReadFloat();
                    float f3 = dr.ReadFloat();
                    mesh.Normals.Add(new Vector3(f1, f2, f3));
                }
                int boneCount = dr.ReadInt();
                mesh.Bones = new List<Model3DBone>(boneCount);
                for (int b = 0; b < boneCount; b++)
                {
                    Model3DBone bone = new Model3DBone();
                    mesh.Bones.Add(bone);
                    bone.Name = dr.ReadFullString();
                    int weights = dr.ReadInt();
                    bone.IDs = new List<int>(weights);
                    bone.Weights = new List<float>(weights);
                    for (int w = 0; w < weights; w++)
                    {
                        bone.IDs.Add(dr.ReadInt());
                        bone.Weights.Add(dr.ReadFloat());
                    }
                    bone.MatrixA = ReadMat(dr);
                }
            }
            mod.RootNode = ReadSingleNode(null, dr);
            return mod;
        }

        public Model3DNode ReadSingleNode(Model3DNode root, DataReader dr)
        {
            Model3DNode n = new Model3DNode();
            n.Parent = root;
            n.Name = dr.ReadFullString();
            n.MatrixA = ReadMat(dr);
            int cCount = dr.ReadInt();
            n.Children = new List<Model3DNode>(cCount);
            for (int i = 0; i < cCount; i++)
            {
                n.Children.Add(ReadSingleNode(n, dr));
            }
            return n;
        }

        public Matrix ReadMat(DataReader reader)
        {
            return new Matrix(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(),
                reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(),
                reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(),
                reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
        }

        public List<Vector3> GetCollisionVertices(Model3D input)
        {
            List<Vector3> vertices = new List<Vector3>(input.Meshes.Count * 100);
            bool colOnly = false;
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLower().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if ((!colOnly || mesh.Name.ToLower().Contains("collision")) && !mesh.Name.ToLower().Contains("nocollide"))
                {
                    vertices.AddRange(mesh.Vertices);
                }
            }
            return vertices;
        }

        public MobileMeshShape MeshToBepu(Model3D input)
        {
            List<Vector3> vertices = GetCollisionVertices(input);
            List<int> indices = new List<int>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                indices.Add(indices.Count);
            }
            return new MobileMeshShape(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }
    }
}
