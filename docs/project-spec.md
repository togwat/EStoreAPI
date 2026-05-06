# Project Specification — EStoreAPI

## 1. Overview

EStoreAPI is a repair shop management system for internal staff use. It provides a digital job intake form that records customer and device details alongside the problems being repaired, a managed problem/price catalogue that generates a quote suggestion for each job, and tools to track job status and look up customer repair history. The system is designed as a scalable foundation for future operational features.

> This system is a modernisation of a legacy PHP web app that first digitised the shop's original paper-and-spreadsheet workflow.

---

## 2. Goals & Non-Goals

### Goals

- [ ] Provide a digital job intake form that links a customer, device, and selected problems
- [ ] Provide an easy way for staff to manage repair pricing (add, update problems and their prices per device)
- [ ] Generate a quote suggestion from selected problems, adjustable by the technician to account for discounts
- [ ] Allow staff to look up customer repair history and track job status
- [ ] Establish a clean, scalable architecture that supports future feature growth

### Non-Goals

- Customer-facing portal (internal staff tool only)
- Inventory or parts management
- Payment processing or invoicing

---

## 3. Users & Roles

| Role | Description | Permissions |
|------|-------------|-------------|
| Staff | Counter and technician staff who create and manage jobs | Full CRUD on jobs, customers, devices, problems |

All roles require a user account with password authentication to access the system. User management is infrequent and handled administratively (e.g. direct database queries or a basic admin interface).

---

## 4. Functional Requirements

### 4.1 Customer Management

- [ ] Create, read, update, delete customer records
- [ ] Search customers by name or phone number


### 4.2 Device Management

- [ ] Create, read, update device models
- [ ] Search devices by name or type
- [ ] Associate known problems with devices
- [ ] Support one-off or obscure devices without needing entry into the database

### 4.3 Problem Catalogue

- [ ] Maintain a list of common problems per device model, each with a set price
- [ ] Allow staff to add, update, and remove problems and their prices

### 4.4 Job Management

- [ ] Create a job linking a customer, a device, and one or more problems
- [ ] Support jobs where the problem is unknown at intake
- [ ] Record receive time; set estimated and actual pickup times
- [ ] Capture an estimated price and a final collected price
- [ ] Mark a job as completed
- [ ] Set warranty status for a completed job based on warranty policy timeframe
- [ ] Allow new jobs to be linked to an existing job as warranty repair

### 4.5 Authentication

- [ ] Password-based authentication — all API routes require a valid session
- [ ] User accounts stored in the database with hashed passwords
- [ ] User creation and modification handled administratively (no self-registration)

### 4.6 Frontend

- [ ] Login interface for authentication
- [ ] Job intake form links customer, device, and selected problems in one submission; price input shows the sum of selected problem prices as a placeholder, which the technician can accept or override
- [ ] Job list view showing outstanding and completed jobs
- [ ] Customer lookup to retrieve repair history

---

## 5. Non-Functional Requirements

| Concern | Requirement |
|---------|-------------|
| Availability | Must be available during business hours, downtime acceptable outside of these; Ideally 24/7 availability |
| Performance | UI interactions should feel immediate for in-person job intake |
| Security | Hosted on a public URL; all routes protected behind authentication |
| Scalability | Architecture should support adding new features (e.g. reporting, notifications) without major rework |
| Data retention | Job and customer records kept for warranty and history purposes; stale customer records (no active warranty and no activity for 5 years) removed for privacy |

---

## 6. Architecture

### 6.1 Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend API | ASP.NET Core 10 (C#) |
| Database | PostgreSQL via Entity Framework Core 10 |
| Frontend | React + Vite (TypeScript) |
| API docs | Swagger / OpenAPI |
| Containerisation | Docker (3-image setup: API, client, DB) |

### 6.2 High-Level Architecture
![architecture diagram](assets/architecture.svg)

### 6.3 Data Model
![data model diagram](assets/datamodel.svg)

The Device entity represents a device model rather than individual devices.
The Problem entity represents the generic problems a device model may have.

---

## 7. API Summary

See [api-reference.md](api-reference.md) for full endpoint documentation.

| Resource   | Base path          |
|------------|--------------------|
| Customers  | `/api/customers`   |
| Devices    | `/api/devices`     |
| Jobs       | `/api/jobs`        |
| Problems   | `/api/problems`    |

---

## 8. Future Work
- Tracking of external partners (jobs handed off to partner repair shops/technicians)
- Integrated AI agent that helps find job records via text description
