namespace SsoServer.Constants
{
    public class UserRoles
    {
        /// <summary>
        /// Administrator role
        /// </summary>
        public const string Administrator = nameof(Administrator);
        /// <summary>
        /// Finance role
        /// </summary>
        public const string SuperAdministrator = nameof(SuperAdministrator);

        public const string User = nameof(User);


        /// <summary>
        /// The above roles represented in an array
        /// </summary>
        public static readonly string[] SupportedRoles = new string[] { Administrator, SuperAdministrator, User };
    }
}
