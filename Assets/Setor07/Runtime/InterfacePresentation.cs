using UnityEngine;
using UnityEngine.UI;

namespace Setor07.UI
{
    public enum InterfaceScreen { Lobby, Formation, Settings, Inventory, Store, Events, Vault }

    /// <summary>Inspector preview with runtime bindings for Solo formation and gameplay launch.</summary>
    [ExecuteAlways]
    public sealed class InterfacePresentation : MonoBehaviour
    {
        public InterfaceMockData mockData;
        public InterfaceScreen previewScreen;
        public GameObject[] screens;
        public Text operatorName, operatorRole, coins, diamonds, mode, formation;
        public Text[] volumeLabels;
        public Slider[] volumeSliders;
        public Button formationButton, formationCloseButton, soloButton, duoButton, squadButton, playButton;

        public event System.Action PlayRequested;

        private bool interactionsRequested;
        private bool launchInteractable = true;

        private void OnEnable()
        {
            ApplyMockData();
            ShowScreen(previewScreen);
            if (Application.isPlaying && interactionsRequested) BindInteractions();
        }
        private void OnDisable() { UnbindInteractions(); }
        private void OnValidate() { if (isActiveAndEnabled) { ApplyMockData(); ShowScreen(previewScreen); } }

        public void BindInteractions()
        {
            interactionsRequested = true;
            UnbindInteractions();
            if (formationButton != null) formationButton.onClick.AddListener(OpenFormation);
            if (formationCloseButton != null) formationCloseButton.onClick.AddListener(CloseFormation);
            if (soloButton != null) soloButton.onClick.AddListener(SelectSolo);
            if (playButton != null) playButton.onClick.AddListener(RequestPlay);
            SetLaunchInteractable(launchInteractable);
        }

        public void SetLaunchInteractable(bool interactable)
        {
            launchInteractable = interactable;
            if (formationButton != null) formationButton.interactable = interactable;
            if (formationCloseButton != null) formationCloseButton.interactable = interactable;
            if (soloButton != null) soloButton.interactable = interactable;
            if (playButton != null) playButton.interactable = interactable;
            if (duoButton != null) duoButton.interactable = false;
            if (squadButton != null) squadButton.interactable = false;
        }

        private void UnbindInteractions()
        {
            if (formationButton != null) formationButton.onClick.RemoveListener(OpenFormation);
            if (formationCloseButton != null) formationCloseButton.onClick.RemoveListener(CloseFormation);
            if (soloButton != null) soloButton.onClick.RemoveListener(SelectSolo);
            if (playButton != null) playButton.onClick.RemoveListener(RequestPlay);
        }

        private void OpenFormation()
        {
            if (launchInteractable && isActiveAndEnabled) ShowScreen(InterfaceScreen.Formation);
        }

        private void CloseFormation()
        {
            if (launchInteractable && isActiveAndEnabled) ShowScreen(InterfaceScreen.Lobby);
        }

        private void SelectSolo()
        {
            if (!launchInteractable || !isActiveAndEnabled) return;
            Set(formation, "SOLO");
            ShowScreen(InterfaceScreen.Lobby);
        }

        private void RequestPlay()
        {
            if (launchInteractable && isActiveAndEnabled) PlayRequested?.Invoke();
        }

        public void ShowScreen(InterfaceScreen screen)
        {
            previewScreen = screen;
            if (screens == null) return;
            for (int i = 0; i < screens.Length; i++)
                if (screens[i] != null) screens[i].SetActive(i == 0 || i == (int)screen);
        }

        public void ApplyMockData()
        {
            if (mockData == null) return;
            Set(operatorName, mockData.operatorName); Set(operatorRole, mockData.role);
            Set(coins, mockData.coins); Set(diamonds, mockData.diamonds);
            Set(mode, mockData.mode); Set(formation, mockData.formation);
            int[] values = { mockData.masterVolume, mockData.gameVolume, mockData.chatVolume };
            for (int i = 0; i < values.Length; i++)
            {
                if (volumeLabels != null && i < volumeLabels.Length) Set(volumeLabels[i], values[i] + "%");
                if (volumeSliders != null && i < volumeSliders.Length && volumeSliders[i] != null)
                    volumeSliders[i].SetValueWithoutNotify(values[i] / 100f);
            }
        }

        private static void Set(Text target, string value) { if (target != null) target.text = value; }
    }
}
