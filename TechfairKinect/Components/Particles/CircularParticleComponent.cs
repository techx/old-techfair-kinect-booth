using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using Microsoft.Kinect;
using TechfairKinect.Components.Particles.ParticleStringGeneration;

namespace TechfairKinect.Components.Particles
{
    internal class CircularParticleComponent : ParticleComponent
    {
        private enum ExplodingState
        { 
            NotExploding,
            ExplodingOut,
            ExplodingIn,
            Exploded
        }

        private const double RadiusMultiplier = 1.1;

        private static double InitialLayerRadius = 0.0075;
        private static double ScreenThresholdHeightPercentage = double.Parse(ConfigurationManager.AppSettings["ScreenThresholdHeightPercentage"]); 
        private static double ExplodeVelocity = double.Parse(ConfigurationManager.AppSettings["ExplodeVelocity"]);

        private readonly ParticleFactory _particleFactory;

        private List<Particle> _particles;
        public override IEnumerable<Particle> Particles
        {
            get { return _particles; }
        }

        private ExplodingState _explodingState;
        private Action _onExplodeCompleted;

        private Vector3D _leftHand;
        private Vector3D _rightHand;

        public CircularParticleComponent(Size screenBounds)
        {
            _particleFactory = new ParticleFactory();
            _particles = _particleFactory.Create(screenBounds).ToList();

            _particles.Sort((a, b) =>
            {
                var delta = a.Position.X - b.Position.X;
                return Math.Abs(delta) < double.Epsilon ? 0 :
                    (delta > double.Epsilon ? 1 : -1);
            });

            _explodingState = ExplodingState.NotExploding;
            _onExplodeCompleted = null;

            _leftHand = null;
            _rightHand = null;
        }

        public override void UpdatePhysics(double timeStep)
        {
            if (_explodingState == ExplodingState.Exploded)
            {
                _particles
                    .Where(p => p.PreviousPositions.Count > 0)
                    .ToList()
                    .ForEach(p => 
                        {
                            lock (p.PreviousPositions) 
                                p.PreviousPositions.RemoveLast();
                        });
                return;
            }

            if (_explodingState == ExplodingState.ExplodingIn)
                CheckExplodingInCompleted();
            else if (_explodingState == ExplodingState.ExplodingOut)
                CheckExplodingOutCompleted();

            UpdateParticleCenters(timeStep);
        }

        private void UpdateParticleCenters(double timeStep)
        {
            lock (_particles)
            {
                if (_leftHand == null || _rightHand == null ||
                    (_leftHand.Y < ScreenThresholdHeightPercentage && _rightHand.Y < ScreenThresholdHeightPercentage))
                {
                    _particles.ForEach(p => p.Update(timeStep));
                    return;
                }

                if (_leftHand.Y > ScreenThresholdHeightPercentage)
                {
                    if (_rightHand.Y > ScreenThresholdHeightPercentage)
                    {
                        UpdateParticlesByLayer(0, _particles.Count / 2, new Vector3D(_leftHand.X, 1 - _leftHand.Y, _leftHand.Z), timeStep);
                        UpdateParticlesByLayer(_particles.Count / 2, _particles.Count, new Vector3D(_rightHand.X, 1 - _rightHand.Y, _rightHand.Z), timeStep);
                    }
                    else
                        UpdateParticlesByLayer(0, _particles.Count, new Vector3D(_leftHand.X, 1 - _leftHand.Y, _leftHand.Z), timeStep);
                }
                else //_rightHand.Y > ScreenThresholdHeightPercentage
                    UpdateParticlesByLayer(0, _particles.Count, new Vector3D(_rightHand.X, 1 - _rightHand.Y, _rightHand.Z), timeStep);
            }
        }

        private void CheckExplodingInCompleted()
        {
            if (_particles.All(p => Math.Abs(p.Velocity.X) < 1E-10 &&
                                    Math.Abs(p.Velocity.Y) < 1E-10))
            {
                _explodingState = ExplodingState.NotExploding;

                _onExplodeCompleted();
            }
        }

        private void CheckExplodingOutCompleted()
        {
            if (_particles.All(p => (p.Position.X < 0 || p.Position.X > 1.0) || 
                                    (p.Position.Y < 0 || p.Position.Y > 1.0)))
            {
                _explodingState = ExplodingState.Exploded;
                _particles.ForEach(p => p.Velocity = Vector3D.Zero); //stop them from getting too far away

                _onExplodeCompleted();
            }
        }

        public override void ResetSkeleton()
        {
            ResetParticles();
        }

        public override void UpdateSkeleton(Dictionary<JointType, ScaledJoint> scaledSkeleton)
        {
            _leftHand = scaledSkeleton[JointType.HandLeft].LocationScreenPercent;
            _rightHand = scaledSkeleton[JointType.HandRight].LocationScreenPercent;

            if (_leftHand.Y < ScreenThresholdHeightPercentage && _rightHand.Y < ScreenThresholdHeightPercentage)
                ResetParticles();
        }

        private Vector3D ScaleCenter(Vector3D center)
        {
            return new Vector3D(
                center.X, 
                center.Y / (1 - ScreenThresholdHeightPercentage), 
                center.Z);
        }

        private void UpdateParticlesByLayer(int startIndex, int endIndex, Vector3D center, double timeStep)
        {
            center = ScaleCenter(center);

            int mid = (startIndex + endIndex) / 2;
            UpdateParticle(_particles[mid], center, timeStep);

            int left = mid - 1, right = mid + 1;

            double currentRadius = InitialLayerRadius;

            while (left >= startIndex || right < endIndex)
            {
                currentRadius *= RadiusMultiplier;

                int particles = (int)(Math.PI / (4 * Math.Asin(InitialLayerRadius / currentRadius)));

                int newLeft = left - Math.Min(left - startIndex, particles);
                int newRight = right + Math.Min(endIndex - right - 1, particles);

                ActivateLayer(center, currentRadius, newLeft, left, right, newRight, timeStep);

                left = newLeft - 1; right = newRight + 1;
            }
        }

        private void ActivateLayer(Vector3D center, double layerRadius, int leftStart, int leftEnd, int rightStart, int rightEnd, double timeStep)
        {
            double anglePerCircle = Math.PI / (leftEnd - leftStart + 1);

            for (int i = leftStart; i <= leftEnd; i++)
                UpdateParticle(_particles[i], CalculatePosition(center, layerRadius, anglePerCircle * (i - leftStart)), timeStep);

            anglePerCircle = Math.PI / (rightEnd - rightStart + 1);

            for (int i = rightStart; i <= rightEnd; i++)
                UpdateParticle(_particles[i], CalculatePosition(center, layerRadius, Math.PI + anglePerCircle * (i - rightStart)), timeStep);
        }

        private void UpdateParticle(Particle particle, Vector3D center, double timeStep)
        {
            particle.ProjectedCenter = center;
            particle.Update(timeStep);
        }

        private Vector3D CalculatePosition(Vector3D center, double radius, double angle)
        {
            return center + radius * new Vector3D(Math.Cos(angle) * AppSize.Height / AppSize.Width, Math.Sin(angle), 0);
        }

        private void ResetParticles()
        {
            for (int i = 0; i < _particles.Count; i++)
                _particles[i].ProjectedCenter = _particles[i].DeactivatedCenter;
        }

        public override void ExplodeOut(Action onCompleted)
        {
            if (_explodingState == ExplodingState.Exploded ||
                _explodingState == ExplodingState.ExplodingOut)
            {
                onCompleted();
                return;
            }

            _onExplodeCompleted = onCompleted;
            _explodingState = ExplodingState.ExplodingOut;

            for (int i = 0; i < _particles.Count; i++)
            {
                var particle = _particles[i];
                particle.Exploding = true;

                var center = new Vector3D(0.5, 0.5, 0.0);

                var unitVector = particle.Position.X == center.X && particle.Position.Y == center.Y ? //TODO: change for 3d
                    new Vector3D(0, 1.0, 0) : (particle.Position - center).UnitVector();

                particle.Velocity = unitVector * ExplodeVelocity;
            }
        }

        public override void ExplodeIn(Action onCompleted)
        {
            if (_explodingState == ExplodingState.NotExploding || 
                _explodingState == ExplodingState.ExplodingIn)
            {
                onCompleted();
                return;
            }

            _onExplodeCompleted = onCompleted;
            _explodingState = ExplodingState.ExplodingIn;

            for (int i = 0; i < _particles.Count; i++)
            {
                _particles[i].Exploding = false;
                _particles[i].Velocity = Vector3D.Zero;
            }
        }
    }
}
