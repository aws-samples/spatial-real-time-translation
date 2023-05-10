using UnityEngine;

namespace Amazon.Config
{
    [CreateAssetMenu(fileName = "AWSConfig", menuName = "AWS/AWS Config", order = 0)]
    public class AWSConfig : ScriptableObject
    {
        [SerializeField]
        private AwsRegion _region = AwsRegion.USWest2;

        public Amazon.RegionEndpoint Region
        {
            get
            {
                if (Utils.RegionLookup.TryGetValue(_region, out Amazon.RegionEndpoint region))
                {
                    return region;
                }
                return Amazon.RegionEndpoint.USEast1;
            }
        }

        [Header("Resources")]
        public Cognito Cognito;
    }
}



