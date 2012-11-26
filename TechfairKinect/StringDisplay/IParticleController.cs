using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Kinect;

namespace TechfairKinect.StringDisplay
{
    internal interface IParticleController
    {
        IEnumerable<Particle> Particles { get; }
        Size Size { get; set; }

        void UpdatePhysics(double timeStep);
        void UpdateJoint(Joint joint);

        void ExplodeOut(Action onCompleted);
        void FlyIn(Action onCompleted);
    }
}
