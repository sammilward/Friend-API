namespace FriendAPI.RabbitMQ.Requests
{
    public class ResponseToFriendRabbitRequest
    {
        public string SenderId { get; set; }
        public string RecieverId { get; set; }
        public bool? Accept { get; set; }
        public bool? Reject { get; set; }
    }
}
