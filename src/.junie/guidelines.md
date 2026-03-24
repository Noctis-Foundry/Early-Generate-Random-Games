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

# Testing Guidelines

- Unit tests must be located in the `GameRandom.UnitTests` project.
- The folder structure in `GameRandom.UnitTests` must match the main project's folder structure (e.g., unit tests for the `Service` folder must be placed in a `Service` folder within the `GameRandom.UnitTests` project).
- Integration tests must be located in the `GameRandom.IntegrationTests` project.
- All new tests must be documented in the `docs` folder at the root of the project. 
- Use `UnitTestsDoc.md` for Unit tests and `IntegrationTestsDoc.md` for Integration tests. Create these files if they do not exist.

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
