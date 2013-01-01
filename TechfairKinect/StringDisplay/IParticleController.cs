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
        void UpdateSkeleton(Dictionary<JointType, ScaledJoint> scaledSkeleton);

        void ExplodeOut(Action onCompleted);
        void ExplodeIn(Action onCompleted);
    }
}
