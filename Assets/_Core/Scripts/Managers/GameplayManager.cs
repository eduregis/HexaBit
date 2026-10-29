using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Cinemachine;

namespace HexaBit.Core {
    public class GameplayManager : MonoBehaviour {
        public static GameplayManager Instance { get; private set; }

        [Header("Hero Setup")]
        [SerializeField] private HeroController heroPrefab;
        [SerializeField] private List<HeroData> heroesData;
        [SerializeField] private List<Transform> spawnPoints;
        private List<HeroController> activeHeroes = new List<HeroController>();

        [Header("XP Settings")]
        [SerializeField] private int baseXPToLevel = 20;
        [SerializeField] private int xpPerLevelMultiplier = 15;

        [Header("Runtime Stats")]
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int totalKills = 0;

        [Header("Camera Setup")]
        [SerializeField] private CinemachineCamera vcam;

        [Header("Level Up UI")]
        [SerializeField] private GameObject levelUpPanelPrefab;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject pausePanelPrefab;
        [SerializeField] private GameObject restartPanelPrefab;

        [Header("Upgrade Pool")]
        [SerializeField] private UpgradePoolData upgradePoolData;

        [Header("Timer")]
        [SerializeField] private float currentTime = 0f;
        private bool isTimerPaused = false;

        private InputSystemUIInputModule cachedUIModule;
        private bool _initialized = false;

        // Prevents multiple Level Up panels from opening simultaneously
        private bool isLevelUpActive = false;

        // References to the dynamically instantiated UI panels
        private PauseUIManager currentPauseInstance;
        private RestartUIManager currentRestartInstance;

        public int CurrentXP => currentXP;
        public int CurrentLevel => currentLevel;
        public int TotalKills => totalKills;
        public int XPToNextLevel => baseXPToLevel + (currentLevel - 1) * xpPerLevelMultiplier;
        public float CurrentTime => currentTime;
        public bool IsTimerPaused => isTimerPaused;

        // Events
        public UnityEvent<int> OnXPChanged;
        public UnityEvent<int> OnLevelUp;
        public UnityEvent<int> OnKillCountChanged;
        public UnityEvent<float> OnTimerUpdated;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable() {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start() {
            InitializeForScene();
            _initialized = true;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (!_initialized) return;
            InitializeForScene();
        }

        private void InitializeForScene() {
            // Reset stats
            currentXP = 0;
            currentLevel = 1;
            totalKills = 0;
            currentTime = 0f;
            isTimerPaused = false;
            isLevelUpActive = false;

            // Clear UI references on scene reload
            currentPauseInstance = null;
            currentRestartInstance = null;

            OnXPChanged?.Invoke(currentXP);
            OnLevelUp?.Invoke(currentLevel);
            OnKillCountChanged?.Invoke(totalKills);
            OnTimerUpdated?.Invoke(currentTime);

            activeHeroes.Clear();

            // Clear old subscriptions to avoid duplicates (static event)
            HeroController.OnHeroDied -= OnHeroDied;
            HeroController.OnHeroDied += OnHeroDied;

            CacheUIModule();

            // Instantiate heroes
            for (int i = 0; i < heroesData.Count; i++) {
                Transform spawnPos = (i < spawnPoints.Count) ? spawnPoints[i] : null;
                Vector3 position = spawnPos != null ? spawnPos.position : Vector3.zero;

                HeroController newHero = Instantiate(heroPrefab, position, Quaternion.identity);
                newHero.SetHeroData(heroesData[i]);
                AssignUIModuleToHero(newHero);
                activeHeroes.Add(newHero);
            }

            if (activeHeroes.Count == 0) Debug.LogError("No heroes were spawned! Check Hero Data list.");

            // Camera setup
            if (activeHeroes.Count > 0) {
                if (vcam == null) vcam = FindFirstObjectByType<CinemachineCamera>();
                if (vcam != null) {
                    vcam.Follow = activeHeroes[0].transform;
                    vcam.LookAt = activeHeroes[0].transform;
                }
            }
        }

        private void CacheUIModule() {
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null) {
                cachedUIModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (cachedUIModule == null) Debug.LogWarning("GameplayManager: InputSystemUIInputModule not found on EventSystem!");
            } else {
                Debug.LogWarning("GameplayManager: EventSystem not found in the scene!");
            }
        }

        private void AssignUIModuleToHero(HeroController hero) {
            if (hero == null) return;

            PlayerInput playerInput = hero.GetComponent<PlayerInput>();
            if (playerInput != null) {
                if (cachedUIModule != null) {
                    playerInput.uiInputModule = cachedUIModule;
                } else {
                    Debug.LogWarning($"GameplayManager: Cannot assign UI Input Module to {hero.name} - module is null!");
                }
            } else {
                Debug.LogWarning($"GameplayManager: PlayerInput not found on {hero.name}!");
            }
        }

        private void OnHeroDied() {
            bool allDead = true;
            foreach (var hero in activeHeroes) {
                if (hero != null && !hero.IsDead) {
                    allDead = false;
                    break;
                }
            }

            if (allDead) {
                OpenRestartMenu();
            }
        }

        /// <summary>
        /// Instantiates and opens the Pause menu.
        /// </summary>
        public void OpenPauseMenu() {
            if (currentPauseInstance != null) return; // Already open

            if (pausePanelPrefab != null) {
                GameObject pauseObj = Instantiate(pausePanelPrefab);
                currentPauseInstance = pauseObj.GetComponent<PauseUIManager>();

                // Trigger initialization event (Fade In, Slide In, Pause Time)
                MenuInitializationEvent initEvent = pauseObj.GetComponentInChildren<MenuInitializationEvent>();
                if (initEvent != null) {
                    initEvent.InvokeEvent();
                } else {
                    Debug.LogWarning("GameplayManager: PausePanelPrefab is missing MenuInitializationEvent component!");
                    Time.timeScale = 0f; // Fallback
                }
            }
        }

        /// <summary>
        /// Instantiates and opens the Restart menu.
        /// </summary>
        private void OpenRestartMenu() {
            if (currentRestartInstance != null) return;

            if (restartPanelPrefab != null) {
                GameObject restartObj = Instantiate(restartPanelPrefab);
                currentRestartInstance = restartObj.GetComponent<RestartUIManager>();

                // Trigger initialization event (Fade In)
                MenuInitializationEvent initEvent = restartObj.GetComponentInChildren<MenuInitializationEvent>();
                if (initEvent != null) {
                    initEvent.InvokeEvent();
                } else {
                    Debug.LogWarning("GameplayManager: RestartPanelPrefab is missing MenuInitializationEvent component!");
                }
            }
        }

        private void Update() {
            if (!isTimerPaused) {
                currentTime += Time.deltaTime;
                OnTimerUpdated?.Invoke(currentTime);
            }
        }

        public void SetTimerPaused(bool paused) {
            isTimerPaused = paused;
        }

        public void AddXP(int amount, HeroController hero) {
            currentXP += amount;
            OnXPChanged?.Invoke(currentXP);

            // If a Level Up panel is already open, just accumulate XP. 
            // The panel will check for pending Level Ups when it closes.
            if (isLevelUpActive) return;

            CheckForLevelUp();
        }

        public void CheckForLevelUp() {
            if (isLevelUpActive) return;

            if (currentXP >= XPToNextLevel) {
                currentXP -= XPToNextLevel;
                currentLevel++;
                isLevelUpActive = true;

                HeroController targetHero = activeHeroes[Random.Range(0, activeHeroes.Count)];

                if (levelUpPanelPrefab != null) {
                    GameObject panelObj = Instantiate(levelUpPanelPrefab);
                    LevelUpUIManager uiManager = panelObj.GetComponent<LevelUpUIManager>();

                    List<LevelUpOption> options = GenerateChoices(3, targetHero);

                    uiManager.OpenWithOptions(options, targetHero, (int selectedIndex) => {
                        LevelUpOption chosenOption = options[selectedIndex];
                        chosenOption.onSelected.Invoke();

                        // Unlock the level up state and check if there's leftover XP
                        isLevelUpActive = false;
                        CheckForLevelUp();
                    });

                    MenuInitializationEvent initEvent = panelObj.GetComponentInChildren<MenuInitializationEvent>();
                    if (initEvent != null) {
                        initEvent.InvokeEvent();
                    } else {
                        Debug.LogWarning("GameplayManager: LevelUpPanelPrefab is missing MenuInitializationEvent component!");
                        Time.timeScale = 0f; // Fallback
                    }
                } else {
                    isLevelUpActive = false;
                }

                OnLevelUp?.Invoke(currentLevel);
                Debug.Log($"Level Up! Now Level {currentLevel}. Reward goes to: {targetHero.name}");
            }
        }

        public void AddKill() {
            totalKills++;
            OnKillCountChanged?.Invoke(totalKills);
        }

        public class LevelUpOption {
            public string displayName;
            public string description;
            public Sprite icon;
            public bool isWeapon;
            public int targetLevel;
            public System.Action onSelected;
        }

        public HeroController GetActiveHero(int index) {
            if (index >= 0 && index < activeHeroes.Count) return activeHeroes[index];
            return null;
        }

        private List<LevelUpOption> GenerateChoices(int count, HeroController hero) {
            List<LevelUpOption> options = new List<LevelUpOption>();

            if (upgradePoolData == null) {
                Debug.LogError("GameplayManager: upgradePoolData is null! Please assign it in the Inspector.");
                return options;
            }

            List<Object> availableItems = upgradePoolData.GetAvailableItems(hero);

            if (availableItems.Count == 0) {
                Debug.Log("No available upgrades for hero " + hero.name);
                return options;
            }

            List<Object> shuffledPool = availableItems.OrderBy(x => System.Guid.NewGuid()).ToList();

            foreach (var item in shuffledPool) {
                if (options.Count >= count) break;

                if (item is WeaponData weaponData) {
                    LevelUpOption option = weaponData.GetUpgradeOption(hero);
                    options.Add(option);
                } else if (item is BuffData buffData) {
                    LevelUpOption option = buffData.GetUpgradeOption(hero);
                    options.Add(option);
                }
            }

            while (options.Count < count) {
                options.Add(new LevelUpOption {
                    displayName = "Skip",
                    description = "",
                    icon = null,
                    isWeapon = false,
                    targetLevel = 0,
                    onSelected = () => { Debug.Log("Executing: Skip"); }
                });
            }

            return options;
        }
    }
}