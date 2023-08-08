namespace SsoServer.Constants
{
    public class UserRoles
    {
        /// <summary>
        /// Administrator role
        /// </summary>
        public static readonly string Administrator = nameof(Administrator);
        /// <summary>
        /// Finance role
        /// </summary>
        public static readonly string SuperAdministrator = nameof(SuperAdministrator);

        public static readonly string Viewer = nameof(Viewer);

        public static readonly string Editor = nameof(Editor);

        /// <summary>
        /// The above roles represented in an array
        /// </summary>
        public static readonly string[] SupportedRoles = new string[] { Administrator, SuperAdministrator, Viewer, Editor };
    }
}
