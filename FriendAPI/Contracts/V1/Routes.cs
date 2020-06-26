namespace FriendAPI.Contracts.V1
{
    public class Routes
    {
        public const string Version = "v1";

        public const string Base = "/" + Version;

        public static class RouteRoutes
        {
            public const string GetAll = Base + "/friends";

            public const string GetFriendStatus = Base + "/friendstatus/{otherUserId}";

            public const string Create = Base + "/friend";

            public const string Delete = Base + "/friend/{recieverId}";

            public const string Update = Base + "/friend/{senderId}";
        }
    }
}
