# AI Working Journal

## Motor & Property Claims Management Platform

**AI Tool:** ChatGPT  
**Development Period:** November 2026  
**Project Stack:** .NET 10, C#, ASP.NET Core Web API, Entity Framework Core, SQL Server LocalDB, Angular, TypeScript

### Purpose

This journal records how AI assistance was used throughout the development of the Motor & Property Claims Management Platform.

It documents significant technical discussions, AI recommendations, implementation decisions, challenges, changes made, and reasons behind those decisions. It is intended as a practical working record rather than a complete transcript of every conversation.

The developer remained responsible for evaluating recommendations, integrating the code, making technical decisions, and verifying the application.

---

## 1. Architecture and Technology Decisions

**My request:**

Seek guidance on designing the claims management platform using .NET, Angular, SQL Server, REST APIs and Kafka, while considering the assessment's limited development time.

**AI recommendation:**

Use a modular monolithic architecture with separate API, Application, Domain and Infrastructure layers. Maintain separate unit and integration test projects.

**My decision:**

Accepted the layered architecture and retained the existing project structure instead of restarting the project or introducing additional architectural complexity.

**Reasoning:**

The existing structure already provided sufficient separation of responsibilities. Restarting would introduce unnecessary implementation risk and consume time that could be spent completing the required functionality.

Kafka was considered during the initial architecture discussion but was not introduced into the completed implementation because it was not necessary for the core claims workflow.

---

## 2. Backend-First Development Approach

**My request:**

Clarify whether frontend implementation should begin before completing backend integration testing.

**AI recommendation:**

Prioritise backend functionality and testing before completing frontend integration.

**My decision:**

Accepted the backend-first approach, then implemented frontend components progressively once the relevant API functionality was available.

**Reasoning:**

Separating backend and frontend issues makes debugging easier. It also ensures the frontend consumes established API contracts instead of relying on assumptions.

**Implementation:**

The backend was developed incrementally, covering claim submission, authentication, authorisation, claim review, approval, rejection, settlement and management reporting before the corresponding frontend workflows were completed.

---

## 3. Backend Domain and Claim Lifecycle

**My request:**

Develop the claims workflow with appropriate business rules and clear separation between domain logic, application handlers, API controllers and persistence.

**AI recommendation:**

Use a Claim aggregate to manage claim operations and enforce business rules. Implement application handlers and repository interfaces to coordinate operations without placing business logic directly in controllers.

**My decision:**

Accepted the proposed layered implementation and progressively completed the claim lifecycle.

**Implementation:**

The backend included:

- Motor and property claim submission.
- Claim status management.
- Claimant ownership and claim retrieval.
- Additional-information requests and responses.
- Claims Officer review and assignment.
- Officer approval and rejection.
- Manager approval and rejection.
- Settlement and payment recording.
- Management overview and financial exposure reporting.

**Reasoning:**

Keeping business rules in the backend ensures that workflow restrictions are enforced independently of frontend behaviour.

---

## 4. Authentication and Role-Based Authorisation

**My request:**

Implement authentication and access control for Claimants, Claims Officers and Managers.

**AI recommendation:**

Use ASP.NET Core Identity and JWT authentication, with role-based authorisation on protected API endpoints. Derive the authenticated user's identity from the token rather than trusting user identifiers submitted by the frontend.

**My decision:**

Accepted the recommendation.

**Implementation:**

Three application roles were established:

- Claimant
- ClaimsOfficer
- Manager

The backend provides a login endpoint that returns an access token and user information. The Angular application stores the authentication session and uses an HTTP interceptor to attach the bearer token to API requests.

Role guards protect the corresponding frontend routes.

**Reasoning:**

Authentication and authorisation must be enforced at both application navigation and API levels. Hiding a frontend page alone is insufficient to protect sensitive claim operations.

**Verification:**

Authentication, role-based navigation and API authorisation tests were completed successfully.

---

## 5. Database Design and Integration Testing

**My request:**

Implement database persistence and test the API endpoints and claim workflows without compromising the development database.

**AI recommendation:**

Use Entity Framework Core with SQL Server LocalDB, migrations and separate unit and integration test projects. Integration tests should use isolated databases rather than the development database.

**My decision:**

Accepted the approach and retained the existing integration test database safeguards.

**Implementation:**

The backend uses SQL Server LocalDB for development. Integration tests create uniquely named databases and apply migrations independently.

Unit and integration tests cover claim submission, authorisation, claim assignment, management overview and claim lifecycle operations.

**Reasoning:**

Isolated integration databases reduce the risk of test data affecting development records. Retaining database cleanup safeguards also prevents accidental deletion of the development database.

**Verification:**

Backend build and test execution completed successfully.

---

## 6. API Configuration and Angular Integration

**My request:**

Clarify which backend URL and port should be configured in Angular based on the ASP.NET Core launch settings.

**AI recommendation:**

Use the actual address reported by the running backend rather than assuming the configured HTTPS port.

**My decision:**

Used `http://localhost:5223/api/v1` after the backend terminal confirmed that the application was listening on `http://localhost:5223`.

**Reasoning:**

The frontend configuration must match the actual running backend address and protocol.

**Verification:**

Backend startup output confirmed the HTTP listening address, and the frontend was able to communicate with the API after the CORS configuration was corrected.

---

## 7. CORS and Frontend Connectivity Debugging

**My request:**

Investigate frontend API requests failing because of CORS errors and connection issues.

**AI recommendation:**

Configure an ASP.NET Core CORS policy allowing the Angular development origin, `http://localhost:4200`, and ensure the middleware is executed in the correct order.

**My decision:**

Accepted the CORS configuration and adjusted the development middleware configuration to match the local HTTP setup.

**Reasoning:**

The issue was caused by the frontend and backend running on different origins. Correctly configuring CORS was necessary before authentication and API integration could work.

**Verification:**

The browser successfully completed the CORS preflight request, and frontend API communication was restored.

---

## 8. Angular Authentication Interceptor

**My request:**

Review the existing Angular HTTP configuration and explain how to register the authentication interceptor.

**AI recommendation:**

Use `provideHttpClient(withInterceptors([authInterceptor]))` instead of a plain `provideHttpClient()`, while retaining the existing router provider.

**My decision:**

Adopted the interceptor-enabled HTTP configuration.

**Reasoning:**

The authentication interceptor centralises the handling of authentication information for outgoing HTTP requests instead of requiring every API service to implement it separately.

**Verification:**

Angular authentication and protected API access tests completed successfully.

---

## 9. Claimant Dashboard and Claim Submission

**My request:**

Implement the claimant-facing interface for viewing submitted claims, monitoring claim status and submitting motor and property claims.

**AI recommendation:**

Use reusable Angular services and models to communicate with the existing claims API. Use reactive forms for claim submission and validate the input before sending requests.

**My decision:**

Accepted the recommendation and implemented the claimant dashboard, My Claims page and claim submission page.

**Implementation:**

The claimant interface supports:

- Viewing claim statistics.
- Listing submitted claims.
- Filtering and paginating claims.
- Submitting motor claims.
- Submitting property claims.
- Displaying submission results and API validation errors.

**Reasoning:**

Separating claim submission and claim tracking into dedicated pages makes the application easier to navigate and maintain.

**Verification:**

Motor and property claim submission, dashboard loading and claim listing tests completed successfully.

---

## 10. Claimant Navigation and Additional Information

**My request:**

Review the claimant's My Claims template and explain how to navigate to an individual claim details page. Also clarify how to enforce the backend's 4,000-character additional-information response limit.

**AI recommendation:**

Use Angular `RouterLink` with the existing claim ID and enforce the response limit through both HTML `maxlength` and reactive-form `Validators.maxLength(4000)`.

**My decision:**

Accepted the validation approach and incorporated the claim identifier into the details navigation.

**Reasoning:**

Using the existing claim identifier preserves the relationship between the selected claim and its details page. Matching frontend validation with backend validation provides consistent input constraints.

**Additional technical consideration:**

The existing backend initially restricted individual claim retrieval to Claims Officers and Managers. The claimant details workflow therefore required an ownership-aware retrieval approach rather than simply exposing the staff endpoint to every authenticated user.

**Verification:**

Claim details retrieval and additional-information workflow tests completed successfully.

---

## 11. Claims Officer Worklist

**My request:**

Investigate why the officer dashboard continued to display placeholder content after login.

**AI recommendation:**

Implement a functional Claims Officer Worklist, connect it to the existing claims API, and verify that the officer login route points to the actual worklist component rather than a placeholder.

**My decision:**

Challenged the initial incomplete implementation because the page still displayed placeholder content and lacked its stylesheet. Requested that the implementation be completed with the actual HTML, SCSS, routing and API integration.

**Reasoning:**

Creating a component alone does not make it accessible or functional. Angular routing must connect the intended component to the officer's landing page, and the page requires its own styling and working API integration.

**Implementation:**

The officer interface was developed to support assigned and unassigned claims, claim review initiation and navigation to individual claim reviews.

**Verification:**

Officer login, worklist navigation and claim review workflow tests completed successfully.

---

## 12. Claims Officer Review and Decision Workflow

**My request:**

Implement the Claims Officer claim review interface, including the ability to request additional information, approve claims and reject claims.

**AI recommendation:**

Build a claim review page that retrieves the full claim details and exposes actions according to the current claim status.

**My decision:**

Accepted the workflow-based design and retained backend authorisation and business rules as the final authority for every action.

**Implementation:**

The review page supports:

- Reviewing claimant and policy information.
- Reviewing motor or property incident details.
- Viewing additional-information history.
- Requesting further information.
- Proposing a settlement amount.
- Approving claims within the officer's approval authority.
- Rejecting claims with a reason.
- Displaying manager approval escalation.

**Reasoning:**

The interface should not offer actions that are invalid for the current claim status. However, the backend must still enforce these restrictions because frontend validation alone cannot guarantee security or workflow correctness.

**Verification:**

Officer review, information request, approval, rejection and manager escalation tests completed successfully.

---

## 13. Manager Dashboard and Approval Queue

**My request:**

Implement a Manager Dashboard to monitor claims, review financial exposure and process escalated claims.

**AI recommendation:**

Reuse the existing management overview endpoint and claims APIs. Display financial exposure by currency and provide a queue of claims awaiting manager approval.

**My decision:**

Accepted the shared API approach and implemented a dedicated manager dashboard with an approval queue and claim detail panel.

**Implementation:**

The dashboard includes:

- Outstanding claim counts.
- Counts grouped by claim status.
- Financial exposure grouped by currency.
- Pending manager approval queue.
- Claim details for managerial review.
- Manager approval and rejection actions.

**Reasoning:**

Reusing the existing backend endpoints avoids duplicating business logic. Separating the management overview from individual claim decisions also makes the dashboard easier to understand.

**Verification:**

Management overview, approval queue, manager approval and manager rejection tests completed successfully.

---

## 14. Settlement and Payment Management

**My request:**

Implement the settlement and payment management functionality using the existing backend endpoints.

**AI recommendation:**

Use a shared settlement management component for Claims Officers and Managers instead of duplicating the same functionality across two dashboards.

**My decision:**

Accepted the shared component approach and implemented separate role-protected routes that reuse the same interface.

**Implementation:**

The page supports:

- Viewing approved claims awaiting settlement.
- Viewing settled claims.
- Recording settlement.
- Recording payment references.
- Updating payment status to Paid or Failed.
- Displaying settlement and payment history.
- Retrying payment after a failed attempt.

**Reasoning:**

Settlement and payment are related processes but represent different stages of the claim lifecycle. Recording settlement should not automatically imply that payment has succeeded.

A failed payment should not result in claim rejection. The claim remains approved and can undergo another payment attempt.

**Verification:**

Settlement recording, successful payment, failed payment and payment retry tests completed successfully.

---

## 15. Frontend Implementation Completeness and Code Review

**My request:**

Ensure that each frontend implementation includes the necessary TypeScript logic, HTML template, SCSS styling, API integration and route configuration rather than providing incomplete component code.

**AI recommendation:**

Implement each feature as a complete unit and verify that its dependencies and navigation are connected.

**My decision:**

Explicitly challenged incomplete implementations, particularly placeholder pages and missing CSS files. Requested complete implementations before proceeding to the next development step.

**Reasoning:**

A component that compiles but is not connected to the application does not satisfy the functional requirement. The same applies to pages that contain HTML but lack styling or working interactions.

**Verification:**

Frontend build and functional testing completed successfully.

---

## 16. Testing and Final Verification

**My request:**

Verify that the implemented backend and frontend functionality is ready for the final project checkpoint.

**AI recommendation:**

Complete backend and frontend build checks, unit and integration tests, and end-to-end workflow testing before updating the repository.

**My decision:**

Completed the required local build and testing activities and confirmed that the implementation passed.

**Verification status: COMPLETED**

The following verification activities were completed:

- Backend solution build.
- Backend unit and integration tests.
- Angular production build.
- Authentication and role-based navigation.
- Claim submission and retrieval.
- Claims Officer review and decision workflow.
- Manager approval and rejection.
- Settlement and payment workflow.
- Integration between Angular and the ASP.NET Core API.

The application was also tested through the relevant user workflows using the different application roles.

---

## 17. Git Repository and Development Checkpoints

**My request:**

Maintain a Git repository with meaningful commits that demonstrate the development process.

**AI recommendation:**

Commit completed milestones separately rather than combining all development work into one large commit.

**My decision:**

Accepted the milestone-based approach and maintained a stable backend checkpoint before continuing with frontend implementation.

**Recorded checkpoint:**

`feat: complete claims backend and Angular integration`

**Next checkpoint:**

`feat: complete claims management frontend`

**Reasoning:**

Meaningful commit history makes it easier to understand the development sequence, identify changes and demonstrate how the application evolved.

The final frontend checkpoint is intended to be committed after the completed local verification.

---

## Reflection

AI was used as a development assistant for architectural guidance, implementation support, debugging, code review and testing guidance.

Its recommendations were not accepted automatically. They were evaluated against the existing codebase, assessment requirements, implementation constraints and actual backend contracts.

Some recommendations were challenged or modified when the implementation was incomplete, such as when the Claims Officer page remained a placeholder or lacked CSS. In those cases, additional implementation details were requested before proceeding.

The most useful aspect of AI assistance was accelerating implementation while helping maintain consistency between backend business rules and frontend behaviour.

The developer remained responsible for technical decisions, integration, testing and verification of the final application.
