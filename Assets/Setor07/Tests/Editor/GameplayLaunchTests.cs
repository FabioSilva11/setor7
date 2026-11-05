using System.Collections;
using Blocks.Gameplay.Core;
using Blocks.Gameplay.Shooter;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Setor07.UI.Tests
{
    public class GameplayLaunchTests
    {
        [UnityTest]
        public IEnumerator SoloMenuAndPlayKeepLoadingUntilPlayerAndWeaponAreReady()
        {
            EditorSceneManager.OpenScene("Assets/Setor07/Scenes/Interface.unity");
            yield return new EnterPlayMode();
            var boot = Object.FindFirstObjectByType<InterfaceBootstrap>();
            Assert.That(boot, Is.Not.Null);
            double deadline = Time.realtimeSinceStartupAsDouble + 180;
            while (!boot.IsReady && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(boot.IsReady, Is.True, "The visual lobby must finish loading.");
            Assert.That(EventSystem.current, Is.Not.Null, "Mouse and touch need an active EventSystem.");

            var view = boot.Presentation;
            FindButton(view, "Formation · mock").onClick.Invoke();
            Assert.That(view.screens[(int)InterfaceScreen.Formation].activeSelf, Is.True,
                "The formation selector must open when clicked.");
            foreach (string name in new[] { "Duo · mock", "Squad · mock" })
            {
                var allied = FindButton(view, name);
                Assert.That(allied.gameObject.activeSelf, Is.True);
                Assert.That(allied.interactable, Is.False);
                allied.onClick.Invoke();
                Assert.That(view.formation.text, Is.EqualTo("SOLO"));
                Assert.That(view.screens[(int)InterfaceScreen.Formation].activeSelf, Is.True);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Interface"));
            }
            FindButton(view, "Solo · mock").onClick.Invoke();
            Assert.That(view.screens[(int)InterfaceScreen.Formation].activeSelf, Is.False);
            Assert.That(view.formation.text, Is.EqualTo("SOLO"));
            FindButton(view, "Formation · mock").onClick.Invoke();
            FindButton(view, "Close · mock").onClick.Invoke();
            Assert.That(view.screens[(int)InterfaceScreen.Formation].activeSelf, Is.False);

            var loading = boot.loadingScreen;
            var play = FindButton(view, "Jogar · future action");
            var launcher = boot.GetComponent<GameplayLaunchController>();
            launcher.gameplayScenePath = "Assets/Setor07/Scenes/DoesNotExist.unity";
            play.onClick.Invoke();
            Assert.That(loading.activeSelf, Is.True);
            yield return null;
            yield return null;
            Assert.That(launcher.HasLaunchError, Is.True, "Missing content must not strand the user.");
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("Interface"));
            Assert.That(launcher.TryLaunch(), Is.False, "Do not restart a failed transition behind its error screen.");
            var back = loading.GetComponentInChildren<Button>();
            Assert.That(back, Is.Not.Null, "A failed launch needs a way back.");
            back.onClick.Invoke();
            Assert.That(loading.activeSelf, Is.False);
            Assert.That(play.interactable, Is.True);
            launcher.gameplayScenePath = GameplayLaunchController.ShooterScene;
            play.onClick.Invoke();
            Assert.That(loading.activeSelf, Is.True, "Loading must appear synchronously on JOGAR.");
            Assert.That(play.interactable, Is.False, "A second click must not start a second load.");
            play.onClick.Invoke();
            yield return null;
            Assert.That(loading != null && loading.activeSelf, Is.True, "Render loading before starting the scene load.");

            deadline = Time.realtimeSinceStartupAsDouble + 300;
            while (!HasReadyPlayer() && Time.realtimeSinceStartupAsDouble < deadline)
            {
                Assert.That(loading != null && loading.activeSelf, Is.True,
                    "Do not reveal gameplay until the local player and equipped weapon are ready.");
                yield return null;
            }
            Assert.That(HasReadyPlayer(), Is.True, "The existing host must spawn a playable equipped character.");
            while (loading != null && loading.activeSelf && Time.realtimeSinceStartupAsDouble < deadline)
                yield return null;
            Assert.That(loading == null || !loading.activeSelf, Is.True);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo("Assets/Shooter/TestScenes/[BB] Shooter.unity"));
            Assert.That(GameNetworkManager.Instance.IsHost, Is.True);
            Assert.That(Object.FindObjectsByType<GameNetworkManager>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
        }

        private static bool HasReadyPlayer()
        {
            var manager = GameNetworkManager.Instance;
            var player = manager != null && manager.IsConnectedClient ? manager.LocalClient?.PlayerObject : null;
            if (player == null || !player.IsSpawned) return false;
            var controller = player.GetComponent<CorePlayerManager>();
            var weapon = player.GetComponent<WeaponController>();
            return controller != null && controller.enabled && controller.CoreCamera != null &&
                controller.CoreCamera.enabled && weapon != null && weapon.CurrentWeapon != null;
        }

        private static Button FindButton(InterfacePresentation view, string name)
        {
            foreach (var button in view.GetComponentsInChildren<Button>(true))
                if (button.name == name) return button;
            Assert.Fail("Missing button: " + name);
            return null;
        }

        [UnityTearDown]
        public IEnumerator RestoreEditMode()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
        }
    }
}
