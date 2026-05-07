# Submission Gap Checklist

> Last checked: 2026-05-07
> Scope: MSc submission package, backend/API documentation, design artefacts, test evidence, and project management artefacts.
>
> Update: the missing `Digrams_V2.docx` artefact has been rebuilt as a submission-ready Word document. The user confirmed the other Word documents have already been submitted, so this checklist now acts as a backend documentation/evidence cross-check rather than a request to rework those submitted files.

## Files Reviewed

Readable Word documents:

- `darft - Change Management System.docx`
- `draft - Group Charter.docx`
- `draft - Initial Project Plan (GANNT)_v2.docx`
- `draft - Intended Project Management Approach.docx`
- `draft - Meeting Format and Schedule.docx`
- `draft - Projected costing & Effort estimation.docx`
- `draft - Risk Assessment.docx`
- `draft - Role and Work Allocation per Person.docx`
- `Version control.docx`

Important note:

- The original `Digrams_V2.docx` in the submission root could not be read locally because the OneDrive file is a reparse-point/online-only file and returned permission denied during extraction.
- A fixed document was generated with embedded diagram images at `C:\Users\user\OneDrive - apiit.lk\MSC SE Semester 1 Project\Submission\Digrams_V2_FIXED.docx`.
- A final-folder copy was also generated at `C:\Users\user\OneDrive - apiit.lk\MSC SE Semester 1 Project\Submission\Final\submission2worddocverison\Digrams_V2.docx`.
- `Digrams_V1.docx` was readable as a fallback, but it only clearly showed use case, class, activity, and high-level architecture material. The generated V2 document uses the wider design set in `docs/FUNCTIONAL-DESIGN-DIAGRAMS.md`.

## Submission Coverage Summary

| Submission area | Current coverage | Additional action needed |
|-----------------|------------------|--------------------------|
| Group charter | Present | Polish grammar and remove draft naming before final export |
| Change management mechanism | Present | Rename `darft` to `draft` or final title; add final change-control examples if available |
| Project plan / Gantt | Present | Confirm sprint dates/status match latest Jira before final export |
| Project management approach | Present | Good coverage; optionally add final Jira/PR evidence screenshots |
| Meeting format and schedule | Present | Add actual meeting minutes/attendance/action evidence if required by marking rubric |
| Costing and effort estimation | Present | Correct typos and confirm story point totals match Jira |
| Risk assessment | Present | Good base; add explicit security/privacy risks from backend scan if space allows |
| Role and work allocation | Present | Good traceability table; update Jira status from In Progress/Backlog to final state before submission |
| Version control | Present | Include latest CI, PR, deployment, and security-scan evidence |
| Functional requirements | Covered in repo docs | Export/include `docs/REQUIREMENTS-SPECIFICATION.md` in final submission pack |
| Non-functional requirements | Covered in repo docs | Export/include `docs/NON-FUNCTIONAL-REQUIREMENTS.md` in final submission pack |
| Legal, ethical, societal, environmental considerations | Covered partly in Risk Assessment | Also reference/include this under requirements so the rubric item is directly satisfied |
| UML, DFD, ERD diagrams | Covered in repo docs | Export/include `docs/FUNCTIONAL-DESIGN-DIAGRAMS.md`; verify `Digrams_V2.docx` contains all diagram types |
| Interface design / wireframes | Not found in listed final Word docs | Add Figma link, screenshots, and short mapping to user journeys/user stories |
| Proposed DB design | Covered in repo docs | Export/include `docs/DATABASE-DESIGN.md` |
| Test plan | Covered in repo docs | Export/include `docs/TEST-PLAN.md` and add actual result evidence after dry run |
| Requirements traceability matrix | Present in repo docs and role-allocation table | Consider adding a dedicated final RTM appendix/docx for easier assessor marking |
| Final completion report | Covered in repo docs | Use `docs/PROJECT-COMPLETION-REPORT.md` for final backend/API readiness sign-off |

## Additional Missing Items To Add Before Submission

### 1. Final requirements document

Create or export a final document from:

- `docs/REQUIREMENTS-SPECIFICATION.md`
- `docs/NON-FUNCTIONAL-REQUIREMENTS.md`

It should explicitly include:

- functional requirements
- non-functional requirements
- legal considerations
- ethical considerations
- societal considerations
- environmental considerations
- requirements traceability to Jira/user stories

Reason:

- The Risk Assessment covers legal/ethical/societal/environmental points, but the rubric asks for them under Functional and Non-Functional Requirements. Make this direct and easy to mark.

### 2. Interface design / wireframes artefact

Add a final wireframes document or appendix containing:

- Figma URL
- screenshots of the main customer screens
- screenshots of the admin/back-office screens
- short mapping from each wireframe to the related user story
- note that the frontend is a separate Next.js project and this backend repo supports the API contracts

Minimum wireframes/screens to include:

- homepage / featured categories
- category browsing
- search/filter results
- tool/service detail
- rental cost calculator
- login/register
- submit review
- approved reviews/comments/company response
- my reviews
- admin dashboard
- moderation queue
- add/edit tool/service
- manage images
- manage categories

### 3. Export latest functional design diagrams

Use `docs/FUNCTIONAL-DESIGN-DIAGRAMS.md` as the source of truth because it now includes:

- use case diagram
- class diagram
- activity diagram
- high-level architecture diagram
- sequence diagram
- DFD
- ERD
- design traceability summary

Before final submission:

- make `Digrams_V2.docx` available offline
- check it contains sequence diagram, DFD, and ERD
- replace any older V1-only diagram content if needed

### 4. Export proposed database design

Use `docs/DATABASE-DESIGN.md` as the final DB design source.

It already covers:

- table catalogue
- table schemas
- primary keys and foreign keys
- relationships
- indexes
- check constraints
- business rules enforced above the database
- ER diagram
- EF migration process
- normalisation/design justification

### 5. Add test execution evidence, not only the test plan

Use `docs/TEST-PLAN.md` for the test plan, then add final evidence:

- black-box test result table with pass/fail values
- dry-run checklist completed with dates/evidence
- usability participant result table
- screenshots or terminal logs for build/test/migration/API smoke tests
- defect log showing no critical open defects, or accepted known issues

### 6. Add backend/API completion evidence

Before moving Jira backend items to Done, include evidence for:

- Epic 1 API contract coverage: `TASK-29`
- Epic 2 API contract coverage: `TASK-30`
- Epic 3 remaining first-image create gap: `TASK-26`
- CI and coverage automation: `TASK-23`
- Azure App Service smoke test after migrations/configuration
- Next.js API route compatibility, especially no assumed `GET /api/tools` endpoint

### 7. Add security and deployment appendix

Use:

- `docs/security/BACKEND-SECURITY-SCAN-2026-05-07.md`
- `docs/DEPLOYMENT-TO-AZURE-APP-SERVICE.md`

Include:

- committed secrets removed
- local secrets externalised
- Azure SQL password rotation still required if not completed
- JWT/auth role checks
- CORS settings for Next.js
- NuGet vulnerability scan status
- CI secret-scan status
- Azure migration/smoke-test evidence

### 8. Polish file names and wording

Before final upload, fix visible naming/grammar issues:

- `darft` -> `draft` or final title
- `Digrams` -> `Diagrams`
- `GANNT` -> `Gantt`
- `Ealry` -> `Early`
- replace `Ł` with `GBP` or the correct pound symbol if the document encoding is wrong
- remove "draft" from final submission filenames if your lecturer expects final artefacts
- update Jira statuses in screenshots/tables so they do not still show older Backlog/In Progress values after work is complete

## Recommended Final Submission Pack Order

1. Group Charter
2. Intended Project Management Approach
3. Meeting Format and Schedule
4. Initial Project Plan / Gantt
5. Role and Work Allocation
6. Costing and Effort Estimation
7. Risk Assessment
8. Change Management System
9. Version Control
10. Requirements Specification
11. Non-Functional Requirements
12. Requirements Traceability Matrix
13. Interface Design / Wireframes
14. Functional Design Diagrams
15. Database Design
16. Test Plan and Test Evidence
17. Backend Security and Deployment Evidence
18. Implementation Gap/Completion Summary

## Backend-Specific Final Checklist

- `dotnet build ReviewPortal.slnx` passes
- `dotnet test ReviewPortal.slnx` passes
- migrations are applied locally
- latest idempotent SQL migration script exists where schema/seed changes were made
- Azure SQL migration applied using rotated credentials
- `/health` works on Azure App Service
- public API endpoints work from the deployed API
- admin endpoints return `401` without token and `403` for customer token
- admin/moderator endpoints work with correct role token
- CORS allows the deployed Next.js origin
- security scan and package vulnerability scan are clean or documented with accepted residual risk
