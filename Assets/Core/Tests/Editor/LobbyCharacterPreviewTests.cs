using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Blocks.Gameplay.Core.Tests
{
    public class LobbyCharacterPreviewTests
    {
        private const string ModelPath = "Assets/Core/Art/Models/LobbyCharacter.prefab";

        [Test]
        public void LobbyAssetContainsOnlyVisualComponentsAndAnIdleAnimator()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            Assert.That(model, Is.Not.Null);
            Assert.That(model.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0));
            Assert.That(model.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(model.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(model.GetComponentsInChildren<AudioSource>(true), Is.Empty);
            var animator = model.GetComponentInChildren<Animator>();
            Assert.That(animator, Is.Not.Null);
            Assert.That(animator.avatar, Is.Not.Null);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            Assert.That(animator.runtimeAnimatorController.animationClips.Any(c => c != null && c.name == "Idle"), Is.True);
        }

        [Test]
        public void PreviewReleasesItsCameraAndTextureWhenLeavingLobbyAndCanReturn()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var image = new Image();
            var preview = new LobbyCharacterPreview(model, image);
            try
            {
                Assert.That(preview.IsVisible, Is.False);
                preview.SetVisible(true);
                Assert.That(preview.IsVisible, Is.True);
                var firstTexture = image.image as RenderTexture;
                Assert.That(firstTexture, Is.Not.Null);
                Assert.That(firstTexture.width, Is.EqualTo(384));
                Assert.That(firstTexture.height, Is.EqualTo(768));
                Assert.That(firstTexture.antiAliasing, Is.EqualTo(1));
                var camera = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)
                    .Single(c => c.name == "Lobby preview camera");
                Assert.That(camera.cullingMask, Is.EqualTo(1 << 31));
                Assert.That(camera.allowHDR, Is.False);
                Assert.That(camera.allowMSAA, Is.False);
                Assert.That(camera.GetComponent<AudioListener>(), Is.Null);

                preview.SetVisible(false);
                Assert.That(preview.IsVisible, Is.False);
                Assert.That(image.image, Is.Null);
                Assert.That(firstTexture == null, Is.True);
                Assert.That(camera == null, Is.True);

                preview.SetVisible(true);
                Assert.That(preview.IsVisible, Is.True);
                Assert.That(image.image, Is.Not.Null);
                preview.Dispose();
                preview.Dispose();
                Assert.That(preview.IsVisible, Is.False);
                Assert.That(image.image, Is.Null);
                preview.SetVisible(true);
                Assert.That(preview.IsVisible, Is.False);
            }
            finally
            {
                preview.Dispose();
            }
        }

        [Test]
        public void NativeLobbyStillContainsHostAndClientControlsAndThePreview()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Blocks/MultiplayerSession/Scenes/QuickJoin/ShooterLobby.uxml").CloneTree();
            Assert.That(tree.Q<Button>("host-button"), Is.Not.Null);
            Assert.That(tree.Q<Button>("client-button"), Is.Not.Null);
            Assert.That(tree.Q<Image>("lobby-character-preview"), Is.Not.Null);
        }

        [Test]
        public void ReturnToLobbyControlIsOutsideThePanelHiddenDuringGameplay()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/Blocks/MultiplayerSession/Scenes/QuickJoin/ShooterLobby.uxml").CloneTree();
            var returnButton = tree.Q<Button>("return-lobby-button");
            Assert.That(returnButton, Is.Not.Null);
            Assert.That(tree.Q("connection-panel").Contains(returnButton), Is.False);
        }

        [Test]
        public void PreviewRejectsGameplayObjectsBeforeCloningThem()
        {
            var invalidModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var image = new Image();
            var preview = new LobbyCharacterPreview(invalidModel, image);
            try
            {
                LogAssert.Expect(LogType.Error,
                    "The lobby preview needs a visual-only model, without gameplay components.");
                preview.SetVisible(true);
                Assert.That(preview.IsVisible, Is.False);
                Assert.That(image.image, Is.Null);
            }
            finally
            {
                preview.Dispose();
                Object.DestroyImmediate(invalidModel);
            }
        }
    }
}
