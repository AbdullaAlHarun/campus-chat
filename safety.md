# CampusChat — Safety and Privacy

## 1. Personal Data
CampusChat stores display names, biographies, avatar colours, study years, discussion posts, direct messages, and membership information. These details may identify students or reveal their conversations.

## 2. Direct Message Retention
Direct messages are kept in memory while the application is running and are lost when the server restarts. For a real application, I would propose a limited retention period, such as 90 days, to reduce unnecessary storage of private conversations.

## 3. Students Leaving CampusChat
The current application does not implement account deletion. In a production system, departing students should be able to request profile deletion. Their posts could be anonymised where necessary to preserve discussion context.

## 4. Abuse and Moderation
Students can delete their own posts. A production version should provide a reporting system. A designated moderator would review reports and decide whether content should be removed according to published community rules.

## 5. Direct Chat Privacy and Auditor Access
Direct chats are restricted to their participants, with an exception for the auditor account. Students should be clearly informed that an auditor can read conversations. The auditor should have read-only access.

## 6. Information Not Logged
The application should avoid recording passwords, private-thread access codes, sensitive personal information, and unnecessary message contents in diagnostic logs.

## 7. Private-Thread Access Codes
Private-thread access codes are stored in the shared in-memory thread data. The service validates the code before allowing access and does not expose it through returned thread objects. A production application should store access-code hashes instead of plaintext codes.