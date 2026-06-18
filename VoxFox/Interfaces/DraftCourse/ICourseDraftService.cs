using VoxFox.Models.DTOs.Draft.CourseDraftDto;
using VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;

namespace VoxFox.Interfaces.DraftCourse;

public interface ICourseDraftService
{
	Task<ServiceResult<CourseDraftDto>> CreateDraftAsync(Guid courseId, Guid authorId);
	Task<ServiceResult<CourseDraftDto>> GetDraftAsync(Guid courseId, Guid authorId);
	Task<ServiceResult<CourseDraftDto>> UpdateDraftAsync(Guid draftId, CreateCourseDraftDto dto, Guid authorId);
	Task<ServiceResult<bool>> SubmitDraftAsync(Guid draftId, Guid authorId);
	Task<ServiceResult<bool>> ApproveDraftAsync(Guid draftId, Guid moderatorId);
	Task<ServiceResult<bool>> RejectDraftAsync(Guid draftId, string? reason, Guid moderatorId);
	Task<ServiceResult<bool>> ClaimDraftAsync(Guid draftId, Guid moderatorId);
	Task<ServiceResult<bool>> ReleaseDraftAsync(Guid draftId, Guid moderatorId);
	Task<ServiceResult<bool>> DeleteDraftAsync(Guid draftId, Guid authorId);
	Task<ServiceResult<CourseDraftDto>> GetDraftForReviewAsync(Guid draftId);
	Task<List<CourseDraftDto>> GetPendingDraftsAsync();
}
