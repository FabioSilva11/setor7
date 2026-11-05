using UnityEngine;
using UnityEngine.UI;

namespace Setor07.UI
{
    public enum InterfaceScreen { Lobby, Formation, Settings, Inventory, Store, Events, Vault }

    /// <summary>Inspector-driven visual review. Business actions intentionally have no listeners.</summary>
    [ExecuteAlways]
    public sealed class InterfacePresentation : MonoBehaviour
    {
        public InterfaceMockData mockData;
        public InterfaceScreen previewScreen;
        public GameObject[] screens;
        public Text operatorName, operatorRole, coins, diamonds, mode, formation;
        public Text[] volumeLabels;
        public Slider[] volumeSliders;

        private void OnEnable() { ApplyMockData(); ShowScreen(previewScreen); }
        private void OnValidate() { if (isActiveAndEnabled) { ApplyMockData(); ShowScreen(previewScreen); } }

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
