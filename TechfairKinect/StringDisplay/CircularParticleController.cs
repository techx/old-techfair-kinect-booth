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
        private const string ParticleStringSettingsKey = "ParticleString";
        private const string ParticleRadiusSettingsKey = "ParticleRadius";

        private static JointType[] UsableJoints = new[]
        {
            JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
            JointType.ShoulderCenter,
            JointType.HandRight, JointType.WristRight, JointType.ElbowRight, JointType.ShoulderRight
        };

        private Dictionary<JointType, Tuple<int, int>> _jointParticleIntervals; //particles are referenced by their x-coordinate

        private readonly ParticleStringGenerator _particleStringGenerator;
        private List<Particle> _particles;

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
                _particles = _particleStringGenerator.GenerateParticles(value).ToList();
                UsableJoints.ToList().ForEach(joint => _jointParticleIntervals[joint] = null);
                _size = value;
            }
        }

        public CircularParticleController(Size screenBounds)
        {
            _jointParticleIntervals = new Dictionary<JointType, Tuple<int, int>>();

            _particleStringGenerator = new ParticleStringGenerator(GetParticleString(), GetParticleRadius());
            _particles = _particleStringGenerator.GenerateParticles(screenBounds).ToList();
        }

        private static string GetParticleString()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(ParticleStringSettingsKey))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in settings", ParticleStringSettingsKey));

            return ConfigurationManager.AppSettings[ParticleStringSettingsKey];
        }

        private static double GetParticleRadius()
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(ParticleRadiusSettingsKey))
                throw new ConfigurationErrorsException(string.Format("Key {0} not found in settings", ParticleRadiusSettingsKey));

            var value = ConfigurationManager.AppSettings[ParticleRadiusSettingsKey];
            double radius;

            if (!double.TryParse(value, out radius))
                throw new ConfigurationErrorsException(string.Format("Invalid value \"{0}\" for settings key {1} (expected double)", value, ParticleRadiusSettingsKey));

            return radius;
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
