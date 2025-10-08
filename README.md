# 3D-Family-Tree-Graph
The vision of this project is to provide and emersive 3D experience for exploring your Family History.
This project should be the foundation for:
 - A fun interactive playground for exploration, discovery, challenges, and learning (Think Quests and Side Quests)
 - Bringing your family history out of the dark and into an interactive world will motivate you to correct errors, collect more details and connect with your family.

## How to Launch This Application

### Prerequisites
- **Unity 2021.1.16f1** (Download from [Unity Archive](https://unity.com/releases/editor/archive))
- **Git** (to clone the repository)
- **Operating System**: Windows, macOS, or Linux
- **Minimum 4GB RAM** (8GB+ recommended for larger family trees)

### Getting Started

#### Option 1: Open in Unity Editor (Development)
1. **Clone the repository**
   ```bash
   git clone https://github.com/xfaith4/3D-Family-Tree-Graph.git
   cd 3D-Family-Tree-Graph
   ```

2. **Open the project in Unity**
   - Launch Unity Hub
   - Click "Add" and select the cloned repository folder
   - Ensure Unity version 2021.1.16f1 is installed
   - Open the project

3. **Load the starting scene**
   - In Unity, navigate to `Assets/Scenes/`
   - Open `aaStart RootsMagicNamePicker.unity`

4. **Run the application**
   - Click the Play button in Unity Editor
   - The application will automatically load `HofstetterFamilyTree.rmtree` (example data with 511 individuals) if available

#### Option 2: Build Standalone Application
1. **Open the project in Unity** (follow steps 1-3 from Option 1)

2. **Build the application**
   - Go to `File > Build Settings`
   - Ensure all scenes are included (should show: aaStart RootsMagicNamePicker, DigiKamPicker, MyTribeScene)
   - Select your target platform (Windows, macOS, or Linux)
   - Click "Build" or "Build and Run"
   - Choose an output folder for the executable

3. **Run the standalone application**
   - Navigate to the build output folder
   - Run the executable file
   - The application will launch with the file browser to select your family tree data

### First Launch
On first launch, the application will:
- Automatically load `HofstetterFamilyTree.rmtree` from the project root if it exists
- Otherwise, present a file browser to select a RootsMagic (.rmtree, .rmgc) or GEDCOM (.ged) file
- Store your file selection for future launches

### Controls
- **WASD / Arrow Keys**: Move character
- **Mouse**: Look around
- **Space**: Jump
- **Shift**: Sprint
- **E**: Interact with panels and objects
- **Menu Button**: Return to file selection
- **Start Button**: Show person details

## Features
- **3D Family Tree Visualization**: Explore your family tree in an immersive 3D environment
- **RootsMagic Database Support**: Load and visualize family data from RootsMagic (.rmtree, .rmgc) databases
- **GEDCOM File Support**: Import and convert GEDCOM (.ged) files to explore genealogy data
- **Auto-load Default Data**: Automatically loads HofstetterFamilyTree.rmtree on first launch
- **Interactive Exploration**: Navigate through generations, view family relationships, and discover your heritage

## Data Support
This project now supports two primary data formats:

### RootsMagic Databases (.rmtree, .rmgc)
- Native support for RootsMagic database files
- Includes the HofstetterFamilyTree.rmtree as example data with 511 individuals

### GEDCOM Files (.ged)
- Automatically converts GEDCOM files to RootsMagic SQLite format
- Preserves family relationships, dates, and genealogy information
- Includes the "Hofstetter Family Tree.ged" as example data

For more details on GEDCOM implementation, see [GEDCOM_IMPLEMENTATION.md](GEDCOM_IMPLEMENTATION.md).

Screen Shot:
![alt text](https://github.com/shuskey/3D-Family-Tree-Graph/blob/main/ScreenShots/JosephWithPhotoAndNames.JPG?raw=true)
