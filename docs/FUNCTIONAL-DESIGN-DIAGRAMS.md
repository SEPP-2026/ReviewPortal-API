# Functional Design Diagrams - Shelton Tool-Hire Review Portal

> Purpose: MSc submission design artefact covering UML, DFD, ERD, sequence, activity, and high-level architecture diagrams.
>
> Scope: full web system design, including the Next.js client, ASP.NET Core Web API, SQL Server/Azure SQL database, catalogue, rental calculator, reviews, moderation, image management, dashboard, and back-office administration.
>
> Diagram source format: Mermaid. Older PlantUML/plain-text diagram drafts have been converted into Mermaid syntax so this file is the source of truth for generated Word diagram documents.

---

## 1. Use Case Diagram

This diagram shows the major public-customer, registered-customer, admin, and moderator use cases.

```mermaid
flowchart LR
    PublicCustomer["Public Customer<br/>(guest or signed-in)"]
    Guest["Guest Visitor"]
    RegisteredUser["Registered Customer"]
    Admin["Admin"]
    Moderator["Moderator"]

    Guest -. "specialises" .-> PublicCustomer
    RegisteredUser -. "specialises" .-> PublicCustomer

    subgraph PublicPortal["Public Portal"]
        UC1(("Browse categories"))
        UC2(("View tool/service details"))
        UC3(("Search tools/services"))
        UC4(("Filter and sort catalogue"))
        UC5(("Calculate rental cost"))
        UC6(("Read approved reviews"))
        UC7(("Register account"))
        UC8(("Log in / log out"))
        UC9(("Submit review"))
        UC10(("Comment on approved review"))
        UC11(("View own review status"))
        UC12(("Submit booking request"))
        UC13(("Manage account"))
    end

    subgraph BackOffice["Back-Office Portal"]
        UC14(("Staff log in"))
        UC15(("Manage booking requests"))
        UC16(("View moderation queue"))
        UC17(("Approve review"))
        UC18(("Reject review"))
        UC19(("Approve/reject comments"))
        UC20(("Post company response"))
        UC21(("Manage tools/services"))
        UC22(("Upload/delete images"))
        UC23(("Manage categories"))
        UC24(("View admin dashboard"))
    end

    PublicCustomer --> UC1
    PublicCustomer --> UC2
    PublicCustomer --> UC3
    PublicCustomer --> UC4
    PublicCustomer --> UC5
    PublicCustomer --> UC6
    PublicCustomer --> UC9
    PublicCustomer --> UC10
    PublicCustomer --> UC11
    PublicCustomer --> UC12

    Guest --> UC7
    Guest --> UC8
    RegisteredUser --> UC8
    RegisteredUser --> UC13

    Admin --> UC14
    Admin --> UC15
    Admin --> UC16
    Admin --> UC17
    Admin --> UC18
    Admin --> UC19
    Admin --> UC20
    Admin --> UC21
    Admin --> UC22
    Admin --> UC23
    Admin --> UC24

    Moderator --> UC14
    Moderator --> UC15
    Moderator --> UC16
    Moderator --> UC17
    Moderator --> UC18
    Moderator --> UC19
    Moderator --> UC20
```

---

## 2. Class Diagram

This UML class diagram summarises the current backend domain model, service layer, validation layer, and controller boundaries.

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
        decimal? DepositAmount
        bool IsActive
        decimal? OverallRating
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
        int? UserId
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
        string? RejectionReason
        DateTime CreatedDate
        CalculateOverallRating()
    }

    class ReviewComment {
        int Id
        int ReviewId
        int? UserId
        string CommenterName
        string CommentText
        ReviewStatus Status
        string? RejectionReason
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

    class UserRole {
        <<enumeration>>
        Customer
        Admin
        Moderator
    }

    class ReviewStatus {
        <<enumeration>>
        Pending
        Approved
        Rejected
    }

    class AuthService {
        RegisterAsync()
        LoginAsync()
        GetCurrentUserAsync()
        ChangePasswordAsync()
        ForgotPasswordAsync()
        ResetPasswordAsync()
    }

    class CategoryService {
        GetAllCategoriesAsync()
        GetFeaturedCategoriesAsync()
        GetCategoryByIdAsync()
        GetToolsByCategoryAsync()
        CreateCategoryAsync()
        UpdateCategoryAsync()
        DeleteCategoryAsync()
    }

    class ToolService {
        GetToolByIdAsync()
        SearchToolsAsync()
        CalculateRentalCostAsync()
        GetAdminToolsAsync()
        GetAdminToolByIdAsync()
        CreateToolAsync()
        UpdateToolAsync()
        SetToolStatusAsync()
    }

    class ImageService {
        UploadToolImageAsync()
        DeleteToolImageAsync()
        ValidateImageFile()
    }

    class ReviewService {
        CreateReviewAsync()
        GetApprovedReviewsAsync()
        AddCommentAsync()
        GetApprovedCommentsAsync()
        AddCompanyResponseAsync()
        UpdateCompanyResponseAsync()
        DeleteCompanyResponseAsync()
        GetPendingReviewsAsync()
        ModerateReviewAsync()
        ModerateCommentAsync()
    }

    class DashboardService {
        GetDashboardStatsAsync()
    }

    class RequestValidatorRunner {
        ValidateAsync()
    }

    class PublicControllers {
        CategoriesController
        ToolsController
        ToolReviewsController
        ReviewCommentsController
        ReviewResponsesController
        UserReviewsController
        AuthController
    }

    class AdminControllers {
        AdminToolsController
        AdminCategoriesController
        AdminModerationController
        AdminDashboardController
    }

    Category "1" --> "0..*" Tool : contains
    Tool "1" --> "0..*" ToolImage : has
    Tool "1" --> "0..*" Review : receives
    User "0..1" --> "0..*" Review : optional registered author
    Review "1" --> "0..*" ReviewComment : has
    User "0..1" --> "0..*" ReviewComment : optional registered commenter
    Review "1" --> "0..1" CompanyResponse : has
    User "1" --> "0..*" CompanyResponse : authors
    User --> UserRole
    Review --> ReviewStatus
    ReviewComment --> ReviewStatus

    PublicControllers --> AuthService
    PublicControllers --> CategoryService
    PublicControllers --> ToolService
    PublicControllers --> ReviewService
    AdminControllers --> CategoryService
    AdminControllers --> ToolService
    AdminControllers --> ImageService
    AdminControllers --> ReviewService
    AdminControllers --> DashboardService
    AuthService --> RequestValidatorRunner
    CategoryService --> RequestValidatorRunner
    ToolService --> RequestValidatorRunner
    ReviewService --> RequestValidatorRunner
```

---

## 3. Activity Diagrams

### 3.1 Review Submission and Moderation Activity

This activity diagram shows the customer review lifecycle from submission through moderation, rejection, approval, rating recalculation, and public display. It explicitly supports both guest and signed-in customer submissions.

```mermaid
flowchart TD
    Start([Start])
    ViewTool["Customer opens tool/service detail"]
    SelectReview["Select Write a Review"]
    AuthCheck{"Logged in?"}
    UseAccount["Use registered customer profile"]
    CaptureGuest["Capture guest reviewer name and email"]
    CompleteReview["Enter review text and five ratings"]
    ValidateReview{"Text and ratings valid?"}
    ReturnValidation["Return validation errors"]
    SavePending["Save review with Pending status"]
    ShowPending["Show awaiting moderation message"]
    Queue["Review appears in moderation queue"]
    OpenQueue["Admin/Moderator opens moderation queue"]
    Decision{"Approve review?"}
    Reject["Set status to Rejected and store reason"]
    ShowReason["Reason available in own review status"]
    Approve["Set status to Approved"]
    Recalculate["Recalculate tool rating and review count"]
    Publish["Review visible on public tool/service page"]
    End([End])

    Start --> ViewTool --> SelectReview --> AuthCheck
    AuthCheck -->|Yes| UseAccount --> CompleteReview
    AuthCheck -->|No| CaptureGuest --> CompleteReview
    CompleteReview --> ValidateReview
    ValidateReview -->|No| ReturnValidation --> CompleteReview
    ValidateReview -->|Yes| SavePending --> ShowPending --> Queue --> OpenQueue
    OpenQueue --> Decision
    Decision -->|Reject| Reject --> ShowReason --> End
    Decision -->|Approve| Approve --> Recalculate --> Publish --> End
```

### 3.2 Admin Tool/Service and Image Management Activity

This activity diagram shows the current admin catalogue management flow, including validation, create/update, image upload/delete, and public visibility.

```mermaid
flowchart TD
    Start([Start])
    Login["Admin logs in"]
    Authz{"Admin role valid?"}
    Denied["Return 401 or 403"]
    OpenAdmin["Open admin tool/service management"]
    ChooseAction{"Action"}
    CreateOrEdit["Create or edit tool/service details and pricing"]
    ValidateTool{"Required fields and rates valid?"}
    SaveTool["Save tool/service record"]
    UploadImage["Upload JPG, PNG, or WebP image"]
    ValidateImage{"Image type and size valid?"}
    StoreImage["Store image and link to tool/service"]
    DeleteImage["Delete image request"]
    LastImage{"Would this delete last image?"}
    BlockDelete["Reject deletion to keep at least one image"]
    UpdateStatus["Activate or deactivate tool/service"]
    PublicQuery["Public catalogue/search/detail query"]
    Visible{"IsActive?"}
    ShowPublic["Show tool/service publicly"]
    HidePublic["Hide from public browsing/search"]
    End([End])

    Start --> Login --> Authz
    Authz -->|No| Denied --> End
    Authz -->|Yes| OpenAdmin --> ChooseAction
    ChooseAction -->|Create/Edit| CreateOrEdit --> ValidateTool
    ValidateTool -->|No| CreateOrEdit
    ValidateTool -->|Yes| SaveTool --> End
    ChooseAction -->|Upload image| UploadImage --> ValidateImage
    ValidateImage -->|No| UploadImage
    ValidateImage -->|Yes| StoreImage --> End
    ChooseAction -->|Delete image| DeleteImage --> LastImage
    LastImage -->|Yes| BlockDelete --> End
    LastImage -->|No| End
    ChooseAction -->|Change status| UpdateStatus --> PublicQuery --> Visible
    Visible -->|Yes| ShowPublic --> End
    Visible -->|No| HidePublic --> End
```

---

## 4. High-Level System Architecture Diagram

This diagram shows the Clean Architecture structure, deployment shape, external configuration, testing, and frontend integration points.

```mermaid
flowchart TB
    subgraph ClientLayer["Client Layer"]
        Browser["Customer/Admin Browser"]
        NextJs["Next.js Web App"]
    end

    subgraph ApiHost["Azure App Service / ASP.NET Core Web API"]
        Middleware["Middleware: HTTPS, CORS, JWT Auth, Exception Handling"]
        Controllers["API Controllers"]

        subgraph CleanArchitecture["Clean Architecture"]
            Application["Application Layer: DTOs, Interfaces, Services, Validators, Result Pattern"]
            Domain["Domain Layer: Entities, Enums, Business Rules"]
            Infrastructure["Infrastructure Layer: EF Core, Repositories, JWT, Password Hashing, Migrations, Image Storage"]
        end
    end

    subgraph DataLayer["Data Layer"]
        SqlServer["Azure SQL / SQL Server"]
        ImageStorage["Azure Blob Storage: Tool/Service Images"]
    end

    subgraph DevOpsLayer["DevOps and Configuration"]
        GitHubActions["GitHub Actions CI/CD"]
        AppSettings["Azure App Service Settings / User Secrets"]
        Tests["Unit and Integration Tests"]
        SecurityScan["Secret and Package Vulnerability Scans"]
    end

    Browser --> NextJs
    NextJs -->|"HTTPS JSON API + Bearer JWT"| Middleware
    Middleware --> Controllers
    Controllers --> Application
    Application --> Domain
    Application --> Infrastructure
    Infrastructure --> SqlServer
    Infrastructure --> ImageStorage
    SqlServer --> Infrastructure
    ImageStorage --> Infrastructure
    Infrastructure --> Application
    Application --> Controllers
    Controllers --> NextJs

    AppSettings --> ApiHost
    GitHubActions --> Tests
    GitHubActions --> SecurityScan
    GitHubActions --> ApiHost
```

---

## 5. Sequence Diagrams

### 5.1 Review Lifecycle Sequence

This sequence diagram covers customer review submission, moderation, approval, rejection, and rating-summary updates.

```mermaid
sequenceDiagram
    actor Customer as Public Customer
    participant Web as WebApp
    participant API as API Controller
    participant ReviewService
    participant Db as Database
    actor Staff as Admin / Moderator

    Note over Customer,Staff: Submit Review
    Customer->>Web: Submit review form
    Note right of Customer: Guest or signed-in user
    Web->>API: POST review
    API->>ReviewService: SubmitReview(request)
    ReviewService->>ReviewService: Validate review
    ReviewService->>Db: Save review<br/>Status = Pending
    Db-->>ReviewService: Saved
    ReviewService-->>API: Review submitted
    API-->>Web: Awaiting approval
    Web-->>Customer: Show confirmation message

    Note over Customer,Staff: Moderate Review
    Staff->>Web: Open moderation queue
    Web->>API: Get pending reviews
    API->>ReviewService: GetPendingReviews()
    ReviewService->>Db: Query pending reviews
    Db-->>ReviewService: Pending reviews
    ReviewService-->>API: Pending review list
    API-->>Web: Pending review list
    Web-->>Staff: Display moderation queue

    alt Approve review
        Staff->>Web: Approve review
        Web->>API: Approve review
        API->>ReviewService: ApproveReview(reviewId)
        ReviewService->>Db: Set status = Approved<br/>Update rating summary
        Db-->>ReviewService: Updated
        ReviewService-->>API: Approved
        API-->>Web: Success
        Web-->>Staff: Show approved status
    else Reject review
        Staff->>Web: Reject review with reason
        Web->>API: Reject review
        API->>ReviewService: RejectReview(reviewId, reason)
        ReviewService->>Db: Set status = Rejected<br/>Store rejection reason
        Db-->>ReviewService: Updated
        ReviewService-->>API: Rejected
        API-->>Web: Success
        Web-->>Staff: Show rejected status
    end
```

### 5.2 Admin Tool/Service and Image Sequence

This sequence diagram covers the back-office flow for creating or updating a tool/service and managing images.

```mermaid
sequenceDiagram
    actor Admin
    participant Web as Next.js Admin UI
    participant API as ASP.NET Core API
    participant AdminTools as AdminToolsController
    participant ToolService
    participant ImageService
    participant Db as SQL Server
    participant Storage as Image Storage

    Admin->>Web: Enter tool/service details and pricing
    Web->>API: POST /api/admin/tools
    API->>AdminTools: Authorize Admin
    AdminTools->>ToolService: CreateToolAsync(request)
    ToolService->>ToolService: Validate category, rates, and required fields
    ToolService->>Db: Insert Tool and initial image reference
    Db-->>ToolService: Save successful
    ToolService-->>AdminTools: AdminToolDetailDto
    AdminTools-->>Web: 201 Created

    Admin->>Web: Upload additional image
    Web->>API: POST /api/admin/tools/{id}/images
    API->>AdminTools: Authorize Admin
    AdminTools->>ImageService: UploadToolImageAsync(toolId, file)
    ImageService->>ImageService: Validate file type and size
    ImageService->>Storage: Store image file
    ImageService->>Db: Insert ToolImage record
    Db-->>ImageService: Save successful
    ImageService-->>AdminTools: ToolImageDto
    AdminTools-->>Web: 201 Created

    Admin->>Web: Deactivate unavailable item
    Web->>API: PATCH /api/admin/tools/{id}/status
    API->>AdminTools: Authorize Admin
    AdminTools->>ToolService: SetToolStatusAsync(id, isActive=false)
    ToolService->>Db: Update Tool.IsActive
    Db-->>ToolService: Save successful
    ToolService-->>AdminTools: Success result
    AdminTools-->>Web: 200 OK
```

---

## 6. Data Flow Diagrams

### 6.1 Context-Level DFD

This context-level DFD shows the system boundary and external actors that exchange data with the Review Portal.

```mermaid
flowchart LR
    Customer["Customer / Registered User"]
    Staff["Admin / Moderator"]
    Frontend["Next.js Web App"]
    Portal(("ReviewPortal API System"))
    Db[("SQL Server / Azure SQL")]
    Storage[("Image Storage")]

    Customer -->|"Browse/search requests, rental dates, reviews, comments, login details"| Frontend
    Staff -->|"Admin login, catalogue updates, image uploads, moderation decisions"| Frontend
    Frontend -->|"HTTPS JSON API requests and Bearer JWT"| Portal
    Portal -->|"API responses, validation errors, JWTs, catalogue data, moderation data"| Frontend
    Frontend -->|"Rendered pages, forms, results, dashboard views"| Customer
    Frontend -->|"Admin screens and operation feedback"| Staff

    Portal -->|"Read/write users, categories, tools, reviews, comments, responses"| Db
    Db -->|"Persisted records and query results"| Portal
    Portal -->|"Store/delete/retrieve image paths and files"| Storage
    Storage -->|"Image URLs/files"| Portal
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
    P6(("6. Admin Tool and Category Management"))
    P7(("7. Image Management"))
    P8(("8. Dashboard Reporting"))

    D1[("D1 Users")]
    D2[("D2 Categories")]
    D3[("D3 Tools")]
    D4[("D4 ToolImages")]
    D5[("D5 Reviews")]
    D6[("D6 ReviewComments")]
    D7[("D7 CompanyResponses")]
    D8[("D8 Image Storage")]

    Customer -->|"Category/search/filter/detail requests"| P1
    P1 -->|"Read categories"| D2
    P1 -->|"Read active tools and ratings"| D3
    P1 -->|"Read image references"| D4
    P1 -->|"Catalogue/detail results"| Customer

    Customer -->|"Start and end date/time"| P2
    P2 -->|"Read hourly/daily/weekly rates"| D3
    P2 -->|"Cost breakdown and total"| Customer

    Customer -->|"Register/login/password reset"| P3
    Admin -->|"Admin login"| P3
    Moderator -->|"Moderator login"| P3
    P3 -->|"Create/read/update users"| D1
    P3 -->|"JWT token and role claims"| Customer
    P3 -->|"JWT token and role claims"| Admin
    P3 -->|"JWT token and role claims"| Moderator

    Customer -->|"Review/comment/company-response requests"| P4
    P4 -->|"Read user and tool context"| D1
    P4 --> D3
    P4 -->|"Write pending reviews"| D5
    P4 -->|"Write pending comments"| D6
    P4 -->|"Write official response for approved review"| D7
    P4 -->|"Confirmation or validation error"| Customer

    Moderator -->|"Approve/reject reviews/comments"| P5
    Admin -->|"Approve/reject reviews/comments"| P5
    P5 -->|"Read pending reviews/comments"| D5
    P5 --> D6
    P5 -->|"Update status and rejection reason"| D5
    P5 --> D6
    P5 -->|"Update cached rating/count"| D3
    P5 -->|"Moderation result and exact counts"| Moderator

    Admin -->|"Create/update/status/category commands"| P6
    P6 -->|"Read/write categories"| D2
    P6 -->|"Read/write tools"| D3
    P6 -->|"Admin operation result"| Admin

    Admin -->|"Upload/delete image commands"| P7
    P7 -->|"Read/write tool images"| D4
    P7 -->|"Store/delete files"| D8
    P7 -->|"Image operation result"| Admin

    Admin -->|"Request dashboard stats"| P8
    P8 -->|"Read aggregates"| D3
    P8 --> D5
    P8 --> D6
    P8 --> D7
    P8 -->|"Dashboard statistics"| Admin
```

---

## 7. Entity Relationship Diagram

The ERD summarises the database entities, keys, and relationships. The full field-level explanation is maintained in [ERD.md](ERD.md) and [DATABASE-DESIGN.md](DATABASE-DESIGN.md).

```mermaid
erDiagram
    Users {
        int Id PK
        string Name
        string Email UK
        string PasswordHash
        string PasswordResetTokenHash
        datetime PasswordResetTokenExpiryUtc
        string Role
        datetime CreatedDate
    }

    Categories {
        int Id PK
        string Name UK
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
        int ReviewId FK,UK
        int StaffUserId FK
        string ResponseText
        datetime CreatedDate
        datetime UpdatedDate
    }

    Categories ||--o{ Tools : contains
    Tools ||--o{ ToolImages : has
    Tools ||--o{ Reviews : receives
    Users ||--o{ Reviews : writes
    Reviews ||--o{ ReviewComments : has
    Users ||--o{ ReviewComments : writes
    Reviews ||--o| CompanyResponses : has
    Users ||--o{ CompanyResponses : authors
```

---

## 8. Design Traceability Summary

| Diagram | Supports |
|---------|----------|
| Use Case Diagram | Functional scope for Epic 1, Epic 2, and Epic 3 |
| Class Diagram | Domain model, services, validators, controllers, and role/status enums |
| Activity Diagrams | Review moderation workflow and admin tool/image management workflow |
| High-Level Architecture Diagram | Clean Architecture, Azure hosting, configuration, CI/CD, tests, database, image storage, and frontend integration |
| Sequence Diagrams | End-to-end review lifecycle and admin tool/image management |
| DFD Context and Level 1 | External actors, system boundary, API processes, data stores, and data movement |
| ERD | Relational schema supporting users, categories, tools/services, images, reviews, comments, company responses, and rating aggregation |
| Database Design | Detailed table schemas, relationships, keys, indexes, constraints, and migration process in [DATABASE-DESIGN.md](DATABASE-DESIGN.md) |
