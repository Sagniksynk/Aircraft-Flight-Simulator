# ✈️ Aircraft Flight Simulator

![Unity](https://img.shields.io/badge/Unity-6%20%2F%202022.3%2B-black?style=flat&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-blue)
![Architecture](https://img.shields.io/badge/Architecture-Component%20Based-orange)
![Physics](https://img.shields.io/badge/Physics-Aerodynamic%20Simulation-green)
![Status](https://img.shields.io/badge/Status-Prototype-success)

**Aircraft Flight Simulator** is a physics-based fixed-wing flight simulator developed in Unity. The core aerodynamics are based on the research paper by **Khan and Nahon (2015)** — *"Real-time modeling of agile fixed-wing UAV aerodynamics"*, simulating realistic lift, drag, stall, and control surface behavior.

This project extends the base aerodynamics with a **realistic engine startup system**, **propeller physics**, **multi-camera views via Cinemachine**, **in-flight photo capture**, and a **balloon drop mission system**.

## 🎮 Gameplay Features

* **Realistic Aerodynamic Physics**:
    * Lift, drag, and torque are computed per-surface using angle of attack, air density, and flap deflection.
    * Stall behavior with smooth transition between low-AoA and post-stall regimes.
    * Control surfaces (ailerons, elevator, rudder, flaps) respond to player input with configurable sensitivity.

* **Engine Start/Stop System**:
    * Aircraft starts cold and dark — engine must be manually started with the **E** key.
    * Propeller gradually spools from 0 → idle RPM (650) on startup, mimicking a Cessna 172 Lycoming engine.
    * Engine shutdown gracefully winds the propeller down over several seconds.

* **Realistic Throttle & Propeller**:
    * Throttle smoothly ramps up/down with configurable spool times instead of instant on/off.
    * Propeller RPM is decoupled from throttle — it has its own inertia-based spool up (4s) and spool down (8s).
    * Propeller visually rotates proportional to current RPM (idle: 650 RPM, max: 2700 RPM).

* **Balloon Drop Mission**:
    * Drop balloons at designated **Drop Zones** scattered across the terrain.
    * Balloons inherit the aircraft's velocity for realistic drop trajectories.
    * HUD shows remaining balloons, nearest zone distance, and completion status.

* **In-Flight Photo Capture**:
    * Capture high-resolution (1920×1080) screenshots from the active camera view.
    * Photos are saved as PNG files with timestamps to the persistent data path.
    * Includes a screen flash effect and a thumbnail preview on the HUD.

* **Multi-Camera System (Cinemachine)**:
    * **External Camera**: Smooth 3rd-person follow with Cinemachine damping.
    * **Nose Camera**: Fixed to the aircraft nose tip — first-person forward view.
    * **Cockpit Camera**: Pilot head position with right-mouse-button head look.
    * Smooth blending transitions between views powered by Cinemachine Brain.

* **Aviation-Style HUD**:
    * Airspeed in knots, altitude in feet.
    * Throttle percentage, flap status, brake status, engine state.
    * On-screen controls reference.

## 🕹️ Controls

| Key | Action |
| :--- | :--- |
| **E** | Engine Start / Stop |
| **Space** | Throttle Full / Idle (Toggle) |
| **F** | Flaps Toggle |
| **B** | Brakes Toggle |
| **Arrow Up / Down** | Pitch (Elevator) |
| **Arrow Left / Right** | Roll (Ailerons) |
| **Yaw Axis** | Yaw (Rudder) |
| **X** | Drop Balloon |
| **P** | Capture Photo |
| **C** | Cycle Camera (External → Nose → Cockpit) |
| **V** | Reset Head Look |
| **Right Mouse Button** | Look Around (Nose / Cockpit view) |

## 🛠️ Technical Architecture

### 1. Aerodynamic Simulation (Khan & Nahon 2015)
* **Per-Surface Force Calculation**: Each `AeroSurface` computes lift, drag, and torque independently based on local airflow velocity, angle of attack, and flap deflection.
* **Aspect Ratio Correction**: Lift slope is corrected for finite wing effects using the aspect ratio of each surface.
* **Stall Modeling**: Smooth stitching between linear lift regime and post-stall flat-plate aerodynamics with configurable stall angles.
* **Predictive Integration**: `AircraftPhysics` uses a half-step velocity/angular-velocity prediction to improve simulation stability.

### 2. Component Architecture
* **`AircraftPhysics.cs`**: Core physics engine. Sums aerodynamic forces from all `AeroSurface` components and applies thrust. Uses predictive force averaging for stability.
* **`AeroSurface.cs`**: Individual aerodynamic surface. Computes lift/drag coefficients with flap deflection, induced drag, and skin friction. Configurable via `AeroSurfaceConfig` ScriptableObjects.
* **`AirplaneController.cs`**: Player input handler. Manages engine state, throttle ramping, propeller rotation, control surface deflection, and HUD updates.
* **`CameraController.cs`**: Cinemachine-based camera switcher. Manages three virtual cameras with priority-based switching and head look for cockpit views.
* **`BalloonDropper.cs`**: Mission system. Handles balloon instantiation with velocity inheritance, drop zone proximity detection, and scoring.
* **`DropZone.cs`**: Target marker with configurable radius. Provides editor gizmos for visual placement.
* **`PhotoCapture.cs`**: Screenshot system. Renders the active camera to a `RenderTexture`, encodes to PNG, and saves to disk with UI feedback.

### 3. Cessna 172 Reference Values
| Parameter | Value |
| :--- | :--- |
| Engine | Lycoming IO-360-L2A (180 hp) |
| Idle RPM | 650 RPM |
| Max RPM | 2700 RPM |
| Engine Spool Up | ~3 seconds |
| Propeller Spool Down | ~8 seconds |
| Cruise Speed | ~122 knots (63 m/s) |
| Stall Speed | ~48 knots (25 m/s) |
| Max Thrust | ~720 N (×1.5 drag correction = 1080 N) |

## 📁 Project Structure
Assets/Main/ ├── Core/ │   ├── Scripts/ │   │   ├── AircraftPhysics.cs │   │   ├── AeroSurface.cs │   │   ├── AeroSurfaceConfig.cs │   │   ├── BiVector3.cs │   │   ├── CameraController.cs │   │   ├── Drop/ │   │   │   ├── BalloonDropper.cs │   │   │   └── DropZone.cs │   │   └── Photo/ │   │       └── PhotoCapture.cs │   └── Config/ │       └── AircraftPhysicsDisplaySettings.asset ├── Game/ │   ├── Scripts/ │   │   └── AirplaneController.cs │   └── Prefabs/ │       └── Balloon.prefab └── Scenes/

## 🚀 How to Play (Installation)

1.  **Clone this repo**:
    ```bash
    https://github.com/Sagniksynk/Aircraft-Flight-Simulator.git
    ```
2.  **Open in Unity**:
    * Launch **Unity Hub**.
    * Click **Add** → browse to the cloned project folder.
    * Open the project (Unity 2022.3+ or Unity 6 recommended).
3.  **Open the Scene**:
    * Navigate to `Assets/Main/Scenes/` and open the flight scene.
4.  **Play**:
    * Press **Play** in the Editor.
    * Press **E** to start the engine, then **Space** to throttle up.

## 📦 Dependencies

| Package | Version | Purpose |
| :--- | :--- | :--- |
| Cinemachine | 2.10.3 | Multi-camera system with smooth blending |
| Universal Render Pipeline | 17.0.3 | Rendering |
| AI Navigation | 2.0.4 | Terrain navigation |

## 📚 References

* **W. Khan and M. Nahon**, "Real-time modeling of agile fixed-wing UAV aerodynamics," 2015 *International Conference on Unmanned Aircraft Systems* (ICUAS), Denver, CO, 2015, pp. 1188-1195, doi: 10.1109/ICUAS.2015.7152411.

## 📄 License

This project is for educational purposes.

---

*Developed by Sagnik Dasgupta*
