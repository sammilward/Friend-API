using FriendAPI.Contracts.V1;
using FriendAPI.Contracts.V1.Requests;
using FriendAPI.RabbitMQ.Producer;
using FriendAPI.RabbitMQ.Requests;
using FriendAPI.RabbitMQ.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace FriendAPI.Controllers
{
    [ApiController]
    public class FriendController : ControllerBase
    { 
        private readonly ILogger<FriendController> _logger;
        private readonly IFriendAPIRabbitRPCService _friendAPIRabbitRPCService;

        private const string UserIdClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private const string GetAllFriendsMethod = "GetAllFriends";
        private const string GetFriendStatusMethod = "GetFriendStatus";
        private const string CreateFriendMethod = "CreateFriend";
        private const string DeleteFriendMethod = "DeleteFriend";
        private const string UpdateFriendMethod = "UpdateFriend";

        public FriendController(ILogger<FriendController> logger, IFriendAPIRabbitRPCService routeAPIRabbitProducer)
        {
            _logger = logger;
            _friendAPIRabbitRPCService = routeAPIRabbitProducer;
        }

        [Authorize]
        [HttpPost(Routes.RouteRoutes.Create)]
        public async Task<IActionResult> CreateAsync(CreateFriendRequest createFriendRequest)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            if (id == createFriendRequest.requestedUserId) return BadRequest("User can not send themself a friend request");

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(CreateAsync)}: Recieved request.");

            var createFriendRabbitRequest = new CreateFriendRabbitRequest()
            {
                SenderId = id,
                RecieverId = createFriendRequest.requestedUserId
            };

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(CreateAsync)}: Sending request to FriendService for method {CreateFriendMethod}.");
            var createFriendRabbitResponse = await _friendAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<CreateFriendRabbitResponse>(CreateFriendMethod, createFriendRabbitRequest);

            if (createFriendRabbitResponse.Successful)
            {
                _logger.LogInformation($"{nameof(FriendController)}.{nameof(CreateAsync)}: Friend Request created.");
                return Ok();
            }
            else
            {
                _logger.LogInformation($"{nameof(FriendController)}.{nameof(CreateAsync)}: Friend Request creation failed, user does not exist with id: {createFriendRequest.requestedUserId}.");
                return NotFound();
            }
        }

        [Authorize]
        [HttpPut(Routes.RouteRoutes.Update)]
        public async Task<IActionResult> UpdateAsync([FromRoute] string senderId, [FromBody] ResponseToFriendRequest responseToFriendRequest)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            if (id == senderId) return BadRequest("User can not respond to a friend request for themself");

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(CreateAsync)}: Recieved request.");

            var responseToFriendRabbitRequest = new ResponseToFriendRabbitRequest()
            {
                RecieverId = id,
                SenderId = senderId,
                Accept = responseToFriendRequest.Accept,
                Reject = responseToFriendRequest.Reject,
            };

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(UpdateAsync)}: Sending request to FriendService for method {UpdateFriendMethod}.");
            var responseToFriendRabbitResponse = await _friendAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<ResponseToFriendRabbitResponse>(UpdateFriendMethod, responseToFriendRabbitRequest);

            if (responseToFriendRabbitResponse.Successful)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpDelete(Routes.RouteRoutes.Delete)]
        public async Task<IActionResult> DeleteAsync([FromRoute] string recieverId)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            if (id == recieverId) return BadRequest("User can not delete them self as a friend");

            var deleteFriendRabbitRequest = new DeleteFriendRabbitRequest()
            {
                SenderId = id,
                RecieverId = recieverId,
            };

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(UpdateAsync)}: Sending request to FriendService for method {DeleteFriendMethod}.");
            var deleteFriendRabbitResponse = await _friendAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<DeleteFriendRabbitResponse>(DeleteFriendMethod, deleteFriendRabbitRequest);

            if (deleteFriendRabbitResponse.Successful)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpGet(Routes.RouteRoutes.GetAll)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetAllFriendsRequest getAllFriendsRequest)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            var getAllFriendsRabbitRequest = new GetAllFriendsRabbitRequest()
            {
                Id = id,
                Requests = getAllFriendsRequest.Requests,
                Requested = getAllFriendsRequest.Requested
            };

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(GetAllAsync)}: Sending request to FriendService for method {GetAllFriendsMethod}.");
            var getAllFriendsRabbitResponse = await _friendAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetAllFriendsRabbitResponse>(GetAllFriendsMethod, getAllFriendsRabbitRequest);

            if (getAllFriendsRabbitResponse.FoundUsers)
            {
                return Ok(getAllFriendsRabbitResponse.Users);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpGet(Routes.RouteRoutes.GetFriendStatus)]
        public async Task<IActionResult> GetFriendStatusAsync([FromRoute] string otherUserId)
        {
            if (!User.Claims.Any(x => x.Type == UserIdClaim)) return Unauthorized();
            var id = User.Claims.First(x => x.Type == UserIdClaim).Value;

            if (id == otherUserId) return BadRequest("Can not return friend status for the same user");

            var friendStatusQueryRequest = new GetFriendStatusRabbitRequest()
            {
                QueryingUser = id,
                OtherUser = otherUserId
            };

            _logger.LogInformation($"{nameof(FriendController)}.{nameof(GetAllAsync)}: Sending request to FriendService for method {GetFriendStatusMethod}.");
            var getFriendStatusRabbitResponse = await _friendAPIRabbitRPCService.PublishRabbitMessageWaitForResponseAsync<GetFriendStatusRabbitResponse>(GetFriendStatusMethod, friendStatusQueryRequest);

            if (getFriendStatusRabbitResponse.Successful)
            {
                return Ok(getFriendStatusRabbitResponse.FriendStatus);
            }
            else return NotFound("Other user does not exist");
        }
    }
}
