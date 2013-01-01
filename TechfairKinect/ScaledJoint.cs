using Microsoft.Kinect;

namespace TechfairKinect
{
    internal class ScaledJoint
    {
        public JointType JointType { get; set; }
        public Vector3D LocationScreenPercent { get; set; }

        public override string ToString()
        {
            return string.Format("{{{0}, {1}, {2}}}", LocationScreenPercent.X, LocationScreenPercent.Y, LocationScreenPercent.Z);
        }
    }
}
