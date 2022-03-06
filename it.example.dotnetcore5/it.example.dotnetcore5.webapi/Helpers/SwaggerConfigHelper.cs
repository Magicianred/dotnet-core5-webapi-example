using System;

namespace it.example.dotnetcore5.webapi.Helpers
{
    public class SwaggerConfigHelper
    {
        public enum VersioningType
        {
            None, CustomHeader, QueryString, AcceptHeader
        }
        public static String QueryStringParam { get; private set; }
        public static String CustomHeaderParam { get; private set; }
        public static String AcceptHeaderParam { get; private set; }
        public static VersioningType CurrentVersioningMethod { get => currentVersioningMethod; set => currentVersioningMethod = value; }

        private const VersioningType none = VersioningType.None;
        private static VersioningType currentVersioningMethod = none;

        public static void UseCustomHeaderApiVersion(string parameterName)
        {
            CurrentVersioningMethod = VersioningType.CustomHeader;
            CustomHeaderParam = parameterName;
        }

        public static void UseQueryStringApiVersion()
        {
            QueryStringParam = "api-version";
            CurrentVersioningMethod = VersioningType.QueryString;
        }
        public static void UseQueryStringApiVersion(string parameterName)
        {
            CurrentVersioningMethod = VersioningType.QueryString;
            QueryStringParam = parameterName;
        }
        public static void UseAcceptHeaderApiVersion(String paramName)
        {
            CurrentVersioningMethod = VersioningType.AcceptHeader;
            AcceptHeaderParam = paramName;
        }
    }
}
