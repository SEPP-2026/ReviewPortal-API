namespace ReviewPortal.Application.DTOs.Reviews;

public record CreateCommentRequest(
    string CommenterName,
    string CommentText
);
