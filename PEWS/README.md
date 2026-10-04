# Puntland Early Warning System (PEWS)

A professional disaster management platform for Puntland, Somalia — built as a Final Year Project.

## Overview

PEWS enables citizens and government authorities to monitor, report, manage, and respond to disasters including floods, droughts, storms, cyclones, fires, earthquakes, and disease outbreaks.

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 MVC |
| Language | C# |
| ORM | Entity Framework Core 9 (Code First) |
| Database | SQL Server |
| Auth | ASP.NET Core Identity |
| UI | Bootstrap 5 + Custom CSS |
| Charts | Chart.js 4 |
| Icons | Font Awesome 6 |

## Features

### Roles
- **Super Admin** — Full system access, role management
- **Admin** — User management, alert publishing, report verification
- **Emergency Officer** — Alert creation, report management
- **Citizen** — Submit reports, view alerts, emergency contacts

### Modules
- Dashboard with real-time statistics and charts
- Disaster Reports (CRUD, image upload, verification workflow)
- Disaster Alerts (create, publish, broadcast notifications)
- Risk Monitoring (regions, districts, risk levels)
- Emergency Contacts (hospitals, police, fire, ambulance, etc.)
- User Management (activate/deactivate, role assignment)
- Notifications system
- Analytics with monthly/type/severity/region charts

## Getting Started

### Prerequisites
- .NET 9 SDK
- SQL Server (Express or full)

### Setup

```bash
git clone https://github.com/YOUR_USERNAME/puntland-early-warning-system.git
cd puntland-early-warning-system/PEWS
```

Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PEWSDb;Trusted_Connection=True;TrustServerCertificate=true"
  }
}
```

Run migrations and start:
```bash
dotnet ef database update
dotnet run
```

### Default Credentials
| Email | Password | Role |
|---|---|---|
| superadmin@pews.gov.so | Admin@123456 | Super Admin |

## Database Schema

Key tables: Users, Roles, DisasterReports, DisasterTypes, Alerts, AlertCategories, EmergencyContacts, Notifications, Regions, Districts, ReportImages, WeatherData, AuditLogs

## License

This project is developed as an academic Final Year Project. All rights reserved.
