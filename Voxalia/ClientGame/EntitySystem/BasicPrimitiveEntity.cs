﻿using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.ParticleSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    public class BasicPrimitiveEntity: PrimitiveEntity
    {
        public BasicPrimitiveEntity(Region tregion, bool cast_shadows)
            : base(tregion, cast_shadows)
        {
        }

        public Model model;

        public override void Destroy()
        {
        }

        public override void Spawn()
        {
            ppos = Position;
        }

        Location ppos = Location.Zero;

        public override void Render()
        {
            TheClient.SetEnts();
            if (TheClient.RenderTextures)
            {
                TheClient.Textures.White.Bind();
            }
            if (model != null)
            {
                TheClient.Rendering.SetMinimumLight(0f);
                BEPUutilities.Matrix matang = BEPUutilities.Matrix.CreateFromQuaternion(Angles);
                //matang.Transpose();
                Matrix4 matang4 = new Matrix4(matang.M11, matang.M12, matang.M13, matang.M14, matang.M21, matang.M22, matang.M23, matang.M24,
                    matang.M31, matang.M32, matang.M33, matang.M34, matang.M41, matang.M42, matang.M43, matang.M44);
                Matrix4 mat = matang4 * Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
                GL.UniformMatrix4(2, false, ref mat);
                model.Draw(); // TODO: Animation?
                if (model.Name == "projectiles/arrow.dae")
                {
                    float offs = 0.1f;
                    BEPUutilities.Vector3 offz;
                    BEPUutilities.Quaternion.TransformZ(offs, ref Angles, out offz);
                    BEPUutilities.Vector3 offx;
                    BEPUutilities.Quaternion.TransformX(offs, ref Angles, out offx);
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => ppos + new Location(offz),
                        (o) => Position + new Location(offz), (o) => 1f, 1f, Location.One,
                        Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => ppos - new Location(offz),
                        (o) => Position - new Location(offz), (o) => 1f, 1f, Location.One,
                        Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => ppos + new Location(offx),
                        (o) => Position + new Location(offx), (o) => 1f, 1f, Location.One,
                        Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                    TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => ppos - new Location(offx),
                        (o) => Position - new Location(offx), (o) => 1f, 1f, Location.One,
                        Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                    ppos = Position;
                }
            }
            else
            {
                TheClient.Rendering.SetMinimumLight(1f);
                TheClient.Rendering.SetColor(Color4.DarkRed);
                TheClient.Rendering.RenderCylinder(GetPosition(), GetPosition() - Velocity / 20f, 0.01f);
                TheClient.Particles.Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => ppos, (o) => Position, (o) => 0.01f, 2f,
                    Location.One, Location.One, true, TheClient.Textures.GetTexture("white"));
                ppos = Position;
                TheClient.Rendering.SetColor(Color4.White);
            }
        }
    }

    public class BulletEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] e)
        {
            if (e.Length < 4 + 12 + 12)
            {
                return null;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(tregion, false);
            bpe.Scale = new Location(Utilities.BytesToFloat(Utilities.BytesPartial(e, 0, 4)));
            bpe.SetPosition(Location.FromBytes(e, 4));
            bpe.SetVelocity(Location.FromBytes(e, 4 + 12));
            return bpe;
        }
    }

    public class PrimitiveEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] e)
        {
            if (e.Length < 4 + 12 + 12)
            {
                return null;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(tregion, false);
            bpe.Position = Location.FromBytes(e, 0);
            bpe.Velocity = Location.FromBytes(e, 12);
            bpe.Angles = Utilities.BytesToQuaternion(e, 12 + 12);
            bpe.Scale = Location.FromBytes(e, 12 + 12 + 16);
            bpe.Gravity = Location.FromBytes(e, 12 + 12 + 16 + 12);
            bpe.model = tregion.TheClient.Models.GetModel(tregion.TheClient.Network.Strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(e, 0, 12 + 12 + 16 + 12 + 12))));
            return bpe;
        }
    }
}
