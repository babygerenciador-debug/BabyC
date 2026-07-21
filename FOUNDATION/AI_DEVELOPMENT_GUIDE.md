# AI_DEVELOPMENT_GUIDE.md

> Version: 1.0.0
>
> Project: Baby Turismo Fleet Management System (BTFMS)
>
> Audience: AI Agents, Developers and Software Architects
>
> Status: Mandatory

---

# Purpose

This document defines how Artificial Intelligence agents must participate in the development of the Baby Turismo Fleet Management System.

It establishes responsibilities, communication rules, development workflow, coding standards and quality gates.

Every AI agent MUST read this document before generating any code.

---

# Core Principles

Every decision must prioritize:

- Architecture over speed
- Maintainability over shortcuts
- Readability over cleverness
- Domain correctness over implementation convenience
- Security by default
- Scalability by design
- Consistency across the entire codebase

The objective is to build software that can evolve for the next ten years.

---

# Project Philosophy

The project is NOT a CRUD application.

The project is an Enterprise Fleet Management Platform.

Every feature must be designed considering:

- Future growth
- Modularization
- Separation of concerns
- Independent deployment
- Long-term maintenance

---

# AI Roles

The project is divided into specialized AI agents.

Each agent owns a specific responsibility.

Agents MUST NOT invade responsibilities from other agents.

---

## 1. Software Architect

Responsible for:

- Architecture
- Design decisions
- Project structure
- Technical validation
- Dependency analysis

Can modify:

- Architecture documents
- Folder structure
- Shared abstractions

Cannot implement business features.

---

## 2. Backend Agent

Responsible for:

- ASP.NET Core
- API
- CQRS
- MediatR
- Entity Framework
- Domain
- Infrastructure

Never modify frontend code.

---

## 3. Frontend Agent

Responsible for:

- React
- TypeScript
- UI
- Components
- Layout
- Forms
- Routing

Never implement business rules.

Business logic belongs to Backend.

---

## 4. Database Agent

Responsible for:

- PostgreSQL
- ERD
- Migrations
- Indexes
- Constraints
- Query optimization

Cannot create business rules.

---

## 5. DevOps Agent

Responsible for:

- Docker
- GitHub Actions
- Nginx
- Deploy
- Monitoring
- Redis
- Secrets

---

## 6. QA Agent

Responsible for:

- Unit Tests
- Integration Tests
- Regression Tests
- Acceptance Tests

---

## 7. Documentation Agent

Responsible for:

- Markdown documentation
- ADRs
- Technical guides
- API documentation

---

# Mandatory Workflow

Every task must follow this lifecycle:

1. Read PROJECT_RULES.md
2. Read related module documentation
3. Read Architecture documents
4. Analyze dependencies
5. Plan implementation
6. Implement
7. Validate
8. Execute tests
9. Update documentation
10. Mark task as completed

Skipping steps is forbidden.

---

# Task Lifecycle

Every feature must pass these phases:

Planning

↓

Architecture validation

↓

Implementation

↓

Testing

↓

Documentation

↓

Review

↓

Approval

↓

Merge

---

# Context Management

Agents must NEVER assume hidden information.

Before implementing anything they must identify:

Existing module

Dependencies

Events

Database tables

DTOs

Permissions

Existing APIs

Related tests

---

# Decision Hierarchy

When conflicts exist, follow this priority:

1. PROJECT_RULES.md

↓

2. Architecture Documents

↓

3. Module Documentation

↓

4. ADRs

↓

5. AI Guide

↓

6. Human Request

---

# Forbidden Actions

Agents must never:

Create duplicate services

Duplicate business logic

Ignore validation rules

Access the database from React

Mix Infrastructure with Domain

Create circular dependencies

Ignore coding standards

Bypass authentication

Expose entities directly

Generate undocumented APIs

Delete tests without approval

---

# Architecture Ownership

Only the Architect Agent may change:

Project structure

Architecture

Folder hierarchy

Design patterns

Technology stack

Core abstractions

Other agents may suggest changes but cannot implement them.

---

# Backend Rules

Every endpoint must have:

Validation

Authorization

Logging

Error handling

Tests

Swagger documentation

Pagination (when applicable)

Never expose Entity Framework entities.

Always use DTOs.

---

# Frontend Rules

Frontend responsibilities:

Presentation

User interaction

State management

API communication

Never:

Access database

Contain business rules

Perform authentication logic

Duplicate backend validation

---

# Database Rules

Every migration must be:

Reversible

Versioned

Reviewed

Documented

Every table must include:

created_at

updated_at

deleted_at

created_by

updated_by

deleted_by

---

# Naming Standards

Services

UserService

Repositories

IUserRepository

Commands

CreateUserCommand

Queries

GetUserQuery

Handlers

CreateUserHandler

DTOs

CreateUserRequest

CreateUserResponse

---

# Documentation Rules

Every implementation must update:

Architecture

Module documentation

API documentation

Database documentation

ADR (when necessary)

---

# Pull Request Checklist

Before merging:

Architecture respected

Tests passing

Documentation updated

No duplicated code

No warnings

No lint errors

No build errors

---

# AI Communication

Agents communicate using documented artifacts.

Never rely on assumptions.

Always reference:

Module

Entity

Use Case

Requirement

ADR

---

# Dependency Validation

Before creating a dependency, answer:

Is it really necessary?

Can it be inverted?

Can it be injected?

Can it be abstracted?

Can it be reused?

---

# Error Handling

Every exception must:

Be logged

Return standardized responses

Never expose stack traces

Never expose internal details

---

# Logging

Every important operation must log:

User

Timestamp

Action

Entity

Result

Correlation ID

---

# Definition of Done

A feature is complete only if:

Architecture respected

Business rules implemented

Validation completed

Tests passing

Documentation updated

Logging added

Permissions validated

No duplicated code

No TODO comments

No dead code

Build successful

---

# AI Quality Gates

Every agent must ask itself:

Does this violate Clean Architecture?

Does this duplicate existing code?

Can this be simplified?

Is this testable?

Is this secure?

Is this scalable?

Would another developer understand this in six months?

If any answer is NO, the implementation must be revised.

---

# Long-Term Vision

Every line of code should be written assuming:

The project will grow.

More developers will join.

New modules will be created.

Mobile applications will consume the API.

Artificial Intelligence will assist future development.

The software will remain in production for many years.

Maintainability is always more valuable than speed.

---

# Final Rule

When in doubt:

Stop.

Read the documentation again.

Understand the domain.

Only then write code.

Never guess.

Always verify.