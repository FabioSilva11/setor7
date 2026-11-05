using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Setor07.UI
{
    /// <summary>Loads only the visual prefab; never starts the existing gameplay or networking systems.</summary>
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

        private void SetProgress(float progress)
        {
            if (progressFill != null) progressFill.fillAmount = progress;
            if (loadingLabel != null) loadingLabel.text = "CARREGANDO RECURSOS... " + Mathf.RoundToInt(progress * 100) + "%";
        }
    }
}
