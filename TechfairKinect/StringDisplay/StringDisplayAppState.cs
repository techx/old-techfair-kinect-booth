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

        public void UpdateJoint(Joint joint)
        {
            if (_ready)
                _particleController.UpdateJoint(joint);
        }

        public void OnGesture(GestureType gestureType)
        {
            throw new NotImplementedException();
        }

        public void OnTransitionTo()
        {
            _ready = false;
            _particleController.FlyIn(() => _ready = true);
        }

        public void OnTransitionFrom(Action onCompleted)
        {
            _particleController.ExplodeOut(onCompleted);
        }
    }
}
