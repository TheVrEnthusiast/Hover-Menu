using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine.SceneManagement;
using System.Diagnostics;

namespace HoverMenu
{
    public class ModMenu : MelonMod
    {
        private GameObject modMenuPanel;
        private GameObject statsMenuPanel;
        private GameObject sceneMenuPanel;
        private GameObject contentPanel;
        private GameObject fpsCounterText;
        private GameObject memoryUsageText;
        private bool isMenuOpen = false;
        private bool isStatsMenuOpen = false;
        private bool isSceneMenuOpen = false;
        private bool Debugz = false;//this defanatly does stuff. dont remove it.
        private float deltaTime = 0.0f;
        private ModLoader modLoader;
        private static readonly string sceneSaveFile = Path.Combine(Application.persistentDataPath, "lastscene.txt");
        public static List<string> AvailableScenes { get; private set; } = new List<string>();

        ///scene list for scene loader when i add it.
        private static readonly List<string> SelectScenes = new List<string>
        {
            "00_Title_RoomSize", "01_MainMenu", "02_TargetPractice", "03_Viewer_Credits", "0_SplashScreen", "CHARACTERLOAD",
            "level_CargoShip", "level_CityWastes", "level_Crater", "level_DeadDocks", "level_GroundCloudCanyon", "level_NewEden",
            "level_SpaceElevator", "level_Swamp", "level_WarpGate", "RECALIBRATE", "targetShoot_Coliseum", "ZZ_ReturnToMain",
            "BanditCanyon", "BanditCargobay", "BanditDesert", "BanditDesertII", "BanditRuins", "BanditSwamp", "Canyon",
            "CargoBay", "Crater", "Cutscene_Ending", "Desert", "Desert2", "Elevator", "Islands", "Ruins", "Skyfall", "Swamp"
        };

        //So when HJ starts this will Make the Eventsystem so ui will work.
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing DebugMenu and ModLoader...");

            modLoader = new ModLoader();
            modLoader.StartAssetBundleLoading();

            GameObject eventSystemObject = new GameObject("EventSystemInitializer");
            eventSystemObject.AddComponent<EventSystemInitializer>();

            MelonLogger.Msg("Initializing SceneLogger...");

            if (Debugz) { Debugz = !Debugz; }
            {
                MelonLogger.Msg("Checking status..." + "Status is vaild, Activating Menus");

            }

            //should help load into the same scene that you were in when you reload the mods
            MelonLogger.Msg("Checking for previous scene...");
            if (File.Exists(sceneSaveFile))
            {
                string lastScene = File.ReadAllText(sceneSaveFile);
                if (!string.IsNullOrEmpty(lastScene))
                {
                    MelonLogger.Msg($"Loading last scene: {lastScene}");
                    SceneManager.LoadScene(lastScene);
                    File.Delete(sceneSaveFile);
                }
            }

            CreateModMenuUI();
            CreateStatsMenuUI();
            UpdateModList();

        }

        private System.Collections.IEnumerator WaitForModLoader()
        {
            while (ModLoader.Instance == null)
            {
                yield return null;
            }

            ModLoader.Instance.StartAssetBundleLoading();
            MelonLogger.Msg("Asset Bundle Loading started successfully.");
        }

        // THIS IS ALL THE INPUTS FOR UI. And also the Null checker.
        public override void OnUpdate()
        {
            if (modMenuPanel == null || statsMenuPanel == null)
            {
                MelonLogger.Error("ModMenuPanel or StatsMenuPanel is null. Reinitializing UI...");
                CreateModMenuUI();
                CreateStatsMenuUI();
                UpdateModList();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                isMenuOpen = !isMenuOpen;
                modMenuPanel.SetActive(isMenuOpen);
                statsMenuPanel.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                isStatsMenuOpen = !isStatsMenuOpen;
                statsMenuPanel.SetActive(isStatsMenuOpen);
                modMenuPanel.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveCurrentSceneAndRestart();
            }

            //scene menus keybind.
            if (Input.GetKeyDown(KeyCode.B))
            {     
                isSceneMenuOpen = !isSceneMenuOpen;
                sceneMenuPanel.SetActive(isSceneMenuOpen);
            }

            UpdatePerformanceStats();
            UpdateModList();
        }

        //THe Performance stats. like fps and Memory usage
        private void UpdatePerformanceStats()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;

            fpsCounterText.GetComponent<Text>().text = $"FPS: {Mathf.Ceil(fps)}";

            float memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            memoryUsageText.GetComponent<Text>().text = $"Memory: {memoryUsage:F2} MB";
        }

        private void CreateModMenuUI()
        {
            MelonLogger.Msg("Creating Mod Menu UI...");

            modMenuPanel = new GameObject("ModMenuPanel");
            var canvas = modMenuPanel.AddComponent<Canvas>();
            var canvasScaler = modMenuPanel.AddComponent<CanvasScaler>();
            var graphicRaycaster = modMenuPanel.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            modMenuPanel.SetActive(false);

            var backgroundImage = modMenuPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var rectTransform = modMenuPanel.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(600, 400);
            rectTransform.anchoredPosition = Vector2.zero;

            // Title
            var titleObject = new GameObject("Title");
            titleObject.transform.SetParent(modMenuPanel.transform);
            var titleText = titleObject.AddComponent<Text>();
            titleText.text = "Loaded Mods - DebugMenu";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            var titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(580, 40);
            titleRect.anchoredPosition = new Vector2(0, 180);
            if (Debugz) { Debugz = !Debugz; }

            //this is the statsbutton
            CreateButton(modMenuPanel, "StatsButton", "Stats", new Vector2(-250, 180), ToggleStatsMenu);

            //content panel, so this is were the mods will go
            contentPanel = new GameObject("ContentPanel");
            contentPanel.transform.SetParent(modMenuPanel.transform);
            var contentRect = contentPanel.AddComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(580, 300);
            contentRect.anchoredPosition = new Vector2(0, -20);

            var verticalLayout = contentPanel.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.spacing = 10;
        }



        // Stats Menu UI panel thang
        private void CreateStatsMenuUI()
        {
            MelonLogger.Msg("Creating Stats Menu UI...");

            statsMenuPanel = new GameObject("StatsMenuPanel");
            var canvas = statsMenuPanel.AddComponent<Canvas>();
            var canvasScaler = statsMenuPanel.AddComponent<CanvasScaler>();
            var graphicRaycaster = statsMenuPanel.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            statsMenuPanel.SetActive(false);

            var backgroundImage = statsMenuPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            var rectTransform = statsMenuPanel.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 400);
            rectTransform.anchoredPosition = new Vector2(310, 0);

            // Title
            var titleObject = new GameObject("Title");
            titleObject.transform.SetParent(statsMenuPanel.transform);
            var titleText = titleObject.AddComponent<Text>();
            titleText.text = "Stats Menu";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 24;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
          
            var titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(280, 40);
            titleRect.anchoredPosition = new Vector2(0, 180);
            

            //this is the Text creation for easy creation.
            fpsCounterText = CreateText(statsMenuPanel, "FPSText", "FPS: ", new Vector2(-35, 140), 16);
            memoryUsageText = CreateText(statsMenuPanel, "MemoryText", "Memory: ", new Vector2(100, 140), 16);

            //this is a the button creation so i can make easy ui.
            CreateButton(statsMenuPanel, "LightButton", "Enable/Disable Lights", new Vector2(0, 80), ToggleLights);
            CreateButton(statsMenuPanel, "ParticleButton", "Enable/Disable Particles", new Vector2(0, 40), ToggleParticles);
            CreateButton(statsMenuPanel, "ReloadButton", "Reload Scene", new Vector2(0, 0), ReloadScene);
            CreateButton(statsMenuPanel, "ExitButton", "Exit Game", new Vector2(0, -40), ExitGame);
        }

        private void CreateSceneMenuUI()
        {
            //add this goober(me)
            if (Debugz) { Debugz = !Debugz; }
            {
                MelonLogger.Msg("Goober why are you messing with my source code and creating things.");
            }
        }

        private GameObject CreateText(GameObject parent, string name, string text, Vector2 position, int fontSize)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent.transform);
            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);
            rectTransform.anchoredPosition = position;

            return textObject;
        }

        private void CreateButton(GameObject parent, string name, string buttonText, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent.transform);

            var button = buttonObject.AddComponent<Button>();

            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = Color.black;

            var buttonTextObject = new GameObject("ButtonText");
            buttonTextObject.transform.SetParent(buttonObject.transform);
            var text = buttonTextObject.AddComponent<Text>();
            text.text = buttonText;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(180, 30);
            rectTransform.anchoredPosition = position;

            var textRect = buttonTextObject.GetComponent<RectTransform>();
            textRect.sizeDelta = rectTransform.sizeDelta;
            textRect.anchoredPosition = Vector2.zero;

            button.onClick.AddListener(onClick);
        }

        
        //Custom Button for scene Loader set up.
        private void CreateSceneButton(string sceneName)
        {
            var buttonObject = new GameObject(sceneName);
            buttonObject.transform.SetParent(contentPanel.transform);
            var button = buttonObject.AddComponent<Button>();
            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = Color.black;

            var buttonTextObject = new GameObject("ButtonText");
            buttonTextObject.transform.SetParent(buttonObject.transform);
            var text = buttonTextObject.AddComponent<Text>();
            text.text = sceneName;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);
            rectTransform.anchoredPosition = Vector2.zero;

            var textRect = buttonTextObject.GetComponent<RectTransform>();
            textRect.sizeDelta = rectTransform.sizeDelta;
            textRect.anchoredPosition = Vector2.zero;

            button.onClick.AddListener(() => MenuLoadScene(sceneName));
        }
        private void UpdateModList()
        {
            foreach (Transform child in contentPanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            List<MelonMod> loadedMods = MelonLoader.MelonHandler.Mods;

            foreach (var mod in loadedMods)
            {
                var modTextObject = new GameObject(mod.Info.Name);
                modTextObject.transform.SetParent(contentPanel.transform);

                var modText = modTextObject.AddComponent<Text>();
                modText.text = $"{mod.Info.Name} (v{mod.Info.Version})";
                modText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                modText.fontSize = 18;
                modText.color = Color.white;
                modText.alignment = TextAnchor.MiddleCenter;

                var rectTransform = modTextObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(580, 30);
            }
        }

        public static void LoadScene(string sceneName)
        {
            if (AvailableScenes.Contains(sceneName))
            {
                MelonLogger.Msg($"Loading scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                MelonLogger.Error($"Scene '{sceneName}' not found!");
            }
        }

        public void SaveCurrentSceneAndRestart()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            File.WriteAllText(sceneSaveFile, currentScene);
            MelonLogger.Msg($"Saved scene: {currentScene}");

            string gameExePath = Process.GetCurrentProcess().MainModule.FileName;
            MelonLogger.Msg($"Restarting game: {gameExePath}");

            Process.Start(gameExePath);
            Application.Quit();
        }

        public static void MenuLoadScene(string sceneName)
        {
            if (AvailableScenes.Contains(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
        ///////////////////////////////////////////////////////////////////
        ////ALL LOGIC FOR THE BUTTONS THAT DO THINGS IN GAME ARE BELOW ////
        ///////////////////////////////////////////////////////////////////


        private void ToggleStatsMenu()
        {
            isStatsMenuOpen = !isStatsMenuOpen;
            statsMenuPanel.SetActive(isStatsMenuOpen);
            modMenuPanel.SetActive(false);
        }

        private void ToggleLights()
        {
            var lights = GameObject.FindObjectsOfType<Light>();
            foreach (var light in lights)
            {
                light.enabled = !light.enabled;
            }
        }

        private void ToggleParticles()
        {
            var particleSystems = GameObject.FindObjectsOfType<ParticleSystem>();
            foreach (var particleSystem in particleSystems)
            {
                particleSystem.gameObject.SetActive(!particleSystem.gameObject.activeSelf);
            }
        }

        private void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            UpdateModList();
        }

        private void ExitGame()
        {
            Application.Quit();
        }
    }
}