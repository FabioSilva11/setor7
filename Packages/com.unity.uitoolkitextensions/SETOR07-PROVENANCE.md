# Embedded Unity UI Toolkit Extensions

Upstream: https://github.com/Unity-UI-Extensions/com.unity.uitoolkitextensions

Version: 1.0.0

Pinned upstream commit: `9144c4ca6dec55f78a6197a2ce1944ea11dc98a6`

License: MIT; original notice is retained in `LICENSE`.

This is a project-local embedded Unity package. It does not require a Git download to open or build this project. Runtime and Editor source files are unchanged from the pinned upstream version.

Local import repairs:

- `Runtime/Controls/DropDownControl.meta`: removed a second concatenated YAML document after `assetBundleVariant`; preserved GUID `5f67aa49e3aa4a548a7d2574c5fd3f6a`.
- `Runtime/Controls/ImageCropOverlayControl.meta`: removed the same duplicate YAML content; preserved GUID `8ec0e7d11d7b4763ae20a1c0e5f7d5c2`.

Repository administration files and the optional external `Examples~` submodule are excluded. The game uses `CollapsibleSection` for the lobby group panel; its player styles are defined explicitly in the project's `QuickJoin.uss`.
