# Unity UI Extensions — pinned upstream package

Source: https://github.com/Unity-UI-Extensions/com.unity.uiextensions
Branch: release
Commit: d47f838ba918bbb9b26f242bd944940a711f525f
Package version: 3.0.0
Retrieved: 2026-11-05

Embedded locally for reproducible offline imports. Repository automation and Git metadata are omitted. The package's BSD-3-Clause license is preserved in LICENSE.md. See package.json for upstream requirements (Unity 6000.0+, com.unity.ugui 2.0.0+). The project requests uGUI 2.5.0; Unity 6000.6 resolves its core uGUI package to 2.6.0. Compatibility patches are listed below.

## Local compatibility patch

The upstream `using UnityEditor;` imports are wrapped in `#if UNITY_EDITOR` in these runtime files so player compilation can resolve the package without an editor assembly:

- Runtime/Scripts/Controls/UIGraphicSector/Rendering.cs
- Runtime/Scripts/Controls/UIGraphicSector/Layout.cs
- Runtime/Scripts/Controls/UICircleSegmented/UICircleSegmentedCurve.cs

The actual EditorUtility calls in these files already have upstream editor guards. These three import guards do not change runtime behavior. Additional compatibility changes are listed below.

## uGUI 2.6 interface compatibility

Unity 6000.6 ships uGUI 2.6.0 as a core package, overriding this project's manifest 2.5.0 request. uGUI 2.6 adds `ILayoutElement.maxWidth` and `maxHeight`. `Runtime/UnityUIExtensions.asmdef` defines `UNITY_UI_EXTENSIONS_UGUI_2_6_OR_NEWER` for com.unity.ugui >=2.6.0, and `UIPrimitiveBase.cs` supplies those two properties only for that package range. Both return `LayoutUtility.DefaultMaxSize`, matching uGUI Image's unconstrained maximum sizing. Earlier uGUI versions retain upstream behavior and compile without the new API. This addresses CS0535 observed during Unity compilation in Logs/Setor07/red-tests.log.

## Additional Unity 6000.6 API compatibility

- `Runtime/Scripts/Utilities/ExtensionMethods.cs`: for Unity 6000.6+, the prefab candidate check uses `GetEntityId().IsValid()` in place of the obsolete-error `GetInstanceID() >= 0` heuristic. The scene validity, scene loading and hidden-object checks remain unchanged; EntityId is not a signed legacy instance ID.
- `Runtime/Scripts/Controls/ColorPicker/ColorPickerPresets.cs`: reset-generated preference keys use `GetEntityId().ToString()` on Unity 6000.6+, retaining the legacy implementation below that version. Existing serialized keys are unchanged.
- `Runtime/Scripts/Layout/TableLayoutGroup.cs` and `FlowLayoutGroup.cs`: for uGUI 2.6 only, a private compatibility overload maps upstream's four layout inputs to the new five-argument API, inserting `LayoutUtility.DefaultMaxSize` as the maximum. Existing min/preferred/flexible values and axis order are preserved.
- The package-version expression uses Unity-supported finite range `[2.6.0,3.0.0)`; root's Unity resolver rejected the open-ended range.

These changes address errors observed in `Logs/Setor07/render.log`. Root adds the engine ScreenCapture module dependency in the project manifest for upstream ColorSampler.

## Verification

On 2026-11-05, the package Runtime sources compiled successfully with Unity's installed Roslyn compiler and Unity-generated response file, with output redirected to a temporary directory and the installed UnityEngine.ScreenCaptureModule added as a reference. Exit code 0; 11 upstream deprecation/serialization warnings remain. This isolated check does not replace the parent task's full Unity import, scene generation and rendering verification.
The Editor assembly also compiled successfully against that temporary Runtime reference assembly: exit code 0; five upstream deprecation warnings remain.
