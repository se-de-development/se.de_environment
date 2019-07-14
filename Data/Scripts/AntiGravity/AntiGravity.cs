using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Digi.Scripts
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class GravityExample : MySessionComponentBase
    {
        private const string PLANET_NAME = "Anti_G";
        private const double PLANET_GRAVITY_ACCEL = -19.62;
        private const float PLANET_GRAVITY_RADIUS = 9862f;
        private const int PLANET_SCAN_TICKS = 60 * 30; // scan for new planets every 30 seconds

        private int skip = PLANET_SCAN_TICKS; // scan for planets on the first update
        private readonly List<MyEntity> ents = new List<MyEntity>();
        private readonly HashSet<MyPlanet> planets = new HashSet<MyPlanet>();

        public override void UpdateBeforeSimulation()
        {
            try
            {
                if(MyAPIGateway.Session == null)
                    return;

                if(++skip >= PLANET_SCAN_TICKS)
                {
                    skip = 0;
                    planets.Clear();
                    var ents = MyEntities.GetEntities();
                    foreach(var ent in ents)
                    {
                        var planet = ent as MyPlanet;

                        if(planet != null && planet.StorageName.StartsWith(PLANET_NAME))
                            planets.Add(planet);
                    }
                }

                if(planets.Count == 0)
                    return;

                var camPos = MyAPIGateway.Session.Camera.WorldMatrix.Translation;
                var sphere = new BoundingSphereD(Vector3D.Zero, PLANET_GRAVITY_RADIUS);

                foreach(var planet in planets)
                {
                    if(!MyAPIGateway.Session.IsServer && Vector3D.DistanceSquared(camPos, planet.WorldMatrix.Translation) < 1000 * 1000)
                        continue;

                    ents.Clear();
                    sphere.Center = planet.WorldMatrix.Translation;
                    MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref sphere, ents, MyEntityQueryType.Dynamic);

                    foreach(var ent in ents)
                    {
                        if(ent.Physics == null || ent.Physics.IsStatic || ent.IsPreview)
                            continue;

                        var character = ent as IMyCharacter;

                        if(character != null)
                        {
                            if(character.CurrentMovementState != MyCharacterMovementEnum.Jump
                            && character.CurrentMovementState != MyCharacterMovementEnum.Falling
                            && character.CurrentMovementState != MyCharacterMovementEnum.Died
                            && !character.EnabledThrusts
                            && character.Physics.Gravity.LengthSquared() < 0.001f)
                            {
							}
							else
							{	
                                var dir = (sphere.Center - ent.WorldMatrix.Translation);
                                dir.Normalize();
                                var forceLocal = Vector3D.TransformNormal(dir * ent.Physics.Mass * PLANET_GRAVITY_ACCEL, character.WorldMatrixInvScaled);
                                ent.Physics.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, forceLocal, null, null);
                            }
                        }
                        else
                        {
                            var dir = (sphere.Center - ent.Physics.CenterOfMassWorld);
                            dir.Normalize();
                            ent.Physics.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, dir * ent.Physics.Mass * PLANET_GRAVITY_ACCEL, null, null);
                        }
                    }
                }

                ents.Clear();
            }
            catch(Exception e)
            {
                MyAPIGateway.Utilities.ShowNotification("[ Error in " + GetType().FullName + ": " + e.Message + " ]", 10000, MyFontEnum.Red);
                MyLog.Default.WriteLine(e);
            }
        }
    }
}