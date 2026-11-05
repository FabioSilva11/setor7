using System;
using Setor07.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Setor07.Editor
{
    /// <summary>Connects the existing interface prefab without rebuilding its visual hierarchy.</summary>
    public static class InterfaceGameplaySetup
    {
        [MenuItem("Setor 07/Connect Solo interface interactions")]
        public static void Apply()
        {
            var root = PrefabUtility.LoadPrefabContents(InterfaceBuilder.PrefabPath);
            try
            {
                var view = root.GetComponent<InterfacePresentation>();
                if (view == null) throw new InvalidOperationException("The interface prefab has no presentation component.");

                const string launch = "Lobby/Launch controls/";
                const string formation = "Formation/Formation popover/Panel/";
                view.formationButton = RequireButton(root.transform, launch + "Formation · mock");
                view.playButton = RequireButton(root.transform, launch + "Jogar · future action");
                view.soloButton = RequireButton(root.transform, formation + "Solo · mock");
                view.duoButton = RequireButton(root.transform, formation + "Duo · mock");
                view.squadButton = RequireButton(root.transform, formation + "Squad · mock");

                var close = RequireTransform(root.transform, formation + "Close · mock");
                view.formationCloseButton = close.GetComponent<Button>();
                if (view.formationCloseButton == null) view.formationCloseButton = close.gameObject.AddComponent<Button>();
                var closeGraphic = close.GetComponent<Graphic>();
                if (closeGraphic != null) closeGraphic.raycastTarget = true;
                view.formationCloseButton.targetGraphic = closeGraphic;
                view.formationCloseButton.transition = Selectable.Transition.None;
                view.formationCloseButton.navigation = new Navigation { mode = Navigation.Mode.None };
                view.SetLaunchInteractable(true);

                var group = root.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }

                PrefabUtility.SaveAsPrefabAsset(root, InterfaceBuilder.PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            Debug.Log("SETOR07_SOLO_INTERFACE_CONNECTED " + InterfaceBuilder.PrefabPath);
        }

        private static Button RequireButton(Transform root, string path)
        {
            var button = RequireTransform(root, path).GetComponent<Button>();
            if (button == null) throw new InvalidOperationException("The interface control has no Button: " + path);
            return button;
        }

        private static Transform RequireTransform(Transform root, string path)
        {
            var target = root.Find(path);
            if (target == null) throw new InvalidOperationException("The interface control is missing: " + path);
            return target;
        }
    }
}
