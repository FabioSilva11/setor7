using System;
using System.IO;
using Blocks.Gameplay.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

/// <summary>Renders the real interface assets for review without building another player.</summary>
[InitializeOnLoad]
public static class Setor07InterfacePreview
{
    private const string PendingKey = "Setor07.InterfacePreview.Pending";
    private static double s_Started;
    private static int s_Frames;
    private static RenderTexture s_LobbyTexture;
    private static RenderTexture s_HudTexture;
    private static LobbyCharacterPreview s_Character;

    static Setor07InterfacePreview()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Capture()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            throw new InvalidOperationException("Interface capture requires a graphics device.");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
        SessionState.SetBool(PendingKey, false);
        try
        {
            QualitySettings.SetQualityLevel(0, true);
            var lobby = CreateDocument("Lobby review",
                "Assets/Blocks/MultiplayerSession/Scenes/QuickJoin/ShooterLobby.uxml", out s_LobbyTexture);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Core/Art/Models/LobbyCharacter.prefab");
            s_Character = new LobbyCharacterPreview(model, lobby.Q<Image>("lobby-character-preview"));
            s_Character.SetVisible(true);

            var hud = CreateDocument("HUD review", "Assets/Shooter/Scripts/Runtime/Framework/WeaponHUD.uxml",
                out s_HudTexture);
            hud.AddToClassList("touch-enabled");
            hud.Q("message-overlay").style.display = DisplayStyle.None;
            hud.Q<Label>("weapon-type-label").text = "RIFLE";
            hud.Q<Label>("ammo-label").text = "24 / 30";
            foreach (var name in new[] { "player-ammo-bar", "player-health-bar", "player-stamina-bar" })
            {
                var bar = hud.Q<ProgressBar>(name);
                if (bar != null) { bar.value = 80; bar.title = ""; }
            }
            s_Frames = 0;
            s_Started = EditorApplication.timeSinceStartup;
            EditorApplication.update += CaptureWhenReady;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static VisualElement CreateDocument(string name, string path, out RenderTexture texture)
    {
        texture = new RenderTexture(1280, 576, 0, RenderTextureFormat.ARGB32)
        {
            name = name + " 1280x576",
            antiAliasing = 1,
            useMipMap = false
        };
        texture.Create();
        var panel = Object.Instantiate(AssetDatabase.LoadAssetAtPath<PanelSettings>(
            "Assets/Core/Settings/PanelSettings.asset"));
        panel.targetTexture = texture;
        panel.clearColor = true;
        panel.colorClearValue = new Color(0.04f, 0.065f, 0.09f, 1f);
        var document = new GameObject(name).AddComponent<UIDocument>();
        document.panelSettings = panel;
        document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
        return document.rootVisualElement;
    }

    private static void CaptureWhenReady()
    {
        if (!EditorApplication.isPlaying) return;
        s_Frames++;
        if (s_Frames < 60 || EditorApplication.timeSinceStartup - s_Started < 5) return;
        EditorApplication.update -= CaptureWhenReady;
        try
        {
            Directory.CreateDirectory("Logs/InterfacePreview");
            Save(s_LobbyTexture, "Logs/InterfacePreview/lobby.png");
            Save(s_HudTexture, "Logs/InterfacePreview/hud.png");
            Debug.Log("SETOR07_INTERFACE_PREVIEW_CAPTURED 1280x576");
            s_Character?.Dispose();
            s_Started = EditorApplication.timeSinceStartup;
            EditorApplication.update += ExitAfterCleanup;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static void ExitAfterCleanup()
    {
        // Let the player loop destroy the animator and its graph before closing the Editor.
        if (EditorApplication.timeSinceStartup - s_Started < 0.5) return;
        EditorApplication.update -= ExitAfterCleanup;
        EditorApplication.Exit(0);
    }

    private static void Save(RenderTexture texture, string path)
    {
        var previous = RenderTexture.active;
        var image = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        try
        {
            RenderTexture.active = texture;
            image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
        }
    }
}
