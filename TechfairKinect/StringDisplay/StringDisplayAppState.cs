using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Kinect;
using TechfairKinect.AppState;
using TechfairKinect.Gestures;
using TechfairKinect.Components;
using TechfairKinect.Components.Particles;

namespace TechfairKinect.StringDisplay
{
    internal class StringDisplayAppState : IAppState
    {
        public RectangleF ComponentRectangle { get { return new RectangleF(0.0f, 0.0f, _appSize.Width, _appSize.Height); } }
        public ComponentType ComponentType { get { return ComponentType.StringDisplay; } }

        private Size _appSize;
        public Size AppSize
        {
            get { return _appSize; }
            set
            {
                if (_appSize == value)
                    return;
                _appSize = value;

                Components.ForEach(c => c.AppSize = value);
            }
        }

        public List<IComponent> Components { get; set; }
        public List<IComponentRenderer> Renderers { get; set; }

        private ParticleComponent _particleComponent { get; set; }

        public event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;

        public IEnumerable<Particle> Particles { get { return _particleComponent.Particles; } }

        public StringDisplayAppState(Size screenBounds)
        {
            var componentTuple = new ComponentFactory().CreateStringDisplayComponentRendererTupes(screenBounds).ToList();
            Components = componentTuple.Select(tuple => tuple.Item1).ToList();
            Renderers = componentTuple.Select(tuple => tuple.Item2).ToList();

            _particleComponent = (ParticleComponent)Components.Single(c => c.ComponentType == ComponentType.Particles);
        }

        public void ResetSkeleton()
        {
            Components.ForEach(c => c.ResetSkeleton());
        }

        public void UpdateSkeleton(Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            Components.ForEach(c => c.UpdateSkeleton(scaledSkeleton));
        }

        public void UpdatePhysics(double timeStep)
        {
            Components.ForEach(c => c.UpdatePhysics(timeStep));
        }

        public void OnGesture(GestureType gestureType)
        {
            Components.ForEach(c => c.OnGesture(gestureType));
        }

        public void OnTransitionTo()
        {
            _particleComponent.ExplodeIn(() => { });
        }

        public void OnTransitionFrom(Action onCompleted)
        {
            _particleComponent.ExplodeOut(onCompleted);
        }
    }
}
