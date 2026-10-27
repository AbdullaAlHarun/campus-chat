# CampusChat

CampusChat is a Blazor Web App developed in C# for students to communicate through discussion threads and direct messages. The application supports student profiles, public and private discussions, replies, and unread-message notifications.

## Technologies

- C# and ASP.NET Core Blazor
- Interactive Server render mode
- .NET
- In-memory data storage
- Bootstrap for the user interface

## How to Run

1. Clone the repository:

   `git clone https://github.com/AbdullaAlHarun/campus-chat.git`

2. Open the project in JetBrains Rider or another compatible .NET development environment.

3. Run the `CampusChat` application using the IDE, or from the repository root:

   `dotnet run --project CampusChat`

4. Open the local URL displayed when the application starts.

The application requires a compatible .NET SDK. No external database is required.

## How to Use CampusChat

**Profiles:** On the sign-in page, select an existing user or create a new student profile. Users can update their display name, bio, avatar colour and study year. They can also view other students' profiles and joined threads.

**Discussion threads:** Browse or search threads, create public or private threads, and join discussions. Private threads require the correct access code. Public threads can be read without joining.

**Posts and replies:** Members can write posts and reply to existing messages. Each post displays its author and timestamp. Users can delete their own posts.

**Direct chats:** Students can exchange private messages with another student. Direct chats are available only to their participants, except for the auditor's read-only access.

**Unread notifications:** Joined threads display unread-message counts. Counts update when new posts arrive and reset when the user opens the thread.

**Auditor:** A predefined auditor account can read all discussion threads and direct chats for review purposes.

## Data Storage

CampusChat uses shared in-memory storage. Changes remain available across browser sessions while the application server is running, but they are not saved permanently when the server restarts.

The selected user is maintained using browser session storage so that refreshing the page does not require selecting the user again.

## Project Documentation

- `design.md` — architecture and design decisions
- `safety.md` — privacy, data handling and moderation
- `ai.md` — AI assistance and development reflection
