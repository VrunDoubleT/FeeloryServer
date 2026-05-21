using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Friend;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Services.Interfaces;

public interface IFriendService
{
    Task<Result> SendRequestAsync(Guid senderId, Guid receiverId);
    Task<Result> AcceptRequestAsync(Guid currentUserId, Guid requestId);
    Task<Result> RejectRequestAsync(Guid currentUserId, Guid requestId);
    Task<Result> RemoveFriendAsync(Guid currentUserId, Guid friendUserId);
    Task<Result<CursorPaginationResponse<FriendDto>>>
        GetFriendsAsync(
            Guid userId,
            CursorPaginationRequest request);
    Task<Result<CursorPaginationResponse<FriendRequestDto>>>
        GetPendingRequestsAsync(
            Guid userId,
            CursorPaginationRequest request);
}