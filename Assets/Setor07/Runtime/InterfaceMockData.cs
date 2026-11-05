using System;
using UnityEngine;

namespace Setor07.UI
{
    [CreateAssetMenu(menuName = "Setor 07/Interface mock data")]
    public sealed class InterfaceMockData : ScriptableObject
    {
        public string operatorName = "KIRITO";
        public string role = "OPERADOR";
        public string coins = "45.200";
        public string diamonds = "1.540";
        public string mode = "PARTIDA RANQUEADA";
        public string formation = "SOLO";
        public int backpackUsed;
        public int backpackCapacity = 150;
        [Range(0, 100)] public int masterVolume = 80;
        [Range(0, 100)] public int gameVolume = 85;
        [Range(0, 100)] public int chatVolume = 70;
        public StoreItem[] storeItems = {
            new StoreItem("Traje Espectro Noturno", "LENDÁRIO", "Conjunto tático furtivo com efeitos holográficos reativos.", "1.200", ""),
            new StoreItem("Conjunto Caveira Neon", "ÉPICO", "Visual urbano com iluminação fluorescente e jaqueta preta.", "650", "20.000"),
            new StoreItem("Jaqueta Urbana Street", "ÉPICO", "Estilo casual moderno de alta mobilidade para combate.", "", "10.000"),
            new StoreItem("Máscara Tática Carbono", "RARO", "Proteção balística visual com respirador pressurizado.", "199", "5.000")
        };

        [Serializable]
        public sealed class StoreItem
        {
            public string title, rarity, description, diamonds, coins;
            public StoreItem(string title, string rarity, string description, string diamonds, string coins)
            { this.title = title; this.rarity = rarity; this.description = description; this.diamonds = diamonds; this.coins = coins; }
        }
    }
}
