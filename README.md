# HopeIsland – Huawei ICT Competition Innovation Track 2026

## 📌 Project Background & Value 
Hope Island is a game-based digital therapeutic (DTx) support system aimed at minimizing motivation loss, fatigue, and motor skill regressions (such as tremors) experienced by pediatric oncology patients due to long-term chemotherapy and hospital stays . Our project provides a personalized, adaptive, and safe rehabilitation experience by analyzing children's physical reflexes in the game and their emotional states in real-time .

## 🛠 Technical Approach 
* **Game Engine:** Unity 
* **Programming Language:** C#, Python
* **AI Framework:** MindSpore Lite (Edge AI) 
* **Cloud & Mobile Services:** Huawei Cloud, HMS Core (Safety Detect, Analytics Kit, Account Kit) .

## 🧠 AI Model Details 
In training our model, a **custom subset of the AffectNet dataset consisting of 38,000 images** reflecting real-world conditions was used. To ensure resource efficiency, **Transfer Learning** was applied over the **MobileNetV2** architecture.
* **Performance Optimization:** During our training process, our model's Loss value was successfully reduced from 2.6 down to 0.4 levels, achieving an **accuracy rate of 71.5%** in pediatric oncology-focused emotion analysis.
* **Real-Time Inference:** This score technically proves that our model can make consistent predictions in real-time at speeds of dozens of frames per second on mobile platforms (with Int8 quantization on MindSpore Lite) .
* **Confusion Matrix:** Our model has a highly precise capability in distinguishing 'Positive' and 'Negative' states, which are the most critical for our project. This ensures the consistency of our decisions when dynamically adjusting the difficulty of the 'Hope Island' game based on the child's morale .

## 🚀 Key Features 
* **Multimodal Analysis:** Real-time emotion analysis via WebCam (Vision Layer) and tremor tracking via screen tap coordinates (Physical Layer) .
* **Dynamic Difficulty Adaptation:** Automatic adjustment of the game difficulty based on the child's instantaneous emotional state and motor skill performance, based on the Flow Theory.
* **Hardware-Free Motor Skill Tracking:** Detecting tremors from screen tap (Tap-Only) deviations using Euclidean distance, without the need for an additional wearable device.
* **Privacy by Design (Edge AI):** Performing AI inferences entirely on the device and ensuring medical data privacy by immediately destroying the images .

## ⚙️ Reproduction Steps 
To test and run the project locally, please follow these steps :
1. Clone the repository: `git clone https://github.com/ardabulent/HopeIsland.git`
2. Open Unity Hub and click "Add project from disk".
3. Select the cloned `HopeIsland` directory.
4. Open the project in **Unity **.
5. Go to the `Assets/Scenes` folder and open the main scene (`IntroScene` or `StartScene`).
6. Press the **Play** button in the Unity Editor to start.

## 💻 Key Code Explanation 
* **`BasitGorevManager.cs`:** Follows tasks in the health routine.
* **`Balloon.cs`:** Controls the behaviors (movement mechanics, popping) of the balloon objects appearing on the screen. It is responsible for transmitting the interaction data at the moment the child taps the balloon (for tremor calculation) to the main system.
* **`BalloonSpawner.cs`:** Responsible for dynamically generating balloons in the scene. It optimizes the spawn frequency and speed of the balloons according to the "Difficulty State" data coming from MindSpore.
* **`BackgroundMusic.cs`:** Manages the atmospheric music and sound effects of the application. It provides a smooth audio experience during transitions between menus and games.
