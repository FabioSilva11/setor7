using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Blocks.Gameplay.Core
{
    /// <summary>A visual-only model and small camera, alive only while the lobby is visible.</summary>
    public sealed class LobbyCharacterPreview : IDisposable
    {
        private const int PreviewLayer = 31;
        private readonly GameObject m_ModelPrefab;
        private readonly Image m_Image;
        private GameObject m_Root;
        private RenderTexture m_Texture;
        private bool m_Disposed;

        public bool IsVisible => m_Root != null && m_Root.activeSelf;

        public LobbyCharacterPreview(GameObject modelPrefab, Image image)
        {
            m_ModelPrefab = modelPrefab;
            m_Image = image;
            if (image != null)
            {
                image.pickingMode = PickingMode.Ignore;
                image.scaleMode = ScaleMode.ScaleToFit;
            }
        }

        public void SetVisible(bool visible)
        {
            if (m_Disposed || m_Image == null || m_ModelPrefab == null) return;
            if (!visible)
            {
                Release();
                return;
            }
            if (m_Root != null) return;

            // The asset is deliberately just an FBX model, its materials and an idle animator.
            // Reject accidental assignments of a player prefab before any Awake can execute.
            if (m_ModelPrefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0 ||
                m_ModelPrefab.GetComponentsInChildren<Collider>(true).Length != 0)
            {
                Debug.LogError("The lobby preview needs a visual-only model, without gameplay components.");
                return;
            }

            m_Root = new GameObject("Lobby character preview") { hideFlags = HideFlags.HideAndDontSave };
            m_Root.transform.position = new Vector3(0f, -10000f, 0f);
            var model = Object.Instantiate(m_ModelPrefab, m_Root.transform, false);
            model.name = "Lobby character (visual only)";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, -12f, 0f);
            foreach (var child in m_Root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = PreviewLayer;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
            foreach (var animator in model.GetComponentsInChildren<Animator>(true))
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.Rebind();
                animator.Update(0f);
            }

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            var bounds = new Bounds(model.transform.position + Vector3.up, Vector3.zero);
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            }
            float height = Mathf.Max(bounds.size.y, 1f);
            var cameraObject = new GameObject("Lobby preview camera") { layer = PreviewLayer };
            cameraObject.transform.SetParent(m_Root.transform, false);
            var camera = cameraObject.AddComponent<Camera>();
            camera.cullingMask = 1 << PreviewLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.orthographic = true;
            // Skinned local bounds also include the wide T-pose, which would make an idle model tiny.
            camera.orthographicSize = height * 0.57f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = height * 8f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.useOcclusionCulling = false;
            camera.depth = -10f;
            cameraObject.transform.position = bounds.center + Vector3.forward * height * 3f;
            cameraObject.transform.LookAt(bounds.center);
            var data = camera.GetUniversalAdditionalCameraData();
            data.SetRenderer(0);
            data.renderPostProcessing = false;
            data.renderShadows = false;
            data.requiresColorTexture = false;
            data.requiresDepthTexture = false;
            data.volumeLayerMask = 0;
            data.allowXRRendering = false;

            AddLight("Lobby key light", new Vector3(1.6f, 2.8f, 3f), new Color(1f, 0.91f, 0.79f), 8f);
            AddLight("Lobby fill light", new Vector3(-1.8f, 1.6f, 1.5f), new Color(0.66f, 0.81f, 1f), 4f);

            m_Texture = new RenderTexture(384, 768, 16, RenderTextureFormat.ARGB32)
            {
                name = "Lobby character 384x768",
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave
            };
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) m_Texture.Create();
            camera.targetTexture = m_Texture;
            m_Image.image = m_Texture;
        }

        private void AddLight(string name, Vector3 localPosition, Color color, float intensity)
        {
            var lightObject = new GameObject(name) { layer = PreviewLayer };
            lightObject.transform.SetParent(m_Root.transform, false);
            lightObject.transform.localPosition = localPosition;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.cullingMask = 1 << PreviewLayer;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        private void Release()
        {
            if (m_Image != null) m_Image.image = null;
            if (m_Root != null)
            {
                m_Root.SetActive(false);
                Destroy(m_Root);
                m_Root = null;
            }
            if (m_Texture != null)
            {
                m_Texture.Release();
                Destroy(m_Texture);
                m_Texture = null;
            }
        }

        private static void Destroy(Object value)
        {
            if (Application.isPlaying) Object.Destroy(value);
            else Object.DestroyImmediate(value);
        }

        public void Dispose()
        {
            Release();
            m_Disposed = true;
        }
    }
}
