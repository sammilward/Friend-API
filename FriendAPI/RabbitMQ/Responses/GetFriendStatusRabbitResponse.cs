using FriendAPI.Models;

namespace FriendAPI.RabbitMQ.Responses
{
    public class GetFriendStatusRabbitResponse
    {
        public FriendStatusEnum FriendStatus { get; set; }
        public bool Successful { get; set; }
    }
}
