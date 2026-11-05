using UnityEngine;
using System.Collections;
using Blocks.Gameplay.Core;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;
using Cursor = UnityEngine.Cursor;

namespace Blocks.Gameplay.Shooter
{
    /// <summary>
    /// Manages the weapon-related HUD elements including ammo display, weapon icons, and dynamic reticle.
    /// Integrates both UI Toolkit (UIElements) and UGUI systems for comprehensive weapon feedback.
    /// </summary>
    public class WeaponHUD : CoreHUD
    {
        #region Fields & Properties

        [Header("Listening to")]
        [Tooltip("Event triggered when clip ammo count changes.")]
        [SerializeField] private IntPairEvent onClipAmmoChanged;
        [Tooltip("Event triggered when weapon is swapped.")]
        [SerializeField] private WeaponSwapEvent onWeaponChanged;
        [Tooltip("Event triggered when aiming state changes.")]
        [SerializeField] private BoolEvent onAimingStateChanged;
        [Tooltip("Event triggered when weapon is fired.")]
        [SerializeField] private FloatEvent onWeaponFiredEvent;
        [Tooltip("Event triggered when reload starts.")]
        [SerializeField] private FloatEvent onReloadStartedEvent;
        [Tooltip("Event triggered when weapon spread changes.")]
        [SerializeField] private FloatEvent onWeaponSpreadChanged;

        [Header("Component Dependencies")]
        [Tooltip("Reference to the aim controller for reticle management.")]
        [SerializeField] private AimController aimController;

        [Header("UGUI")]
        [Tooltip("UGUI image component for displaying reload progress indicator.")]
        [SerializeField] private Image weaponReload;

        private Coroutine m_ReloadCoroutine;
        private Coroutine m_MessageCoroutine;
        private Coroutine m_ReticleCooldownCoroutine;

        private Label m_AmmoLabel;
        private Label m_WeaponTypeLabel;
        private UnityEngine.UIElements.Image m_WeaponIcon;
        private ProgressBar m_PlayerAmmoBar;
        private VisualElement m_PlayerReticle;
        private WeaponData m_CurrentWeaponData;
        private VisualElement m_BackpackOverlay;
        private Button m_OpenBackpack;
        private Button m_CloseBackpack;
        private Button m_ItemsTab;
        private Button m_DevicesTab;
        private Label m_BackpackEmptyMessage;
        private Label m_BackpackWeaponName;
        private Label m_BackpackWeaponAmmo;
        private UnityEngine.UIElements.Image m_BackpackWeaponIcon;
        private bool m_BackpackOpen;
        private CursorLockMode m_PreviousCursorLock;
        private bool m_PreviousCursorVisibility;
        private ShooterTouchControls m_TouchControls;

        #endregion

        #region Unity Methods

        protected override void Initialize()
        {
            base.Initialize();
            if (aimController == null)
            {
                Debug.LogError("AimController is not assigned on WeaponHUD!", this);
            }

            UpdateWeaponReticleAndIcon(onWeaponChanged.LastValue);
            UpdateReticleVisibility(onAimingStateChanged.LastValue);
        }

        private void LateUpdate()
        {
            if (!IsOwner) return;
            m_TouchControls?.UpdateSafeArea();

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame && m_BackpackOverlay != null)
                SetBackpackOpen(!m_BackpackOpen);
            else if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && m_BackpackOpen)
                CloseBackpack();

            if (aimController == null) return;

            // Reset reticle position to ensure it stays centered
            // This prevents drift that can occur from UI Toolkit layout calculations
            if (m_PlayerReticle != null)
            {
                if (m_PlayerReticle.style.position != StyleKeyword.Initial)
                {
                    m_PlayerReticle.style.position = StyleKeyword.Initial;
                    m_PlayerReticle.style.left = StyleKeyword.Initial;
                    m_PlayerReticle.style.top = StyleKeyword.Initial;
                }
            }

            // Keep the UGUI reload indicator centered on the reticle
            if (weaponReload != null)
            {
                weaponReload.rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        #endregion

        public override void OnNetworkDespawn()
        {
            CloseBackpack();
            m_TouchControls?.Dispose();
            m_TouchControls = null;
            if (m_OpenBackpack != null) m_OpenBackpack.clicked -= OpenBackpack;
            if (m_CloseBackpack != null) m_CloseBackpack.clicked -= CloseBackpack;
            if (m_ItemsTab != null) m_ItemsTab.clicked -= SelectItems;
            if (m_DevicesTab != null) m_DevicesTab.clicked -= SelectDevices;
            base.OnNetworkDespawn();
        }

        private void OpenBackpack() => SetBackpackOpen(true);
        private void CloseBackpack() => SetBackpackOpen(false);
        private void SelectItems() => SelectBackpackTab(false);
        private void SelectDevices() => SelectBackpackTab(true);

        private void SetBackpackOpen(bool open)
        {
            if (m_BackpackOverlay == null || m_BackpackOpen == open) return;
            m_BackpackOpen = open;
            m_TouchControls?.SetGameplayEnabled(!open);
            GetComponentInParent<CoreInputHandler>()?.SetHudInputBlocked(open);
            GetComponentInParent<ShooterInputHandler>()?.SetHudInputBlocked(open);
            m_BackpackOverlay.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;

            if (open)
            {
                m_PreviousCursorLock = Cursor.lockState;
                m_PreviousCursorVisibility = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                SelectItems();
                m_CloseBackpack?.Focus();
            }
            else
            {
                m_CloseBackpack?.Blur();
                Cursor.lockState = m_PreviousCursorLock;
                Cursor.visible = m_PreviousCursorVisibility;
            }
        }

        private void SelectBackpackTab(bool devices)
        {
            m_ItemsTab?.EnableInClassList("backpack-tab--selected", !devices);
            m_DevicesTab?.EnableInClassList("backpack-tab--selected", devices);
            if (m_BackpackEmptyMessage != null)
                m_BackpackEmptyMessage.text = devices ? "Nenhum dispositivo coletado." : "Nenhum item coletado.";
        }

        #region Protected Methods

        protected override void RegisterAdditionalListeners()
        {
            onWeaponFiredEvent.RegisterListener(HandleWeaponFired);
            onClipAmmoChanged.RegisterListener(UpdateClipAmmoDisplay);
            onReloadStartedEvent.RegisterListener(HandleReloadStarted);
            onWeaponSpreadChanged.RegisterListener(UpdateReticleSpread);
            onWeaponChanged.RegisterListener(UpdateWeaponReticleAndIcon);
            onAimingStateChanged.RegisterListener(UpdateReticleVisibility);
        }

        protected override void UnregisterAdditionalListeners()
        {
            onWeaponFiredEvent.UnregisterListener(HandleWeaponFired);
            onClipAmmoChanged.UnregisterListener(UpdateClipAmmoDisplay);
            onReloadStartedEvent.UnregisterListener(HandleReloadStarted);
            onWeaponSpreadChanged.UnregisterListener(UpdateReticleSpread);
            onWeaponChanged.UnregisterListener(UpdateWeaponReticleAndIcon);
            onAimingStateChanged.UnregisterListener(UpdateReticleVisibility);
        }

        protected override void QueryHUDElements(VisualElement root)
        {
            base.QueryHUDElements(root);
            m_WeaponTypeLabel = root.Q<Label>("weapon-type-label");
            m_AmmoLabel = root.Q<Label>("ammo-label");
            m_PlayerAmmoBar = root.Q<ProgressBar>("player-ammo-bar");
            m_PlayerReticle = root.Q<VisualElement>("player-reticle");
            m_WeaponIcon = root.Q<UnityEngine.UIElements.Image>("weapon-icon");
            m_BackpackOverlay = root.Q("backpack-overlay");
            m_OpenBackpack = root.Q<Button>("open-backpack");
            m_CloseBackpack = root.Q<Button>("close-backpack");
            m_ItemsTab = root.Q<Button>("items-tab");
            m_DevicesTab = root.Q<Button>("devices-tab");
            m_BackpackEmptyMessage = root.Q<Label>("backpack-empty-message");
            m_BackpackWeaponName = root.Q<Label>("backpack-weapon-name");
            m_BackpackWeaponAmmo = root.Q<Label>("backpack-weapon-ammo");
            m_BackpackWeaponIcon = root.Q<UnityEngine.UIElements.Image>("backpack-weapon-icon");

            if (m_OpenBackpack != null) m_OpenBackpack.clicked += OpenBackpack;
            if (m_CloseBackpack != null) m_CloseBackpack.clicked += CloseBackpack;
            if (m_ItemsTab != null) m_ItemsTab.clicked += SelectItems;
            if (m_DevicesTab != null) m_DevicesTab.clicked += SelectDevices;

            m_TouchControls?.Dispose();
            m_TouchControls = new ShooterTouchControls(root,
                GetComponentInParent<CoreInputHandler>(), GetComponentInParent<ShooterInputHandler>());
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) m_TouchControls?.CancelAll();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) m_TouchControls?.CancelAll();
        }

        private void OnDisable()
        {
            CloseBackpack();
            m_TouchControls?.CancelAll();
        }

        protected override void SetHUDDefaults()
        {
            base.SetHUDDefaults();
            if (weaponReload != null)
            {
                float baseSize = m_CurrentWeaponData?.reticleBaseSize ?? 80f;
                weaponReload.rectTransform.sizeDelta = new Vector2(baseSize, baseSize);
                weaponReload.gameObject.SetActive(false);
            }
            if (m_PlayerReticle != null)
            {
                m_PlayerReticle.style.position = Position.Absolute;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the ammo display with current and maximum clip values.
        /// Color codes the display: red when empty, yellow when low, white when adequate.
        /// </summary>
        /// <param name="clip">Contains current ammo (value1) and max ammo (value2).</param>
        private void UpdateClipAmmoDisplay(IntPair clip)
        {
            if (m_AmmoLabel != null)
            {
                m_AmmoLabel.text = $"{clip.value1.ToString()} / {clip.value2.ToString()}";
                if (clip.value1 == 0) m_AmmoLabel.style.color = Color.red;
                else if (clip.value1 <= clip.value2 * 0.3f) m_AmmoLabel.style.color = Color.yellow;
                else m_AmmoLabel.style.color = Color.white;
            }

            if (m_BackpackWeaponAmmo != null) m_BackpackWeaponAmmo.text = $"{clip.value1} / {clip.value2}";

            if (m_PlayerAmmoBar != null)
            {
                m_PlayerAmmoBar.value = clip.value1;
                m_PlayerAmmoBar.highValue = clip.value2;
            }
        }

        /// <summary>
        /// Updates the weapon reticle image, icon, and related UI when the weapon changes.
        /// Handles both weapon equip and unequip scenarios.
        /// </summary>
        /// <param name="payload">Contains the new weapon reference and swap information.</param>
        private void UpdateWeaponReticleAndIcon(WeaponSwapPayload payload)
        {
            if (m_PlayerReticle == null) return;

            if (payload.NewWeapon != null)
            {
                m_CurrentWeaponData = payload.NewWeapon.GetWeaponData();
                if (m_CurrentWeaponData != null)
                {
                    m_PlayerReticle.style.backgroundImage = new StyleBackground(m_CurrentWeaponData.reticleImage);
                    m_WeaponIcon.sprite = m_CurrentWeaponData.weaponIcon;
                    if (m_BackpackWeaponIcon != null) m_BackpackWeaponIcon.sprite = m_CurrentWeaponData.weaponIcon;
                    if (m_BackpackWeaponName != null) m_BackpackWeaponName.text = m_CurrentWeaponData.weaponName;

                    if (m_WeaponTypeLabel != null) m_WeaponTypeLabel.text = m_CurrentWeaponData.weaponName;
                }

                if (payload.NewWeapon is ModularWeapon simpleWeapon)
                {
                    simpleWeapon.GetAmmoInfo(out int current, out int max);
                    UpdateClipAmmoDisplay(new IntPair { value1 = current, value2 = max });
                    UpdateReticleSpread(simpleWeapon.GetCurrentSpreadAngle());
                }
            }
            else
            {
                m_CurrentWeaponData = null;
                if (m_WeaponTypeLabel != null) m_WeaponTypeLabel.text = "SEM ARMA";
                if (m_BackpackWeaponName != null) m_BackpackWeaponName.text = "SEM ARMA";
                if (m_BackpackWeaponIcon != null) m_BackpackWeaponIcon.sprite = null;
                if (m_BackpackWeaponAmmo != null) m_BackpackWeaponAmmo.text = "";
                if (m_WeaponIcon != null) m_WeaponIcon.sprite = null;
                if (m_AmmoLabel != null) m_AmmoLabel.text = "";
                if (m_PlayerAmmoBar != null) m_PlayerAmmoBar.value = 0;
                m_PlayerReticle.style.backgroundImage = StyleKeyword.None;
                m_PlayerReticle.style.display = DisplayStyle.None;
                UpdateReticleSpread(0);
            }
        }

        /// <summary>
        /// Controls reticle visibility based on aiming state and weapon availability.
        /// The reticle only shows when actively aiming with a valid weapon equipped.
        /// </summary>
        /// <param name="isAiming">Whether the player is currently aiming.</param>
        private void UpdateReticleVisibility(bool isAiming)
        {
            if (m_PlayerReticle == null) return;
            bool shouldShow = isAiming && m_CurrentWeaponData != null;
            m_PlayerReticle.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Handles weapon fire feedback by triggering a visual cooldown effect on the reticle.
        /// </summary>
        /// <param name="fireRate">The fire rate of the weapon, used for cooldown timing.</param>
        private void HandleWeaponFired(float fireRate)
        {
            if (m_ReticleCooldownCoroutine != null) StopCoroutine(m_ReticleCooldownCoroutine);
            m_ReticleCooldownCoroutine = StartCoroutine(ReticleCooldownCoroutine(fireRate));
        }

        /// <summary>
        /// Updates the reticle size based on weapon spread angle.
        /// Larger spread results in a larger reticle, providing visual feedback for weapon accuracy.
        /// </summary>
        /// <param name="spreadAngle">The current spread angle of the weapon in degrees.</param>
        private void UpdateReticleSpread(float spreadAngle)
        {
            if (m_PlayerReticle == null) return;

            float baseSize = m_CurrentWeaponData?.reticleBaseSize ?? 80f;
            float spreadMultiplier = m_CurrentWeaponData?.reticleSpreadMultiplier ?? 100f;

            // Calculate final reticle size by adding spread contribution to base size
            float finalSize = baseSize + (spreadAngle * spreadMultiplier);

            m_PlayerReticle.style.width = finalSize;
            m_PlayerReticle.style.height = finalSize;
            if (weaponReload != null)
            {
                var weaponReloadSize = finalSize * 0.7f;
                weaponReload.rectTransform.sizeDelta = new Vector2(weaponReloadSize, weaponReloadSize);
            }
        }

        /// <summary>
        /// Initiates the reload animation coroutine when reload begins.
        /// </summary>
        /// <param name="reloadTime">Duration of the reload in seconds.</param>
        private void HandleReloadStarted(float reloadTime)
        {
            if (m_ReloadCoroutine != null) StopCoroutine(m_ReloadCoroutine);
            m_ReloadCoroutine = StartCoroutine(ReloadAnimationCoroutine(reloadTime));
        }

        /// <summary>
        /// Provides visual feedback for weapon fire rate by tinting the reticle grey temporarily.
        /// </summary>
        /// <param name="duration">How long to display the cooldown effect.</param>
        private IEnumerator ReticleCooldownCoroutine(float duration)
        {
            if (m_PlayerReticle == null) yield break;
            m_PlayerReticle.style.unityBackgroundImageTintColor = new StyleColor(Color.grey);
            yield return new WaitForSeconds(duration);
            m_PlayerReticle.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
        }

        /// <summary>
        /// Animates the reload progress indicator as a radial fill over the reload duration.
        /// </summary>
        /// <param name="reloadTime">Total time for the reload animation in seconds.</param>
        private IEnumerator ReloadAnimationCoroutine(float reloadTime)
        {
            if (weaponReload == null) yield break;
            weaponReload.gameObject.SetActive(true);
            weaponReload.fillAmount = 0;

            float elapsedTime = 0f;
            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                weaponReload.fillAmount = elapsedTime / reloadTime;
                yield return null;
            }
            weaponReload.fillAmount = 1;
            weaponReload.gameObject.SetActive(false);
        }

        #endregion

    }
}

