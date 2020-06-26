namespace FriendAPI.RabbitMQ.Requests
{
    public class GetFriendStatusRabbitRequest
    {
        public string QueryingUser { get; set; }
        public string OtherUser { get; set; }
    }
}
