# Functional Design Diagrams - Shelton Tool-Hire Review Portal

> Purpose: MSc submission design artefact covering UML, DFD, ERD, and high-level architecture for the Shelton Tool-Hire Review Portal.
>
> Scope: Full web system design, including the Next.js client, ASP.NET Core Web API, SQL Server database, catalogue, rental calculator, reviews, moderation, and back-office administration.

---

## 1. Use Case Diagram

This diagram shows the main functional use cases for customers, registered users, admins, and moderators.

```mermaid
flowchart LR
    Customer["Customer / Visitor"]
    RegisteredUser["Registered User"]
    Admin["Admin"]
    Moderator["Moderator"]

    subgraph PublicPortal["Public Portal"]
        UC1(("Browse categories"))
        UC2(("View tool/service details"))
        UC3(("Search tools/services"))
        UC4(("Filter and sort catalogue"))
        UC5(("Calculate rental cost"))
        UC6(("Read approved reviews"))
        UC7(("Register account"))
        UC8(("Log in"))
        UC9(("Submit review"))
        UC10(("Comment on approved review"))
        UC11(("View my reviews"))
    end

    subgraph BackOffice["Back-Office Portal"]
        UC12(("Manage tools/services"))
        UC13(("Create tool/service"))
        UC14(("Edit pricing and details"))
        UC15(("Activate/deactivate tool/service"))
        UC16(("Manage images"))
        UC17(("Manage categories"))
        UC18(("View moderation queue"))
        UC19(("Approve/reject reviews"))
        UC20(("Approve/reject comments"))
        UC21(("Post company response"))
        UC22(("View dashboard stats"))
    end

    Customer --> UC1
    Customer --> UC2
    Customer --> UC3
    Customer --> UC4
    Customer --> UC5
    Customer --> UC6
    Customer --> UC7
    Customer --> UC8
    Customer --> UC9
    Customer --> UC10

    RegisteredUser --> UC8
    RegisteredUser --> UC9
    RegisteredUser --> UC10
    RegisteredUser --> UC11

    Admin --> UC8
    Admin --> UC12
    Admin --> UC13
    Admin --> UC14
    Admin --> UC15
    Admin --> UC16
    Admin --> UC17
    Admin --> UC18
    Admin --> UC19
    Admin --> UC20
    Admin --> UC21
    Admin --> UC22

    Moderator --> UC8
    Moderator --> UC18
    Moderator --> UC19
    Moderator --> UC20
```

---

## 2. Class Diagram

This UML class diagram summarises the core backend domain model and the main service/controller boundaries.

```mermaid
classDiagram
    direction LR

    class User {
        int Id
        string Name
        string Email
        string PasswordHash
        string PasswordResetTokenHash
        DateTime PasswordResetTokenExpiryUtc
        UserRole Role
        DateTime CreatedDate
    }

    class Category {
        int Id
        string Name
        string Description
        string ImageUrl
    }

    class Tool {
        int Id
        int CategoryId
        string Name
        string Description
        decimal HourlyRate
        decimal DailyRate
        decimal WeeklyRate
        string SpecialNotes
        bool DepositRequired
        decimal DepositAmount
        bool IsActive
        decimal OverallRating
        int ReviewCount
        DateTime CreatedDate
        DateTime UpdatedDate
    }

    class ToolImage {
        int Id
        int ToolId
        string ImageUrl
        int DisplayOrder
        DateTime UploadedDate
    }

    class Review {
        int Id
        int ToolId
        int UserId
        string ReviewerName
        string ReviewerEmail
        string ReviewText
        int EquipmentRating
        int CustomerServiceRating
        int TechnicalSupportRating
        int AfterSalesRating
        int ValueForMoneyRating
        decimal OverallRating
        ReviewStatus Status
        string RejectionReason
        DateTime CreatedDate
        CalculateOverallRating()
    }

    class ReviewComment {
        int Id
        int ReviewId
        int UserId
        string CommenterName
        string CommentText
        ReviewStatus Status
        string RejectionReason
        DateTime CreatedDate
    }

    class CompanyResponse {
        int Id
        int ReviewId
        int StaffUserId
        string ResponseText
        DateTime CreatedDate
        DateTime UpdatedDate
    }

    class CategoryService {
        GetAllCategoriesAsync()
        GetCategoryByIdAsync()
        CreateCategoryAsync()
        UpdateCategoryAsync()
        DeleteCategoryAsync()
    }

    class ToolService {
        GetToolsByCategoryAsync()
        GetToolByIdAsync()
        SearchToolsAsync()
        FilterByPriceRangeAsync()
        CalculateRentalCostAsync()
        CreateToolAsync()
        UpdateToolAsync()
        SetToolStatusAsync()
    }

    class ReviewService {
        CreateReviewAsync()
        GetApprovedReviewsAsync()
        AddCommentAsync()
        GetApprovedCommentsAsync()
        AddCompanyResponseAsync()
        GetPendingReviewsAsync()
        ModerateReviewAsync()
        ModerateCommentAsync()
    }

    class AuthService {
        RegisterAsync()
        LoginAsync()
        GetCurrentUserAsync()
        ChangePasswordAsync()
        ForgotPasswordAsync()
        ResetPasswordAsync()
    }

    class Controllers {
        CategoriesController
        ToolsController
        ToolReviewsController
        ReviewCommentsController
        ReviewResponsesController
        UserReviewsController
        AuthController
        AdminToolsController
        AdminModerationController
    }

    Category "1" --> "0..*" Tool : contains
    Tool "1" --> "1..*" ToolImage : has
    Tool "1" --> "0..*" Review : receives
    User "1" --> "0..*" Review : writes
    Review "1" --> "0..*" ReviewComment : has
    User "1" --> "0..*" ReviewComment : comments
    Review "1" --> "0..1" CompanyResponse : has
    User "1" --> "0..*" CompanyResponse : authors

    Controllers --> CategoryService
    Controllers --> ToolService
    Controllers --> ReviewService
    Controllers --> AuthService
```

---

## 3. Activity Diagram

This activity diagram shows the review lifecycle from customer submission through moderation and publication.

```mermaid
flowchart TD
    Start([Start])
    ViewTool["Customer views tool/service detail page"]
    ChooseReview["Customer selects Write a Review"]
    AuthDecision{"Logged in?"}
    UseProfile["Use registered user profile"]
    CaptureAnon["Capture reviewer name and email"]
    CompleteForm["Enter review text and five ratings"]
    ValidateForm{"Valid review text and all ratings 1-5?"}
    ValidationError["Return validation error"]
    SavePending["Save review with Pending status"]
    Confirmation["Show confirmation that review awaits approval"]
    Queue["Review appears in moderation queue"]
    ModeratorDecision{"Moderator decision"}
    Reject["Set status to Rejected and store reason"]
    NotifyRejected["Rejected reason visible in My Reviews"]
    Approve["Set status to Approved"]
    Recalculate["Recalculate tool OverallRating and ReviewCount"]
    Publish["Approved review visible on public tool/service page"]
    End([End])

    Start --> ViewTool --> ChooseReview --> AuthDecision
    AuthDecision -->|Yes| UseProfile --> CompleteForm
    AuthDecision -->|No| CaptureAnon --> CompleteForm
    CompleteForm --> ValidateForm
    ValidateForm -->|No| ValidationError --> CompleteForm
    ValidateForm -->|Yes| SavePending --> Confirmation --> Queue
    Queue --> ModeratorDecision
    ModeratorDecision -->|Reject| Reject --> NotifyRejected --> End
    ModeratorDecision -->|Approve| Approve --> Recalculate --> Publish --> End
```

---

## 4. High-Level System Architecture Diagram

This diagram shows the deployment and Clean Architecture shape of the system.

```mermaid
flowchart TB
    subgraph ClientLayer["Client Layer"]
        Browser["Customer/Admin Browser"]
        NextJs["Next.js Web App"]
    end

    subgraph ApiHost["Azure App Service / ASP.NET Core Web API"]
        Controllers["API Controllers"]
        Middleware["Middleware: HTTPS, CORS, JWT Auth, ProblemDetails"]

        subgraph CleanArchitecture["Clean Architecture"]
            Application["Application Layer: DTOs, Interfaces, Services, Result Pattern"]
            Domain["Domain Layer: Entities, Enums, Business Rules"]
            Infrastructure["Infrastructure Layer: EF Core, Repositories, Auth Providers, Migrations"]
        end
    end

    subgraph DataLayer["Data Layer"]
        SqlServer["Azure SQL / SQL Server"]
        FileStorage["Configured Image Storage: Local Uploads or Azure Blob"]
    end

    Browser --> NextJs
    NextJs -->|"HTTPS JSON API + Bearer JWT"| Middleware
    Middleware --> Controllers
    Controllers --> Application
    Application --> Domain
    Application --> Infrastructure
    Infrastructure --> SqlServer
    Infrastructure --> FileStorage

    SqlServer --> Infrastructure
    Infrastructure --> Application
    Application --> Controllers
    Controllers --> NextJs
```

---

## 5. Sequence Diagram

This sequence diagram covers a core end-to-end flow: customer review submission, moderation, rating recalculation, and public display.

```mermaid
sequenceDiagram
    actor Customer
    participant Web as Next.js Web App
    participant API as ASP.NET Core API
    participant ReviewController as ToolReviewsController
    participant ReviewService
    participant Db as SQL Server
    actor Moderator
    participant AdminController as AdminModerationController

    Customer->>Web: Fill review form
    Web->>API: POST /api/tools/{toolId}/reviews
    API->>ReviewController: Route request
    ReviewController->>ReviewService: CreateReviewAsync(toolId, request, userId)
    ReviewService->>ReviewService: Validate text and five rating values
    ReviewService->>Db: Insert Review with Status = Pending
    Db-->>ReviewService: Save successful
    ReviewService-->>ReviewController: Created ReviewDto
    ReviewController-->>Web: 201 Created
    Web-->>Customer: Show awaiting moderation message

    Moderator->>Web: Open moderation queue
    Web->>API: GET /api/admin/moderation/pending
    API->>AdminController: Authorize Admin or Moderator
    AdminController->>ReviewService: GetPendingReviewsAsync(page, pageSize)
    ReviewService->>Db: Query pending reviews/comments
    Db-->>ReviewService: Pending items
    ReviewService-->>AdminController: Paged moderation result
    AdminController-->>Web: 200 OK

    Moderator->>Web: Approve review
    Web->>API: PUT /api/admin/moderation/reviews/{id}
    API->>AdminController: Authorize Admin or Moderator
    AdminController->>ReviewService: ModerateReviewAsync(id, approve)
    ReviewService->>Db: Update Review.Status = Approved
    ReviewService->>Db: Recalculate Tool.OverallRating and ReviewCount
    Db-->>ReviewService: Save successful
    ReviewService-->>AdminController: Success
    AdminController-->>Web: 200 OK

    Customer->>Web: View tool/service page
    Web->>API: GET /api/tools/{toolId}/reviews
    API->>ReviewController: Route request
    ReviewController->>ReviewService: GetApprovedReviewsAsync(toolId)
    ReviewService->>Db: Query Status = Approved reviews
    Db-->>ReviewService: Approved reviews
    ReviewService-->>ReviewController: ToolReviewsDto
    ReviewController-->>Web: 200 OK
    Web-->>Customer: Display approved review and updated rating
```

---

## 6. Data Flow Diagrams

### 6.1 Context-Level DFD

This DFD shows the system boundary and the external actors that exchange data with the Review Portal.

```mermaid
flowchart LR
    Customer["Customer / Registered User"]
    Staff["Admin / Moderator"]
    Portal(("Shelton Tool-Hire Review Portal"))
    Db[("SQL Server Database")]
    Storage[("Image Storage")]

    Customer -->|"Browse/search requests, rental dates, reviews, comments, login details"| Portal
    Portal -->|"Catalogue data, cost breakdowns, approved reviews, auth responses, my reviews"| Customer

    Staff -->|"Admin login, tool/category updates, moderation decisions, responses, image uploads"| Portal
    Portal -->|"Moderation queue, dashboard stats, admin operation results"| Staff

    Portal -->|"Read/write catalogue, users, reviews, comments, responses"| Db
    Db -->|"Persisted data"| Portal

    Portal -->|"Store/retrieve tool images"| Storage
    Storage -->|"Image paths/files"| Portal
```

### 6.2 Level 1 DFD

This DFD decomposes the main backend processes and data stores.

```mermaid
flowchart TB
    Customer["Customer / Registered User"]
    Admin["Admin"]
    Moderator["Moderator"]

    P1(("1. Catalogue Browsing"))
    P2(("2. Rental Calculator"))
    P3(("3. Authentication"))
    P4(("4. Reviews and Comments"))
    P5(("5. Moderation"))
    P6(("6. Admin Catalogue Management"))
    P7(("7. Dashboard Reporting"))

    D1[("D1 Users")]
    D2[("D2 Categories")]
    D3[("D3 Tools")]
    D4[("D4 ToolImages")]
    D5[("D5 Reviews")]
    D6[("D6 ReviewComments")]
    D7[("D7 CompanyResponses")]
    D8[("D8 Image Storage")]

    Customer -->|"Category/search/filter/detail requests"| P1
    P1 -->|"Read categories/tools/images/ratings"| D2
    P1 --> D3
    P1 --> D4
    P1 -->|"Catalogue and detail responses"| Customer

    Customer -->|"Start and end date/time"| P2
    P2 -->|"Read rates"| D3
    P2 -->|"Cost breakdown and total"| Customer

    Customer -->|"Register/login/password requests"| P3
    Admin -->|"Admin login"| P3
    Moderator -->|"Moderator login"| P3
    P3 -->|"Create/read/update users"| D1
    P3 -->|"JWT token and user details"| Customer
    P3 -->|"JWT token and role claims"| Admin
    P3 -->|"JWT token and role claims"| Moderator

    Customer -->|"Review/comment submission"| P4
    P4 -->|"Read tool and user context"| D1
    P4 --> D3
    P4 -->|"Write Pending review/comment"| D5
    P4 --> D6
    P4 -->|"Confirmation or validation error"| Customer

    Moderator -->|"Approve/reject reviews/comments"| P5
    Admin -->|"Approve/reject reviews/comments"| P5
    P5 -->|"Read pending items"| D5
    P5 --> D6
    P5 -->|"Update status/rejection reason"| D5
    P5 --> D6
    P5 -->|"Update cached rating/count"| D3
    P5 -->|"Moderation result"| Moderator

    Admin -->|"Create/update/status/category/image commands"| P6
    P6 -->|"Read/write categories/tools/images"| D2
    P6 --> D3
    P6 --> D4
    P6 -->|"Store/delete files"| D8
    P6 -->|"Admin operation result"| Admin

    Admin -->|"Request dashboard stats"| P7
    P7 -->|"Read aggregate data"| D3
    P7 --> D5
    P7 --> D6
    P7 --> D7
    P7 -->|"Dashboard statistics"| Admin
```

---

## 7. Entity Relationship Diagram

The ERD below summarises the database entities, keys, and relationships. The full field-level explanation is maintained in [ERD.md](ERD.md).

```mermaid
erDiagram
    Users {
        int Id PK
        string Name
        string Email
        string PasswordHash
        string PasswordResetTokenHash
        datetime PasswordResetTokenExpiryUtc
        string Role
        datetime CreatedDate
    }

    Categories {
        int Id PK
        string Name
        string Description
        string ImageUrl
    }

    Tools {
        int Id PK
        int CategoryId FK
        string Name
        string Description
        decimal HourlyRate
        decimal DailyRate
        decimal WeeklyRate
        string SpecialNotes
        bool DepositRequired
        decimal DepositAmount
        bool IsActive
        decimal OverallRating
        int ReviewCount
        datetime CreatedDate
        datetime UpdatedDate
    }

    ToolImages {
        int Id PK
        int ToolId FK
        string ImageUrl
        int DisplayOrder
        datetime UploadedDate
    }

    Reviews {
        int Id PK
        int ToolId FK
        int UserId FK
        string ReviewerName
        string ReviewerEmail
        string ReviewText
        int EquipmentRating
        int CustomerServiceRating
        int TechnicalSupportRating
        int AfterSalesRating
        int ValueForMoneyRating
        decimal OverallRating
        string Status
        string RejectionReason
        datetime CreatedDate
    }

    ReviewComments {
        int Id PK
        int ReviewId FK
        int UserId FK
        string CommenterName
        string CommentText
        string Status
        string RejectionReason
        datetime CreatedDate
    }

    CompanyResponses {
        int Id PK
        int ReviewId FK
        int StaffUserId FK
        string ResponseText
        datetime CreatedDate
        datetime UpdatedDate
    }

    Categories ||--o{ Tools : "contains"
    Tools ||--o{ ToolImages : "has"
    Tools ||--o{ Reviews : "receives"
    Users ||--o{ Reviews : "writes"
    Reviews ||--o{ ReviewComments : "has"
    Users ||--o{ ReviewComments : "comments"
    Reviews ||--|| CompanyResponses : "has"
    Users ||--o{ CompanyResponses : "authors"
```

---

## 8. Design Traceability Summary

| Diagram | Supports |
|---------|----------|
| Use Case Diagram | Functional scope for Epic 1, Epic 2, and Epic 3 |
| Class Diagram | Domain model, services, and controller boundaries |
| Activity Diagram | Review submission, moderation, and publication workflow |
| High-Level Architecture Diagram | Clean Architecture, API hosting, database, image storage, and frontend integration |
| Sequence Diagram | End-to-end review lifecycle across frontend, API, service, and database |
| DFD Context and Level 1 | External actors, system boundary, processes, data stores, and data movement |
| ERD | Relational schema supporting catalogue, users, reviews, comments, company responses, and images |
| Database Design | Detailed table schemas, relationships, keys, indexes, constraints, and migration process in [DATABASE-DESIGN.md](DATABASE-DESIGN.md) |
