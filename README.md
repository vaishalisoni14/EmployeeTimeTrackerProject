# EmployeeTimeTrackerProject

A C# Console Application designed to track employee work hours by fetching time entry data from a remote API, generating an HTML table of total hours worked, and creating a pie chart visualization to represent the distribution of hours.

## Overview
This project fulfills the following tasks as part of an assessment:
- Fetches time entry data from a specified remote API.
- Calculates the total hours worked per employee, filtering out invalid or deleted entries.
- Generates an HTML table listing employees with their total hours, highlighting those with less than 100 hours in red.
- Creates a pie chart image (`piechart.png`) illustrating the proportional distribution of hours worked across employees.

## Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later (e.g., .NET 8.0 for the latest version).
- Visual Studio 2019, 2022, or a compatible IDE (e.g., Visual Studio Code with C# extension).
- An active internet connection to access the API endpoint.
- Git installed for cloning the repository.

## Installation
1. Clone the repository to your local machine:
   ```bash
   git clone https://github.com/vaishalisoni14/EmployeeTimeTrackerProject.git