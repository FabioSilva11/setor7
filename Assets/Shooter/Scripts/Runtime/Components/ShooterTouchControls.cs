using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Blocks.Gameplay.Core;
using UnityEngine;
using UnityEngine.UIElements;
using PointerType = UnityEngine.UIElements.PointerType;

[assembly: InternalsVisibleTo("Blocks.Gameplay.Shooter.Tests")]

namespace Blocks.Gameplay.Shooter
{
    /// <summary>Routes the mobile HUD to the template's existing player input events.</summary>
    internal sealed class ShooterTouchControls : IDisposable
    {
        private readonly VisualElement m_Root;
        private readonly VisualElement m_Hud;
        private readonly VisualElement m_Backpack;
        private readonly List<PointerBinding> m_Bindings = new List<PointerBinding>();
        private readonly CoreInputHandler m_CoreInput;
        private readonly CoreMovement m_CoreMovement;
        private readonly ShooterInputHandler m_ShooterInput;
        private readonly bool m_IsMobile;
        private readonly SprintToggle m_Sprint;
        private Rect m_SafeArea;
        private Vector2 m_ScreenSize;
        private Vector2 m_PanelSize;

        public ShooterTouchControls(VisualElement root, CoreInputHandler core, ShooterInputHandler shooter)
        {
            m_Root = root;
            m_Hud = root.Q("shooter-hud");
            m_Backpack = root.Q("backpack-overlay");
            m_CoreInput = core;
            m_CoreMovement = core != null ? core.GetComponent<CoreMovement>() : null;
            m_ShooterInput = shooter;
            m_IsMobile = Application.isMobilePlatform;
            if (m_Hud == null) return;

            var lookSurface = new VisualElement { name = "touch-look-surface" };
            lookSurface.AddToClassList("touch-look-surface");
            m_Hud.Insert(0, lookSurface);

            foreach (var name in new[] { "audio", "microphone", "chat", "mark", "emotes", "camera",
                "settings", "heal", "grenade", "interact", "crouch", "prone" })
            {
                var element = root.Q(name);
                if (element == null) continue;
                element.AddToClassList("hud-control--unavailable");
                element.tooltip = "Em breve";
                element.pickingMode = PickingMode.Position;
            }

            foreach (var name in new[] { "move-indicator", "run", "jump", "fire", "aim", "reload" })
                root.Q(name)?.EnableInClassList("touch-only-control", true);
            root.EnableInClassList("touch-enabled", m_IsMobile);

            if (!m_IsMobile) return;

            m_Hud.RegisterCallback<DetachFromPanelEvent>(OnHudDetached);

            if (m_CoreInput != null)
            {
                var run = root.Q("run");
                var caption = run?.Q<Label>();
                m_Sprint = new SprintToggle(active =>
                {
                    m_CoreInput.SetTouchSprint(active);
                    run?.EnableInClassList("hud-control--active", active);
                    if (run != null) run.tooltip = active ? "Parar de correr" : "Ativar corrida";
                    if (caption != null) caption.text = active ? "CORRENDO" : "CORRER";
                });
                BindPress("run", m_Sprint.Toggle);
            }

            m_Bindings.Add(new PointerBinding(lookSurface, null, evt =>
            {
                if (m_CoreInput != null)
                    m_CoreInput.AddTouchLook(ToLookInput(evt.deltaPosition));
            }, null));

            var move = root.Q("move-indicator");
            var thumb = move?.ElementAt(0);
            if (move != null && m_CoreInput != null)
            {
                Action<Vector2> setMove = localPosition =>
                {
                    var center = move.contentRect.center;
                    float radius = Mathf.Max(1f, move.contentRect.width * 0.4f);
                    var input = ToMoveInput(localPosition - center, radius);
                    var direction = new Vector2(input.x, -input.y);
                    m_CoreInput.SetTouchMove(input);
                    if (thumb != null)
                    {
                        thumb.style.left = center.x + direction.x * radius - 23f;
                        thumb.style.top = center.y + direction.y * radius - 23f;
                    }
                };
                m_Bindings.Add(new PointerBinding(move,
                    evt => setMove(move.WorldToLocal(evt.position)),
                    evt => setMove(move.WorldToLocal(evt.position)),
                    () =>
                    {
                        setMove(move.contentRect.center);
                        m_Sprint?.Cancel();
                    }));
            }

            if (m_CoreInput != null)
            {
                BindHold("jump", m_CoreInput.SetTouchJump);
                BindHold("fire", m_CoreInput.SetTouchPrimaryAction);
            }
            if (m_ShooterInput != null)
            {
                BindHold("aim", m_ShooterInput.SetTouchAim);
                BindPress("reload", m_ShooterInput.TouchReload);
                BindPress("next-weapon", m_ShooterInput.TouchNextWeapon);
                BindPress("previous-weapon", m_ShooterInput.TouchPreviousWeapon);
            }
        }

        // UI coordinates grow downwards. Player look/move input grows upwards.
        internal static Vector2 ToLookInput(Vector2 uiDelta) =>
            new Vector2(uiDelta.x, -uiDelta.y) * 0.18f;

        internal static Vector2 ToMoveInput(Vector2 uiOffset, float radius) =>
            Vector2.ClampMagnitude(new Vector2(uiOffset.x, -uiOffset.y) / Mathf.Max(radius, 1f), 1f);

        private void BindHold(string name, Action<bool> action)
        {
            var element = m_Root.Q(name);
            if (element != null)
                m_Bindings.Add(new PointerBinding(element, _ => action(true), null, () => action(false)));
        }

        private void BindPress(string name, Action action)
        {
            var element = m_Root.Q(name);
            if (element != null)
                m_Bindings.Add(new PointerBinding(element, _ => action(), null, null));
        }

        public void SetGameplayEnabled(bool enabled)
        {
            CancelAll();
            foreach (var binding in m_Bindings) binding.Enabled = enabled;
        }

        public void CancelAll()
        {
            m_Sprint?.Cancel();
            foreach (var binding in m_Bindings) binding.Cancel();
        }

        private void OnHudDetached(DetachFromPanelEvent evt) => CancelAll();

        public void UpdateSafeArea()
        {
            // Stamina depletion and movement locks are still controlled by the native player manager.
            if (m_Sprint != null && m_Sprint.IsActive && m_CoreMovement != null && !m_CoreMovement.IsSprinting)
                m_Sprint.Cancel();
            if (!m_IsMobile || m_Root.panel == null || Screen.width <= 0 || Screen.height <= 0) return;
            var safe = Screen.safeArea;
            var screenSize = new Vector2(Screen.width, Screen.height);
            var panelSize = m_Root.layout.size;
            if (safe == m_SafeArea && screenSize == m_ScreenSize && panelSize == m_PanelSize) return;
            if (panelSize.x <= 0 || panelSize.y <= 0) return;
            CancelAll();
            m_SafeArea = safe;
            m_ScreenSize = screenSize;
            m_PanelSize = panelSize;
            float scaleX = panelSize.x / Screen.width;
            float scaleY = panelSize.y / Screen.height;
            foreach (var surface in new[] { m_Hud, m_Backpack })
            {
                if (surface == null) continue;
                surface.style.left = safe.xMin * scaleX;
                surface.style.right = (Screen.width - safe.xMax) * scaleX;
                surface.style.top = (Screen.height - safe.yMax) * scaleY;
                surface.style.bottom = safe.yMin * scaleY;
            }
        }

        public void Dispose()
        {
            CancelAll();
            m_Hud?.UnregisterCallback<DetachFromPanelEvent>(OnHudDetached);
            foreach (var binding in m_Bindings) binding.Dispose();
            m_Bindings.Clear();
            m_Root.Q("touch-look-surface")?.RemoveFromHierarchy();
        }

        internal sealed class SprintToggle
        {
            private readonly Action<bool> m_OnChanged;
            public bool IsActive { get; private set; }

            public SprintToggle(Action<bool> onChanged) => m_OnChanged = onChanged;

            public void Toggle()
            {
                IsActive = !IsActive;
                m_OnChanged?.Invoke(IsActive);
            }

            public void Cancel()
            {
                if (!IsActive) return;
                IsActive = false;
                m_OnChanged?.Invoke(false);
            }
        }

        private sealed class PointerBinding : IDisposable
        {
            private readonly VisualElement m_Element;
            private readonly Action<PointerDownEvent> m_Press;
            private readonly Action<PointerMoveEvent> m_Move;
            private readonly Action m_Release;
            private int m_PointerId = -1;
            public bool Enabled { get; set; } = true;

            public PointerBinding(VisualElement element, Action<PointerDownEvent> press,
                Action<PointerMoveEvent> move, Action release)
            {
                m_Element = element;
                m_Press = press;
                m_Move = move;
                m_Release = release;
                element.pickingMode = PickingMode.Position;
                element.RegisterCallback<PointerDownEvent>(OnDown);
                element.RegisterCallback<PointerMoveEvent>(OnMove);
                element.RegisterCallback<PointerUpEvent>(OnUp);
                element.RegisterCallback<PointerCancelEvent>(OnCancel);
                element.RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
                element.RegisterCallback<DetachFromPanelEvent>(OnDetach);
            }

            private void OnDown(PointerDownEvent evt)
            {
                if (!Enabled || evt.pointerType != PointerType.touch || m_PointerId != -1) return;
                m_PointerId = evt.pointerId;
                m_Element.CapturePointer(evt.pointerId);
                if (m_Element.ClassListContains("hud-control"))
                    m_Element.AddToClassList("hud-control--pressed");
                m_Press?.Invoke(evt);
                evt.StopPropagation();
            }

            private void OnMove(PointerMoveEvent evt)
            {
                if (evt.pointerId != m_PointerId) return;
                m_Move?.Invoke(evt);
                evt.StopPropagation();
            }

            private void OnUp(PointerUpEvent evt)
            {
                if (evt.pointerId != m_PointerId) return;
                Cancel();
                evt.StopPropagation();
            }

            private void OnCancel(PointerCancelEvent evt)
            {
                if (evt.pointerId == m_PointerId) Cancel();
            }

            private void OnCaptureOut(PointerCaptureOutEvent evt)
            {
                if (evt.pointerId == m_PointerId) Cancel();
            }

            private void OnDetach(DetachFromPanelEvent evt) => Cancel();

            public void Cancel()
            {
                if (m_PointerId == -1) return;
                int pointer = m_PointerId;
                m_PointerId = -1;
                m_Release?.Invoke();
                m_Element.RemoveFromClassList("hud-control--pressed");
                if (m_Element.HasPointerCapture(pointer)) m_Element.ReleasePointer(pointer);
            }

            public void Dispose()
            {
                Cancel();
                m_Element.UnregisterCallback<PointerDownEvent>(OnDown);
                m_Element.UnregisterCallback<PointerMoveEvent>(OnMove);
                m_Element.UnregisterCallback<PointerUpEvent>(OnUp);
                m_Element.UnregisterCallback<PointerCancelEvent>(OnCancel);
                m_Element.UnregisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
                m_Element.UnregisterCallback<DetachFromPanelEvent>(OnDetach);
            }
        }
    }
}
