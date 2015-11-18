﻿using System.Collections.Generic;
using BEPUutilities;

namespace Voxalia.Shared.BlockShapes
{
    public class BSD73: BlockShapeDetails
    {
        public Vector3 Normal = new Vector3(-0.5f, 0, 1);

        public BSD73()
        {
            Normal.Normalize();
        }

        public override List<Vector3> GetVertices(Vector3 pos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Vertices = new List<Vector3>();
            Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
            Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0.5f));
            Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
            Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0.5f));
            Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 0.5f));
            Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
            if (!BOTTOM)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
            }
            if (!XP)
            {
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0.5f));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 0.5f));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0.5f));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
            }
            if (!YP)
            {
                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 0.5f));
            }
            if (!YM)
            {
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 0.5f));
                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
            }
            return Vertices;
        }

        public override List<BEPUutilities.Vector3> GetNormals(Vector3 blockPos, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> Norms = new List<Vector3>();
            for (int i = 0; i < 6; i++)
            {
                Norms.Add(Normal);
            }
            if (!BOTTOM)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(0, 0, -1));
                }
            }
            if (!XP)
            {
                for (int i = 0; i < 6; i++)
                {
                    Norms.Add(new Vector3(1, 0, 0));
                }
            }
            if (!YP)
            {
                for (int i = 0; i < 3; i++)
                {
                    Norms.Add(new Vector3(0, 1, 0));
                }
            }
            if (!YM)
            {
                for (int i = 0; i < 3; i++)
                {
                    Norms.Add(new Vector3(0, -1, 0));
                }
            }
            return Norms;
        }

        public override List<Vector3> GetTCoords(Vector3 blockPos, Material mat, bool XP, bool XM, bool YP, bool YM, bool TOP, bool BOTTOM)
        {
            List<Vector3> TCoords = new List<Vector3>();
            int tID_TOP = mat.TextureID(MaterialSide.TOP);
            TCoords.Add(new Vector3(0, 1, tID_TOP));
            TCoords.Add(new Vector3(1, 1, tID_TOP));
            TCoords.Add(new Vector3(0, 0, tID_TOP));
            TCoords.Add(new Vector3(1, 1, tID_TOP));
            TCoords.Add(new Vector3(1, 0, tID_TOP));
            TCoords.Add(new Vector3(0, 0, tID_TOP));
            if (!BOTTOM)
            {
                int tID_BOTTOM = mat.TextureID(MaterialSide.BOTTOM);
                TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
                TCoords.Add(new Vector3(0, 1, tID_BOTTOM));
                TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 0, tID_BOTTOM));
                TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
            }
            if (!XP)
            {
                int tID_XP = mat.TextureID(MaterialSide.XP);
                TCoords.Add(new Vector3(0, 0, tID_XP));
                TCoords.Add(new Vector3(1, 1, tID_XP));
                TCoords.Add(new Vector3(0, 1, tID_XP));
                TCoords.Add(new Vector3(0, 0, tID_XP));
                TCoords.Add(new Vector3(1, 0, tID_XP));
                TCoords.Add(new Vector3(1, 1, tID_XP));
            }
            if (!YP)
            {
                int tID_YP = mat.TextureID(MaterialSide.YP);
                TCoords.Add(new Vector3(0, 1, tID_YP));
                TCoords.Add(new Vector3(1, 1, tID_YP));
                TCoords.Add(new Vector3(1, 0, tID_YP));
            }
            if (!YM)
            {
                int tID_YM = mat.TextureID(MaterialSide.YM);
                TCoords.Add(new Vector3(1, 0, tID_YM));
                TCoords.Add(new Vector3(1, 1, tID_YM));
                TCoords.Add(new Vector3(0, 1, tID_YM));
            }
            return TCoords;
        }

        public override bool OccupiesXP()
        {
            return false;
        }

        public override bool OccupiesYP()
        {
            return false;
        }

        public override bool OccupiesXM()
        {
            return false;
        }

        public override bool OccupiesYM()
        {
            return false;
        }

        public override bool OccupiesTOP()
        {
            return false;
        }

        public override bool OccupiesBOTTOM()
        {
            return true;
        }
    }
}
