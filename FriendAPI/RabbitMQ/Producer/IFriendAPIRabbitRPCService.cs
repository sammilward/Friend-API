using System.Threading.Tasks;

namespace FriendAPI.RabbitMQ.Producer
{
    public interface IFriendAPIRabbitRPCService
    {
        Task<T> PublishRabbitMessageWaitForResponseAsync<T>(string method, object requestModel);
    }
}
