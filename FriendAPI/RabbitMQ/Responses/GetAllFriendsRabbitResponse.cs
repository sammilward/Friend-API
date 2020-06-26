using FriendAPI.Models;
using System.Collections.Generic;

namespace FriendAPI.RabbitMQ.Responses
{
    public class GetAllFriendsRabbitResponse
    {
        public bool FoundUsers { get; set; }
        public List<User> Users { get; set; }
    }
}
