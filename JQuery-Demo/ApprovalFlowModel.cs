using System.Collections.Generic;

namespace ApprovalFlow
{
	// Model classes
	public class ApprovalFlowModel
	{
		public string RequestId { get; set; }
		public string RequestDate { get; set; }
		public string Amount { get; set; }
		public string Requester { get; set; }
		public string Status { get; set; }
		public List<ApprovalModel> Approvals { get; set; }
	}
}