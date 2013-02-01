using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Kinect;
using TechfairKinect.AppState;
using System;

namespace TechfairKinect.Gestures.Keyboard
{
    internal class KeyboardSkeletonUpdater : ISkeletonUpdater
    {
        private static Dictionary<Keys, JointType> JointsByKeySelector = new Dictionary<Keys, JointType>
        {
            { Keys.T, JointType.Head },
            
            { Keys.A, JointType.HandLeft },
            { Keys.S, JointType.WristLeft },
            { Keys.D, JointType.ElbowLeft },
            { Keys.F, JointType.ShoulderLeft },
            
            { Keys.G, JointType.ShoulderCenter },
            { Keys.H, JointType.Spine },

            { Keys.J, JointType.ShoulderRight },
            { Keys.K, JointType.ElbowRight },
            { Keys.L, JointType.WristRight },
            { Keys.OemSemicolon, JointType.HandRight },

            { Keys.Z, JointType.FootLeft },
            { Keys.X, JointType.AnkleLeft },
            { Keys.C, JointType.KneeLeft },
            
            { Keys.V, JointType.HipLeft },
            { Keys.B, JointType.HipCenter },
            { Keys.N, JointType.HipRight },

            { Keys.M, JointType.KneeRight },
            { Keys.Oemcomma, JointType.AnkleRight },
            { Keys.OemPeriod, JointType.FootRight }
        };

        private const double Scale = 5;
        private static Dictionary<Keys, Vector3D> MovementVectorsByKey = new Dictionary<Keys, Vector3D>
        {
            { Keys.Up, new Vector3D(0, 0.01, 0) },
            { Keys.Down, new Vector3D(0, -0.01, 0) },

            { Keys.Left, new Vector3D(-0.01, 0, 0) },
            { Keys.Right, new Vector3D(0.01, 0, 0) },
            

            { Keys.NumPad1, new Vector3D(0, 0, 0.01) }, //both numpad and regular num support
            { Keys.D1, new Vector3D(0, 0, 0.01) },
            { Keys.NumPad2, new Vector3D(0, 0, -0.01) },
            { Keys.D2, new Vector3D(0, 0, -0.01) }
        };

        private static Keys ExplodeOut = Keys.End;
        private static Keys ExplodeIn = Keys.Home;

        private IAppState _currentAppState;
        public IAppState CurrentAppState 
        {
            get { return _currentAppState; }
            set
            {
                _currentAppState = value;
                _currentAppState.UpdateSkeleton(_currentSkeleton);
            }
        }

        private const double TotalGestureIndices = 8;
        private const double RadiansPerGestureIndex = Math.PI / TotalGestureIndices;

        private readonly GestureRecognizer _gestureRecognizer;
        private int _gestureIndex;
        
        private readonly Dictionary<JointType, ScaledJoint> _currentSkeleton;
        private List<JointType> _currentJoints;

        public KeyboardSkeletonUpdater()
        {
            _gestureIndex = -1;
            
            _gestureRecognizer = new GestureRecognizer();
            _gestureRecognizer.OnGesture += (sender, args) =>
                { 
                    if (_currentAppState != null) 
                        _currentAppState.OnGesture(args.GestureType); 
                };

            _currentJoints = new List<JointType>() { JointType.HandLeft };
            _currentSkeleton = new KeyboardSkeletonFactory().CreateInitialSkeleton();
        }

        public void OnKeyPressed(KeyEventArgs keys)
        {
            var key = keys.KeyCode;

            if (key == Keys.Tab)
                PerformGesture(!keys.Shift);
            else
                PerformNormalKey(key, keys.Shift);

            UpdateListeners();
        }

        private void PerformGesture(bool forward)
        {
            var direction = forward ? 1 : -1;

            if (_gestureIndex + direction < 0 || _gestureIndex + direction > TotalGestureIndices)
                return;

            _gestureIndex += direction;


            var shoulderElbowLength = CalculateJointPairLength(JointType.ShoulderLeft, JointType.ElbowLeft);
            var elbowWristLength = CalculateJointPairLength(JointType.ElbowLeft, JointType.WristLeft);
            var wristHandLength = CalculateJointPairLength(JointType.WristLeft, JointType.HandLeft);

            var angleLeft = Math.PI - (RadiansPerGestureIndex * _gestureIndex - Math.PI / 2);
            var shoulderLeftPos = _currentSkeleton[JointType.ShoulderLeft].LocationScreenPercent;

            SetJointPosition(JointType.ElbowLeft, 
                shoulderLeftPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength, angleLeft));
            SetJointPosition(JointType.WristLeft, 
                shoulderLeftPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength + elbowWristLength, angleLeft));
            SetJointPosition(JointType.HandLeft, 
                shoulderLeftPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength + elbowWristLength + wristHandLength, angleLeft));

            var angleRight = Math.PI - angleLeft;
            var shoulderRightPos = _currentSkeleton[JointType.ShoulderRight].LocationScreenPercent;

            SetJointPosition(JointType.ElbowRight,
                shoulderRightPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength, angleRight));
            SetJointPosition(JointType.WristRight,
                shoulderRightPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength + elbowWristLength, angleRight));
            SetJointPosition(JointType.HandRight,
                shoulderRightPos + Vector3D.FromMagnitudeAngle(shoulderElbowLength + elbowWristLength + wristHandLength, angleRight));
        }

        private double CalculateJointPairLength(JointType lhs, JointType rhs)
        {
            return (_currentSkeleton[lhs].LocationScreenPercent - _currentSkeleton[rhs].LocationScreenPercent).Magnitude();
        }

        private void SetJointPosition(JointType joint, Vector3D position)
        {
            _currentSkeleton[joint].LocationScreenPercent = position;
        }

        private void PerformNormalKey(Keys key, bool shift)
        {
            if (key == ExplodeOut)
                _currentAppState.OnGesture(GestureType.ExplodeOut);
            if (key == ExplodeIn)
                _currentAppState.OnGesture(GestureType.ExplodeIn);

            if (JointsByKeySelector.ContainsKey(key))
            {
                lock (_currentJoints)
                {
                    if (!shift)
                        _currentJoints.Clear();
                    AddSelectedJoint(key);
                }
            }

            if (MovementVectorsByKey.ContainsKey(key))
                PerformMovement(key);
        }

        private void AddSelectedJoint(Keys jointKey)
        {
            _currentJoints.Add(JointsByKeySelector[jointKey]);
        }

        private void PerformMovement(Keys movementKey)
        {
            lock (_currentJoints)
            {
                foreach (var joint in _currentJoints)
                {
                    var newPosition = _currentSkeleton[joint].LocationScreenPercent + Scale * MovementVectorsByKey[movementKey];

                    if (newPosition.X > 1.0 || newPosition.X < 0 ||
                        newPosition.Y > 1.0 || newPosition.Y < 0)
                        return;

                    _currentSkeleton[joint].LocationScreenPercent = newPosition;
                }
            }
        }

        private void UpdateListeners()
        {
            if (_currentAppState != null)
                _currentAppState.UpdateSkeleton(_currentSkeleton);
            _gestureRecognizer.UpdateSkeleton(_currentSkeleton);
        }
    }
}
