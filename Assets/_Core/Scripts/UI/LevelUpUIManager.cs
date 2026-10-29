using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class LevelUpUIManager : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private LevelUpButton[] choiceButtons;

        [Header("Hero Display (Level Up)")]
        [SerializeField] private Image heroLowerBodyImage;
        [SerializeField] private Image heroUpperBodyImage;

        [Header("Inventory Chips")]
        [SerializeField] private InventoryChip[] weaponChips; // 3 slots
        [SerializeField] private InventoryChip[] buffChips;   // 3 slots

        [Header("Inventory Color Images")]
        [SerializeField] private Image weaponColorImage; // Image behind weapon chips
        [SerializeField] private Image buffColorImage;   // Image behind buff chips

        [Header("Visual Configuration")]
        [SerializeField] private Sprite[] levelFrames; // Size 6 (Lv.1 to Lv.6)
        [SerializeField] private Sprite defaultIcon;

        [Header("Colors")]
        [SerializeField] private Color weaponColor = new Color(1f, 0.6f, 0f, 1f); // Orange
        [SerializeField] private Color buffColor = new Color(0.2f, 0.6f, 1f, 1f); // Cyan

        private System.Action<int> _onChoiceMade;
        private int _currentSelectedIndex = -1;
        private HeroController _targetHero;

        public void OpenWithOptions(
            List<GameplayManager.LevelUpOption> options,
            HeroController targetHero,
            System.Action<int> onChoiceMade) {
            _targetHero = targetHero;
            _onChoiceMade = onChoiceMade;

            // Pause the game timer
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(true);
            }

            Time.timeScale = 0f;
            levelUpPanel.SetActive(true);

            // --- Apply hero visuals (Upper & Lower Body) ---
            UpdateHeroVisuals();

            // --- Apply colors to external images ---
            if (weaponColorImage != null)
                weaponColorImage.color = weaponColor;
            if (buffColorImage != null)
                buffColorImage.color = buffColor;

            // --- Populate Inventory Chips ---
            PopulateInventoryChips(_targetHero);

            // --- Populate Choice Buttons ---
            for (int i = 0; i < choiceButtons.Length && i < options.Count; i++) {
                LevelUpButton btn = choiceButtons[i];
                GameplayManager.LevelUpOption opt = options[i];

                if (btn.backgroundImage != null) {
                    Color bgColor = opt.isWeapon ? weaponColor : buffColor;
                    btn.backgroundImage.color = bgColor;
                }

                if (btn.frameImage != null && levelFrames != null && levelFrames.Length > 0) {
                    int frameIndex = Mathf.Clamp(opt.targetLevel - 1, 0, levelFrames.Length - 1);
                    btn.frameImage.sprite = levelFrames[frameIndex];
                }

                if (btn.iconImage != null)
                    btn.iconImage.sprite = opt.icon != null ? opt.icon : defaultIcon;

                if (btn.nameText != null)
                    btn.nameText.text = opt.displayName;

                if (btn.descriptionText != null)
                    btn.descriptionText.text = opt.description;

                if (btn.levelText != null) {
                    if (opt.targetLevel == 1)
                        btn.levelText.text = "NEW";
                    else if (opt.targetLevel >= 6)
                        btn.levelText.text = "MAX";
                    else
                        btn.levelText.text = $"Lv.{opt.targetLevel}";
                }

                if (btn.button != null) {
                    btn.button.onClick.RemoveAllListeners();
                    int capturedIndex = i;
                    btn.button.onClick.AddListener(() => {
                        _onChoiceMade?.Invoke(capturedIndex);
                        CloseLevelUpMenu();
                    });
                }

                btn.SetSelected(false);
                btn.gameObject.SetActive(true);
            }

            for (int i = options.Count; i < choiceButtons.Length; i++) {
                if (choiceButtons[i] != null)
                    choiceButtons[i].gameObject.SetActive(false);
            }

            SetupButtonSelectionListeners();

            if (choiceButtons.Length > 0 && choiceButtons[0] != null && choiceButtons[0].button != null) {
                SetSelectedButton(0);
                EventSystem.current.SetSelectedGameObject(choiceButtons[0].button.gameObject);
            }
        }

        /// <summary>
        /// Updates the visual of the hero on the Level Up UI based on the target hero's data.
        /// </summary>
        private void UpdateHeroVisuals() {
            if (_targetHero == null) return;

            HeroData data = _targetHero.GetHeroData();

            if (data != null) {
                if (heroLowerBodyImage != null) {
                    heroLowerBodyImage.sprite = data.levelUpLowerBody;
                    heroLowerBodyImage.gameObject.SetActive(data.levelUpLowerBody != null);
                }

                if (heroUpperBodyImage != null) {
                    heroUpperBodyImage.sprite = data.levelUpUpperBody;
                    heroUpperBodyImage.gameObject.SetActive(data.levelUpUpperBody != null);
                }
            }
        }

        private void PopulateInventoryChips(HeroController hero) {
            if (hero == null) return;

            // --- Populate Weapon Chips ---
            int weaponSlots = Mathf.Min(weaponChips.Length, hero.weaponSlots.Length);
            for (int i = 0; i < weaponSlots; i++) {
                WeaponBase weapon = hero.weaponSlots[i];
                if (weapon != null && weapon.data != null) {
                    Sprite icon = weapon.data.icon;
                    Sprite frame = GetLevelFrame(weapon.currentLevel);
                    weaponChips[i].SetChip(icon, frame);
                } else {
                    weaponChips[i].ClearChip();
                }
            }

            // Clear remaining weapon chips
            for (int i = weaponSlots; i < weaponChips.Length; i++) {
                weaponChips[i].ClearChip();
            }

            // --- Populate Buff Chips ---
            int buffSlots = Mathf.Min(buffChips.Length, hero.activeBuffs.Count);
            for (int i = 0; i < buffSlots; i++) {
                ActiveBuff buff = hero.activeBuffs[i];
                if (buff != null && buff.data != null) {
                    Sprite icon = buff.data.icon;
                    Sprite frame = GetLevelFrame(buff.currentLevel);
                    buffChips[i].SetChip(icon, frame);
                } else {
                    buffChips[i].ClearChip();
                }
            }

            // Clear remaining buff chips
            for (int i = buffSlots; i < buffChips.Length; i++) {
                buffChips[i].ClearChip();
            }
        }

        private Sprite GetLevelFrame(int level) {
            if (levelFrames == null || levelFrames.Length == 0) return null;
            int index = Mathf.Clamp(level - 1, 0, levelFrames.Length - 1);
            return levelFrames[index];
        }

        private void SetupButtonSelectionListeners() {
            for (int i = 0; i < choiceButtons.Length; i++) {
                if (choiceButtons[i] == null || choiceButtons[i].button == null) continue;

                EventTrigger trigger = choiceButtons[i].button.gameObject.GetComponent<EventTrigger>();
                if (trigger == null)
                    trigger = choiceButtons[i].button.gameObject.AddComponent<EventTrigger>();

                trigger.triggers.Clear();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry();
                selectEntry.eventID = EventTriggerType.Select;
                int capturedIndex = i;
                selectEntry.callback.AddListener((data) => {
                    SetSelectedButton(capturedIndex);
                });
                trigger.triggers.Add(selectEntry);
            }
        }

        private void SetSelectedButton(int index) {
            if (_currentSelectedIndex == index) return;

            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < choiceButtons.Length) {
                if (choiceButtons[_currentSelectedIndex] != null)
                    choiceButtons[_currentSelectedIndex].SetSelected(false);
            }

            _currentSelectedIndex = index;

            if (index >= 0 && index < choiceButtons.Length && choiceButtons[index] != null) {
                choiceButtons[index].SetSelected(true);
            }
        }

        public void CloseLevelUpMenu() {
            Time.timeScale = 1f;

            // Resume the game timer
            if (GameplayManager.Instance != null) {
                GameplayManager.Instance.SetTimerPaused(false);
            }

            Destroy(gameObject);
        }

        private void OnDestroy() {
            Time.timeScale = 1f;
        }
    }
}