# PlayVRehab

**A serious game for lower limb rehabilitation using Azure Kinect motion capture.**

![Unity](https://img.shields.io/badge/Unity-C%23-black?logo=unity)
![Python](https://img.shields.io/badge/Python-Flask-blue?logo=python)
![Azure Kinect](https://img.shields.io/badge/Azure-Kinect-0078D4?logo=microsoft-azure)
![SQLite](https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite)
![License](https://img.shields.io/badge/License-Academic-lightgrey)

PlayVRehab is a Unity-based serious game developed as a Master's thesis project in Telecommunications and Computer Engineering. Patients perform physical rehabilitation exercises, such as **jumps**, **crouches** and **leg holds**, to navigate in-game obstacles, with their movements being tracked by an **Azure Kinect** sensor. Session data is stored in a database and made available to clinicians through a web dashboard.

## Table of Contents

- [About the Project](#about-the-project)
- [Features](#features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Web Interface](#web-interface)
- [Screenshots & Demos](#screenshots--demos)
- [License](#license)
- [Author](#author)
- [Acknowledgements](#acknowledgements)

---

## About the Project

Lower limb rehabilitation requires consistent, measurable, and engaging exercise routines. PlayVRehab addresses this by gamifying the rehabilitation process. In it, the patients play a game where the only controls are their own body movements, captured by an **Azure Kinect** sensor.

The system has three integrated components:

1. **Unity Serious Game** - the patient-facing application running in real time.
2. **Flask Backend + Web Interface** - a clinician-facing dashboard to review session results.
3. **SQLite Database** - stores patient, clinician and session data.

This project was developed as a **master’s degree dissertation** at ISCTE-IUL.

---

## Features

- **Gesture-controlled gameplay** - jump, crouch, and leg-hold movements are the game controls
- **Serious game design** - engaging obstacle course mechanics adapted for rehabilitation
- **Real-time motion capture** - Azure Kinect tracks full lower limb movement without wearables
- **Clinician dashboard** - web interface for reviewing patient session results and progress
- **Session logging** - all exercise sessions are automatically stored and queryable
- **User management** - separate accounts for patients and clinicians

---

## Architecture

<img width="1000" alt="PlayVRehab-Architecture" src="https://github.com/user-attachments/assets/ea5a81e0-e475-48b2-bb5b-c31c9a938d68" />

---

## Tech Stack

| Component | Technology | 
|---|---|
| Serious Game | Unity (C#) |
| Motion Capture | Azure Kinect SDK |
| Backend API | Python / Flask |
| Web Interface | HTML, CSS, JavaScript |
| Database | SQLite |

---

## Project Structure

```
PlayVRehab/
├── UnityScripts/          # C# scripts for the Unity serious game
|   └── ...                # Movement detection, obstacle logic, session recording
|
├── WebInterface/          # Clinician-facing web dashboard
|   ├── templates/         # HTML templates
|   ├── static/            # CSS and JavaScript assets
|   |   └── Images/        # Images used
|   └── app.py             # Flask application entry point
|
├── Database/              # Contains the database structure file
|
└── README.md
```

> The full Unity project (assets, scenes, packages) is not included in this repository. Only the C# scripts are provided.

---

## Web Interface

The web dashboard allows clinicians to:

- View registered patients and their profiles
- Browse session history with timestamps
- Review exercise performance metrics per session

---

## Screenshots & Demos

### Unity Serious Game

https://github.com/user-attachments/assets/9a111881-112c-4d5c-aa78-fbf5b0d259a2

https://github.com/user-attachments/assets/8d9db103-f2b8-4047-abf5-e11db60bbffc

### Clinician Dashboard

<img width="800" alt="PlayVRehab-Clinician-Dashboard" src="https://github.com/user-attachments/assets/8a546951-30a1-4bfe-80d0-66bd683a8af6" />

<img width="800" alt="PlayVRehab-Clinician-Dashboard-Patient-Results" src="https://github.com/user-attachments/assets/31936e4b-400b-4db7-853f-b2430e96000a" />

> The data shown in demos and screenshots is for demonstration purposes only.

---

## License

This project was developed as part of a master’s dissertation and does not currently carry a formal open-source licence.

> If you wish to use or adapt this work, please contact the author directly.

---

## Author

**Afonso Candeias**
Master's degree in Telecommunications and Computer Engineering

- GitHub: [@notCandeias66](https://github.com/notCandeias66)

---

## Acknowledgements
- Developed as part of a master’s thesis in Telecommunications and Computer Engineering
- Professor Octavian Adrian Postolache
- ISCTE-Instituto Universitário de Lisboa
