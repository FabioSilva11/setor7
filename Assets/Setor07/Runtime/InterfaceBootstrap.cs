using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Setor07.UI
{
    /// <summary>Loads the lobby and connects its Solo launch to the existing gameplay.</summary>
    public sealed class InterfaceBootstrap : MonoBehaviour
    {
        public string resourcePath = "Setor07/Interface";
        [Min(0)] public float minimumDisplaySeconds = 1.5f;
        public Canvas canvas;
        public GameObject loadingScreen;
        public Text loadingLabel;
        public Image progressFill;
        public Image optionalBackground;
        public Sprite loadingBackground;
        public InterfacePresentation Presentation { get; private set; }
        public bool IsReady { get; private set; }
        private GameplayLaunchController launcher;

        private void Awake()
        {
            // Keep this in the lobby scene; Shooter has its own EventSystem.
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var input = new GameObject("Setor07 UI Input", typeof(EventSystem), typeof(InputSystemUIInputModule));
                input.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
        }

        private IEnumerator Start()
        {
            IsReady = false;
            loadingScreen.SetActive(true);
            if (optionalBackground != null)
            {
                optionalBackground.sprite = loadingBackground;
                optionalBackground.enabled = loadingBackground != null;
            }
            double began = Time.realtimeSinceStartupAsDouble;
            SetProgress(0);
            // Guarantee a rendered loading frame before loading or instantiating the interface.
            yield return null;
            var request = Resources.LoadAsync<GameObject>(resourcePath);
            while (!request.isDone) { SetProgress(request.progress * .8f); yield return null; }
            var prefab = request.asset as GameObject;
            if (prefab == null)
            {
                loadingLabel.text = "NÃO FOI POSSÍVEL CARREGAR A INTERFACE";
                Debug.LogError("Setor07: missing visual interface resource at " + resourcePath);
                yield break;
            }
            var instance = Instantiate(prefab, canvas.transform, false);
            var group = instance.GetComponent<CanvasGroup>();
            // Hide in the same synchronous step as Instantiate, before any frame can render it.
            group.alpha = 0; group.interactable = false; group.blocksRaycasts = false;
            loadingScreen.transform.SetAsLastSibling();
            Presentation = instance.GetComponent<InterfacePresentation>();
            Presentation.ApplyMockData();
            Presentation.BindInteractions();
            launcher = gameObject.AddComponent<GameplayLaunchController>();
            launcher.Initialize(this);
            Presentation.PlayRequested += HandlePlayRequested;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)instance.transform);
            SetProgress(.95f);
            yield return null;
            while (Time.realtimeSinceStartupAsDouble - began < minimumDisplaySeconds) yield return null;
            SetProgress(1);
            yield return null;
            group.alpha = 1; group.interactable = true; group.blocksRaycasts = true;
            IsReady = true;
            loadingScreen.SetActive(false);
        }

        private void HandlePlayRequested() => launcher.TryLaunch();

        private void OnDestroy()
        {
            if (Presentation != null) Presentation.PlayRequested -= HandlePlayRequested;
        }

        internal void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (progressFill != null) progressFill.fillAmount = progress;
            if (loadingLabel != null) loadingLabel.text = "CARREGANDO RECURSOS... " + Mathf.RoundToInt(progress * 100) + "%";
        }
    }
}
