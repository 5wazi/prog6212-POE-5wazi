# Contract Monthly Claim System (CMCS)

## Overview
The Contract Monthly Claim System (CMCS) is a .NET MVC web-based application designed to streamline the submission, review, and approval of monthly claims for independent contractor lecturers. The system provides role-based dashboards for **Lecturers**, **Programme Coordinators**, **Academic Managers**, and **HR**, ensuring efficiency, accountability, and transparency.

## Features

**Login**
- Email and password input
- Login and "Continue without login" buttons
- Role-based access control using User and UserRole tables
- Session-based authentication for secure navigation across pages

**Lecturer Dashboard**
- Welcome message 
- Quick access to:
  - Submit New Claim
  - Upload Documents
  - Download Claim Form
  - Help & Support
- Claim Summary (Total, Pending, Approved, Rejected)
- Recent Activity (latest claims with status badges)

**Lecturer Claim Page**
- Filters section (Month selector, Status dropdown, Apply/Reset buttons)
- Claims Table (Claim ID, Date, Hours Worked, Hourly Rate, Total, Status)
- Automated validation for monthly hours claimed
- Total Amount auto-calculation based on Hours Worked × HourlyRate

**Lecturer Submit Claim Page**
- Claim Submission Form
  - Full Name, Module/Claim Title, Hours Worked, Hourly Rate, Total (auto-calculated)
  - Description / Notes
  - Submission Date (auto-filled)
- File Upload Component
  - Supports multiple files
  - Accepted formats: .pdf, .docx, .xlsx
  - Max file size: 20MB
  - Client-side and server-side validation
  - Displays errors if files exceed size or unsupported type
- Reset and Submit buttons

**Programme Coordinator Dashboard**
- Welcome message 
- Quick access to:
  - Review Pending Claims
  - Manage Lecturers
  - Generate Reports
  - Help & Support
- Claim Summary (Total, Pending, Approved, Rejected)

**Programme Coordinator Claim Verification Page**
- Claims table
- Filters section (lecturer, claim status, apply/reset button)
- Approve/Reject and claim detail buttons

**Programme Coordinator Review Claim Page**
- Claim details and uploaded documents display
- Feedback text area
- Approve/Reject and "Back to claims" button

**Academic Manager Dashboard**
- Welcome message 
- Quick access to:
  - Finalise Pending Claims
  - Generate Reports
  - Audit Trail
  - Help & Support

**Academic Manager Claim Page**
- Claims table
- Filters section (Lecturer/Claim ID, Month, Apply/Reset buttons)
- Approve/Reject and claim detail buttons

**Academic Review Claim Page**
- Displays coordinator verification status
- Claim details and uploaded documents display
- Feedback text area
- Approve/Reject and "Back to pending claims" button

**HR Dashboard & User Management**
- Welcome message
- Quick access to:
  - Add New Users
  - Edit Existing Users
  - View All Users
  - Generate Reports
- **CreateUser / EditUser**
  - Dropdown for roles selection
  - Validation of input fields
  - Save new or updated user data to database
- Role-based access ensures HR can manage accounts securely

**Reports & PDF Export**
- Filter claims by Month, Lecturer, and Status
- Display grouped report by Lecturer with Total Hours and Total Amount
- Export report to PDF with all filtered claims and summaries
- Dynamic file naming based on filters applied

**Common Features**
- Navbar with navigation (Dashboard, Claims, Documents, My Profile)
- User dropdown for switching roles (Lecturer, Coordinator, Manager, HR)
- Logout functionality clears session securely
- Responsive design with Bootstrap 5
- Dashboard shows real-time claim status tracking: Submitted → Verified → Approved / Rejected
- Role-based access control ensures each user can only perform actions allowed for their role

**Tech Stack**
- Frontend: HTML5, CSS3, Bootstrap 5, JavaScript (for auto-calculating totals and client-side validation)
- Backend: ASP.NET Core MVC
  - Form submission handling
  - Claim workflow logic (submission → verification → approval)
  - File upload processing with validation
  - Session-based authentication
- Database: Relational database supporting role-based access and claim tracking
  - User Table: Stores all users (Lecturers, Coordinators, Managers, HR)
  - UserRole Table: Assigns roles to users for role-based access
  - Claim Table: Stores claim details, hours, rates, status
  - Document Table: Stores file metadata and links to claims
  - Review Table: Tracks verification and approval with timestamps and feedback
- Version Control: Git

**Installation**
1. Clone the repository:
   ```bash
   git clone https://github.com/5wazi/prog6212-POE-5wazi.git
   ```
2. Open the project in Visual Studio
3. Restore NuGet packages
4. Run the application using IIS Express or Kestrel

**Usage**
- Lecturers:
  - Submit claims, upload supporting documents
  - Track claim status in real-time
  - HourlyRate and monthly claimed hours are validated automatically
- Programme Coordinators:
  - Verify claims, leave feedback
  - Approve or reject claims
- Academic Managers:
  - Review verified claims from coordinators
  - Approve or reject final claims
  - Maintain audit trail for accountability
- HR:
  - Add, edit, and view users
  - Generate reports
  - Manage roles and permissions
