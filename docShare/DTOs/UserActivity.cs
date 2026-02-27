namespace API.DTOs
{
    public class ReqVoteDto
    {
        public bool? IsLike { get; set; }
    }
    public class ReqFollowDto
    {
        public int FollowedId { get; set; }
    }
    public class ReqSaveDocDto
    {
        public bool IsSaved { get; set; }
    }

}
