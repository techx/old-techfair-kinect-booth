using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Kinect;
using TechfairKinect.AppState;

namespace TechfairKinect.Gestures.Kinect
{
    class KinectSkeletonUpdater : ISkeletonUpdater
    {
        private IAppState _currentAppState;
        public IAppState CurrentAppState
        {
            get { return _currentAppState; }
            set
            {
                _currentAppState = value;
                if (_lastSkeleton != null)
                    _currentAppState.UpdateSkeleton(_lastSkeleton);
            }
        }

        private readonly KinectSensorWrapper _kinectWrapper;
        private int _lastSkeletonId;
        private Dictionary<JointType, ScaledJoint> _lastSkeleton;

        public KinectSkeletonUpdater()
        {
            _lastSkeletonId = -1;
            _lastSkeleton = null;

            _kinectWrapper = new KinectSensorWrapper();
            _kinectWrapper.OnSkeletonRead += OnSkeletonRead;
        }

        private void OnSkeletonRead(object sender, SkeletonReadEventArgs e)
        {
            if (e.SkeletonId != _lastSkeletonId)
                _currentAppState.ResetSkeleton();

            _lastSkeletonId = e.SkeletonId;
            _lastSkeleton = e.Skeleton;

            UpdateListeners();
        }

        private void UpdateListeners()
        {
            if (_currentAppState != null)
                _currentAppState.UpdateSkeleton(_lastSkeleton);
        }

        public void OnKeyPressed(KeyEventArgs keys)
        {
        }
    }
}
