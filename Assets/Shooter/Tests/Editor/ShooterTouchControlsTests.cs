using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Blocks.Gameplay.Shooter.Tests
{
    public class ShooterTouchControlsTests
    {
        [Test]
        public void ShooterPresentationPreservesTemplateBindings()
        {
            const string path = "Assets/Shooter/Scripts/Runtime/Framework/WeaponHUD.uxml";
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            Assert.That(asset, Is.Not.Null);
            var tree = asset.CloneTree();
            foreach (var name in new[] { "weapon-type-label", "weapon-icon", "ammo-label",
                "player-ammo-bar", "player-health-bar", "player-stamina-bar", "player-reticle",
                "notification-container", "message-overlay", "message-label", "countdown-label" })
                Assert.That(tree.Q(name), Is.Not.Null, name);
            Assert.That(tree.Q<Button>("open-backpack"), Is.Not.Null);
            Assert.That(tree.Q<Button>("close-backpack"), Is.Not.Null);
        }

        [Test]
        public void MobileOrientationAllowsOnlyLandscape()
        {
            Assert.That(PlayerSettings.defaultInterfaceOrientation, Is.EqualTo(UIOrientation.AutoRotation));
            Assert.That(PlayerSettings.allowedAutorotateToPortrait, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToPortraitUpsideDown, Is.False);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeLeft, Is.True);
            Assert.That(PlayerSettings.allowedAutorotateToLandscapeRight, Is.True);
        }

        [TestCase(20, 0, 1, 0)]
        [TestCase(-20, 0, -1, 0)]
        [TestCase(0, -20, 0, 1)]
        [TestCase(0, 20, 0, -1)]
        public void CameraFollowsDragDirection(float dx, float dy, float expectedX, float expectedY)
        {
            var input = ShooterTouchControls.ToLookInput(new Vector2(dx, dy));
            // The template applies yaw and then pitch with this same sign convention.
            var forward = Quaternion.Euler(0, input.x, 0) *
                Quaternion.Euler(-input.y, 0, 0) * Vector3.forward;
            Assert.That(forward.x * expectedX + forward.y * expectedY, Is.GreaterThan(0));
        }

        [TestCase(44, 0, 1, 0)]
        [TestCase(-44, 0, -1, 0)]
        [TestCase(0, -44, 0, 1)]
        [TestCase(0, 44, 0, -1)]
        public void JoystickFollowsDragDirection(float x, float y, float expectedX, float expectedY)
        {
            var input = ShooterTouchControls.ToMoveInput(new Vector2(x, y), 44);
            Assert.That(input.x, Is.EqualTo(expectedX).Within(0.0001f));
            Assert.That(input.y, Is.EqualTo(expectedY).Within(0.0001f));
        }

        [Test]
        public void DiagonalMovementIsClamped()
        {
            var input = ShooterTouchControls.ToMoveInput(new Vector2(100, -100), 44);
            Assert.That(input.magnitude, Is.EqualTo(1).Within(0.0001f));
            Assert.That(input.x, Is.GreaterThan(0));
            Assert.That(input.y, Is.GreaterThan(0));
        }

        [Test]
        public void ReleasingFireFingerKeepsMovementFingerHeld()
        {
            int moveReleases = 0;
            int fireReleases = 0;
            var move = Binding(() => moveReleases++);
            var fire = Binding(() => fireReleases++);
            try
            {
                Down(move, 1);
                Down(fire, 2);
                Up(move, 2);
                Assert.That(moveReleases, Is.Zero);
                Up(fire, 2);
                Assert.That(fireReleases, Is.EqualTo(1));
                Assert.That(moveReleases, Is.Zero);
                Up(move, 1);
                Assert.That(moveReleases, Is.EqualTo(1));
            }
            finally
            {
                ((IDisposable)move).Dispose();
                ((IDisposable)fire).Dispose();
            }
        }

        [Test]
        public void CancellationAndDisposalReleaseHeldActionOnlyOnce()
        {
            int releases = 0;
            var binding = Binding(() => releases++);
            Down(binding, 1);
            binding.GetType().GetMethod("Cancel").Invoke(binding, null);
            Up(binding, 1);
            ((IDisposable)binding).Dispose();
            Assert.That(releases, Is.EqualTo(1));
        }

        [TestCase("touch-look-surface")]
        [TestCase("move-indicator")]
        public void GestureSurfacesDoNotReceiveButtonPressedTint(string name)
        {
            var surface = new VisualElement { name = name };
            var binding = Binding(null, surface);
            try
            {
                Down(binding, 1);
                Assert.That(surface.ClassListContains("hud-control--pressed"), Is.False);
            }
            finally { ((IDisposable)binding).Dispose(); }
        }

        [Test]
        public void ButtonPressedTintEndsWhenItsFingerIsReleased()
        {
            var button = new VisualElement();
            button.AddToClassList("hud-control");
            var binding = Binding(null, button);
            try
            {
                Down(binding, 1);
                Assert.That(button.ClassListContains("hud-control--pressed"), Is.True);
                Up(binding, 1);
                Assert.That(button.ClassListContains("hud-control--pressed"), Is.False);
            }
            finally { ((IDisposable)binding).Dispose(); }
        }

        [Test]
        public void SprintCanBeToggledAndCancellationDoesNotRearmIt()
        {
            var type = typeof(ShooterTouchControls).GetNestedType("SprintToggle", BindingFlags.NonPublic);
            Assert.That(type, Is.Not.Null, "Mobile sprint needs a latch so the joystick and camera remain usable.");
            var states = new System.Collections.Generic.List<bool>();
            var sprint = Activator.CreateInstance(type, new object[] { (Action<bool>)states.Add });
            type.GetMethod("Toggle").Invoke(sprint, null);
            Assert.That(type.GetProperty("IsActive").GetValue(sprint), Is.True);
            type.GetMethod("Toggle").Invoke(sprint, null);
            Assert.That(type.GetProperty("IsActive").GetValue(sprint), Is.False);
            type.GetMethod("Toggle").Invoke(sprint, null);
            type.GetMethod("Cancel").Invoke(sprint, null);
            type.GetMethod("Cancel").Invoke(sprint, null);
            Assert.That(type.GetProperty("IsActive").GetValue(sprint), Is.False);
            Assert.That(states, Is.EqualTo(new[] { true, false, true, false }));
        }

        private static object Binding(Action release, VisualElement element = null)
        {
            var type = typeof(ShooterTouchControls).GetNestedType("PointerBinding", BindingFlags.NonPublic);
            return Activator.CreateInstance(type, new object[] { element ?? new VisualElement(), null, null, release });
        }

        private static void Down(object binding, int pointer)
        {
            using var evt = PointerDownEvent.GetPooled();
            SetPointer(evt, pointer);
            Invoke(binding, "OnDown", evt);
        }

        private static void Up(object binding, int pointer)
        {
            using var evt = PointerUpEvent.GetPooled();
            SetPointer(evt, pointer);
            Invoke(binding, "OnUp", evt);
        }

        private static void SetPointer(object evt, int pointer)
        {
            evt.GetType().GetProperty("pointerId").SetValue(evt, pointer);
            evt.GetType().GetProperty("pointerType").SetValue(evt, UnityEngine.UIElements.PointerType.touch);
        }

        private static void Invoke(object binding, string method, object evt) =>
            binding.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(binding, new[] { evt });
    }
}
