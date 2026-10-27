# CampusChat — Design Documentation

## 1. Project Overview

CampusChat is a Blazor Web App developed using C# and Interactive Server rendering. The application allows students to create profiles, participate in discussion threads, send direct messages, and receive unread-message notifications.

The application separates its data models, business logic, and user interface into different parts. This makes the code easier to understand, maintain, and extend.

## 2. Application Architecture

CampusChat uses three main layers:

**Models:** Define the data used by the application, including users, discussion threads, posts, direct chats, and direct messages.

**Services:** Handle application logic such as creating profiles, joining threads, posting messages, checking permissions, and calculating unread counts.

**Blazor components:** Display information and handle user interaction. Components call services instead of accessing the shared data directly.

The application uses dependency injection to make services available to Blazor components.

## 3. Data Models

| Model | Responsibility |
|---|---|
| `User` | Stores profile information, study year, avatar colour, and auditor status. |
| `Thread` | Stores discussion details, privacy settings, creator, and members. |
| `Post` | Represents a discussion message or reply, including author and timestamp. |
| `DirectChat` | Represents a conversation between two users. |
| `DirectMessage` | Stores individual messages within direct conversations. |

Separate models are used for discussion posts and direct messages because they have different access rules and purposes.

## 4. Service Design

**CampusChatStore** holds the application's shared in-memory collections. A synchronization lock protects changes to shared data.

**UserService** manages profiles, including creating, retrieving, and updating users.

**ThreadService** manages discussion threads, searches, memberships, and access-code validation.

**PostService** handles discussion posts, replies, and deletion permissions.

**DirectChatService** manages private conversations and their messages.

**UnreadService** calculates unread discussion activity and updates read status.

**UserStateService** maintains the currently selected user using protected browser session storage.

**ChangeNotifier** informs components when shared application data changes, allowing the interface to update without a manual browser refresh.

## 5. Thread Privacy and Access Control

CampusChat supports public and private discussion threads.

Public threads can be read without joining. Private threads require a valid access code before a regular student can join and read their posts.

The access code is validated inside `ThreadService.JoinThread()`, rather than only in the user interface. This prevents the page from being the sole place responsible for checking permissions.

Service methods return copies of thread objects without exposing their access codes.

The auditor account has special read access to private threads and direct chats.

## 6. Real-Time Updates and Unread Messages

CampusChat uses Blazor Interactive Server rendering and a change-notification service to update connected components.

When users add posts, send messages, or change thread membership, the relevant services notify the application.

Unread counts are tracked for joined threads. Opening a thread updates its read status, while new posts in unopened joined threads increase the unread count.

## 7. Data Storage and Session Management

The application uses in-memory storage instead of an external database. This reduces setup complexity and allows the project to run without database configuration.

Data remains available across connected browsers while the application server is running. However, newly created data is lost when the server restarts.

The selected user is stored separately using `ProtectedSessionStorage`, allowing the application to retain the user's selection after refreshing the browser.

## 8. Design Decisions and Trade-offs

**Service-based architecture:** Business rules are placed in services to reduce duplication and keep Razor components focused on presentation.

**Separate direct-message models:** Discussion threads and direct conversations have different structures and access rules, so they use separate models.

**In-memory storage:** This simplifies development and testing, but does not provide permanent data persistence.

**Seeded users:** Predefined student profiles and an auditor account make it easier to demonstrate and test application functionality.

**Real-time notifications:** A shared notification mechanism avoids requiring users to refresh their browsers manually.

**Simplified sign-in:** Users select a profile rather than entering a password. This is suitable for the assignment demonstration but is not secure authentication for a production application.

## 9. Possible Future Improvements

A production version could use a database for permanent storage, proper user authentication, stronger access-code protection, and more advanced moderation tools.

These improvements are outside the current application's scope.