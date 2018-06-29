using Amazon;

namespace Hypar
{
    public static class Constants
    {
        internal const string HYPAR_CONFIG = "hypar.json";
        internal const string HYPAR_API_KEY = "xBZJyh85lq9IZnMUx2gKaRZMz8XPmRY6DCmpN8Y3";
        internal const string HYPAR_API_URL = "https://api.hypar.io/dev";
        internal static RegionEndpoint HYPAR_DEFAULT_REGION = RegionEndpoint.USWest1;
        internal const string HYPAR_IAM_ROLE_LAMBDA = "arn:aws:iam::501396736796:role/user_lambda_execution";
    }
}