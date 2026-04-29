# 🏠 ARspace — Virtual Interior Design Application Using Augmented Reality

**ARspace** is a mobile augmented reality (AR) application that allows users to visualize and place 3D furniture in their real-world environment using their smartphone camera. Built with **Unity** and **AR Foundation**, the app provides an immersive interior design experience with room-based furniture recommendations, real-time cost estimation, budget tracking, cloud-based design saving, and gesture-based object interaction.

---

## 📄 Project Report

📥 **[Download the Full Project Report (PDF)](docs/ARspace-Final-Report.pdf)**

---

## ✨ Features

### 🪑 AR Furniture Placement
- Place realistic 3D furniture models onto detected real-world surfaces (floors, tables, etc.)
- Models are placed using AR plane detection via **ARCore** (Android)
- Tap on a detected surface to place the selected furniture item

### 🤏 Gesture-Based Interaction
- **Single Tap** — Select a placed furniture item
- **Drag** — Move a selected furniture item across detected surfaces
- **Two-Finger Rotate** (Mobile) / **Hold R** (PC) — Rotate the selected furniture item
- **Double Tap** — Delete a placed furniture item from the scene

### 🏡 Room-Based Recommendations
- Choose from **3 room categories**: Bedroom, Living Room, and Dining Room
- Each room type shows a curated panel of furniture recommendations
- Furniture cards display the item image, name, price, and a "View in AR" button

### 💰 Real-Time Cost Estimation & Budget Tracking
- Every placed item adds its cost to a running total displayed on-screen
- Set a **custom budget** before starting — the app will warn you when you exceed it
- Over-budget state is highlighted with a red warning panel
- Cost is automatically updated when items are added or removed
- Smart fallback pricing for items without explicit price data

### 💾 Cloud Save & Load System
- Save your entire furniture layout to **Unity Cloud Save** with a single tap
- Each save includes:
  - Full furniture layout with positions, rotations, and scales
  - Inventory summary and total cost
  - Budget and room category context
  - A **screenshot thumbnail** of your design
- **Reload previously saved designs** — browse a scrollable list of saved layouts with thumbnails
- **Delete unwanted designs** from the cloud
- Unique fingerprint (GUID) per save for data integrity verification
- Designs are saved relative to the user's camera position for consistent reloading

### 📸 Screenshot Capture
- Automatically captures a clean screenshot (UI hidden) when saving a design
- Screenshots are resized and compressed for efficient cloud storage
- Used as visual thumbnails in the reload panel

### 🔄 Layout Alignment System
- Loaded designs are grouped and anchored to the user's current position and orientation
- Floor detection ensures furniture snaps to the real ground plane
- A **Lock Layout** button lets you finalize placement, after which individual items become independently movable again

---

## 🛠️ Tech Stack

| Component | Technology |
|---|---|
| **Game Engine** | Unity 2022.3.62f3 (LTS) |
| **AR Framework** | AR Foundation 5.2.2 |
| **AR Backend** | ARCore XR Plugin 5.2.2 (Android) |
| **Cloud Services** | Unity Cloud Save 3.2.0 |
| **Authentication** | Unity Authentication 3.3.1 (Anonymous Sign-In) |
| **UI System** | Unity UI + TextMeshPro 3.0.7 |
| **Scripting** | C# with IL2CPP backend |
| **Target Platform** | Android (API 26+) |
| **Graphics API** | OpenGL ES 3.0 |

---

## 📁 Project Structure

```
AR_Interior_Design/
├── Assets/
│   ├── Scripts/                  # All C# scripts
│   │   ├── ARFurniturePlacer.cs       # AR raycasting, placement, drag, rotate, delete
│   │   ├── FurnitureManager.cs        # Tracks selected & placed furniture objects
│   │   ├── FurnitureCardUI.cs         # UI card for each furniture item in the panel
│   │   ├── FurnitureItem.cs           # Data model for furniture metadata
│   │   ├── FurnitureData.cs           # MonoBehaviour holding name & price on prefabs
│   │   ├── RoomSetupManager.cs        # Room selection, budget input, panel switching
│   │   ├── CostManager.cs            # Real-time cost calculation & budget tracking
│   │   ├── SaveLoadUIManager.cs       # Save/load UI flow, slot management
│   │   ├── CloudSaveManager.cs        # Unity Cloud Save API integration
│   │   ├── ScreenshotManager.cs       # Clean screenshot capture & compression
│   │   ├── DesignSlotCard.cs          # UI card for saved design slots
│   │   └── LayoutAlignmentController.cs # Group-based layout alignment & locking
│   │
│   ├── models/                   # 3D furniture models with textures
│   │   ├── Bedroom/              # Beds, chairs, tables, wardrobes
│   │   ├── Diningroom/           # Dining tables, chair sets
│   │   └── livingroom/           # Sofas, sofa chairs
│   │
│   ├── Prefabs/                  # Unity prefab assets
│   ├── Resources/                # Runtime-loadable assets
│   ├── Scenes/                   # Unity scenes
│   ├── TextMesh Pro/             # TMP font assets
│   └── XR/                       # XR/AR configuration assets
│
├── Packages/                     # Unity Package Manager configuration
├── ProjectSettings/              # Unity project settings
└── .gitignore                    # Unity-specific gitignore
```

---

## 📱 Supported Furniture

### 🛏️ Bedroom
| Item | Model Format |
|---|---|
| Modern Bed | FBX |
| Classic Bed | GLB / FBX |
| Study Table | FBX |
| Wardrobe / Dresser | FBX |
| Chair | FBX |

### 🛋️ Living Room
| Item | Model Format |
|---|---|
| Sofa (2 variants) | FBX |
| Sofa Chair | FBX |

### 🍽️ Dining Room
| Item | Model Format |
|---|---|
| Dining Table | FBX |
| Table + Chair Set | FBX |

---

## 🚀 How to Download & Set Up

### Prerequisites

- **Unity Hub** — [Download here](https://unity.com/download)
- **Unity 2022.3.62f3** (or any Unity 2022.3 LTS version) — Install via Unity Hub
- **Android Build Support** module (installed via Unity Hub → Installs → Add Modules)
- **Git** — [Download here](https://git-scm.com/downloads) (optional, for cloning)
- An **Android phone** with **ARCore support** — [Check compatibility](https://developers.google.com/ar/devices)

### Step 1: Download the Project

**Option A — Clone with Git (Recommended):**
```bash
git clone https://github.com/Anoopthadiyoor/ARspace--Virtual-interior-design-application-using-augmented-reality.git
```

**Option B — Download as ZIP:**
1. Go to the [GitHub repository](https://github.com/Anoopthadiyoor/ARspace--Virtual-interior-design-application-using-augmented-reality)
2. Click the green **"<> Code"** button
3. Select **"Download ZIP"**
4. Extract the ZIP to a folder on your computer (e.g., `D:\Unity\Projects\AR_Interior_Design`)

### Step 2: Open in Unity

1. Open **Unity Hub**
2. Click **"Open" → "Add project from disk"**
3. Navigate to the extracted/cloned folder and select it
4. Unity Hub will detect the project. If it prompts for a Unity version, select **Unity 2022.3.x LTS**
5. Click **Open** — Unity will import all assets (this may take a few minutes on first load)

### Step 3: Configure Unity Services (For Cloud Save)

The cloud save feature uses **Unity Gaming Services**. To enable it:

1. In Unity, go to **Edit → Project Settings → Services**
2. Click **"Create Project ID"** or link to an existing Unity project
3. Enable **Cloud Save** and **Authentication** from the Unity Dashboard:
   - Go to [dashboard.unity3d.com](https://dashboard.unity3d.com)
   - Select your project
   - Under **LiveOps → Cloud Save**, enable the service
   - Under **Player Authentication**, enable **Anonymous Sign-In**

> **Note:** The app will still work without cloud services — save/load features will simply be unavailable.

---

## 📲 How to Build & Run on Your Android Phone

### Step 1: Switch Build Platform

1. In Unity, go to **File → Build Settings**
2. Select **Android** from the platform list
3. Click **"Switch Platform"** (this may take a few minutes)

### Step 2: Connect Your Phone

1. Enable **Developer Options** on your Android phone:
   - Go to **Settings → About Phone** → Tap **"Build Number"** 7 times
2. Enable **USB Debugging** in **Settings → Developer Options**
3. Connect your phone to your PC via USB cable
4. If prompted on your phone, tap **"Allow USB Debugging"**

### Step 3: Build and Run

1. In Unity's **Build Settings**, click **"Build and Run"**
2. Choose a location to save the APK file
3. Unity will build the APK, install it on your connected phone, and launch it automatically
4. **Grant camera permission** when the app requests it

### Alternative: Build APK Only

1. In **Build Settings**, click **"Build"** (instead of "Build and Run")
2. Save the APK file
3. Transfer the APK to your phone via USB, Google Drive, or any file-sharing method
4. On your phone, open the APK file and tap **"Install"**
   - You may need to enable **"Install from Unknown Sources"** in your phone settings

---

## 🎮 How to Use the App

### 1. Launch & Select Room
- Open the app on your phone
- Select a room category from the dropdown: **Bedroom**, **Living Room**, or **Dining**
- Optionally set your **budget** in the input field
- Tap **"Confirm"**

### 2. Browse & Select Furniture
- A recommendation panel appears with furniture cards for your selected room
- Each card shows the item's **image**, **name**, and **price**
- Tap **"View in AR"** on any card to select it for placement

### 3. Place Furniture in AR
- Point your camera at a flat surface (floor, table)
- Wait for AR to detect the surface (you'll see plane indicators)
- **Tap on the surface** to place the selected furniture
- The item appears at the tapped location

### 4. Interact with Placed Objects
| Action | Mobile Gesture | PC (Editor) |
|---|---|---|
| **Select** | Tap on object | Click on object |
| **Move** | Drag with one finger | Click and drag |
| **Rotate** | Two-finger twist | Hold `R` key |
| **Delete** | Double-tap on object | Double-click on object |

### 5. Add More Furniture
- Tap the **"+ Add More"** button to return to the furniture panel
- Select another item and place it

### 6. Monitor Costs
- The **cost panel** (bottom-right) shows:
  - Current **total cost** of all placed items
  - Your set **budget**
  - A **red warning** if you exceed the budget

### 7. Save Your Design
- Tap the **"Save Design"** button (appears after placing items)
- The app captures a **clean screenshot** and saves everything to the cloud
- Your layout, cost data, budget, and room category are all preserved

### 8. Reload a Saved Design
- Tap the **"Reload"** button on the main screen
- Browse your saved designs with **thumbnail previews** and **cost summaries**
- Tap **"Load"** on any design to restore it in your current space
- Tap **"Delete"** to remove a saved design from the cloud

---

## 🖥️ Running in Unity Editor (Laptop/PC)

You can test the app directly in the Unity Editor without a phone:

1. Open the project in Unity
2. Open the scene: **Assets → Scenes → SampleScene**
3. Click the **▶ Play** button in the toolbar
4. Use the **mouse** to simulate touch interactions:
   - **Click** to select/place furniture
   - **Click and drag** to move furniture
   - **Hold R** to rotate the selected object
   - **Double-click** to delete an object
5. AR plane detection is simulated via the **XR Simulation** environment

> **Note:** Some features like real AR surface detection and multi-touch gestures are only fully functional on an actual Android device.

---

## ⚙️ Configuration

### Changing the Default Budget
- Select the **CostManager** GameObject in the scene hierarchy
- In the Inspector, change the **Max Budget** field (default: ₹50,000)

### Adding New Furniture
1. Import your 3D model (FBX, GLB, or OBJ) into `Assets/models/<RoomCategory>/`
2. Create a **Prefab** from the model
3. Add a **BoxCollider** and set the tag to **"Furniture"**
4. Attach the **FurnitureData** script and set the **name** and **price**
5. Place the prefab in `Assets/Resources/` for cloud reload compatibility
6. Create a **FurnitureCardUI** in the appropriate room panel and assign the prefab

---

## 🔧 Troubleshooting

| Issue | Solution |
|---|---|
| AR planes not detecting | Ensure good lighting and point camera at textured surfaces |
| App crashes on launch | Verify Android API level is 26+ and ARCore is supported on your device |
| Furniture not appearing | Check that the prefab has a MeshRenderer and correct scale |
| Cloud save not working | Ensure Unity Services are linked and internet is available |
| Cost not updating | Verify FurnitureData component is attached to the prefab |
| Build fails | Ensure Android Build Support module is installed in Unity Hub |

---

## 📄 License

This project is developed as an academic/educational project.

---

## 👨‍💻 Developer

**Anoop Thadiyoor**

- GitHub: [@Anoopthadiyoor](https://github.com/Anoopthadiyoor)

---

<p align="center">
  Built with ❤️ using Unity & AR Foundation
</p>
