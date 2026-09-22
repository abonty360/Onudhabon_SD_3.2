<div align="center">

# 🎓 Onudhabon (অনুধাবন)
### *Bridging the Educational Divide Through Community Empowerment*

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-6C2A9B?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Cloudinary](https://img.shields.io/badge/Cloud%20Media-Cloudinary-3448C5?style=for-the-badge&logo=cloudinary&logoColor=white)](https://cloudinary.com/)
[![Google Gemini](https://img.shields.io/badge/AI%20Assistant-Google%20Gemini-4285F4?style=for-the-badge&logo=google&logoColor=white)](https://ai.google.dev/)
[![Bootstrap](https://img.shields.io/badge/UI-Bootstrap%205-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Docker](https://img.shields.io/badge/Deployment-Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)

<p align="center">
  <b>Onudhabon</b> is a web-based educational and social welfare platform developed to empower underprivileged, out-of-school, and rural children across Bangladesh. By uniting volunteer educators, local guardians, benevolent donors, and system administrators into an integrated ecosystem, Onudhabon provides free access to structured national curriculum (NCTB) learning resources, individual student progress tracking, transparent fundraising, and AI-powered study assistance.
</p>

---

[Key Highlights](#-key-highlights) •
[System Architecture](#-system-architecture) •
[User Roles & Capabilities](#-user-roles--capabilities) •
[Student Progress Tracking & Promotion](#-student-progress-tracking--promotion-engine) •
[Volunteer Ranking & Reward System](#-volunteer-ranking--rewards-system) •
[AI Assistant & PDF Grounding](#-ai-assistant--pdf-grounding-rag) •
[Project Structure](#-project-structure) •
[Getting Started](#-getting-started) •
[Deployment](#-deployment)

---

</div>

## 🌟 Key Highlights

- **Complete NCTB Curriculum Alignment (Classes 1–12):** Pre-configured class plans dynamically structure lectures and syllabus subjects from primary (Classes 1–3) to higher secondary (Classes 11–12).
- **Comprehensive Student Progress Tracker:** Real-time monitoring of lecture completion, attendance rates, exam marks, lecture evaluations, GPA, and letter grades with an automated class promotion pipeline.
- **Volunteer Educator & Guardian Gamification:** Automated monthly ranking engine calculating volunteer performance scores and designating eligible reward recipients from donation funding.
- **AI Study Assistant with Real-Time PDF Grounding (RAG):** Powered by Google Gemini and UglyToad.PdfPig, learners can attach study notes or textbook PDFs (up to 25MB) to receive contextual explanations, math problem solving, and chapter summaries.
- **Robust Multi-Role Verification Pipeline:** Multi-tier authentication featuring email existence checks, 6-digit OTP verification, NID/certificate document uploads, and administrator approval gates.
- **Integrated Payment Gateway:** Supports SSLCommerz hosted checkout (bKash, Nagad, Rocket, Visa/Mastercard) and seamless in-app payment simulation with instant automated receipt generation.
- **Cloud Media Management:** Cloudinary CDN integration for automatic video transformations, responsive thumbnails, PDF first-page rendering, and secure profile/consent document storage.
- **Interactive Community Forum:** Topic-based discussion boards with Markdown support, comment threads, like/dislike reactions, author identity cards, and admin moderation.

---

## 🏛️ System Architecture

Onudhabon follows the **ASP.NET Core Model-View-Controller (MVC)** architectural pattern with clean service-oriented abstractions and dependency injection.

```mermaid
flowchart TD
    subgraph ClientLayer ["Client Layer (Browser)"]
        RazorViews["Razor Views (.cshtml) + Bootstrap 5"]
        ChatWidget["AI Chatbot Widget (_ChatWidget)"]
        PaymentModals["Payment & Evaluation Modals"]
    end

    subgraph ControllerLayer ["Controller Layer (ASP.NET Core)"]
        AccountCtrl["AccountController\n(Auth, OTP, Profile)"]
        StudentCtrl["StudentController\n(Enrollment, Tracking, Promotion)"]
        AdminCtrl["AdminController\n(Moderation, Ranking, Users)"]
        LectureCtrl["LectureController\n(Video Management)"]
        MaterialCtrl["MaterialController\n(PDF Documents)"]
        ForumCtrl["ForumController\n(Community Posts & Reactions)"]
        DonationCtrl["DonationController\n(SSLCommerz & Receipts)"]
        ChatCtrl["ChatController\n(Gemini Cloud & PDF RAG)"]
    end

    subgraph ServiceLayer ["Service Layer"]
        CloudinarySvc["CloudinaryService\n(Media Upload & Thumbnails)"]
        EmailSvc["EmailService & EmailValidationService\n(SMTP, Resend & MX Validation)"]
        RankingSvc["VolunteerRankingService\n(Monthly Performance Scores)"]
        LlmSvc["GeminiChatService\n(Google Gemini API)"]
        PdfSvc["PdfKnowledgeService\n(UglyToad.PdfPig Text Extraction)"]
        SslcSvc["SSLCommerzService\n(Payment Initiation & IPN)"]
    end

    subgraph DataLayer ["Data & Persistence Layer"]
        AppDb["ApplicationDbContext (EF Core)"]
        Postgres[(PostgreSQL Database)]
        MemoryCache["IMemoryCache (In-Memory Fast Retrieval)"]
    end

    subgraph ExternalAPIs ["External Cloud Services"]
        CloudinaryAPI["Cloudinary CDN"]
        GeminiAPI["Google Gemini LLM"]
        SmtpAPI["Gmail SMTP / Resend"]
        SslcAPI["SSLCommerz Gateway"]
    end

    ClientLayer --> ControllerLayer
    ControllerLayer --> ServiceLayer
    ControllerLayer --> AppDb
    ServiceLayer --> AppDb
    AppDb --> Postgres
    ControllerLayer --> MemoryCache
    ServiceLayer --> ExternalAPIs
```

### Request & Authentication Pipeline
1. **Reverse Proxy & Forwarded Headers:** Handled via `ForwardedHeadersOptions` for cloud environments (Render, Cloudflare, Docker).
2. **Cookie Authentication & Session State:** Uses `Onudhabon.AuthCookie` and `Onudhabon.Session` with a sliding 30-minute expiration window.
3. **Dynamic Principal Validation (`OnValidatePrincipal`):** Intercepts every authenticated request to check real-time status in PostgreSQL. If an account is restricted, declined, or unverified, the principal is immediately rejected and signed out.
4. **Anti-Cache Middleware:** Enforces `Cache-Control: no-cache, no-store, must-revalidate` on auth endpoints and protected dashboards to prevent browser back-button cache leaks after logout.
5. **Private Network Access (PNA):** Supports secure CORS and local callback headers for payment gateway IPN communications.

---

## 👥 User Roles & Capabilities

The platform operates on a granular role-based authorization model:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                             ONUDHABON ROLES                                 │
├──────────────┬──────────────┬──────────────────┬──────────────┬─────────────┤
│    Admin     │   Educator   │  Local Guardian  │    Donor     │   Learner   │
│ (Governance) │  (Teacher)   │ (Student Mentor) │ (Supporter)  │  (Student)  │
└──────────────┴──────────────┴──────────────────┴──────────────┴─────────────┘
```

| Role | Responsibilities & Permissions |
| :--- | :--- |
| **System Admin** | • Full access to the **Admin Dashboard** (`/Admin/Dashboard`).<br>• Review and approve/decline new volunteer registrations (Educators and Local Guardians).<br>• Moderate video lectures, study materials, forum posts, and student enrollments.<br>• Restrict or unblock user accounts immediately.<br>• Monitor monthly volunteer rankings and oversee reward distributions.<br>• Review donation transactions and audit system metrics. |
| **Educator (Volunteer Teacher)** | • Upload recorded video lectures with video files or hosted URLs (`/Lecture/Upload`).<br>• Upload downloadable study materials, lecture sheets, and past papers (`/Material/Upload`).<br>• Automatic video thumbnail generation and PDF preview creation via Cloudinary.<br>• View approval statuses for uploaded content (`Active`, `Pending`, `Declined`).<br>• Compete on the monthly Educator Leaderboard based on content contribution.<br>• Participate in forum discussions and create academic threads. |
| **Local Guardian** | • Register underprivileged children who lack access to formal education (`/Student/Enroll`).<br>• Upload student identification and parent/guardian consent letters to Cloudinary.<br>• Access the **Student Progress Tracker** (`/Student/Progress`).<br>• Update lecture completion, attendance percentage, and teacher notes.<br>• Evaluate students lecture-by-lecture with topics, marks, grades, and remarks.<br>• Record term exam evaluations with syllabus details.<br>• Promote students to the next grade once 100% curriculum completion is attained.<br>• Earn performance points on the Guardian Leaderboard. |
| **Donor** | • Contribute financial aid towards child education and volunteer rewards (`/Donation`).<br>• Complete donations via SSLCommerz (bKash, Nagad, cards) or in-app instant checkout.<br>• Support students anonymously or publicly on the recent supporters feed.<br>• Download and print verified donation receipts with unique transaction IDs. |
| **Student / General Public** | • Browse approved video lectures filtered by Class Level, Subject, and Topic (`/Lecture`).<br>• View lectures using the integrated responsive video player.<br>• Access and download approved study materials and notes (`/Material`).<br>• Read community discussions in the forum (`/Forum`).<br>• Engage with the **Onudhabon AI Assistant** for Q&A and PDF study assistance. |

---

## 📊 Student Progress Tracking & Promotion Engine

The Student Progress Tracking subsystem is one of the most critical components of Onudhabon. It enables Local Guardians and Administrators to monitor and record a child's academic development with precision.

```mermaid
sequenceDiagram
    autonumber
    actor Guardian as Local Guardian
    participant System as Onudhabon Platform
    participant DB as PostgreSQL Database
    participant CDN as Cloudinary

    Guardian->>System: 1. Fill Enrollment Form (Name, Birth Cert ID, Class, Consent File)
    System->>CDN: 2. Upload Consent Document
    CDN-->>System: Return Secure URL
    System->>DB: 3. Query ClassPlans for Class Level NCTB Subjects
    DB-->>System: Return Subjects & Lecture Targets
    System->>DB: 4. Save Student (Status = "Pending")
    Note over System,DB: Admin approves student -> Status becomes "Active"
    Guardian->>System: 5. Open Progress Tracker (/Student/Progress)
    System->>DB: 6. Load Student & SubjectProgressJson
    DB-->>System: Parse JSON into SubjectProgressItems
    System-->>Guardian: Display SaaS KPI Cards, Attendance, Progress Bars & GPAs
    Guardian->>System: 7. Submit Lecture Evaluation (Lec #, Topic, Marks, Grade)
    System->>System: 8. Recalculate Subject GPA, Overall GPA & Progress %
    System->>DB: 9. Save Updated SubjectProgressJson
    Note over Guardian,System: When Overall Progress reaches 100%
    Guardian->>System: 10. Click "Promote to Next Class"
    System->>DB: 11. Class Level += 1, CompletedClasses += 1, Progress = 0%
    System->>DB: 12. Load Next Class NCTB Subjects & Initialize New Progress JSON
    System-->>Guardian: Success: Student Promoted!
```

### 1. Curriculum Architecture & NCTB Class Plans
Curriculum plans are defined in `ClassPlans` and dynamically provisioned based on Bangladesh National Curriculum and Textbook Board (NCTB) standards:
- **Classes 1 to 3:** 3 foundational subjects (*Bangla, English, Math*), 10 lectures each (30 total lectures per academic year).
- **Classes 4 to 8:** 7 core subjects (*Bangla 1st, Bangla 2nd, English 1st, English 2nd, Math, Social Science, General Science*), 12 lectures each (84 total lectures).
- **Classes 9 to 10:** 11 secondary subjects (*adding Physics, Chemistry, Higher Math, Biology*), 12 lectures each (132 total lectures).
- **Classes 11 to 12:** 12 higher secondary subjects (*Physics 1st/2nd, Chemistry 1st/2nd, Higher Math 1st/2nd, Biology 1st/2nd, Bangla, English*), 20 lectures each (240 total lectures).

### 2. The Progress Data Model (`SubjectProgressJson`)
Progress is serialized in the `Students.SubjectProgressJson` column as a structured JSON document, eliminating rigid database schema migrations when curriculum structures evolve:

```json
[
  {
    "name": "Physics 1st paper",
    "totalLectures": 20,
    "completedLectures": 3,
    "syllabus": "Lectures 1 to 3",
    "marks": 88.5,
    "grade": "A+",
    "lectures": [
      {
        "no": 1,
        "top": "Physical World and Measurement",
        "m": 90.0,
        "g": "A+",
        "d": "2026-09-15",
        "rem": "Understood dimensional analysis well."
      },
      {
        "no": 2,
        "top": "Vectors: Addition & Dot Product",
        "m": 85.0,
        "g": "A",
        "d": "2026-09-18",
        "rem": "Good grasp of triangle law."
      }
    ]
  }
]
```

### 3. Grading & GPA Calculation Logic
The grading scale is modeled on the official Bangladeshi grading system:

| Letter Grade | Marks Range | Grade Point (GPA) |
| :---: | :---: | :---: |
| **A+** | 80% – 100% | **5.0** |
| **A** | 70% – 79% | **4.0** |
| **A-** | 60% – 69% | **3.5** |
| **B** | 50% – 59% | **3.0** |
| **C** | 40% – 49% | **2.0** |
| **D** | 33% – 39% | **1.0** |
| **F** | 0% – 32% | **0.0** |

- **Subject GPA:** Computed as the average grade points of all evaluated lectures within that subject.
- **Overall GPA:** Computed as the arithmetic mean of all evaluated lecture points across every subject:
  $$\text{GPA} = \frac{1}{N} \sum_{i=1}^{N} \text{Point}(l_i)$$
- **Fail Rule:** If a student receives an **F** in any evaluated lecture or exam, the overall grade immediately defaults to **F (0.00 GPA)**, reflecting standard academic standards.

### 4. Progress Update Modals
The Tracker interface (`/Student/Progress`) provides three dedicated modal workflows:
1. **Quick Progress Modal:** Update completed lecture counts per subject via sliders or counters, update overall attendance percentage (0–100%), and record qualitative mentor notes.
2. **Exam Evaluation Modal:** Record formal term exam evaluations including covered syllabus, overall subject marks, and letter grades.
3. **Detailed Lecture Evaluation Modal:** Add or edit individual lecture evaluations with lecture number, topic name, individual quiz/assessment score, letter grade, and instructor remarks.

### 5. Automated Promotion Workflow
- When a student's `OverallProgressPercent` reaches **100.0%** (all planned lectures across all curriculum subjects completed) and the enrollment status is not declined, the system enables the **"Promote to Next Class"** button.
- When triggered (`POST /Student/PromoteStudent`), the controller:
  1. Verifies ownership and permissions.
  2. Increments `ClassLevel` (e.g., Class 5 $\to$ Class 6).
  3. Increments `CompletedClasses` counter.
  4. Resets `ProgressPercentage` to **0%**.
  5. Queries `ClassPlans` for the newly assigned grade level's subjects.
  6. Reinitializes `SubjectProgressJson` with the fresh curriculum lecture quotas.
  7. Updates `LastActivityDate` and commits the changes atomically.
  8. If Class 12 is completed, awards graduation recognition.

---

## 🏆 Volunteer Ranking & Rewards System

Onudhabon motivates volunteer educators and guardians through transparent monthly leaderboards managed by `VolunteerRankingService`.

### Scoring Formulas

#### 1. Educator Ranking Score
Educators earn rank based on approved educational content contributed during the selected calendar month:
$$\text{Score}_{\text{Educator}} = \text{Monthly Approved Lectures} + \text{Monthly Approved Study Materials}$$

*Tiebreakers:* Higher lecture count $\to$ higher material count $\to$ alphabetical order.

#### 2. Local Guardian Ranking Score
Local Guardians earn rank based on community student outreach and academic progress achieved under their mentorship:
$$\text{Score}_{\text{Guardian}} = (\text{Enrollment Component} \times 0.5) + (\text{Average Student Progress \%} \times 0.5)$$

Where:
$$\text{Enrollment Component} = \min\left(100.0, \frac{\text{Total Assigned Students}}{\text{Benchmark (10)}} \times 100.0\right)$$

### Monthly Financial Honorarium
- For completed calendar months, the top 3 ranked volunteers in each category become **Reward Eligible** (`IsRewardEligible = true`).
- Configurable reward allocation:
  - **Rank 1:** ৳5,000 BDT
  - **Rank 2:** ৳3,000 BDT
  - **Rank 3:** ৳2,000 BDT
- Rewards are funded through the transparent **Donation Pool**.

---

## 🤖 AI Assistant & PDF Grounding (RAG)

The platform features a floating AI learning assistant accessible on all pages via `_ChatWidget.cshtml` and `ChatController.cs`.

```
┌─────────────────────────────────────────────────────────────┐
│                    Onudhabon AI Assistant                   │
├─────────────────────────────────────────────────────────────┤
│ • Grounded in Google Gemini API (gemini-flash-lite-latest)  │
│ • Client-side PDF upload with client preview & file size cap│
│ • Server-side UglyToad.PdfPig multi-page text extraction    │
│ • 5-minute IMemoryCache for platform database context       │
│ • Strict prompt boundary guarding against hallucination     │
│ • Full Bengali & English bilingual conversation support     │
└─────────────────────────────────────────────────────────────┘
```

### Key AI Capabilities:
1. **Interactive Document Study (PDF RAG):** Learners can click the paperclip icon (📎) and attach study notes, textbook chapters, or question papers (up to 25MB). The system extracts readable text via `PdfKnowledgeService` and grounds responses strictly in the document text to answer questions, explain concepts, and solve exercises.
2. **Platform Guidance & Navigation:** Assists users with platform information, including how to enroll students, upload lectures, donate, or track progress, complete with clickable markdown navigation links.
3. **Strict Domain Boundary Policy:** Prohibits general world trivia, politics, entertainment, or external coding tasks to keep focus entirely on educational support. If an outside query is detected without an attached PDF, the assistant politely declines in English or Bengali and invites the user to attach their study notes.

---

## 💳 Donation System & SSLCommerz Integration

The platform facilitates transparent community funding to support underprivileged students and reward top-performing volunteers.

### Features:
- **SSLCommerz Hosted Checkout:** Integrated with SSLCommerz sandbox and production gateways (`SSLCommerzService.cs`) supporting bKash, Nagad, Rocket, Upay, Visa, MasterCard, and internet banking.
- **Instant bKash / Nagad / Card Simulation:** Integrated in-app simulated payment modal for instantaneous testing without external gateway redirects.
- **IPN (Instant Payment Notification):** Webhook handler validates transaction authenticity, currency (`BDT`), and order IDs before finalizing donation records.
- **Automated Digital Receipts:** Every completed donation generates a unique transaction receipt (`/Donation/Receipt/{id}`) displaying donor details, purpose, transaction ID, payment method, amount, and timestamp, formatted for printing or PDF download.
- **Anonymous Contributions:** Donors can choose to conceal their identities on public supporter listings.

---

## 📁 Project Structure

```
Onudhabon/
├── Controllers/                         # MVC Application Controllers
│   ├── AccountController.cs            # Auth, OTP, Email verification, Password reset, Profile
│   ├── AdminController.cs              # Moderation, volunteer management, monthly rankings
│   ├── ChatController.cs               # AI assistant, PDF upload & grounding endpoint
│   ├── DonationController.cs           # Donation processing, SSLCommerz, digital receipts
│   ├── ForumController.cs              # Community posts, comments, likes/dislikes
│   ├── HomeController.cs               # Landing page, about, privacy, navigation
│   ├── LectureController.cs            # Video lecture streaming, uploading, filtering
│   ├── MaterialController.cs           # PDF/Doc study material management & download
│   └── StudentController.cs            # Student enrollment, progress tracking, promotion
│
├── Models/                              # Domain Entities & ViewModels
│   ├── AdminDashboardViewModel.cs      # Aggregated dashboard metrics & collections
│   ├── AuthViewModel.cs                # Profile editing & credential management
│   ├── ClassPlan.cs                    # NCTB class level curriculum & subjects model
│   ├── Donation.cs                     # Donation entity (amount, method, trxId, status)
│   ├── DonationViewModel.cs            # Donation form & checkout models
│   ├── Forum.cs / ForumPost.cs         # Forum entities, comments & reactions
│   ├── Lecture.cs                      # Video lecture metadata & Cloudinary URLs
│   ├── Material.cs                     # Study material metadata & download counts
│   ├── Notification.cs                 # User approval & system notifications
│   ├── Student.cs                      # Student model with JSON progress mapping
│   ├── StudentEnrollmentViewModel.cs   # Guardian enrollment input model
│   ├── StudentProgressViewModel.cs     # Progress cards, GPA, evaluations & lecture items
│   ├── User.cs                         # Application user entity with qualification fields
│   └── VolunteerRankingViewModel.cs    # Monthly educator & guardian leaderboard models
│
├── Services/                            # Business Logic & External Integrations
│   ├── CloudinaryService.cs            # Cloudinary media uploads, transformations & CDN
│   ├── EmailService.cs                 # SMTP & Resend email delivery for OTP & alerts
│   ├── EmailValidationService.cs       # Real-time email syntax & MX record validation
│   ├── GeminiChatService.cs            # Google Gemini cloud AI integration
│   ├── PdfKnowledgeService.cs          # UglyToad.PdfPig PDF text extraction
│   ├── SSLCommerzService.cs            # SSLCommerz hosted payment gateway integration
│   └── VolunteerRankingService.cs      # Volunteer ranking & score calculation engine
│
├── Data/                                # Persistence & Seeding
│   ├── ApplicationDbContext.cs         # EF Core DbContext with PostgreSQL configuration
│   ├── DbInitializer.cs                # Database seeder (Admin, ClassPlans, Forum, Students)
│   └── PostgresConnectionHelper.cs     # URI to Npgsql connection string parser
│
├── Views/                               # Razor View Templates
│   ├── Account/                        # Login, Register, VerifyOtp, ResetPassword, Profile
│   ├── Admin/                          # Unified Admin Dashboard with management tabs
│   ├── Donation/                       # Donation form, modal checkout, digital receipt
│   ├── Forum/                          # Discussion list, post modals, comment threads
│   ├── Home/                           # Hero landing page, about us, impact statistics
│   ├── Lecture/                        # Video lecture gallery, video player, upload form
│   ├── Material/                       # Study material gallery, preview, upload form
│   ├── Shared/                         # _Layout, _ChatWidget, _ValidationScriptsPartial
│   └── Student/                        # Enrolled students directory, progress tracker
│
├── wwwroot/                             # Static Web Assets
│   ├── css/                            # Custom styles (site.css, layout stylesheets)
│   ├── js/                             # Client-side JavaScript (chat, tracking, modals)
│   ├── images/                         # Logos, badges, and default graphics
│   └── lib/                            # Bootstrap 5, Bootstrap Icons, jQuery
│
├── Migrations/                          # EF Core Database Migrations
├── .github/workflows/ci-cd.yml          # GitHub Actions Continuous Integration pipeline
├── Dockerfile                           # Multi-stage container build definition
├── render.yaml                          # Render cloud deployment blueprint
├── Program.cs                           # Dependency injection, middleware & routing
└── Onudhabon.csproj                     # .NET project file & package dependencies
```

---

## 🛠️ Technology Stack Breakdown

| Component | Technologies Used |
| :--- | :--- |
| **Framework** | ASP.NET Core 10.0 (C# 13 / .NET 10 Web SDK) |
| **Data Access** | Entity Framework Core 10.0, Npgsql PostgreSQL Provider |
| **Database** | PostgreSQL 15+ (Hosted on Neon, Supabase, Render, or Local) |
| **Frontend UI** | Razor Pages (.cshtml), Bootstrap 5.3, Bootstrap Icons 1.11, Plus Jakarta Sans |
| **Cloud Storage** | Cloudinary .NET SDK (Video transformations, PDF previews, Avatars) |
| **Artificial Intelligence** | Google Gemini API (`gemini-flash-lite-latest` / `gemini-1.5-flash`) |
| **Document Processing** | UglyToad.PdfPig (High-performance PDF text extraction) |
| **Payment Gateway** | SSLCommerz REST API (Live & Sandbox) + In-App Simulation |
| **Email Services** | System.Net.Mail (Gmail SMTP TLS 587) & Resend API |
| **Authentication** | ASP.NET Core Cookie Authentication with Custom Principal Validator |
| **Containerization** | Docker (Alpine/Debian ASP.NET 10 Runtime) |
| **Deployment** | Render, Docker Compose, Linux/Windows Server |

---

## ⚙️ Configuration & Environment Variables

The application reads configuration from environment variables or a `.env` file in the root directory:

```ini
# =========================================================
# Database Configuration (PostgreSQL)
# =========================================================
DATABASE_URL=postgres://username:password@localhost:5432/onudhabon_db

# =========================================================
# Cloudinary CDN Configuration
# =========================================================
CLOUDINARY_CLOUD_NAME=your_cloud_name
CLOUDINARY_API_KEY=your_cloudinary_api_key
CLOUDINARY_API_SECRET=your_cloudinary_api_secret
CLOUDINARY_FOLDER=onudhabon

# =========================================================
# Google Gemini AI Configuration
# =========================================================
GEMINI_API_KEY=your_google_gemini_api_key

# =========================================================
# Email / SMTP Configuration (Gmail or Custom)
# =========================================================
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
GMAIL_USER=your_email@gmail.com
GMAIL_APP_PASSWORD=your_google_app_password

# =========================================================
# Payment Gateway (SSLCommerz)
# =========================================================
SSLC_STORE_ID=testbox
SSLC_STORE_PASSWORD=qwerty
SSLC_IS_SANDBOX=true

# =========================================================
# Optional Resend API (Alternative Email Service)
# =========================================================
RESEND_API_KEY=your_resend_api_key
RESEND_FROM_EMAIL=Onudhabon <onboarding@resend.dev>
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- A free [Cloudinary](https://cloudinary.com/) account
- A free [Google AI Studio (Gemini)](https://aistudio.google.com/) API Key

### Local Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/abonty360/Onudhabon_SD_3.2.git
   cd Onudhabon_SD_3.2
   ```

2. **Configure Environment Variables:**
   Create a `.env` file in the project root based on the configuration template above.

3. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

4. **Apply Database Migrations & Seed Data:**
   When the application runs, `Program.cs` automatically applies pending EF Core migrations and seeds initial database records via `DbInitializer.cs`:
   ```bash
   dotnet run
   ```

5. **Access the Application:**
   Open your browser and navigate to:
   ```
   http://localhost:5000  or  https://localhost:5001
   ```

---

## 🔑 Default Credentials & Testing Accounts

The database seeder automatically initializes a default Administrator account and initial class plans upon initial startup:

| Role | Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Educator** | Register via `/Account/Register` | *Set during registration* | Upload Lectures & Materials (Approved by Admin) |
| **Local Guardian** | Register via `/Account/Register` | *Set during registration* | Enroll Students & Track Progress (Approved by Admin) |

> **Note for Testing New Volunteers:**  
> When you register as an Educator or Local Guardian:
> 1. Complete the registration form and verify your email via the 6-digit OTP code.
> 2. Wait for an admin to **Approve**.

---

## Docker & Cloud Deployment

### Running with Docker

1. **Build the Docker Image:**
   ```bash
   docker build -t onudhabon-web:latest .
   ```

2. **Run the Container:**
   ```bash
   docker run -d -p 8080:8080 --env-file .env --name onudhabon-app onudhabon-web:latest
   ```
   Access the app at `http://localhost:8080`.

### Deploying to Render Cloud
A pre-configured `render.yaml` specification is included for zero-downtime deployment on Render.
link: https://onudhabon-sd-3-2.onrender.com/

---

## 🛡️ Security Best Practices Implemented

- **Password Security:** Salted PBKDF2 with HMAC-SHA512 password hashing via ASP.NET Core `IPasswordHasher<User>`.
- **SQL Injection Defense:** All queries utilize parameterized EF Core LINQ expressions and compiled SQL queries.
- **Cross-Site Scripting (XSS) Prevention:** Razor engine HTML-encodes all output by default.
- **Cross-Site Request Forgery (CSRF):** Anti-forgery validation tokens (`[ValidateAntiForgeryToken]`) enforced on all state-altering POST requests.
- **Back-Button Cache Invalidation:** Strict HTTP response headers prevent back-button access to authenticated views after signing out.
- **Session Protection:** Secure, HttpOnly, SameSite=Lax authentication cookies with sliding expiration.


---

## 📄 License

This project was developed for academic and social impact purposes as part of the **Software Development (SD) 3.2 Lab** project at Ahsanullah University of Science and Technology (AUST.

<div align="center">
  <sub>Created for the underprivileged children and volunteers of Bangladesh.</sub>
</div>
