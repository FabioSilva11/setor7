using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Setor07.UI.Tests
{
    public class InterfacePresentationTests
    {
        [Test]
        public void InterfacePrefabContainsEveryReferenceScreenWithoutSavedBusinessListeners()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Setor07/Resources/Setor07/Interface.prefab");
            Assert.That(prefab, Is.Not.Null, "The native HTML presentation prefab has not been created.");
            foreach (string screen in new[] { "Lobby", "Formation", "Settings", "Inventory", "Store", "Events", "Vault" })
                Assert.That(prefab.transform.Find(screen), Is.Not.Null, screen);
            foreach (var button in prefab.GetComponentsInChildren<Button>(true))
                Assert.That(button.onClick.GetPersistentEventCount(), Is.Zero, button.name + " should be wired at runtime.");
            Assert.That(prefab.GetComponent<InterfacePresentation>().mockData, Is.Not.Null);
            Assert.That(prefab.GetComponent<CanvasGroup>(), Is.Not.Null);
        }

        [Test]
        public void StartupSceneIsPresentationAndPreviousGameplayScenesAreRetained()
        {
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo("Assets/Setor07/Scenes/Interface.unity"));
            Assert.That(System.Array.Exists(EditorBuildSettings.scenes, s => s.path.Contains("[BB] Shooter.unity")), Is.True);
        }

        [TestCase(InterfaceScreen.Lobby)]
        [TestCase(InterfaceScreen.Formation)]
        [TestCase(InterfaceScreen.Settings)]
        [TestCase(InterfaceScreen.Inventory)]
        [TestCase(InterfaceScreen.Store)]
        [TestCase(InterfaceScreen.Events)]
        [TestCase(InterfaceScreen.Vault)]
        public void InspectorPreviewShowsOnlyTheRequestedPanel(InterfaceScreen screen)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Setor07/Resources/Setor07/Interface.prefab");
            Assert.That(prefab, Is.Not.Null);
            var instance = Object.Instantiate(prefab);
            try
            {
                var view = instance.GetComponent<InterfacePresentation>();
                view.ShowScreen(screen);
                Assert.That(view.screens[0].activeSelf, Is.True, "Lobby remains behind overlays.");
                for (int i = 1; i < view.screens.Length; i++)
                    Assert.That(view.screens[i].activeSelf, Is.EqualTo(i == (int)screen));
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void RepeatedInteractionBindingRaisesOnePlayRequest()
        {
            var instance = CreateInterface();
            try
            {
                var view = instance.GetComponent<InterfacePresentation>();
                int requests = 0;
                view.PlayRequested += () => requests++;

                view.BindInteractions();
                view.BindInteractions();
                view.BindInteractions();
                view.playButton.onClick.Invoke();

                Assert.That(requests, Is.EqualTo(1), "Rebinding must not enqueue multiple gameplay launches.");
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void LockedLaunchIgnoresPlayAndFormationUntilUnlocked()
        {
            var instance = CreateInterface();
            try
            {
                var view = instance.GetComponent<InterfacePresentation>();
                int requests = 0;
                view.PlayRequested += () => requests++;
                view.BindInteractions();
                view.SetLaunchInteractable(false);

                Assert.That(view.playButton.interactable, Is.False);
                Assert.That(view.formationButton.interactable, Is.False);
                Assert.That(view.soloButton.interactable, Is.False);
                Assert.That(view.formationCloseButton.interactable, Is.False);
                view.playButton.onClick.Invoke();
                view.formationButton.onClick.Invoke();

                Assert.That(requests, Is.Zero, "A loading launch cannot be requested again.");
                Assert.That(view.previewScreen, Is.EqualTo(InterfaceScreen.Lobby));

                view.SetLaunchInteractable(true);
                Assert.That(view.duoButton.interactable, Is.False);
                Assert.That(view.squadButton.interactable, Is.False);
                view.formationButton.onClick.Invoke();
                Assert.That(view.screens[(int)InterfaceScreen.Formation].activeSelf, Is.True);
                view.formationCloseButton.onClick.Invoke();
                Assert.That(view.previewScreen, Is.EqualTo(InterfaceScreen.Lobby));
                view.playButton.onClick.Invoke();
                Assert.That(requests, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void EditorPreviewDoesNotStartRuntimeActions()
        {
            var instance = CreateInterface();
            try
            {
                var view = instance.GetComponent<InterfacePresentation>();
                int requests = 0;
                view.PlayRequested += () => requests++;

                view.formationButton.onClick.Invoke();
                view.playButton.onClick.Invoke();

                Assert.That(view.previewScreen, Is.EqualTo(InterfaceScreen.Lobby));
                Assert.That(requests, Is.Zero);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private static GameObject CreateInterface()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Setor07/Resources/Setor07/Interface.prefab");
            Assert.That(prefab, Is.Not.Null);
            var instance = Object.Instantiate(prefab);
            var group = instance.GetComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            return instance;
        }
    }
}
