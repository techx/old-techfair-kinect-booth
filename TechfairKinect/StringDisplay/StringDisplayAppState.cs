using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Kinect;
using TechfairKinect.AppState;
using TechfairKinect.Gestures;

namespace TechfairKinect.StringDisplay
{
    internal class StringDisplayAppState : IAppState
    {
        private IParticleController _particleController;
        private bool _ready;

        public AppStateType AppStateType { get { return AppStateType.StringDisplay; } }

        private Size _appSize;
        public Size AppSize
        {
            get { return _appSize; }
            set
            {
                if (_appSize == value)
                    return;
                _appSize = value;
                _particleController.Size = _appSize;
            }
        }

        public event EventHandler<StateChangeRequestedEventArgs> StateChangeRequested;

        public IEnumerable<Particle> Particles { get { return _particleController.Particles; } }

        public StringDisplayAppState(Size screenBounds)
        {
            _particleController = new ParticleControllerFactory(screenBounds).Create();
        }

        public void UpdatePhysics(double timeStep)
        {
            _particleController.UpdatePhysics(timeStep);
        }

        public void UpdateSkeleton(Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            if (_ready)
                _particleController.UpdateSkeleton(scaledSkeleton);
        }

        public void OnGesture(GestureType gestureType)
        {
            if (gestureType == GestureType.ExplodeOut)
                _particleController.ExplodeOut(() => System.Diagnostics.Debug.WriteLine("Exploded out"));
            if (gestureType == GestureType.ExplodeIn)
                _particleController.ExplodeIn(() => System.Diagnostics.Debug.WriteLine("Exploded in"));
        }

        public void OnTransitionTo()
        {
            _ready = false;
            _particleController.ExplodeIn(() => _ready = true);
        }

        public void OnTransitionFrom(Action onCompleted)
        {
            _particleController.ExplodeOut(onCompleted);
        }
    }
}
