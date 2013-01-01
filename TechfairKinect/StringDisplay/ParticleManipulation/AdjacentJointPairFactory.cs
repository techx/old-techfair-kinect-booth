using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;

namespace TechfairKinect.StringDisplay.ParticleManipulation
{
    internal class AdjacentJointPairFactory
    {
        private static JointType[] AcceptableJointTypes = new[]
        {
            JointType.HandLeft, JointType.WristLeft, JointType.ElbowLeft, JointType.ShoulderLeft,
            JointType.ShoulderCenter,
            JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight, JointType.HandRight
        };

        public List<AdjacentJointPair> Create(double thresholdHeight)
        {
            return Enumerate(thresholdHeight).ToList();
        }

        private IEnumerable<AdjacentJointPair> Enumerate(double thresholdHeight)
        {
            for (int i = 0; i < AcceptableJointTypes.Length - 1; i++)
                yield return 
                    new AdjacentJointPair(
                        AcceptableJointTypes[i], 
                        AcceptableJointTypes[i + 1], 
                        thresholdHeight);
        }
    }
}
