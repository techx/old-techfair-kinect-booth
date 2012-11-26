using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using Microsoft.Kinect;
using TechfairKinect.StringDisplay.ParticleStringGeneration;

namespace TechfairKinect.StringDisplay
{
    internal class CircularParticleController : IParticleController
    {

        private static JointType[] UsableJoints = new[]
        {
            JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
            JointType.ShoulderCenter,
            JointType.HandRight, JointType.WristRight, JointType.ElbowRight, JointType.ShoulderRight
        };

        private Dictionary<JointType, Tuple<int, int>> _jointParticleIntervals; //particles are referenced by their x-coordinate

        private List<Particle> _particles;
        private readonly ParticleFactory _particleFactory;

        public IEnumerable<Particle> Particles
        {
            get { return _particles; }
        }

        private Size _size;
        public Size Size 
        {
            get { return _size; }
            set
            {
                _particles = _particleFactory.Create(value).ToList();
                UsableJoints.ToList().ForEach(joint => _jointParticleIntervals[joint] = null);
                _size = value;
            }
        }

        public CircularParticleController(Size screenBounds)
        {
            _jointParticleIntervals = new Dictionary<JointType, Tuple<int, int>>();

            _particleFactory = new ParticleFactory();
            _particles = _particleFactory.Create(screenBounds).ToList();
        }

        public void UpdatePhysics(double timeStep)
        {
            _particles.ForEach(particle => particle.Update(timeStep));
        }

        public void UpdateJoint(Joint joint)
        {
            if (_size == null)
                return;

            if (!UsableJoints.Contains(joint.JointType))
                return;

            /*if (_jointParticleIntervals[joint.JointType] != null)
                UpdateParticles(_jointParticleIntervals[joint.JointType]);
            else
                LockParticles(_jointParticleIntervals[joint.JointType]);
            throw new NotImplementedException();*/
        }

        public void ExplodeOut(Action onCompleted)
        {
            throw new NotImplementedException();
        }

        public void FlyIn(Action onCompleted)
        {
            //throw new NotImplementedException();
        }
    }
}
