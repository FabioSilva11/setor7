using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Setor07.UI.Tests
{
    public class InterfacePresentationTests
    {
        [Test]
        public void InterfacePrefabContainsEveryReferenceScreenAndInertActions()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Setor07/Resources/Setor07/Interface.prefab");
            Assert.That(prefab, Is.Not.Null, "The native HTML presentation prefab has not been created.");
            foreach (string screen in new[] { "Lobby", "Formation", "Settings", "Inventory", "Store", "Events", "Vault" })
                Assert.That(prefab.transform.Find(screen), Is.Not.Null, screen);
            foreach (var button in prefab.GetComponentsInChildren<Button>(true))
                Assert.That(button.onClick.GetPersistentEventCount(), Is.Zero, button.name + " must remain mocked.");
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
    }
}
