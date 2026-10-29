using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HexaBit.Core {
    public partial class HeroController : DamageableCharacter {
        [Header("Character Data")]
        [SerializeField] private HeroData heroData;

        [Header("Weapons Slots")]
        [SerializeField] private int maxWeaponSlots = 3;
        public WeaponBase[] weaponSlots;

        [Header("UI")]
        [SerializeField] private HeroHPSlider hpSlider;

        // Public properties for weapons to access
        public float ProjectileSpeedMultiplier { get; private set; } = 1f;
        public float GrowthMultiplier { get; private set; } = 1f;

        public bool IsDead { get; private set; } = false;
        public static event System.Action OnHeroDied;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Animator animator;
        private float currentSpeed;

        public Vector2 FacingDirection { get; private set; } = Vector2.down;

        protected override void Awake() {
            base.Awake();

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) Debug.LogError("HeroController: Rigidbody2D not found!");

            animator = GetComponent<Animator>();

            if (weaponSlots == null || weaponSlots.Length == 0)
                weaponSlots = new WeaponBase[maxWeaponSlots];
        }

        private void Start() {
            if (heroData != null) {
                SetHealth(heroData.maxHealth, heroData.armor);

                base.DamageMultiplier = heroData.damageMultiplier;
                ProjectileSpeedMultiplier = heroData.projectileSpeedMultiplier;
                GrowthMultiplier = heroData.growthMultiplier;

                if (heroData.startingWeapon != null)
                    EquipWeapon(heroData.startingWeapon);

                if (animator != null && heroData.animatorController != null) {
                    animator.runtimeAnimatorController = heroData.animatorController;
                } else if (animator == null) {
                    Debug.LogWarning("HeroController: Animator component not found!");
                } else if (heroData.animatorController == null) {
                    Debug.LogWarning($"HeroController: No Animator Controller assigned in HeroData for {heroData.heroName}!");
                }
            } else {
                Debug.LogWarning("HeroController: HeroData not assigned! Using default values.");
                SetHealth(maxHealth, armor);
            }

            currentSpeed = heroData != null ? heroData.moveSpeed : 3.5f;
            baseSpeed = currentSpeed;
            CurrentSpeed = currentSpeed;

            OnHealthChanged += UpdateHealthBar;

            RefreshStats();
        }

        // --- Movement ---
        public void OnMove(InputValue value) {
            if (Time.timeScale == 0f || IsDead) {
                moveInput = Vector2.zero;
                return;
            }
            moveInput = value.Get<Vector2>().normalized;
            if (moveInput != Vector2.zero) {
                FacingDirection = moveInput;
                UpdateAnimationDirection();
            } else {
                UpdateAnimationDirection();
            }
        }

        private void FixedUpdate() {
            if (Time.timeScale == 0f || IsDead) return;
            // Use CurrentSpeed which is modified by statuses
            float effectiveSpeed = CurrentSpeed > 0 ? CurrentSpeed : currentSpeed;
            Vector2 targetPosition = rb.position + moveInput * effectiveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);

            // Update status effects
            UpdateStatuses();
        }

        private void UpdateAnimationDirection() {
            if (animator == null) return;
            animator.SetFloat("DirectionX", FacingDirection.x);
            animator.SetFloat("Speed", moveInput.magnitude);
        }

        public void SetHeroData(HeroData data) {
            heroData = data;
        }

        public HeroData GetHeroData() {
            return heroData;
        }

        // --- UI Management ---
        private void UpdateHealthBar() {
            if (hpSlider != null) {
                hpSlider.value = (float)currentHealth / maxHealth;
            }
        }

        // --- Override Animate Taking Damage ---
        protected override void AnimateTakingDamage() {
            if (animator == null) return;
                animator.SetTrigger("Hit");
        }

        // --- Override Die ---
        protected override void Die() {
            IsDead = true;
            OnHeroDied?.Invoke();
            Debug.Log("Hero died!");
            if (animator != null)
                animator.SetTrigger("Falling");
        }
    }
}