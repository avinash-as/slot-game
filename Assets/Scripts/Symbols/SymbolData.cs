using System;
using UnityEngine;

namespace SlotGame.Symbols
{
    public enum SymbolType
    {
        Cherry, Lemon, Orange, Plum, Bell, Bar, Seven, Wild, Scatter
    }

    [CreateAssetMenu(fileName = "SymbolData", menuName = "Slot/Symbol Data")]
    public class SymbolData : ScriptableObject
    {
        public SymbolType type;
        public string displayName;
        public Sprite sprite;
        public int weight = 10;
        public int payout3;
        public int payout4;
        public int payout5;
        public bool isWild;
        public bool isScatter;
    }
}