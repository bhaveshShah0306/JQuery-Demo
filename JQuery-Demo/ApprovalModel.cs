namespace ApprovalFlow
{
	public class ApprovalModel
	{
		public int ApprovalId { get; set; }
		public int RequestId { get; set; }
		public string ApproverRole { get; set; }
		public int ApprovalOrder { get; set; }
		public string ApprovedOn { get; set; }
		public int? PreviousApprovalId { get; set; }
		public string Approver { get; set; }
	}
}