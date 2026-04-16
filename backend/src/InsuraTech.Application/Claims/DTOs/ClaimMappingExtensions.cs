using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Claims;

namespace InsuraTech.Application.Claims.DTOs
{
    public static class ClaimMappingExtensions
    {
        public static ClaimResponse ToResponse(this Claim claim) =>
            new()
            {
                Id = claim.Id,
                PolicyId = claim.PolicyId,
                Type = claim.Type.ToString(),
                Status = claim.Status.ToString(),
                ClaimedAmount = claim.ClaimedAmount,
                ApprovedAmount = claim.ApprovedAmount,
                IncidentDate = claim.IncidentDate,
                Description = claim.Description,
                HasBeenAppealed    = claim.HasBeenAppealed,
                RejectionReason    = claim.RejectionReason,
                CreatedByAdvisorId = claim.CreatedByAdvisorId,
                CreatedAt          = claim.CreatedAt,
                UpdatedAt = claim.UpdatedAt,
                StatusHistory = claim.StatusHistory.Select(h => new ClaimStatusHistoryResponse
                {
                    Status = h.Status.ToString(),
                    ResponsibleUser = h.ResponsibleUser,
                    Observations = h.Observations,
                    ChangedAt = h.ChangedAt
                })
            };
    }
}
