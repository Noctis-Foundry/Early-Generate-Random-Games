# Early-Generate-Random-Games

**Early-Generate-Random-Games** is an application for quickly discovering random games and organizing challenges with friends.  
It allows you to randomly select games from your Steam library or other available sources, create player parties, and start challenges.

---

## 🎮 Key Features

- Randomly select a game from Steam or other sources.  
- Player party creation for multiplayer challenges.  
- Set rules or limitations for game challenges.  
- Authorization, random game generation, and party creation are fully implemented.  
- Interactive cross-platform interface built with Avalonia.

---

## 🛠 Tech Stack

- **C# / .NET 10** — main language and platform.  
- **Avalonia** — cross-platform GUI framework.  
- **Entity Framework** — local data storage.  
- **SteamSDK** — Steam integration (authorization, friends library).

---

## 🎯 Core Systems

### Lobby System
Create or join game lobbies to play with friends. The lobby system filters players into separate groups, ensuring you only see information relevant to your current party. Lobby members are displayed in the top panel with avatars, and all updates happen in real-time.

### Game Randomizer
Generate 1-5 random games from your Steam library with optional filtering. You can filter games by categories (Single-player, Co-op, Achievements, etc.), genres (Action, RPG, Strategy, etc.), and release years. The system displays game covers in a dynamic grid for easy selection.

### Filter System
Configure game selection criteria through an intuitive interface. Choose from 47 categories, 12 genres, and years from 2003 to present. Multi-select support allows precise control over which games appear in your random selection.

### Game Progress Table
Track game completion status for all players in your lobby. The table shows player names, current games, completion status, start dates, and end dates. Updates automatically when any player starts or finishes a game.

### Challenge Rules
View detailed challenge rules including roll counts, difficulty requirements, multiplayer conditions, and reward structure. Rules are displayed in a scrollable interface with clear pricing tiers based on completion time.

### Game Status Tracking
Monitor your current game session with detailed information about start date, time spent, and expected completion date. Quick access to Steam page and game status checking.

---

## ⚡ Project Status

> **Beta Version 0.2.0**  
> Available installation files:  
> - **Linux x64**: AppImage  
> - **Windows x64**: NSIS Setup

---

## 🧩 Usage (current functionality)

1. Authorize through Steam.  
2. Create or join a lobby with friends.  
3. Configure filters (optional) for game selection.  
4. Generate random games from your library.  
5. View challenge rules and reward structure.  
6. Track game progress for all lobby members.  
7. Monitor your current game session status.

---

## 🤝 Contributing

We welcome contributions! Please read our [Contributing Guide](Contributing.md) for details on our workflow, branch rules, and how to submit pull requests.

---

## 📄 License

This project is licensed under the Mozilla Public License Version 2.0. See the [LICENSE](LICENSE) file for full details.
