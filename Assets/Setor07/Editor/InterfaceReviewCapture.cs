using System;
using System.IO;
using Setor07.UI;
using UnityEditor;
using UnityEngine;

namespace Setor07.Editor
{
    /// <summary>Captures real Unity camera/Canvas frames, including the actual runtime loading path.</summary>
    [InitializeOnLoad]
    public static class InterfaceReviewCapture
    {
        private const string Key = "Setor07.Capture.Pending";
        private static InterfaceBootstrap boot;
        private static Camera camera;
        private static RenderTexture texture;
        private static double changed;
        private static double bootStarted;
        private static int frame, screenIndex;
        private static bool savedLoading, wide;
        private const string Folder = "Logs/Setor07/Captures";

        static InterfaceReviewCapture() { EditorApplication.playModeStateChanged += Changed; }

        public static void Run()
        {
            InterfaceBuilder.Build();
            SessionState.SetBool(Key, true);
            EditorApplication.EnterPlaymode();
        }

        private static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Key, false)) return;
            SessionState.SetBool(Key, false);
            Directory.CreateDirectory(Folder);
            boot = UnityEngine.Object.FindFirstObjectByType<InterfaceBootstrap>();
            if (boot == null) { Fail("Missing startup bootstrap."); return; }
            boot.minimumDisplaySeconds = 5;
            camera = boot.canvas.worldCamera;
            SetSize(1280, 720);
            frame = 0; screenIndex = 0; savedLoading = false; wide = false;
            changed = EditorApplication.timeSinceStartup;
            bootStarted = changed;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            frame++;
            try
            {
                if (EditorApplication.timeSinceStartup - changed > 100 && !boot.IsReady)
                { Fail("Interface loading timed out."); return; }
                if (!savedLoading && frame >= 3 && !boot.IsReady)
                {
                    Save("00-loading.png"); savedLoading = true;
                }
                if (!boot.IsReady)
                {
                    if (!boot.loadingScreen.activeSelf) throw new InvalidOperationException("Loading dismissed before readiness.");
                    if (boot.Presentation != null && boot.Presentation.GetComponent<CanvasGroup>().alpha != 0)
                        throw new InvalidOperationException("Interface visible before readiness.");
                    return;
                }
                if (EditorApplication.timeSinceStartup - bootStarted < 4.9)
                    throw new InvalidOperationException("Minimum loading display time was ignored.");
                if (boot.Presentation == null || boot.loadingScreen.activeSelf)
                    throw new InvalidOperationException("Loading readiness contract violated.");
                if (screenIndex == 0 && frame > 0)
                {
                    boot.Presentation.ShowScreen(InterfaceScreen.Lobby);
                    changed = EditorApplication.timeSinceStartup; frame = -10; screenIndex = 1; return;
                }
                if (frame < 3 || EditorApplication.timeSinceStartup - changed < .4) return;
                if (!wide)
                {
                    var screen = (InterfaceScreen)(screenIndex - 1);
                    Save(screenIndex.ToString("00") + "-" + screen.ToString().ToLowerInvariant() + ".png");
                    if (screenIndex < 7)
                    {
                        boot.Presentation.ShowScreen((InterfaceScreen)screenIndex);
                        screenIndex++; changed = EditorApplication.timeSinceStartup; frame = -3; return;
                    }
                    wide = true; boot.Presentation.ShowScreen(InterfaceScreen.Lobby);
                    SetSize(1600, 720); changed = EditorApplication.timeSinceStartup; frame = -3; return;
                }
                Save("08-lobby-wide.png");
                if (!savedLoading) throw new InvalidOperationException("No loading frame was captured.");
                File.WriteAllText(Path.Combine(Folder, "verification.txt"),
                    "Actual runtime Resources.LoadAsync + Canvas layout completed.\nLoading hidden only after IsReady.\n7 native screen states captured at 1280x720, lobby at 1600x720.\n");
                Debug.Log("SETOR07_REAL_RENDER_CAPTURED " + Path.GetFullPath(Folder));
                EditorApplication.update -= Tick;
                camera.targetTexture = null;
                texture.Release(); UnityEngine.Object.Destroy(texture);
                EditorApplication.Exit(0);
            }
            catch (Exception ex) { Fail(ex.ToString()); }
        }

        private static void SetSize(int width, int height)
        {
            if (texture != null) { camera.targetTexture = null; texture.Release(); UnityEngine.Object.Destroy(texture); }
            texture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            texture.Create(); camera.targetTexture = texture;
            // Match the normal canvas sizing to the target camera's actual render dimensions.
            var scaler = boot.canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.enabled = false; boot.canvas.scaleFactor = height / 720f;
        }

        private static void Save(string name)
        {
            Canvas.ForceUpdateCanvases();
            var previous = RenderTexture.active;
            var pixels = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); pixels.Apply();
                File.WriteAllBytes(Path.Combine(Folder, name), pixels.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(pixels); }
        }

        private static void Fail(string message)
        {
            EditorApplication.update -= Tick;
            Debug.LogError("SETOR07_CAPTURE_FAILED " + message); EditorApplication.Exit(1);
        }
    }
}
