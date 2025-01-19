using System.IO;

namespace GoLive.Generator.Caching.Core.EmbeddedResources;

public partial class ThisAssembly
{
    /// <summary>
    /// Provides access to embedded resources.
    /// </summary>
    public static partial class Resources
    {
        /// <summary>
        /// Provides access to embedded resources under Resources/_Resources
        /// </summary>
        public static partial class _Resources
        {
            /// <summary>
            /// => @"Resources\AdditionalFiles.cs"
            /// </summary>
            /// <remarks>
            /// GitHub Sponsors is a great way to support open source projects, and it's available throughout most of the world.
            /// You are considered a sponsor if:
            /// - You are directly sponsoring Devlooped
            /// - You contributed code to any of Devlooped's projects.
            /// - You belong to a GitHub organization that is sponsoring Devlooped.
            /// - You contributed to active and popular nuget packages that are OSS.
            /// 
            /// If so, just run: 
            ///     > dotnet tool install -g dotnet-sponsor; sponsor sync devlooped
            ///     
            /// Subsequently, the [Obsolete] attribute will be removed. 
            /// Thanks! 🙏
            /// </remarks>
            /// <see href="https://github.com/devlooped#sponsorlink"/>
            public static partial class AdditionalFiles
            {
                private static string text;
                /// <summary>
                /// Gets the resource as plain text.
                /// </summary>
                public static string Text => text ??= EmbeddedResource.GetContent(@"Resources\AdditionalFiles.cs");

                /// <summary>
                /// Gets the resource as a byte array.
                /// </summary>
                public static byte[] GetBytes() => EmbeddedResource.GetBytes(@"Resources\AdditionalFiles.cs");
                /// <summary>
                /// Gets the resource as a stream.
                /// </summary>
                public static Stream GetStream() => EmbeddedResource.GetStream(@"Resources\AdditionalFiles.cs");
            }
        }
    }
}