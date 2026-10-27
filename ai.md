# CampusChat — AI Usage and Reflection

## 1. What AI Produced

I used ChatGPT to understand the assignment requirements, organize my project plan, and prepare testing scenarios.


## 2. What I Changed and Kept

I used AI suggestions while working on complex features such as `ThreadService.JoinThread()` and `ChangeNotifier`.

I kept the service-based approach for private-thread validation and the notification system for real-time updates. AI also provided complete updated examples for `UserService.cs` and `SignIn.razor`, which I incorporated into the profile creation feature.

I reviewed the functionality and tested the application in JetBrains Rider.

## 3. Why I Made These Decisions

I used service-based validation because access permissions should be checked in the service rather than only in the user interface.

I used `ChangeNotifier` because it allows connected users to receive updates without refreshing their browsers.

I also added profile creation to meet the assignment requirements. I tested the application using different student profiles, private-thread access codes, browser refreshes, and multiple browsers.

## 4. How AI Affected My Programming

AI helped me understand Blazor components, services, dependency injection, and application state management.

It also helped me organize my development process and identify possible problems during testing.

However, I learned that AI suggestions are not always perfect. I need to understand the code, check it against the requirements, and test it myself before accepting a solution.