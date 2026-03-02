using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;

namespace JQuery_Demo
{
	/// <summary>
	/// Summary description for GetApprovals
	/// </summary>
	public class GetApprovals : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "application/json";

			try
			{
				string requestIdParam = context.Request.QueryString["requestId"];

				if (string.IsNullOrEmpty(requestIdParam))
				{
					context.Response.StatusCode = 400;
					context.Response.Write("{\"error\": \"RequestId parameter is required\"}");
					return;
				}

				int requestId = int.Parse(requestIdParam);

				var approvalFlow = GetApprovalFlow(requestId);

				JavaScriptSerializer serializer = new JavaScriptSerializer();
				string json = serializer.Serialize(approvalFlow);

				context.Response.Write(json);
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				context.Response.Write("{\"error\": \"" + ex.Message.Replace("\"", "'") + "\"}");
			}
		}

		private ApprovalFlowModel GetApprovalFlow(int requestId)
		{
			string connectionString = ConfigurationManager.ConnectionStrings["AdventureWorks2019"].ConnectionString;

			ApprovalFlowModel flow = new ApprovalFlowModel
			{
				Approvals = new List<ApprovalModel>()
			};

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();

				// Set basic request info (no Requests table needed)
				flow.RequestId = requestId.ToString();
				flow.RequestDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
				flow.Amount = "$15,000";
				flow.Requester = "John Smith";

				// Get approvals from YOUR existing Approvals table
				string query = @"
                    SELECT 
                        ApprovalID,
                        RequestID,
                        ApproverRole,
                        ApprovalOrder,
                        ApprovedOn,
                        PreviousApprovalID
                    FROM ExpenseApproval
                    WHERE RequestID = @RequestId
                    ORDER BY ApprovalOrder";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@RequestId", requestId);

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ApprovalModel approval = new ApprovalModel
							{
								ApprovalId = Convert.ToInt32(reader["ApprovalID"]),
								RequestId = Convert.ToInt32(reader["RequestID"]),
								ApproverRole = reader["ApproverRole"].ToString(),
								ApprovalOrder = Convert.ToInt32(reader["ApprovalOrder"]),
								ApprovedOn = reader["ApprovedOn"] != DBNull.Value
									? Convert.ToDateTime(reader["ApprovedOn"]).ToString("yyyy-MM-dd HH:mm:ss")
									: null,
								PreviousApprovalId = reader["PreviousApprovalID"] != DBNull.Value
									? Convert.ToInt32(reader["PreviousApprovalID"])
									: (int?)null,
								Approver = GetApproverName(reader["ApproverRole"].ToString())
							};

							flow.Approvals.Add(approval);
						}
					}
				}

				// Determine overall status based on approvals
				bool allApproved = true;
				foreach (var approval in flow.Approvals)
				{
					if (approval.ApprovedOn == null)
					{
						allApproved = false;
						break;
					}
				}
				flow.Status = allApproved ? "Completed" : "Pending";
			}

			return flow;
		}

		private string GetApproverName(string role)
		{
			// This is a simple mapping - in production, you'd query from Users table
			switch (role)
			{
				case "Team Lead":
					return "Sarah Johnson";
				case "Finance Manager":
					return "Michael Chen";
				case "Director":
					return "Emily Rodriguez";
				case "CFO":
					return "David Park";
				default:
					return "Unknown";
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}

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