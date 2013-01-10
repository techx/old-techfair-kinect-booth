using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using TechfairKinect.Gestures;

namespace TechfairKinect.Components.Particles
{
    internal abstract class ParticleComponent : IComponent
    {
        public Size AppSize { get; set; }
        public ComponentType ComponentType { get { return ComponentType.Particles; } }

        public abstract IEnumerable<Particle> Particles { get; }

        public abstract void UpdateSkeleton(Dictionary<JointType, ScaledJoint> skeleton);
        public abstract void ResetSkeleton();

        public abstract void UpdatePhysics(double timeStep);

        public abstract void ExplodeOut(Action onCompleted);
        public abstract void ExplodeIn(Action onCompleted);

        public void OnGesture(GestureType gestureType)
        { 
            if (gestureType == GestureType.ExplodeIn)
                ExplodeIn(() => { });
            if (gestureType == GestureType.ExplodeOut)
                ExplodeOut(() => { });
        }
    }
}
