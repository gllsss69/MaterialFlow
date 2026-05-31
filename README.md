# MaterialFlow

[![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet)](https://dotnet.microsoft.com/)
[![AvaloniaUI](https://img.shields.io/badge/Avalonia-UI-blue)](https://avaloniaui.net/)
[![FFmpeg](https://img.shields.io/badge/FFmpeg-green)](https://ffmpeg.org/)

**MaterialFlow** is a modern desktop application built with **C#** and **Avalonia UI** designed for automated adaptation and conversion of video materials for various media platforms. Whether you are preparing content for YouTube, TikTok, or Instagram, MaterialFlow streamlines the workflow by applying predefined or custom video presets.

---

## 🌟 Key Features

- **Video Project Management:** Easily create and manage video projects, adding source files and tracking metadata.
- **Platform-Specific Presets:** Built-in templates for major platforms (YouTube 1080p, TikTok 720p, Instagram, etc.) with customizable settings (resolution, bitrate, codec, frame rate).
- **FFmpeg Integration:** Reliable and fast video conversion running asynchronously via FFmpeg.
- **Job Queue & Progress Tracking:** Track the status of conversion jobs (Pending, Processing, Completed, Failed) with visual progress indicators.
- **Role-Based Access Control:** 
  - **Admin:** Full access, including preset and platform management.
  - **Editor:** Access to create and convert video projects.
- **Advanced Filtering & Search:** Search by name, filter by status or platform, and sort projects dynamically.
- **Modern UI/UX:** Built with `Material.Avalonia` for a sleek, responsive, and beautiful user experience supporting fluid animations and modern design paradigms.

---

## 🛠️ Tech Stack

- **Language:** C# 13
- **Framework:** [.NET 9.0](https://dotnet.microsoft.com/)
- **UI Framework:** [Avalonia UI](https://avaloniaui.net/)
- **Design System:** [Material.Avalonia](https://github.com/AvaloniaUI/Material.Avalonia) & Material.Icons.Avalonia
- **Video Processing:** [FFmpeg](https://ffmpeg.org/)

---

## 🏗️ Architecture & Entities

The application follows a clean structure utilizing JSON for data storage. Key entities include:

- **User:** Manages authentication and roles (Admin/Editor). Includes secure password hashing.
- **VideoProject:** Represents a user's video conversion project.
- **Platform:** Defines target media platforms and their specific output requirements.
- **Preset:** Reusable configurations (codec, bitrate, resolution) linked to specific platforms.
- **ConversionJob:** Tracks the execution status and output of the FFmpeg process.
- **OutputFile:** Represents the generated video files ready for publishing.

---

## 🚀 Getting Started

### Prerequisites

1. **.NET 9.0 SDK**: Download and install from [Microsoft's official site](https://dotnet.microsoft.com/download/dotnet/9.0).
2. **FFmpeg**: Must be installed and accessible via your system's PATH variable. You can quickly install it using the following package managers:

   - **Windows (winget):**
     ```cmd
     winget install Gyan.FFmpeg
     ```
   
   - **Linux (Ubuntu / Debian):**
     ```bash
     sudo apt update
     sudo apt install ffmpeg
     ```
   
   - **Linux (Fedora):**
     ```bash
     sudo dnf install ffmpeg
     ```
   
   - **Linux (Arch Linux):**
     ```bash
     sudo pacman -S ffmpeg
     ```


### Installation & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/gllsss69/MaterialFlow.git
   cd MaterialFlow
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   cd MaterialFlow
   dotnet run
   ```

 4. Enjoy the seamless video conversion experience with **MaterialFlow!**
---

## 🤝 Contributing
Contributions are welcome! Feel free to open an issue or submit a pull request if you have suggestions for new features or bug fixes.

## 👤 Author

**Maksym Petyk** **(gllsss69)**

## 🎓 Course work
This software product was developed as a Course work in Software Engineering. It serves as a practical implementation demonstrating advanced software architecture paradigms, asynchronous multi-process automation via CLI utilities (FFmpeg), and modern cross-platform GUI development using the MVVM pattern.