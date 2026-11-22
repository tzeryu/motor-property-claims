using System.ComponentModel.DataAnnotations;

namespace Claims.Application.Claims.DTOs.Workflow;

public sealed class ApproveClaimRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal SettlementAmount { get; init; }
}