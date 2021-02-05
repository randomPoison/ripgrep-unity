namespace Ripgrep.Editor
{
    /// <summary>
    /// Type of archive to use when downloading a build of Ripgrep.
    /// </summary>
    ///
    /// <remarks>
    /// This is used when manually downloading a build with <see cref="InstallOperation"/>.
    /// </remarks>
    public enum ArchiveType
    {
        /// <summary>
        /// A <c>.zip</c> archive, used for Windows.
        /// </summary>
        Zip,

        /// <summary>
        /// A <c>.tar.gz</c> archive, use for macOS and Linux.
        /// </summary>
        Tgz,
    }
}
