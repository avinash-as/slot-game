using UnityEngine;
using SlotGame.Symbols;

namespace SlotGame.Core
{
    public class SymbolView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMesh label;

        public SymbolData Data { get; private set; }

        public void SetData(SymbolData data)
        {
            Data = data;
            if (spriteRenderer != null && data != null && data.sprite != null)
                spriteRenderer.sprite = data.sprite;
            if (label != null && data != null) label.text = data.displayName;
        }
    }
}