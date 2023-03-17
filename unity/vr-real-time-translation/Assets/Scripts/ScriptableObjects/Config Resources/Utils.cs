using System.Collections.Generic;

namespace Amazon.Config
{
    public static class Utils
    {
        public static readonly Dictionary<AwsRegion, Amazon.RegionEndpoint> RegionLookup = new Dictionary<AwsRegion, Amazon.RegionEndpoint>()
        {
            {AwsRegion.USEast1, Amazon.RegionEndpoint.USEast1},
            {AwsRegion.USEast2, Amazon.RegionEndpoint.USEast2},
            {AwsRegion.USWest1, Amazon.RegionEndpoint.USWest1},
            {AwsRegion.USWest2, Amazon.RegionEndpoint.USWest2},
            {AwsRegion.CACentral1, Amazon.RegionEndpoint.CACentral1},
            {AwsRegion.EUNorth1, Amazon.RegionEndpoint.EUNorth1},
            {AwsRegion.EUSouth1, Amazon.RegionEndpoint.EUSouth1},
            {AwsRegion.EUWest1, Amazon.RegionEndpoint.EUWest1},
            {AwsRegion.EUWest2, Amazon.RegionEndpoint.EUWest2},
            {AwsRegion.EUWest3, Amazon.RegionEndpoint.EUWest3},
            {AwsRegion.EUCentral1, Amazon.RegionEndpoint.EUCentral1},
            {AwsRegion.APNortheast1, Amazon.RegionEndpoint.APNortheast1},
            {AwsRegion.APNortheast2, Amazon.RegionEndpoint.APNortheast2},
            {AwsRegion.APNortheast3, Amazon.RegionEndpoint.APNortheast3},
            {AwsRegion.APEast1, Amazon.RegionEndpoint.APEast1},
            {AwsRegion.APSoutheast1, Amazon.RegionEndpoint.APSoutheast1},
            {AwsRegion.APSoutheast2, Amazon.RegionEndpoint.APSoutheast2},
            {AwsRegion.APSoutheast3, Amazon.RegionEndpoint.APSoutheast3},
            {AwsRegion.APSouth1, Amazon.RegionEndpoint.APSouth1},
            {AwsRegion.AFSouth1, Amazon.RegionEndpoint.AFSouth1},
            {AwsRegion.SAEast1, Amazon.RegionEndpoint.SAEast1},
            {AwsRegion.MESouth1, Amazon.RegionEndpoint.MESouth1},
        };
    }
}