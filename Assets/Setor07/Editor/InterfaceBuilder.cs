using System.IO;
using System.Linq;
using Setor07.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Setor07.Editor
{
    /// <summary>Creates real, editable uGUI objects from the approved HTML layouts.</summary>
    public static class InterfaceBuilder
    {
        public const string ScenePath = "Assets/Setor07/Scenes/Interface.unity";
        public const string PrefabPath = "Assets/Setor07/Resources/Setor07/Interface.prefab";
        private static readonly Color Ink = Hex("0b1014"), Paper = Hex("f0f2eb"), Muted = Hex("839094"),
            Line = Hex("263238"), Accent = Hex("c8f66b"), Panel = Hex("10181d"), Gold = Hex("facc15"), Blue = Hex("38bdf8");
        private static Font font;
        private static Sprite white;
        private static InterfaceMockData data;
        private static InterfacePresentation presentation;

        [MenuItem("Setor 07/Generate HTML interface assets")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/Setor07/Resources/Setor07");
            Directory.CreateDirectory("Assets/Setor07/Scenes");
            Directory.CreateDirectory("Assets/Setor07/Art");
            AssetDatabase.Refresh();
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Core/TextMesh Pro/Fonts/LiberationSans.ttf");
            white = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            PrepareIcons();
            data = AssetDatabase.LoadAssetAtPath<InterfaceMockData>("Assets/Setor07/MockData.asset");
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<InterfaceMockData>();
                AssetDatabase.CreateAsset(data, "Assets/Setor07/MockData.asset");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = WarehouseStageBuilder.Build();
            var canvas = CreateCanvas(camera);
            var root = Rect(canvas.transform, "Interface", 0, 0, 1280, 720);
            Stretch(root);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 1; group.interactable = false; group.blocksRaycasts = false;
            presentation = root.gameObject.AddComponent<InterfacePresentation>();
            presentation.mockData = data;
            var screens = new GameObject[7];
            for (int i = 0; i < screens.Length; i++)
            {
                var screen = Rect(root, ((InterfaceScreen)i).ToString(), 0, 0, 1280, 720);
                Stretch(screen); screens[i] = screen.gameObject;
            }
            BuildLobby(screens[0].transform);
            BuildFormation(screens[1].transform);
            BuildSettings(screens[2].transform);
            BuildInventory(screens[3].transform);
            BuildCatalog(screens[4].transform, true);
            BuildEvents(screens[5].transform);
            BuildCatalog(screens[6].transform, false);
            presentation.screens = screens;
            presentation.ShowScreen(InterfaceScreen.Lobby);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PrefabPath);
            Object.DestroyImmediate(root.gameObject);
            BuildLoading(canvas);
            EditorSceneManager.SaveScene(scene, ScenePath);
            var previous = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            previous.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = previous.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("SETOR07_INTERFACE_GENERATED " + ScenePath);
        }

        private static Canvas CreateCanvas(Camera camera)
        {
            var go = new GameObject("Setor07 · Presentation Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            canvas.sortingOrder = 100;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 1;
            return canvas;
        }

        private static void BuildLobby(Transform root)
        {
            var tint = Image(root, "Menu contrast vignette", 0, 0, 1280, 720, Color.white);
            Stretch(tint.rectTransform); tint.raycastTarget = false;
            AddGradient(tint, Gradient2.Type.Horizontal, new Color(0.02f, .04f, .05f, .75f), new Color(.02f, .04f, .05f, .05f), new Color(.02f, .04f, .05f, .65f));
            var header = Rect(root, "Topbar", 38, 32, 1204, 43);
            header.anchorMax = new Vector2(1, 1); header.offsetMax = new Vector2(-38, -32); header.offsetMin = new Vector2(38, -75);
            var avatar = Border(header, "Avatar", 0, 1, 38, 38, Color.clear, Hex("475453"));
            Label(avatar, "Initial", "K", 0, 0, 38, 38, 20, Paper, TextAnchor.MiddleCenter);
            Image(avatar, "Online dot", 34, 32, 5, 5, Accent);
            presentation.operatorName = Label(header, "Operator name", data.operatorName, 50, 4, 92, 18, 12, Paper, bold: true);
            presentation.operatorRole = Label(header, "Operator role", data.role, 50, 23, 85, 13, 8, Muted);
            var coin = Border(header, "Coin balance", 151, 6, 95, 28, Hex("0d1519"), Hex("233742"));
            Icon(coin, "gem", 9, 7, 14, Gold);
            presentation.coins = Label(coin, "Coins", data.coins, 30, 0, 60, 28, 11, Gold, bold: true);
            var diamond = Border(header, "Diamond balance", 256, 6, 104, 28, Hex("0d1519"), Hex("233742"));
            Icon(diamond, "gem", 8, 7, 14, Blue);
            presentation.diamonds = Label(diamond, "Diamonds", data.diamonds, 29, 0, 53, 28, 11, Blue, bold: true);
            var plus = Button(diamond, "Add diamonds · mock", 80, 5, 18, 18, Hex("12384d"));
            Label(plus, "Plus", "+", 0, 0, 18, 18, 13, Blue, TextAnchor.MiddleCenter);
            var brand = Rect(header, "Brand", -62, 0, 124, 43); brand.anchorMin = brand.anchorMax = new Vector2(.5f, 1);
            Label(brand, "Setor", "S E T O R", 0, 0, 66, 43, 11, Paper);
            Label(brand, "Slash", "/", 74, 0, 16, 43, 23, Accent);
            Label(brand, "Number", "07", 94, 0, 38, 43, 23, Paper);
            var utilities = Right(header, "Utilities", 0, -6, 200, 50);
            string[] names = { "Amigos", "Mensagens", "Ajustes" }, icons = { "user-round-group", "mail", "settings" };
            for (int i = 0; i < names.Length; i++)
            {
                var b = Button(utilities, names[i] + " · mock", i * 68, 0, 64, 48, Color.clear);
                Icon(b, icons[i], 23, 1, 19, Hex("a0adb1"));
                Label(b, "Caption", names[i], 0, 25, 64, 18, 8, Hex("a0adb1"), TextAnchor.MiddleCenter);
            }
            var nav = Rect(root, "Lobby menu", 38, 0, 210, 170); nav.anchorMin = nav.anchorMax = new Vector2(0, .72f);
            string[] labels = { "Loja", "Passe de batalha", "Eventos", "Cofre" };
            string[] navIcons = { "shopping-cart", "tickets", "gift", "package" };
            for (int i = 0; i < labels.Length; i++)
            {
                var b = Button(nav, labels[i] + " · mock", 0, i * 42, 205, 32, Color.clear);
                Icon(b, navIcons[i], 0, 8, 16, Hex("9aafb5"));
                Label(b, "Title", labels[i], 30, 0, 174, 32, 11, Hex("c3cdce"));
            }
            var launch = Right(root, "Launch controls", 38, 0, 282, 200);
            launch.anchorMin = launch.anchorMax = new Vector2(1, 0); launch.anchoredPosition = new Vector2(-320, 238);
            Label(launch, "Mode category", "B A T T L E   R O Y A L E", 0, 0, 282, 18, 8, Muted);
            presentation.mode = Label(launch, "Mode", data.mode, 0, 23, 282, 27, 17, Paper);
            Image(launch, "Divider", 0, 66, 282, 1, Line);
            var team = Button(launch, "Formation · mock", 0, 67, 282, 32, Color.clear);
            Icon(team, "user-round", 0, 8, 14, Hex("9eacb0"));
            presentation.formation = Label(team, "Selected formation", data.formation, 25, 0, 220, 32, 10, Hex("9eacb0"));
            Icon(team, "chevron-down", 268, 9, 12, Hex("9eacb0"));
            var play = Button(launch, "Jogar · future action", 0, 111, 282, 75, Accent);
            Label(play, "Title", "J O G A R", 24, 0, 205, 75, 26, Ink, bold: true);
            Label(play, "Arrow", "↗", 231, 0, 28, 75, 28, Ink);
            Label(launch, "Footnote", "▪  SEU PRÓXIMO MOVIMENTO.", 0, 194, 282, 15, 7, Hex("849494"), TextAnchor.MiddleRight);
        }

        private static void BuildFormation(Transform root)
        {
            var box = Right(root, "Formation popover", 38, 0, 320, 198);
            box.anchorMin = box.anchorMax = new Vector2(1, 0); box.anchoredPosition = new Vector2(-358, 443);
            var panel = Border(box, "Panel", 0, 0, 320, 198, Hex("091216"), Hex("233b47"));
            Label(panel, "Title", "FORMAÇÃO DA EQUIPE", 15, 10, 260, 24, 9, Hex("738e9c"), bold: true);
            Label(panel, "Close · mock", "×", 286, 5, 22, 30, 20, Muted);
            Image(panel, "Divider", 15, 42, 290, 1, Hex("1c2e37"));
            Checkbox(panel, "Aceitar pedido", 16, 55, true);
            Checkbox(panel, "Auto-formação", 16, 81, false);
            string[] titles = { "Solo", "Duo", "Squad" };
            for (int i = 0; i < titles.Length; i++)
            {
                var card = Border(panel, titles[i] + " · mock", 15 + 99 * i, 116, 92, 68, i == 0 ? Hex("28291b") : Hex("0d161b"), i == 0 ? Gold : Hex("203540"));
                card.gameObject.AddComponent<Button>();
                Icon(card, i == 0 ? "user-round" : "user-round-group", 35, 10, 22, i == 0 ? Gold : Muted);
                Label(card, "Label", titles[i], 0, 38, 92, 22, 11, i == 0 ? Gold : Muted, TextAnchor.MiddleCenter, true);
            }
        }

        private static void BuildSettings(Transform root)
        {
            Backdrop(root);
            var panel = Right(root, "Settings drawer", 0, 0, 360, 720);
            panel.anchorMin = new Vector2(1, 0); panel.offsetMin = new Vector2(-360, 0); panel.offsetMax = Vector2.zero;
            AddImage(panel, Panel);
            DrawerHeading(panel, "DO SEU JEITO", "Ajustes.", 360);
            Label(panel, "Audio category", "ÁUDIO E VOZ", 28, 133, 304, 20, 8, Muted);
            string[] labels = { "Volume geral", "Volume do som do jogo", "Volume do chat ao vivo" };
            int[] values = { data.masterVolume, data.gameVolume, data.chatVolume };
            presentation.volumeLabels = new Text[3]; presentation.volumeSliders = new Slider[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 168 + i * 76;
                Label(panel, labels[i], labels[i], 28, y, 255, 24, 11, Hex("bcc9cc"));
                presentation.volumeLabels[i] = Label(panel, "Value", values[i] + "%", 283, y, 49, 24, 10, Accent, TextAnchor.MiddleRight);
                presentation.volumeSliders[i] = MockSlider(panel, "Volume · mock " + i, 28, y + 40, 304, values[i] / 100f);
            }
            Image(panel, "Divider", 28, 396, 304, 1, Line);
            Label(panel, "Visual category", "QUALIDADE VISUAL", 28, 418, 304, 20, 8, Muted);
            string[] quality = { "LEVE", "PADRÃO", "ALTA" };
            for (int i = 0; i < 3; i++)
            {
                var q = Border(panel, quality[i], 28 + i * 101, 453, 101, 34, i == 1 ? Hex("1d2820") : Color.clear, Line);
                q.gameObject.AddComponent<Button>();
                Label(q, "Label", quality[i], 0, 0, 101, 34, 9, i == 1 ? Accent : Muted, TextAnchor.MiddleCenter);
                if (i == 1) Image(q, "Selected", 0, 33, 101, 1, Accent);
            }
            Label(panel, "Vibration", "Vibração", 28, 514, 250, 30, 11, Hex("bbc8cb"));
            var toggle = Image(panel, "Vibration switch · mock", 303, 521, 28, 15, Accent);
            Image(toggle.transform, "Thumb", 18, 4, 7, 7, Ink);
            Footer(panel, "REVER TELA DE CARREGAMENTO ↗", "VOLTAR AO CAMPO  ↗", 360);
        }

        private static void BuildInventory(Transform root)
        {
            Backdrop(root);
            var panel = Rect(root, "Equipment drawer", 0, 0, 520, 720);
            panel.anchorMin = new Vector2(0, 0); panel.offsetMin = Vector2.zero; panel.offsetMax = new Vector2(520, 0);
            AddImage(panel, Panel); DrawerHeading(panel, "SEU LOADOUT", "Equipamento.", 520);
            Label(panel, "Weapons heading", "ARMAS EQUIPADAS", 28, 137, 280, 20, 9, Muted);
            Label(panel, "Positions", "02 POSIÇÕES", 340, 137, 152, 20, 8, Muted, TextAnchor.MiddleRight);
            for (int i = 0; i < 2; i++)
            {
                int y = 177 + i * 92;
                Image(panel, "Divider", 28, y, 464, 1, Line);
                Label(panel, "Weapon slot", "0" + (i + 1), 28, y + 17, 40, 55, 23, i == 0 ? Accent : Muted);
                Label(panel, "Slot caption", i == 0 ? "ARMA PRINCIPAL" : "ARMA SECUNDÁRIA", 85, y + 15, 230, 18, 8, Muted);
                Label(panel, "Weapon", i == 0 ? "RIFLE" : "Não equipada", 85, y + 36, 230, 22, i == 0 ? 14 : 11, i == 0 ? Paper : Muted);
                if (i == 0) Label(panel, "Ammo", "30 / 90", 85, y + 60, 230, 15, 9, Muted);
            }
            Icon(panel, "backpack", 28, 397, 18, Muted);
            Label(panel, "Backpack", "MOCHILA", 58, 390, 230, 30, 10, Paper);
            Label(panel, "Capacity", data.backpackUsed + " / " + data.backpackCapacity, 360, 390, 132, 30, 10, Accent, TextAnchor.MiddleRight);
            Image(panel, "Capacity track", 28, 435, 464, 2, Line);
            Label(panel, "Items tab · mock", "ITENS", 28, 457, 70, 30, 10, Accent);
            Label(panel, "Devices tab · mock", "DISPOSITIVOS", 110, 457, 200, 30, 10, Muted);
            Image(panel, "Tabs rule", 28, 493, 464, 1, Line);
            Icon(panel, "backpack", 246, 527, 28, Hex("506873"));
            Label(panel, "Empty title", "Leve só o que importa.", 28, 568, 464, 25, 15, Hex("a2b7bd"), TextAnchor.MiddleCenter);
            Label(panel, "Empty copy", "Os itens coletados aparecem aqui.", 28, 600, 464, 20, 10, Hex("5f7b86"), TextAnchor.MiddleCenter);
            Footer(panel, "", "EQUIPADO. VAMOS.  ↗", 520);
        }

        private static void BuildCatalog(Transform root, bool store)
        {
            AddImage((RectTransform)root, Hex("0b1419"));
            FullHeader(root, store ? "CATÁLOGO & SUPRIMENTOS" : "SEU INVENTÁRIO", store ? "Loja." : "Cofre.");
            var sidebar = Rect(root, "Categories", 38, 163, 173, 445);
            string[] categories = { "MODA", "COLEÇÃO", "EMOTE", "POSE", "VEÍCULOS", "PERFIL", "OUTROS" };
            string[] icons = { "user-round", "package", "star", "eye", "map-pin", "user-round", "list-checks" };
            for (int i = 0; i < categories.Length; i++)
            {
                var b = Button(sidebar, categories[i] + " · mock", 0, i * 51, 173, 41, i == 0 ? Hex("19251d") : Color.clear);
                if (i == 0) Image(b, "Selection marker", 0, 0, 2, 41, Accent);
                Icon(b, icons[i], 16, 12, 16, i == 0 ? Accent : Hex("66828e"));
                Label(b, "Label", categories[i], 48, 0, 123, 41, 10, i == 0 ? Accent : Hex("8aa1aa"));
            }
            var main = Rect(root, "Catalog content", 255, 160, 987, 480);
            main.anchorMax = new Vector2(1, 1); main.offsetMax = new Vector2(-38, -160); main.offsetMin = new Vector2(255, -640);
            Image(main, "Vertical rule", 0, 0, 1, 480, Hex("20343e"));
            Label(main, "Rarity heading", "RARIDADE", 36, 0, 200, 16, 8, Hex("627e8b"));
            Label(main, "Rarity · mock", "Todas as raridades    ⌄", 36, 24, 240, 24, 11, Hex("adbec5"));
            var search = Right(main, "Search · mock", 0, 18, 280, 32);
            Label(search, "Placeholder", "Buscar por nome...", 0, 0, 250, 32, 10, Hex("607c88"));
            Icon(search, "eye", 260, 8, 15, Hex("698590"));
            Image(search, "Underline", 0, 32, 280, 1, Hex("354d59"));
            var rule = Image(main, "Toolbar rule", 36, 66, 910, 1, Hex("20343e"));
            rule.rectTransform.anchorMax = new Vector2(1, 1); rule.rectTransform.offsetMax = new Vector2(0, -66); rule.rectTransform.offsetMin = new Vector2(36, -67);
            if (store)
            {
                Label(main, "Category title", "Moda", 36, 88, 300, 35, 25, Paper);
                Label(main, "Category description", "Trajes completos, vestuário tático, conjuntos urbanos e visuais para operadores.", 36, 130, 800, 24, 11, Hex("73929f"));
                for (int i = 0; i < data.storeItems.Length; i++) StoreCard(main, data.storeItems[i], i);
            }
            else
            {
                var diamond = Border(main, "Category diamond", 445, 170, 74, 74, Color.clear, Hex("29414e"));
                diamond.localRotation = Quaternion.Euler(0, 0, 45);
                var icon = Icon(diamond, "user-round", 20, 20, 34, Hex("6d8f9d")); icon.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);
                Label(main, "Eyebrow", "SEU ESTILO NO SETOR", 36, 280, 910, 20, 8, Hex("587888"), TextAnchor.MiddleCenter);
                Label(main, "Category", "Moda", 36, 309, 910, 42, 30, Paper, TextAnchor.MiddleCenter);
                Label(main, "Description", "Roupas, conjuntos e acessórios do personagem.", 36, 363, 910, 24, 12, Hex("73929f"), TextAnchor.MiddleCenter);
            }
            ScreenFooter(root, store ? "CATÁLOGO ATUALIZADO · SETOR / 07" : "SEUS ITENS, DO SEU JEITO.", "07 / " + (store ? "LOJA" : "COFRE"));
        }

        private static void StoreCard(Transform parent, InterfaceMockData.StoreItem item, int index)
        {
            var card = Border(parent, item.title + " · mock", 36 + index * 228, 182, 215, 242, Hex("0c1418"), Hex("1e333d"));
            Color rarity = item.rarity == "LENDÁRIO" ? Hex("f5a623") : item.rarity == "ÉPICO" ? Hex("c084fc") : Blue;
            Label(card, "Rarity", item.rarity, 65, 10, 136, 18, 8, rarity, TextAnchor.MiddleRight);
            var tile = Border(card, "Icon tile", 14, 16, 44, 44, Hex("080d10"), Hex("1f3540"));
            Icon(tile, "crown", 11, 11, 22, Accent);
            Label(card, "Product title", item.title, 14, 82, 187, 36, 12, Paper, bold: true);
            var desc = Label(card, "Description", item.description, 14, 124, 187, 45, 10, Hex("6b8793")); desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            Image(card, "Divider", 14, 179, 187, 1, Hex("182830"));
            if (!string.IsNullOrEmpty(item.diamonds)) { Icon(card, "gem", 14, 193, 12, Blue); Label(card, "Diamond price", item.diamonds, 33, 188, 80, 24, 11, Blue, bold: true); }
            if (!string.IsNullOrEmpty(item.coins)) Label(card, "Coin price", "●  " + item.coins, 14, string.IsNullOrEmpty(item.diamonds) ? 190 : 213, 90, 20, 10, Gold);
            var buy = Border(card, "Comprar · future action", 117, 196, 84, 30, Hex("1a281b"), Accent); buy.gameObject.AddComponent<Button>();
            Label(buy, "Label", "COMPRAR", 0, 0, 84, 30, 9, Accent, TextAnchor.MiddleCenter, true);
        }

        private static void BuildEvents(Transform root)
        {
            AddImage((RectTransform)root, Hex("0b1419")); FullHeader(root, "OPORTUNIDADES DO SETOR", "Eventos.");
            var side = Rect(root, "Event categories", 38, 163, 196, 300);
            string[] cats = { "LOGIN DIÁRIO", "MISSÕES DA SEMANA", "DESAFIO RANQUEADO", "RECARGA ESPECIAL" };
            for (int i = 0; i < cats.Length; i++)
            {
                var b = Button(side, cats[i] + " · mock", 0, i * 52, 196, 42, i == 0 ? Hex("19251d") : Color.clear);
                Icon(b, i == 0 ? "gift" : "flag", 13, 13, 16, i == 0 ? Accent : Muted);
                Label(b, "Label", cats[i], 43, 0, 150, 42, 9, i == 0 ? Accent : Muted);
            }
            var banner = Border(root, "Daily login banner", 273, 163, 969, 91, Hex("15251e"), Hex("2d473b"));
            Label(banner, "Eyebrow", "EVENTO EXCLUSIVO", 20, 12, 500, 18, 8, Accent);
            Label(banner, "Title", "Login Diário do Operador", 20, 37, 740, 35, 23, Paper);
            Label(banner, "Day", "DIA 02 / 07", 814, 0, 135, 91, 12, Accent, TextAnchor.MiddleRight);
            string[] rewards = { "500 Moedas", "50 Diamantes", "Caixa Tática", "1.500 Moedas", "2x Tickets", "100 Diamantes", "Skin Cibernética" };
            for (int i = 0; i < 7; i++)
            {
                float y = 272 + i * 49;
                var row = Border(root, "Day " + (i + 1) + " · mock", 273, y, 969, 44, i == 1 ? Hex("14231d") : Hex("0d181e"), i == 1 ? Hex("567442") : Hex("1d3039"));
                Icon(row, i == 1 || i == 5 ? "gem" : "gift", 14, 12, 20, i == 1 ? Blue : Muted);
                Label(row, "Day", "DIA 0" + (i + 1), 49, 0, 82, 44, 9, Muted);
                Label(row, "Reward", rewards[i], 140, 0, 545, 44, 12, Paper);
                var action = Button(row, "Reward action · future", 817, 7, 137, 30, i == 1 ? Accent : Hex("132026"));
                Label(action, "Label", i == 0 ? "RESGATADO" : i == 1 ? "RESGATAR" : "EM BREVE", 0, 0, 137, 30, 9, i == 1 ? Ink : Hex("78939e"), TextAnchor.MiddleCenter, true);
            }
            ScreenFooter(root, "REINICIA ÀS 04:00 (BRT)", "TEMPORADA · 4D 12H");
        }

        private static void BuildLoading(Canvas canvas)
        {
            var loading = Image(canvas.transform, "Loading", 0, 0, 1280, 720, Color.white);
            Stretch(loading.rectTransform); loading.raycastTarget = true;
            AddGradient(loading, Gradient2.Type.Vertical, Hex("020405"), Hex("090d10"), Hex("05080a"));
            var background = Image(loading.transform, "Optional background · assign on Bootstrap", 0, 0, 1280, 720, Color.white);
            Stretch(background.rectTransform); background.preserveAspect = true; background.enabled = false;
            background.transform.SetAsFirstSibling();
            var box = Rect(loading.transform, "Loading indicator", -190, 0, 380, 54);
            box.anchorMin = box.anchorMax = new Vector2(.5f, 0); box.anchoredPosition = new Vector2(-190, 104);
            var label = Label(box, "Loading status", "CARREGANDO RECURSOS... 0%", 0, 0, 380, 26, 14, Paper, TextAnchor.MiddleCenter);
            Image(box, "Progress track", 0, 42, 380, 4, new Color(1, 1, 1, .16f));
            var fill = Image(box, "Progress fill", 0, 42, 380, 4, Accent);
            fill.sprite = white; fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal; fill.fillOrigin = 0; fill.fillAmount = 0;
            var boot = canvas.gameObject.AddComponent<InterfaceBootstrap>();
            boot.canvas = canvas; boot.loadingScreen = loading.gameObject; boot.loadingLabel = label; boot.progressFill = fill; boot.optionalBackground = background;
        }

        private static void FullHeader(Transform root, string eyebrow, string title)
        {
            Label(root, "Small brand", "S / 07", 38, 37, 100, 30, 17, Accent);
            Label(root, "Header eyebrow", eyebrow, 181, 36, 580, 18, 8, Muted);
            Label(root, "Header title", title, 181, 64, 580, 45, 37, Paper);
            var wallet = Right(root, "Wallet", 92, 55, 240, 32);
            Label(wallet, "Coins", "●  " + data.coins, 0, 0, 112, 32, 12, Gold, TextAnchor.MiddleRight, true);
            Icon(wallet, "gem", 135, 9, 15, Blue);
            Label(wallet, "Diamonds", data.diamonds, 158, 0, 82, 32, 12, Blue, TextAnchor.MiddleRight, true);
            var close = Right(root, "Close · mock", 38, 56, 30, 30);
            close.gameObject.AddComponent<Button>(); Label(close, "X", "×", 0, 0, 30, 30, 29, Muted, TextAnchor.MiddleCenter);
            var rule = Image(root, "Header rule", 38, 132, 1204, 1, Hex("20343e"));
            rule.rectTransform.anchorMax = new Vector2(1, 1); rule.rectTransform.offsetMin = new Vector2(38, -133); rule.rectTransform.offsetMax = new Vector2(-38, -132);
        }

        private static void DrawerHeading(Transform root, string eyebrow, string title, int width)
        {
            Label(root, "Eyebrow", eyebrow, 28, 28, width - 80, 18, 8, Muted);
            Label(root, "Title", title, 28, 57, width - 80, 43, 32, Paper);
            var close = Button(root, "Close · mock", width - 56, 26, 28, 28, Color.clear);
            Label(close, "X", "×", 0, 0, 28, 28, 28, Muted, TextAnchor.MiddleCenter);
            Image(root, "Header divider", 0, 115, width, 1, Line);
        }

        private static void Footer(Transform root, string left, string right, int width)
        {
            var f = Rect(root, "Footer", 28, 0, width - 56, 55);
            f.anchorMin = f.anchorMax = new Vector2(0, 0); f.anchoredPosition = new Vector2(28, 55);
            Image(f, "Rule", 0, 0, width - 56, 1, Line);
            Label(f, "Left · mock", left, 0, 8, width - 56, 20, 7, Hex("5c7580"));
            Label(f, "Right · mock", right, 0, 28, width - 56, 25, 9, Accent, TextAnchor.MiddleRight);
        }

        private static void ScreenFooter(Transform root, string left, string right)
        {
            var footer = Rect(root, "Screen footer", 38, 0, 1204, 34);
            footer.anchorMin = Vector2.zero; footer.anchorMax = new Vector2(1, 0);
            footer.offsetMin = new Vector2(38, 32); footer.offsetMax = new Vector2(-38, 66);
            var rule = Image(footer, "Rule", 0, 0, 1204, 1, Hex("20343e"));
            rule.rectTransform.anchorMax = new Vector2(1, 1); rule.rectTransform.offsetMin = new Vector2(0, -1); rule.rectTransform.offsetMax = Vector2.zero;
            Label(footer, "Left", left, 0, 9, 740, 24, 8, Hex("456674"));
            var label = Label(footer, "Right", right, 0, 9, 300, 24, 8, Hex("456674"), TextAnchor.MiddleRight);
            label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(1, 1); label.rectTransform.anchoredPosition = new Vector2(-300, -9);
        }

        private static void Backdrop(Transform parent)
        {
            var image = Image(parent, "Modal backdrop", 0, 0, 1280, 720, new Color(.01f, .02f, .03f, .72f));
            Stretch(image.rectTransform);
        }

        private static void Checkbox(Transform parent, string text, float x, float y, bool selected)
        {
            var box = Border(parent, text + " · mock", x, y, 14, 14, Hex("0b151a"), selected ? Gold : Hex("3c5462"));
            if (selected) Label(box, "Check", "✓", 0, 0, 14, 14, 11, Gold, TextAnchor.MiddleCenter, true);
            Label(parent, text, text, x + 23, y - 3, 230, 20, 11, Hex("b0c0c6"));
        }

        private static Slider MockSlider(Transform root, string name, float x, float y, float width, float value)
        {
            var rect = Rect(root, name, x, y, width, 20);
            Image(rect, "Track", 0, 9, width, 2, Line);
            var fill = Image(rect, "Fill", 0, 9, width, 2, Accent);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = new Vector2(0, 9); fill.rectTransform.offsetMax = new Vector2(0, -9);
            var handle = Image(rect, "Handle", width * value - 4, 6, 8, 8, Accent);
            handle.rectTransform.pivot = new Vector2(.5f, .5f);
            handle.rectTransform.anchoredPosition = Vector2.zero;
            handle.rectTransform.sizeDelta = new Vector2(8, -12);
            var slider = rect.gameObject.AddComponent<Slider>(); slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform; slider.value = value; slider.interactable = false;
            var colors = slider.colors; colors.disabledColor = Color.white; slider.colors = colors;
            slider.targetGraphic = handle;
            return slider;
        }

        private static void PrepareIcons()
        {
            foreach (string path in Directory.GetFiles("Assets/UI/Lucide/Icons", "*.png"))
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer.textureType == TextureImporterType.Sprite) continue;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.SaveAndReimport();
            }
        }

        private static void AddGradient(Image image, Gradient2.Type type, Color a, Color b, Color c)
        {
            var effect = image.gameObject.AddComponent<Gradient2>(); effect.GradientType = type;
            effect.BlendMode = Gradient2.Blend.Override; effect.ModifyVertices = true;
            effect.EffectGradient = new UnityEngine.Gradient {
                colorKeys = new[] { new GradientColorKey(a, 0), new GradientColorKey(b, .5f), new GradientColorKey(c, 1) },
                alphaKeys = new[] { new GradientAlphaKey(a.a, 0), new GradientAlphaKey(b.a, .5f), new GradientAlphaKey(c.a, 1) }
            };
        }

        private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(width, height); rect.anchoredPosition = new Vector2(x, -y); return rect;
        }
        private static RectTransform Right(Transform parent, string name, float inset, float y, float width, float height)
        {
            var rect = Rect(parent, name, 0, y, width, height); rect.anchorMin = rect.anchorMax = Vector2.one;
            rect.anchoredPosition = new Vector2(-inset - width, -y); return rect;
        }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static Image AddImage(RectTransform rect, Color color) { var image = rect.gameObject.AddComponent<Image>(); image.color = color; return image; }
        private static Image Image(Transform parent, string name, float x, float y, float w, float h, Color color) { return AddImage(Rect(parent, name, x, y, w, h), color); }
        private static RectTransform Border(Transform parent, string name, float x, float y, float w, float h, Color color, Color border)
        {
            var rect = Rect(parent, name, x, y, w, h); AddImage(rect, color);
            Image(rect, "Border top", 0, 0, w, 1, border); Image(rect, "Border bottom", 0, h - 1, w, 1, border);
            Image(rect, "Border left", 0, 0, 1, h, border); Image(rect, "Border right", w - 1, 0, 1, h, border); return rect;
        }
        private static RectTransform Button(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var rect = Rect(parent, name, x, y, w, h); var image = AddImage(rect, color);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None }; return rect;
        }
        private static Text Label(Transform parent, string name, string text, float x, float y, float w, float h, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft, bool bold = false)
        {
            var label = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = size; label.color = color; label.alignment = align;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow; return label;
        }
        private static Image Icon(Transform parent, string icon, float x, float y, float size, Color color)
        {
            var image = Image(parent, icon + " · Lucide", x, y, size, size, color);
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Lucide/Icons/" + icon + ".png");
            image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }
}
