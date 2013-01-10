using System;
using System.Collections.Generic;
using Microsoft.Kinect;

namespace TechfairKinect.Gestures.Kinect
{
    internal class SkeletonReadEventArgs : EventArgs
    {
        public int SkeletonId { get; set; }
        public Dictionary<JointType, ScaledJoint> Skeleton { get; set; }
        public long Timestamp { get; set; }

        public SkeletonReadEventArgs(int skeletonId, Dictionary<JointType, ScaledJoint> skeleton, long timestamp)
        {
            SkeletonId = skeletonId;
            Skeleton = skeleton;
            Timestamp = timestamp;
        }
    }
}
