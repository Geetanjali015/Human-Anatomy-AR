
# Human Anatomy AR

<img width="256" alt="logo" src="https://github.com/user-attachments/assets/ed4ad7dc-35d6-4828-9b78-b6f67d2cc75f" width="800" height="300">

A Unity-based, image-target Augmented Reality application for exploring human anatomy through interactive 3D models on Android.

![Unity](https://img.shields.io/badge/Unity-6000.0.43f1-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-Scripting-512BD4?logo=csharp)
![Vuforia](https://img.shields.io/badge/Vuforia_Engine-11.4.4-1F7A3D)
![glTFast](https://img.shields.io/badge/glTFast-6.21.0-orange)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84?logo=android&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-blue)

Point a phone camera at a printed reference image and a full anatomical model appears on top of it. Switch between six body systems, rotate and scale the model with touch gestures, toggle layers, and test yourself with a built-in muscle-spotting quiz.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Anatomical Systems and 3D Assets](#anatomical-systems-and-3d-assets)
- [Technology Stack](#technology-stack)
- [How It Works](#how-it-works)
- [Script Reference](#script-reference)
- [Project Structure](#project-structure)
- [Setup](#setup)
- [Build for Android](#build-for-android)
- [Screenshots](#screenshots)
- [License and Acknowledgements](#license-and-acknowledgements)

---

## Overview

Anatomy is a spatial subject, but it is usually taught through flat diagrams. **Human Anatomy AR** lets learners view structures in three dimensions, anchored in their own physical space, and examine them from any angle by moving the device or the model.

The app targets anatomy students and self-learners. It combines three things:

- **3D visualization** of six anatomical systems, sourced from Z-Anatomy models.
- **Mobile AR** using Vuforia image-target tracking.
- **Interactive learning** through system switching, touch manipulation, and a muscle identification quiz.

## Features

| Feature | What it does | Implemented by |
|---|---|---|
| Image-target AR | Detects a reference image and anchors the anatomy models to it | Vuforia `ImageTarget` + `ARCamera` in `AR Anatomy.unity` |
| Six anatomical systems | Angiology, arthrology, muscular insertions, myology, neurology, splanchnology | `.glb` models under the `AR Object` hierarchy |
| System switching | Forward / backward buttons cycle through the six systems, wrapping at both ends | `ObjectSwitcher.cs` |
| System name label | Displays the currently selected system | `BodySystemLabel.cs` |
| Layer toggles | Buttons show or hide a target object and change colour (green = visible, red = hidden) | `ToggleObject.cs` |
| Rotate | One-finger swipe on the right half of the screen rotates the model | `ObjectControl.cs` |
| Scale | Two-finger pinch scales the model, clamped to 0.1x–2x | `ObjectControl.cs` |
| Camera-facing UI | World-space UI elements rotate to face the AR camera | `UIFaceCamera.cs` |
| Muscle spotting quiz | 3 multiple-choice questions, each highlighting a muscle in dark green, with instant feedback and a final score | `MuscleQuizManager.cs` |

**Not implemented:** plane detection or markerless AR, per-structure anatomical labels, persistent progress tracking, and audio. The app is image-target AR only.

## Anatomical Systems and 3D Assets

Models live in `Assets/3D_Models/` as `.glb` files (Git LFS). All six are instantiated in the main scene and are reachable through the system switcher.

| File | Anatomical focus |
|---|---|
| `angiology.glb` | Blood vessels and the circulatory system |
| `arthrology.glb` | Joints and articulations |
| `muscular_insertions.glb` | Muscle attachment points |
| `myology.glb` | The muscular system (used by the quiz) |
| `neurology.glb` | Nervous system |
| `splanchnology.glb` | Visceral organs |

Models are from [Z-Anatomy on Sketchfab](https://sketchfab.com/Z-Anatomy). `Assets/3D Models/` contains the matching source `.glb` material files and textures.

## Technology Stack

| Technology | Version | Role |
|---|---|---|
| Unity | 6000.0.43f1 (Unity 6) | Engine, scene authoring, build pipeline |
| C# | - | Interaction, UI logic and quiz |
| Vuforia Engine | 11.4.4 (local `.tgz`) | Camera access, image-target detection and tracking |
| glTFast | 6.21.0 | Imports `.glb` models as Unity prefabs |
| Universal Render Pipeline | 17.0.4 | Rendering (separate mobile and PC renderer assets) |
| Input System | 1.20.0 | Enhanced Touch for swipe and pinch |
| Unity UI (uGUI) + TextMeshPro | 2.0.0 | Canvas, buttons, labels, quiz panel |
| Device Simulator Devices | 1.0.0 | Testing UI layouts in the editor |

**Android settings in the project:** IL2CPP scripting backend, ARM64 target architecture, minimum SDK 30.

## How It Works

````mermaid
flowchart LR
    A["App launch"] --> B["Vuforia starts ARCamera, video background on"]
    B --> C{"Image target detected?"}
    C -- No --> B
    C -- Yes --> D["AR Object tracked to target pose"]
    D --> E["Active system model rendered (glTFast prefab)"]
    E --> F["User input"]
    F --> G["Switch system: ObjectSwitcher"]
    F --> H["Rotate and pinch-scale: ObjectControl"]
    F --> I["Toggle layer: ToggleObject"]
    F --> J["Muscle quiz: MuscleQuizManager"]
    G --> K["Label updates: BodySystemLabel"]
````

1. **Initialization.** Vuforia is configured in `Resources/VuforiaConfiguration.asset` with the video background enabled and one simultaneous image target.
2. **Tracking.** The `ImageTarget` uses the `istockphoto-1366264420-612x612` target from the `AR_Anatomy` database (`StreamingAssets/Vuforia/AR_Anatomy.xml`). The AR camera feed is the background, and Vuforia reports the target's pose each frame.
3. **Model placement.** The `AR Object` is a child of the `ImageTarget`, so it follows the tracked pose. The six system models are its children.
4. **Switching.** `ObjectSwitcher` keeps one model active at a time and wraps around at either end.
5. **Interaction.** `ObjectControl` rotates and scales the model; `ToggleObject` shows or hides objects; `UIFaceCamera` keeps world-space UI readable.
6. **Quiz.** `MuscleQuizManager` highlights a muscle by overriding its material colour, scores the answer, then restores the original colour.

This is **marker-based (image-target) AR**.

## Script Reference

| Script | Responsibility | Role in the application |
|---|---|---|
| `ObjectSwitcher.cs` | Holds arrays of system objects and their texts. `ActivateNext()` / `ActivatePrevious()` use modulo arithmetic to cycle; `SetActiveObject()` deactivates everything and enables one | Core navigation between the six systems |
| `BodySystemLabel.cs` | Exposes one public method per system (`ShowAngiology()`, `ShowMyology()`, etc.) that sets a `TMP_Text` label | Shows the current system name |
| `ObjectControl.cs` | Enables Enhanced Touch. One finger moving on the right half of the screen calls `Rotate` around a configurable axis (default Z); two fingers compute pinch delta and scale `localScale`, clamped by `minScale` / `maxScale` | Touch manipulation of the AR model |
| `MuscleQuizManager.cs` | Drives a 3-question quiz: `OpenQuiz()`, `LoadQuestion()`, `CheckAnswer()`, `NextQuestion()`, `ShowResults()`, `CloseQuiz()`. `HighlightMuscle()` finds all renderers on a muscle, saves each material's original colour, and sets a dark green. `RestoreMuscle()` reverts it | Quiz logic, scoring, and 3D highlighting |

The quiz detects the colour property by trying `diffuseFactor`, `_BaseColor`, `_Color`, then `_BaseColorFactor`, so it works with the materials glTFast generates.

## Project Structure

```text
Human-Anatomy-AR/
├── Assets/
│   ├── 3D_Models/          # Six anatomical .glb models (Git LFS)
│   ├── Anatomy Model/      # Source .mtl materials and textures per system
│   ├── Editor/Vuforia/     # Image target textures used by the Vuforia database
│   ├── Imports/            # Logo, splash and tracker images
│   ├── Resources/          # VuforiaConfiguration.asset
│   ├── Scenes/             # AR Anatomy.unity (in build), AR ANATOMY 2.unity (disabled)
│   ├── Scripts/            # The six gameplay scripts
│   ├── Settings/           # URP mobile and PC renderer assets
│   └── StreamingAssets/Vuforia/   # AR_Anatomy / Test image target databases
├── Packages/
│   ├── manifest.json
│   └── com.ptc.vuforia.engine-11.4.4.tgz   # Git LFS
├── ProjectSettings/
├── .gitattributes          # Git LFS rules for *.glb and *.tgz
├── LICENSE
└── README.md
```

## Setup

**Prerequisites**

- Unity Hub with Unity **6000.0.43f1**, including *Android Build Support* (OpenJDK, SDK and NDK)
- [Git](https://git-scm.com/) and [Git LFS](https://git-lfs.com/)
- An Android device with a camera and USB debugging enabled
- A Vuforia developer account and license key

**1. Clone with Git LFS.** The `.glb` models and the Vuforia `.tgz` package are LFS objects. Without LFS you get small pointer files and the project will not import correctly.

```bash
git lfs install
git clone https://github.com/Geetanjali015/Human-Anatomy-AR.git
cd Human-Anatomy-AR
git lfs pull
```

**2. Open the project.** Add the folder in Unity Hub and open it with 6000.0.43f1. Unity resolves packages from `Packages/manifest.json`; Vuforia comes from the local tarball.

**3. Add your Vuforia license key.** In Unity, open *Window > Vuforia Engine > Configuration* and paste your key. 

**4. Open the scene and run.** Open `Assets/Scenes/AR Anatomy.unity`. Print or display the target image at `Assets/Editor/Vuforia/ImageTargetTextures/AR_Anatomy/istockphoto-1366264420-612x612_scaled.jpg`, then press Play with a webcam, or use the Device Simulator to test UI layout.

## Build for Android

1. *File > Build Profiles*, select **Android**, and click *Switch Platform*.
2. Confirm `AR Anatomy` is the enabled scene. 
3. In *Player Settings*, verify: IL2CPP, ARM64, minimum API level 30.
4. **Set your own package name.** The project currently uses the Unity template default (`com.UnityTechnologies.com.unity.template.urpblank`).
5. Connect the device with USB debugging on and use *Build And Run*, or *Build* to produce an APK and install it with `adb install <file>.apk`.
6. Point the camera at the printed target to test.


## Screenshots
<img width="358" height="700" alt="WhatsApp Image 2026-11-16 at 15 30 29 (1)" src="https://github.com/user-attachments/assets/9aa49a0b-8036-468a-a593-0c4ee7934002" />
<img width="358" height="700" alt="WhatsApp Image 2026-11-16 at 15 30 29" src="https://github.com/user-attachments/assets/afaf0f18-1b4f-4801-afd0-2aaeca48b20f" />

<img width="358" height="700" alt="WhatsApp Image 2026-11-16 at 15 30 56" src="https://github.com/user-attachments/assets/0db7ca66-3ca4-4e8f-8f98-e017b6c87ecb" />
<img width="358" height="700" alt="WhatsApp Image 2026-11-16 at 15 30 30" src="https://github.com/user-attachments/assets/a174a1b7-9b5d-4d45-b3cf-a5248782475c" />

