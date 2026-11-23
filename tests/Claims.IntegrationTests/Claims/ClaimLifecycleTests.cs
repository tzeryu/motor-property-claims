using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Claims.Application.Common.Authorization;
using Claims.IntegrationTests.Infrastructure;

using Xunit;

namespace Claims.IntegrationTests.Claims;

public sealed class ClaimLifecycleTests
{
    [Fact]
    public async Task NormalClaimLifecycle_FromSubmissionToSettlement_CompletesSuccessfully()
    {
        using var factory = new ClaimsApiFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using var claimant = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant
            );

            using var officer = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer
            );

            // The factory seeds this user. The Manager role is supplied by
            // the integration-test JWT; no factory or Identity changes are needed.
            using var manager = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOtherClaimantUserId,
                RoleNames.Manager
            );

            // 1. Claimant submits a motor claim.
            var submissionResponse = await claimant.PostAsJsonAsync(
                "/api/v1/claims",
                CreateMotorClaimRequest("POL-LIFECYCLE-001")
            );

            await AssertResponseStatusAsync(
                submissionResponse,
                HttpStatusCode.Created
            );

            using var submissionJson = JsonDocument.Parse(
                await submissionResponse.Content.ReadAsStringAsync()
            );

            var claimId = submissionJson.RootElement
                .GetProperty("id")
                .GetGuid();

            AssertJsonStringEquals(
                submissionJson.RootElement,
                "status",
                "submitted"
            );

            // 2. Claims Officer picks up the claim and starts review.
            var startReviewResponse = await officer.PostAsync(
                $"/api/v1/claims/{claimId}/start-review",
                content: null
            );

            await AssertResponseStatusAsync(
                startReviewResponse,
                HttpStatusCode.NoContent
            );

            // 3. Officer requests additional information.
            var informationRequestResponse = await officer.PostAsJsonAsync(
                $"/api/v1/claims/{claimId}/information-request",
                new
                {
                    message = "Please provide more details about the vehicle damage."
                }
            );

            await AssertResponseStatusAsync(
                informationRequestResponse,
                HttpStatusCode.NoContent
            );

            // 4. Claimant responds to the information request.
            var informationResponse = await claimant.PostAsJsonAsync(
                $"/api/v1/claims/{claimId}/information-response",
                new
                {
                    response = "The front bumper and right headlight were damaged in the collision."
                }
            );

            await AssertResponseStatusAsync(
                informationResponse,
                HttpStatusCode.NoContent
            );

            // 5. Officer approves an amount below the RM10,000 limit.
            const decimal settlementAmount = 8500m;

            var approvalResponse = await officer.PostAsJsonAsync(
                $"/api/v1/claims/{claimId}/approve",
                new { settlementAmount }
            );

            using var approvalJson = await ReadJsonResponseAsync(
                approvalResponse,
                HttpStatusCode.OK
            );

            AssertJsonStringEquals(
                approvalJson.RootElement,
                "status",
                "approved"
            );

            Assert.False(
                approvalJson.RootElement
                    .GetProperty("managerApprovalRequired")
                    .GetBoolean()
            );

            // 6. Officer records settlement.
            var settlementResponse = await officer.PostAsync(
                $"/api/v1/claims/{claimId}/settlement",
                content: null
            );

            await AssertResponseStatusAsync(
                settlementResponse,
                HttpStatusCode.NoContent
            );

            // 7. Officer records successful payment.
            const string paymentReference = "PAY-LIFECYCLE-001";

            var paymentResponse = await officer.PutAsJsonAsync(
                $"/api/v1/claims/{claimId}/payment-status",
                new
                {
                    status = "paid",
                    paymentReference
                }
            );

            await AssertResponseStatusAsync(
                paymentResponse,
                HttpStatusCode.NoContent
            );

            // 8. Claimant verifies the settled claim through their own claim list.
            // GET /claims/{id} is staff-only in the current API contract.
            var claimantListResponse = await claimant.GetAsync(
                "/api/v1/claims/mine?pageNumber=1&pageSize=20"
            );

            using var claimantListJson = await ReadJsonResponseAsync(
                claimantListResponse,
                HttpStatusCode.OK
            );

            var matchingClaims = claimantListJson.RootElement
                .GetProperty("items")
                .EnumerateArray()
                .Where(item => item.GetProperty("id").GetGuid() == claimId)
                .ToArray();

            var claimantClaim = Assert.Single(matchingClaims);

            AssertJsonStringEquals(
                claimantClaim,
                "status",
                "settled"
            );

            // Verify full persisted settlement and payment details using staff access.
            var claimDetailsResponse = await officer.GetAsync(
                $"/api/v1/claims/{claimId}"
            );

            using var claimDetailsJson = await ReadJsonResponseAsync(
                claimDetailsResponse,
                HttpStatusCode.OK
            );

            var claimDetails = claimDetailsJson.RootElement;

            AssertJsonStringEquals(claimDetails, "status", "settled");
            Assert.Equal(
                settlementAmount,
                claimDetails.GetProperty("approvedSettlementAmount").GetDecimal()
            );
            AssertJsonStringEquals(claimDetails, "paymentStatus", "paid");
            Assert.Equal(
                paymentReference,
                claimDetails.GetProperty("paymentReference").GetString()
            );
            Assert.NotEqual(
                JsonValueKind.Null,
                claimDetails.GetProperty("settlementDateUtc").ValueKind
            );
            Assert.NotEqual(
                JsonValueKind.Null,
                claimDetails.GetProperty("paymentRecordedAtUtc").ValueKind
            );

            // 9. Manager verifies the management overview for this isolated database.
            var overviewResponse = await manager.GetAsync(
                "/api/v1/management/overview"
            );

            using var overviewJson = await ReadJsonResponseAsync(
                overviewResponse,
                HttpStatusCode.OK
            );

            var overview = overviewJson.RootElement;

            Assert.Equal(
                0,
                overview.GetProperty("totalOutstandingClaims").GetInt32()
            );
            Assert.Equal(
                1,
                overview.GetProperty("settledClaims").GetInt32()
            );

            var myrExposure = overview
                .GetProperty("liabilityByCurrency")
                .EnumerateArray()
                .Where(item => string.Equals(
                    item.GetProperty("currencyCode").GetString(),
                    "MYR",
                    StringComparison.OrdinalIgnoreCase
                ))
                .ToArray();

            var myr = Assert.Single(myrExposure);

            Assert.Equal(
                settlementAmount,
                myr.GetProperty("settledAmount").GetDecimal()
            );
            Assert.Equal(
                0m,
                myr.GetProperty("pendingApprovalExposure").GetDecimal()
            );
            Assert.Equal(
                0m,
                myr.GetProperty("approvedUnpaidLiability").GetDecimal()
            );
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    [Fact]
    public async Task HighValueClaim_RequiresManagerApproval_BeforeSettlement()
    {
        using var factory = new ClaimsApiFactory();

        try
        {
            await factory.InitializeDatabaseAsync();

            using var claimant = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestClaimantUserId,
                RoleNames.Claimant
            );

            using var officer = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOfficerUserId,
                RoleNames.ClaimsOfficer
            );

            using var manager = factory.CreateAuthenticatedClient(
                ClaimsApiFactory.TestOtherClaimantUserId,
                RoleNames.Manager
            );

            // 1. Claimant submits a motor claim.
            var submissionResponse = await claimant.PostAsJsonAsync(
                "/api/v1/claims",
                CreateMotorClaimRequest("POL-LIFECYCLE-002")
            );

            await AssertResponseStatusAsync(
                submissionResponse,
                HttpStatusCode.Created
            );

            using var submissionJson = JsonDocument.Parse(
                await submissionResponse.Content.ReadAsStringAsync()
            );

            var claimId = submissionJson.RootElement
                .GetProperty("id")
                .GetGuid();

            // 2. Officer starts review.
            var startReviewResponse = await officer.PostAsync(
                $"/api/v1/claims/{claimId}/start-review",
                content: null
            );

            await AssertResponseStatusAsync(
                startReviewResponse,
                HttpStatusCode.NoContent
            );

            // 3. Officer requests approval above the RM10,000 limit.
            const decimal settlementAmount = 15000m;

            var approvalResponse = await officer.PostAsJsonAsync(
                $"/api/v1/claims/{claimId}/approve",
                new { settlementAmount }
            );

            using var approvalJson = await ReadJsonResponseAsync(
                approvalResponse,
                HttpStatusCode.OK
            );

            AssertJsonStringEquals(
                approvalJson.RootElement,
                "status",
                "pendingManagerApproval"
            );
            Assert.True(
                approvalJson.RootElement
                    .GetProperty("managerApprovalRequired")
                    .GetBoolean()
            );

            // 4. The officer cannot approve the escalated claim directly.
            var directApprovalResponse = await officer.PostAsJsonAsync(
                $"/api/v1/claims/{claimId}/approve",
                new { settlementAmount }
            );

            await AssertResponseStatusAsync(
                directApprovalResponse,
                HttpStatusCode.Conflict
            );

            // 5. Manager approves the escalated claim.
            var managerApprovalResponse = await manager.PostAsync(
                $"/api/v1/claims/{claimId}/manager-approve",
                content: null
            );

            await AssertResponseStatusAsync(
                managerApprovalResponse,
                HttpStatusCode.NoContent
            );

            // Verify the manager decision before proceeding to settlement.
            var approvedClaimResponse = await officer.GetAsync(
                $"/api/v1/claims/{claimId}"
            );

            using var approvedClaimJson = await ReadJsonResponseAsync(
                approvedClaimResponse,
                HttpStatusCode.OK
            );

            AssertJsonStringEquals(
                approvedClaimJson.RootElement,
                "status",
                "approved"
            );
            Assert.Equal(
                settlementAmount,
                approvedClaimJson.RootElement
                    .GetProperty("approvedSettlementAmount")
                    .GetDecimal()
            );

            // 6. Officer records settlement.
            var settlementResponse = await officer.PostAsync(
                $"/api/v1/claims/{claimId}/settlement",
                content: null
            );

            await AssertResponseStatusAsync(
                settlementResponse,
                HttpStatusCode.NoContent
            );

            // 7. Officer records successful payment.
            const string paymentReference = "PAY-LIFECYCLE-002";

            var paymentResponse = await officer.PutAsJsonAsync(
                $"/api/v1/claims/{claimId}/payment-status",
                new
                {
                    status = "paid",
                    paymentReference
                }
            );

            await AssertResponseStatusAsync(
                paymentResponse,
                HttpStatusCode.NoContent
            );

            // 8. Verify the final settled state.
            var settledClaimResponse = await officer.GetAsync(
                $"/api/v1/claims/{claimId}"
            );

            using var settledClaimJson = await ReadJsonResponseAsync(
                settledClaimResponse,
                HttpStatusCode.OK
            );

            var settledClaim = settledClaimJson.RootElement;

            AssertJsonStringEquals(settledClaim, "status", "settled");
            Assert.Equal(
                settlementAmount,
                settledClaim.GetProperty("approvedSettlementAmount").GetDecimal()
            );
            AssertJsonStringEquals(settledClaim, "paymentStatus", "paid");
            Assert.Equal(
                paymentReference,
                settledClaim.GetProperty("paymentReference").GetString()
            );
        }
        finally
        {
            await factory.DeleteDatabaseAsync();
        }
    }

    private static object CreateMotorClaimRequest(string policyNumber)
    {
        return new
        {
            claimType = "motor",
            claimantName = "Test Claimant",
            claimantEmail = "test@example.com",
            claimantPhone = "0123456789",

            policyDetails = new
            {
                policyNumber,
                policyholderName = "Test Claimant",
                coverageStartDate = "2026-01-01",
                coverageEndDate = "2026-12-31",
                insuredAmount = 50000m,
                currencyCode = "MYR"
            },

            incidentDate = "2026-11-20",
            incidentLocation = "Kuala Lumpur",
            incidentDescription = "Vehicle collision.",

            motorDetails = new
            {
                vehicleRegistrationNumber = "ABC1234",
                vehicleMake = "Toyota",
                vehicleModel = "Vios"
            }
        };
    }

    private static async Task AssertResponseStatusAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == expectedStatus,
            $"Expected {(int)expectedStatus} ({expectedStatus}) but received " +
            $"{(int)response.StatusCode} ({response.StatusCode}). " +
            $"Response: {responseBody}"
        );
    }

    private static async Task<JsonDocument> ReadJsonResponseAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus)
    {
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == expectedStatus,
            $"Expected {(int)expectedStatus} ({expectedStatus}) but received " +
            $"{(int)response.StatusCode} ({response.StatusCode}). " +
            $"Response: {responseBody}"
        );

        return JsonDocument.Parse(responseBody);
    }

    private static void AssertJsonStringEquals(
        JsonElement json,
        string propertyName,
        string expectedValue)
    {
        var actualValue = json.GetProperty(propertyName).GetString();

        Assert.True(
            string.Equals(
                expectedValue,
                actualValue,
                StringComparison.OrdinalIgnoreCase
            ),
            $"Expected JSON property '{propertyName}' to be '{expectedValue}', " +
            $"but received '{actualValue}'."
        );
    }
}
