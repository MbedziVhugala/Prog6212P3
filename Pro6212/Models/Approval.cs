namespace Prog6212.Models
{
    public class Approval
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public int ApproverId { get; set; }
        public string Decision { get; set; }
        public string Comments { get; set; }
        public DateTime DecisionDate { get; set; } = DateTime.Now;
    }
}
