# Project rules

Language: C#
Framework: .NET 10
Libs: EF, Steamworks

Guidelines:
- Follow SOLID
- Follow MVVM pattern
- Avoid LINQ allocations in hot paths
- Prefer Span/Memory for performance
- No action can be performed without user confirmation (all changes and important steps require approval)
- All documentation must be written in English
- Do not add "Co-authored-by: Junie <junie@jetbrains.com>" to git commits unless explicitly requested by the user.

# Git Commit Rules

Multi-line commit:
feat(auth): add OAuth2 support for Google and GitHub

- Implement OAuth2 authentication flow
- Add Google OAuth provider integration
- Add GitHub OAuth provider integration  
- Update user model to store OAuth credentials
- Add tests for OAuth authentication

Example for single-line commits:

feat: implement user registration endpoint
fix(login): handle incorrect password gracefully  
refactor: extract database layer into separate module
docs: add installation instructions for Windows

commit types: feat, fix, docs, style, refactor, test, perf, ci, chore 
