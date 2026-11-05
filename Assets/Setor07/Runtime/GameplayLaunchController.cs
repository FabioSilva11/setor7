using System;
using System.Collections;
using Blocks.Gameplay.Core;
using Blocks.Gameplay.Shooter;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Cursor = UnityEngine.Cursor;

namespace Setor07.UI
{
    /// <summary>Reveals Solo gameplay only after the actual scene, local player and loadout are ready.</summary>
    [DisallowMultipleComponent]
    public sealed class GameplayLaunchController : MonoBehaviour
    {
        public const string LobbyScene = "Assets/Setor07/Scenes/Interface.unity";
        public const string ShooterScene = "Assets/Shooter/TestScenes/[BB] Shooter.unity";
        public string gameplayScenePath = ShooterScene;
        public bool IsLaunching { get; private set; }
        public bool HasLaunchError { get; private set; }

        private InterfaceBootstrap boot;
        private CanvasGroup lobby;
        private Camera lobbyCamera;
        private RenderMode lobbyRenderMode;
        private int lobbySortOrder;
        private bool changedScene;
        private bool returning;
        private Button backButton;
        private CoreInputHandler coreInput;
        private ShooterInputHandler shooterInput;
        private CorePlayerManager playerController;

        public void Initialize(InterfaceBootstrap bootstrap)
        {
            boot = bootstrap;
            lobby = boot.Presentation.GetComponent<CanvasGroup>();
            lobbyCamera = boot.canvas.worldCamera;
            lobbyRenderMode = boot.canvas.renderMode;
            lobbySortOrder = boot.canvas.sortingOrder;
        }

        public bool TryLaunch()
        {
            if (boot == null || !boot.IsReady || IsLaunching || HasLaunchError) return false;
            IsLaunching = true;
            boot.Presentation.SetLaunchInteractable(false);
            boot.Presentation.ShowScreen(InterfaceScreen.Lobby);
            lobby.alpha = 0;
            lobby.interactable = false;
            lobby.blocksRaycasts = false;
            boot.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            boot.canvas.sortingOrder = 1000;
            boot.loadingScreen.transform.SetAsLastSibling();
            boot.loadingScreen.SetActive(true);
            boot.SetProgress(0);
            StartCoroutine(LoadGameplay());
            return true;
        }

        private IEnumerator LoadGameplay()
        {
            // This frame is part of the contract: loading renders before loading starts.
            yield return null;
            if (!Application.CanStreamedLevelBeLoaded(gameplayScenePath))
            {
                Fail("NÃO FOI POSSÍVEL ENCONTRAR A PARTIDA.");
                yield break;
            }

            DontDestroyOnLoad(boot.canvas.gameObject);
            AsyncOperation operation = null;
            string error = null;
            try { operation = SceneManager.LoadSceneAsync(gameplayScenePath, LoadSceneMode.Single); }
            catch (Exception exception) { error = exception.Message; }
            if (operation == null)
            {
                Debug.LogWarning("Setor07: could not load gameplay scene: " + error);
                Fail("NÃO FOI POSSÍVEL CARREGAR A PARTIDA.");
                yield break;
            }
            operation.allowSceneActivation = false;
            while (operation.progress < .9f)
            {
                boot.SetProgress(Mathf.Clamp01(operation.progress / .9f) * .85f);
                yield return null;
            }
            boot.SetProgress(.85f);
            operation.allowSceneActivation = true;
            while (!operation.isDone) yield return null;
            changedScene = true;

            // Start subscriptions in the gameplay managers must run before the host connects.
            yield return null;
            foreach (var oldLobby in Object.FindObjectsByType<GameNetworkUI>(FindObjectsSortMode.None))
            {
                var document = oldLobby.GetComponent<UIDocument>();
                if (document != null && document.rootVisualElement != null)
                    document.rootVisualElement.style.display = DisplayStyle.None;
                oldLobby.enabled = false;
            }
            var manager = GameNetworkManager.Instance;
            if (manager == null || GameManager.Instance == null)
            {
                Fail("A PARTIDA NÃO ESTÁ DISPONÍVEL.");
                yield break;
            }
            manager.StartHostConnection();
            boot.SetProgress(.9f);
            double deadline = Time.realtimeSinceStartupAsDouble + 120;
            bool ready = false;
            while (Time.realtimeSinceStartupAsDouble < deadline && manager != null)
            {
                if (manager.NetworkState.ConnectionState == GameNetworkManager.ConnectionStates.Failed) break;
                var player = manager.IsConnectedClient ? manager.LocalClient?.PlayerObject : null;
                if (player != null && player.IsSpawned)
                {
                    if (coreInput == null) coreInput = player.GetComponent<CoreInputHandler>();
                    if (shooterInput == null) shooterInput = player.GetComponent<ShooterInputHandler>();
                    if (playerController == null) playerController = player.GetComponent<CorePlayerManager>();
                    SetInputBlocked(true);
                    var weapons = player.GetComponent<WeaponController>();
                    ready = playerController != null && playerController.enabled &&
                        playerController.CoreCamera != null && playerController.CoreCamera.enabled &&
                        weapons != null && weapons.CurrentWeapon != null;
                    if (ready) break;
                }
                yield return null;
            }
            if (!ready)
            {
                Fail("NÃO FOI POSSÍVEL INICIAR A PARTIDA.");
                yield break;
            }

            boot.SetProgress(1);
            Canvas.ForceUpdateCanvases();
            // Let player/HUD initialization finish behind the opaque loading layer.
            yield return null;
            SetInputBlocked(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            boot.loadingScreen.SetActive(false);
            Destroy(boot.canvas.gameObject);
        }

        private void SetInputBlocked(bool blocked)
        {
            if (coreInput != null) coreInput.SetHudInputBlocked(blocked);
            if (shooterInput != null) shooterInput.SetHudInputBlocked(blocked);
            if (playerController != null) playerController.SetMovementInputEnabled(!blocked);
        }

        private void Fail(string message)
        {
            HasLaunchError = true;
            IsLaunching = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (boot.loadingLabel != null) boot.loadingLabel.text = message;
            if (boot.progressFill != null) boot.progressFill.fillAmount = 0;
            CreateBackButton();
        }

        private void CreateBackButton()
        {
            if (backButton != null) { backButton.gameObject.SetActive(true); return; }
            var root = new GameObject("Voltar ao lobby", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(boot.loadingScreen.transform, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.anchoredPosition = new Vector2(0, 60);
            rect.sizeDelta = new Vector2(270, 42);
            root.GetComponent<Image>().color = new Color(.78f, .96f, .42f, 1);
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(root.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.font = boot.loadingLabel != null ? boot.loadingLabel.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = "VOLTAR AO LOBBY"; text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.04f, .07f, .08f, 1);
            text.raycastTarget = false;
            backButton = root.GetComponent<Button>();
            backButton.onClick.AddListener(ReturnToLobby);
        }

        private void ReturnToLobby()
        {
            if (returning) return;
            if (!changedScene)
            {
                HasLaunchError = false;
                backButton.gameObject.SetActive(false);
                boot.canvas.renderMode = lobbyRenderMode;
                boot.canvas.worldCamera = lobbyCamera;
                boot.canvas.sortingOrder = lobbySortOrder;
                lobby.alpha = 1; lobby.interactable = true; lobby.blocksRaycasts = true;
                boot.loadingScreen.SetActive(false);
                boot.Presentation.SetLaunchInteractable(true);
                return;
            }
            returning = true;
            backButton.interactable = false;
            StartCoroutine(ReloadLobby());
        }

        private IEnumerator ReloadLobby()
        {
            var manager = GameNetworkManager.Instance;
            if (manager != null)
            {
                manager.Shutdown();
                while (manager != null && manager.ShutdownInProgress) yield return null;
                if (manager != null) Destroy(manager.gameObject);
            }
            if (GameManager.Instance != null) Destroy(GameManager.Instance.gameObject);
            yield return null;
            var operation = SceneManager.LoadSceneAsync(LobbyScene, LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            Destroy(boot.canvas.gameObject);
        }
    }
}
