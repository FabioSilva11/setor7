using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Setor07.Editor
{
    /// <summary>Builds an editable, visual-only hangar and the project's unarmed lobby model.</summary>
    public static class WarehouseStageBuilder
    {
        private const string MaterialFolder = "Assets/Setor07/Art/Materials";
        private const string CharacterPath = "Assets/Core/Art/Models/LobbyCharacter.prefab";

        public static Camera Build()
        {
            EnsureFolder(MaterialFolder);
            var stage = new GameObject("Warehouse Stage - visual only").transform;
            var structure = new GameObject("Steel structure").transform;
            var details = new GameObject("Hangar details").transform;
            var lighting = new GameObject("Warehouse lighting").transform;
            structure.SetParent(stage, false);
            details.SetParent(stage, false);
            lighting.SetParent(stage, false);

            var concrete = Material("Concrete", "303a40", 0f, 0.24f);
            var steel = Material("Structural steel", "27333d", 0.65f, 0.4f);
            var darkSteel = Material("Dark steel", "14212a", 0.5f, 0.32f);
            var wall = Material("Wall panels", "344650", 0.25f, 0.22f);
            var door = Material("Shutter slats", "2b3d46", 0.5f, 0.34f);
            var rubber = Material("Rubber", "10191d", 0f, 0.12f);
            var paint = Material("Pale floor paint", "9eaa97", 0f, 0.18f);
            var yellow = Material("Hazard ochre", "9f9658", 0.05f, 0.25f);
            var crate = Material("Olive cargo cases", "424c43", 0.2f, 0.32f);
            var cratePanel = Material("Cargo inset panels", "293630", 0.4f, 0.3f);
            var whiteLight = Material("Cool ceiling strips", "aac4c5", 0f, 0f, 3.5f);
            var amberLight = Material("Warm work lamps", "d5af72", 0f, 0f, 3f);
            var limeLight = Material("Lime guidance", "b0c77a", 0f, 0f, 2.1f);

            Box(structure, "Concrete floor", new Vector3(0, -0.16f, 10), new Vector3(24, 0.3f, 38), concrete);
            Box(structure, "Rear wall", new Vector3(0, 3.7f, 18), new Vector3(19, 7.4f, 0.35f), wall);
            Box(structure, "Left wall", new Vector3(-9.4f, 3.7f, 8), new Vector3(0.3f, 7.4f, 20), wall);
            Box(structure, "Right wall", new Vector3(9.4f, 3.7f, 8), new Vector3(0.3f, 7.4f, 20), wall);
            Box(structure, "Ceiling", new Vector3(0, 7.25f, 9), new Vector3(19, 0.2f, 22), darkSteel);

            // Receding portals and diagonal trusses give the backdrop depth at the lobby camera.
            foreach (float z in new[] { 1f, 6f, 11f, 16f })
            {
                foreach (float x in new[] { -8.9f, 8.9f })
                {
                    Box(structure, "Column", new Vector3(x, 3.45f, z), new Vector3(0.3f, 6.9f, 0.42f), steel);
                    Box(details, "Column base plate", new Vector3(x, 0.09f, z), new Vector3(0.65f, 0.18f, 0.75f), darkSteel);
                    Box(details, "Column guard", new Vector3(x, 0.6f, z - 0.25f), new Vector3(0.4f, 1.2f, 0.12f), yellow);
                }
                Box(structure, "Roof crossbeam", new Vector3(0, 6.75f, z), new Vector3(18, 0.25f, 0.3f), steel);
                Box(structure, "Truss lower chord", new Vector3(0, 5.85f, z), new Vector3(17.6f, 0.12f, 0.15f), steel);
                for (int n = 0; n < 8; n++)
                {
                    float x = -8.8f + n * 2.2f;
                    Beam(structure, "Truss diagonal", new Vector3(x, n % 2 == 0 ? 5.85f : 6.75f, z),
                        new Vector3(x + 2.2f, n % 2 == 0 ? 6.75f : 5.85f, z), 0.085f, steel);
                }
            }
            foreach (float x in new[] { -6f, -3f, 0f, 3f, 6f })
                Box(structure, "Longitudinal ceiling rail", new Vector3(x, 6.98f, 8.5f), new Vector3(0.12f, 0.15f, 21), steel);

            // Wide corrugated shutter, inset into a substantial loading-bay frame.
            Box(details, "Loading bay recess", new Vector3(0, 2.75f, 17.67f), new Vector3(11.6f, 5.5f, 0.2f), rubber);
            Box(details, "Shutter", new Vector3(0, 2.65f, 17.48f), new Vector3(10.7f, 5.3f, 0.16f), door);
            for (int i = 0; i < 16; i++)
                Box(details, "Shutter horizontal rib", new Vector3(0, 0.16f + i * 0.33f, 17.36f), new Vector3(10.7f, 0.035f, 0.06f), darkSteel);
            foreach (float x in new[] { -5.55f, 5.55f })
            {
                Box(details, "Loading door frame", new Vector3(x, 2.85f, 17.3f), new Vector3(0.35f, 5.7f, 0.5f), steel);
                Box(details, "Dock guidance light", new Vector3(x, 2.3f, 17.02f), new Vector3(0.045f, 2.8f, 0.04f), limeLight);
            }
            Box(details, "Loading door lintel", new Vector3(0, 5.6f, 17.24f), new Vector3(11.7f, 0.35f, 0.6f), darkSteel);
            Box(details, "Door overhead lamp", new Vector3(0, 5.35f, 16.91f), new Vector3(3.4f, 0.07f, 0.1f), whiteLight);

            // Utility wall panels, mezzanine rails and high windows provide a readable warehouse silhouette.
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    float z = 2.5f + i * 3.1f;
                    Box(details, "Wall panel seam", new Vector3(side * 9.18f, 2.5f, z), new Vector3(0.05f, 4.8f, 0.07f), darkSteel);
                    Box(details, "High window frame", new Vector3(side * 9.13f, 5.05f, z), new Vector3(0.11f, 1.05f, 2.45f), darkSteel);
                    Box(details, "High window glow", new Vector3(side * 9.05f, 5.05f, z), new Vector3(0.04f, 0.85f, 2.2f), whiteLight);
                    Box(details, "Window mullion", new Vector3(side * 8.99f, 5.05f, z), new Vector3(0.05f, 0.9f, 0.065f), steel);
                }
                Box(details, "Wall service pipe", new Vector3(side * 9.05f, 3.65f, 8.3f), new Vector3(0.14f, 0.14f, 18), darkSteel);
                Box(details, "Wall service rail", new Vector3(side * 9.02f, 0.95f, 8.3f), new Vector3(0.18f, 0.09f, 18), steel);
                Box(details, "Rear machinery cabinet", new Vector3(side * 7.2f, 1.3f, 17.4f), new Vector3(1.8f, 2.6f, 0.45f), darkSteel);
                Box(details, "Cabinet inset", new Vector3(side * 7.2f, 1.65f, 17.13f), new Vector3(1.45f, 1.55f, 0.08f), steel);
                Box(details, "Cabinet status light", new Vector3(side * 6.85f, 2.03f, 17.07f), new Vector3(0.07f, 0.07f, 0.04f), limeLight);
            }

            // Fine floor seams and a restrained painted staging box anchor the feet naturally.
            for (int i = -4; i <= 7; i++)
                Box(details, "Concrete expansion joint", new Vector3(0, 0.001f, i * 2.5f), new Vector3(20, 0.006f, 0.018f), darkSteel);
            foreach (float x in new[] { -7f, -3.5f, 3.5f, 7f })
                Box(details, "Concrete slab seam", new Vector3(x, 0.003f, 8), new Vector3(0.015f, 0.006f, 32), darkSteel);
            foreach (float x in new[] { -1.25f, 1.25f })
            {
                Box(details, "Operator staging line", new Vector3(x, 0.008f, 0.5f), new Vector3(0.035f, 0.012f, 2.8f), paint);
                Box(details, "Staging front corner", new Vector3(x * 0.8f, 0.009f, -0.9f), new Vector3(0.52f, 0.012f, 0.035f), paint);
            }
            foreach (float x in new[] { -4.8f, 4.8f })
                Box(details, "Warehouse traffic lane", new Vector3(x, 0.008f, 9.7f), new Vector3(0.06f, 0.012f, 20), yellow);
            for (int i = 0; i < 14; i++)
                Box(details, "Rear caution stripe", new Vector3(-5.15f + i * 0.8f, 0.009f, 16.5f), new Vector3(0.42f, 0.012f, 0.36f), yellow,
                    Quaternion.Euler(0, -35, 0));

            Cargo(details, new Vector3(-4.6f, 0, 6.3f), new Vector3(1.7f, 1.1f, 1.05f), crate, cratePanel, steel);
            Cargo(details, new Vector3(-4.3f, 1.14f, 6.4f), new Vector3(1.2f, 0.8f, 0.95f), crate, cratePanel, steel);
            Cargo(details, new Vector3(-6.5f, 0, 7.2f), new Vector3(1.5f, 1.5f, 1.3f), crate, cratePanel, steel);
            Cargo(details, new Vector3(4.8f, 0, 9.6f), new Vector3(2.1f, 1.25f, 1.2f), crate, cratePanel, steel);
            Cargo(details, new Vector3(6.9f, 0, 10.9f), new Vector3(1.3f, 1.8f, 1.3f), crate, cratePanel, steel);

            foreach (float z in new[] { 2.5f, 8f, 13.5f })
            {
                foreach (float x in new[] { -4.4f, 4.4f })
                {
                    Box(details, "Ceiling fixture housing", new Vector3(x, 6.4f, z), new Vector3(0.38f, 0.12f, 2.4f), darkSteel);
                    Box(details, "Ceiling light strip", new Vector3(x, 6.32f, z), new Vector3(0.25f, 0.035f, 2.2f), whiteLight);
                    AddLight(lighting, "Overhead cool light", LightType.Point, new Vector3(x, 5.7f, z), new Color(0.62f, 0.75f, 0.82f), 5f, 8f);
                }
            }
            foreach (float x in new[] { -7.6f, 7.6f })
            {
                Box(details, "Amber dock lamp", new Vector3(x, 3.35f, 17.05f), new Vector3(0.28f, 0.2f, 0.15f), amberLight);
                AddLight(lighting, "Dock warm light", LightType.Point, new Vector3(x, 3.3f, 16.4f), new Color(1, 0.72f, 0.4f), 4f, 7f);
            }
            AddLight(lighting, "Operator warm key", LightType.Spot, new Vector3(-2.6f, 4.1f, -3.1f), new Color(1, 0.91f, 0.79f), 13f, 12f,
                new Vector3(0, 1.1f, 0));
            AddLight(lighting, "Operator cool fill", LightType.Point, new Vector3(2.3f, 2.4f, -1.7f), new Color(0.63f, 0.77f, 0.9f), 4f, 6f);
            AddLight(lighting, "Operator rim", LightType.Spot, new Vector3(1.7f, 3.4f, 2.5f), new Color(0.77f, 0.88f, 0.6f), 15f, 8f,
                new Vector3(0, 1, 0));
            var sun = AddLight(lighting, "Warehouse ambient direction", LightType.Directional, new Vector3(0, 6, 0), new Color(0.63f, 0.73f, 0.8f), 0.65f, 0f);
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.21f, 0.25f);
            RenderSettings.ambientEquatorColor = new Color(0.10f, 0.14f, 0.17f);
            RenderSettings.ambientGroundColor = new Color(0.07f, 0.085f, 0.09f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.055f, 0.085f, 0.11f);
            RenderSettings.fogDensity = 0.024f;
            RenderSettings.skybox = null;

            CreateCharacter(stage);
            var cameraObject = new GameObject("Warehouse Lobby Camera");
            cameraObject.transform.SetParent(stage, false);
            cameraObject.transform.position = new Vector3(0, 1.4f, -5);
            cameraObject.transform.LookAt(new Vector3(0, 1.04f, 0));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 32f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.allowHDR = true;
            camera.allowMSAA = true;
            AssetDatabase.SaveAssets();
            return camera;
        }

        private static void CreateCharacter(Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
            if (prefab == null) throw new InvalidOperationException("Missing unarmed lobby model: " + CharacterPath);
            if (prefab.GetComponentsInChildren<MonoBehaviour>(true).Length != 0 || prefab.GetComponentsInChildren<Collider>(true).Length != 0)
                throw new InvalidOperationException("LobbyCharacter must contain visual components only.");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            model.name = "Unarmed Operator - idle visual";
            // FBX faces +Z. The hangar camera looks toward +Z from the opposite side.
            model.transform.localRotation = Quaternion.Euler(0, 168f, 0);
            model.transform.localPosition = Vector3.zero;
            foreach (var animator in model.GetComponentsInChildren<Animator>(true))
            {
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.Rebind();
                animator.Update(0f);
            }
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new InvalidOperationException("Lobby model has no renderers.");
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            model.transform.localScale *= 2f / Mathf.Max(0.01f, bounds.size.y);
            bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            model.transform.position -= new Vector3(bounds.center.x, bounds.min.y - 0.015f, bounds.center.z);
            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Quaternion? rotation = null)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            item.transform.localRotation = rotation ?? Quaternion.identity;
            item.GetComponent<MeshRenderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
            item.isStatic = true;
            return item;
        }

        private static void Beam(Transform parent, string name, Vector3 start, Vector3 end, float width, Material material)
        {
            var direction = end - start;
            Box(parent, name, (start + end) * 0.5f, new Vector3(width, direction.magnitude, width), material,
                Quaternion.FromToRotation(Vector3.up, direction.normalized));
        }

        private static void Cargo(Transform parent, Vector3 floor, Vector3 size, Material body, Material inset, Material frame)
        {
            var group = new GameObject("Stackable cargo case").transform;
            group.SetParent(parent, false);
            group.localPosition = floor;
            Box(group, "Cargo body", Vector3.up * size.y * 0.5f, size, body);
            Box(group, "Cargo front inset", new Vector3(0, size.y * 0.5f, -size.z * 0.5f - 0.012f), new Vector3(size.x * 0.8f, size.y * 0.66f, 0.04f), inset);
            foreach (float x in new[] { -size.x * 0.35f, size.x * 0.35f })
                Box(group, "Cargo reinforcing band", new Vector3(x, size.y * 0.5f, 0), new Vector3(0.07f, size.y + 0.04f, size.z + 0.05f), frame);
            foreach (float y in new[] { 0.05f, size.y - 0.05f })
                Box(group, "Cargo lid rail", new Vector3(0, y, -size.z * 0.5f - 0.04f), new Vector3(size.x + 0.04f, 0.07f, 0.08f), frame);
        }

        private static Light AddLight(Transform parent, string name, LightType type, Vector3 position, Color color, float intensity, float range, Vector3? target = null)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            if (target.HasValue) item.transform.LookAt(target.Value);
            var light = item.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = 58f;
            light.innerSpotAngle = 32f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.ForcePixel;
            return light;
        }

        private static Material Material(string name, string hex, float metallic, float smoothness, float emission = 0f)
        {
            string path = MaterialFolder + "/Warehouse " + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) throw new InvalidOperationException("A Lit shader is required to create the warehouse.");
            if (material == null)
            {
                material = new Material(shader) { name = "Warehouse " + name };
                AssetDatabase.CreateAsset(material, path);
            }
            ColorUtility.TryParseHtmlString("#" + hex, out Color color);
            material.SetColor("_BaseColor", color);
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segments[i]);
                current = next;
            }
        }
    }
}
