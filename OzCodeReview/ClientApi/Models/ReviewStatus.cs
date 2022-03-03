namespace OzCodeReview.ClientApi.Models
{
    public enum ReviewStatus
    {
        Uncommited = 0,
        Pending = 10,
        Rejected = 20,
        Corrected = 50,
        Closed = 100
    }
}