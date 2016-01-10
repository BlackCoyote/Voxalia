﻿using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.OtherSystems;
using BEPUutilities;
using BEPUphysics.Constraints.TwoEntity.JointLimits;
using BEPUphysics.Constraints.TwoEntity.Motors;

namespace Voxalia.ServerGame.EntitySystem
{
    public class VehicleEntity: ModelEntity, EntityUseable
    {
        public string vehName;
        public Seat DriverSeat;
        public List<JointVehicleMotor> DrivingMotors = new List<JointVehicleMotor>();
        public List<JointVehicleMotor> SteeringMotors = new List<JointVehicleMotor>();

        public VehicleEntity(string vehicle, Region tregion)
            : base("vehicles/" + vehicle + "_base", tregion)
        {
            vehName = vehicle;
            SetMass(500);
            DriverSeat = new Seat(this, Location.UnitZ * 2);
            Seats = new List<Seat>();
            Seats.Add(DriverSeat);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.VEHICLE;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public bool hasWheels = false;

        List<Model3DNode> GetNodes(Model3DNode node)
        {
            List<Model3DNode> nodes = new List<Model3DNode>();
            nodes.Add(node);
            if (node.Children.Count > 0)
            {
                for (int i = 0; i < node.Children.Count; i++)
                {
                    nodes.AddRange(GetNodes(node.Children[i]));
                }
            }
            return nodes;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            HandleWheels();
        }

        public void HandleWheels()
        {
            if (!hasWheels)
            {
                Model mod = TheServer.Models.GetModel(model);
                if (mod == null) // TODO: mod should return a cube when all else fails?
                {
                    // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                    return;
                }
                Model3D scene = mod.Original;
                if (scene == null) // TODO: Scene should return a cube when all else fails?
                {
                    // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                    return;
                }
                SetOrientation(Quaternion.Identity); // TODO: Remove need for this!
                List<Model3DNode> nodes = GetNodes(scene.RootNode);
                for (int i = 0; i < nodes.Count; i++)
                {
                    string name = nodes[i].Name.ToLowerInvariant();
                    if (name.Contains("wheel"))
                    {
                        Matrix mat = nodes[i].MatrixA;
                        Model3DNode tnode = nodes[i].Parent;
                        while (tnode != null)
                        {
                            mat = tnode.MatrixA * mat;
                            tnode = tnode.Parent;
                        }
                        Location pos = GetPosition() + new Location(mat.M14, mat.M34, -mat.M44); // TODO: Why are the matrices transposed?! // TODO: Why are Y and Z flipped?!
                        VehiclePartEntity wheel = new VehiclePartEntity(TheRegion, "vehicles/" + vehName + "_wheel");
                        wheel.SetPosition(pos);
                        wheel.SetOrientation(Quaternion.Identity); // TOOD: orient
                        wheel.Gravity = Gravity;
                        wheel.CGroup = CGroup;
                        wheel.SetMass(5);
                        wheel.mode = ModelCollisionMode.CONVEXHULL;
                        TheRegion.SpawnEntity(wheel);
                        if (name.After("wheel").StartsWith("f"))
                        {
                            SteeringMotors.Add(ConnectWheel(wheel, false, true));
                        }
                        else if (name.After("wheel").StartsWith("b"))
                        {
                            DrivingMotors.Add(ConnectWheel(wheel, true, true));
                        }
                        else
                        {
                            ConnectWheel(wheel, true, false);
                        }
                        Vector3 angvel = new Vector3(10, 0, 0);
                        wheel.Body.ApplyAngularImpulse(ref angvel);
                        wheel.Body.ActivityInformation.Activate();
                    }
                }
                hasWheels = true;
            }
        }

        public JointVehicleMotor ConnectWheel(VehiclePartEntity wheel, bool driving, bool powered)
        {
            wheel.SetFriction(2.5f);
            Vector3 left = Quaternion.Transform(new Vector3(-1, 0, 0), wheel.GetOrientation());
            //Vector3 forward = Quaternion.Transform(new BEPUutilities.Vector3(0, 1, 0), wheel.GetOrientation());
            Vector3 up = Quaternion.Transform(new Vector3(0, 0, 1), wheel.GetOrientation());
            JointSlider pointOnLineJoint = new JointSlider(this, wheel, -new Location(up));
            JointLAxisLimit suspensionLimit = new JointLAxisLimit(this, wheel, 0f, 0.1f, wheel.GetPosition(), wheel.GetPosition(), -new Location(up));
            JointPullPush spring = new JointPullPush(this, wheel, -new Location(up), true);
            //SwivelHingeAngularJoint swivelHingeAngularJoint = new SwivelHingeAngularJoint(body, wheel, Vector3.Up, Vector3.Right);
            JointSpinner spinner = new JointSpinner(this, wheel, new Location(-left));
            TheRegion.AddJoint(pointOnLineJoint);
            TheRegion.AddJoint(suspensionLimit);
            TheRegion.AddJoint(spring);
            TheRegion.AddJoint(spinner);
            if (powered)
            {
                JointVehicleMotor motor = new JointVehicleMotor(this, wheel, new Location(driving ? left : up), !driving);
                TheRegion.AddJoint(motor);
                return motor;
            }
            return null;
        }

        public bool Use(Entity user)
        {
            if (user.CurrentSeat == DriverSeat)
            {
                DriverSeat.Kick();
                return true;
            }
            return DriverSeat.Accept((PhysicsEntity)user);
        }

        public void HandleInput(PlayerEntity player)
        {
            if (player.Forward)
            {
                foreach (JointVehicleMotor motor in DrivingMotors)
                {
                    motor.Motor.Settings.VelocityMotor.GoalVelocity = 45f;
                }
            }
            else if (player.Backward)
            {
                foreach (JointVehicleMotor motor in DrivingMotors)
                {
                    motor.Motor.Settings.VelocityMotor.GoalVelocity = -45f;
                }
            }
            else
            {
                foreach (JointVehicleMotor motor in DrivingMotors)
                {
                    motor.Motor.Settings.VelocityMotor.GoalVelocity = 0f;
                }
            }
            if (player.Rightward)
            {
                foreach (JointVehicleMotor motor in SteeringMotors)
                {
                    motor.Motor.Settings.Servo.Goal = -20f;
                }
            }
            else if (player.Leftward)
            {
                foreach (JointVehicleMotor motor in SteeringMotors)
                {
                    motor.Motor.Settings.Servo.Goal = 20f;
                }
            }
            else
            {
                foreach (JointVehicleMotor motor in SteeringMotors)
                {
                    motor.Motor.Settings.Servo.Goal = 0f;
                }
            }
        }
    }
}
