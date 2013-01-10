using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Kinect;
using TechfairKinect.AppState;

namespace TechfairKinect.Gestures.Keyboard
{
    internal class KeyboardGestureRecognizer : IGestureRecognizer
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
        
        private readonly Dictionary<JointType, ScaledJoint> _currentSkeleton;
        private JointType _currentJoint;

        public KeyboardGestureRecognizer()
        {
            _currentJoint = JointType.HandLeft;
            _currentSkeleton = new KeyboardSkeletonFactory().CreateInitialSkeleton();
        }

        public void OnKeyPressed(KeyEventArgs keys)
        {
            var key = keys.KeyCode;

            if (key == ExplodeOut)
                _currentAppState.OnGesture(GestureType.ExplodeOut);
            if (key == ExplodeIn)
                _currentAppState.OnGesture(GestureType.ExplodeIn);

            if (JointsByKeySelector.ContainsKey(key))
                UpdateCurrentJoint(key);

            if (MovementVectorsByKey.ContainsKey(key))
                PerformMovement(key);

            UpdateListeners();
        }

        private void UpdateCurrentJoint(Keys jointKey)
        {
            _currentJoint = JointsByKeySelector[jointKey];
        }

        private void PerformMovement(Keys movementKey)
        {
            var newPosition = _currentSkeleton[_currentJoint].LocationScreenPercent + MovementVectorsByKey[movementKey];

            if (newPosition.X > 1.0 || newPosition.X < 0 ||
                newPosition.Y > 1.0 || newPosition.Y < 0)
                return;

            _currentSkeleton[_currentJoint].LocationScreenPercent = newPosition;
        }

        private void UpdateListeners()
        {
            if (_currentAppState != null)
                _currentAppState.UpdateSkeleton(_currentSkeleton);
        }
    }
}
