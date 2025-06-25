using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GridSystem.VisualElements
{
    public class Tile : Button, ITile
    {
        public List<IPiece> pieces { get; set; } = new();

        public int pieceID
        {
            get
            {
                if (pieces.Count <= 0) return (int)PieceType.none;
                int value = (int)PieceType.generic;
                for (int i = 0; i < pieces.Count; i++)
                {
                    value |= pieces[i].id;
                }

                return value;
            }
        }

        public Color defaultColor { get; } = ColorExtensions.GrayShade(.8f);

        public Color selectableColor { get; } = ColorExtensions.GrayShade(.4f);

        public Color validColor { get; } = Color.green;

        public Color invalidColor { get; } = Color.red;
        public List<Color> colors { get; set; } = new();

        public ITile.State state { get; set; }
        public Action<ITile> onClick { get; set; }
        public Action<ITile> onEnter { get; set; }
        public Action<ITile> onExit { get; set; }

        private bool left = true;

        public Tile()
        {
            this.SetPadding(0);
            style.backgroundColor = defaultColor;
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerExit);
            clicked += OnPointerClick;
        }

        public void UpdateStateVisual()
        {
            colors.Clear();
            if (state != 0 && state != ITile.State.Generic)
            {
                if (NumberUtil.ContainsBytes((int)state, (int)ITile.State.Selectable))
                    AddColor(selectableColor);
                if (NumberUtil.ContainsBytes((int)state, (int)ITile.State.Valid))
                    AddColor(validColor);
                if (NumberUtil.ContainsBytes((int)state, (int)ITile.State.Invalid))
                    AddColor(invalidColor);
            }

            UpdateColor();
        }

        public void SetColor(Color color)
        {
            colors.Clear();
            AddColor(color);
        }

        public void AddColor(Color color)
        {
            Color invertedColor = ColorExtensions.InvertColor(color);
            invertedColor.a = 0;

            colors.Add(invertedColor);
            UpdateColor();
        }

        public void UpdateColor()
        {
            Color newColor = Color.white;
            for (int i = 0; i < colors.Count; i++)
            {
                newColor -= colors[i] * (.25f + (1f / (colors.Count + 1f)));
            }

            ApplyColor(newColor);
        }

        public void RemoveColor(Color color)
        {
            Color invertedColor = ColorExtensions.InvertColor(color);
            invertedColor.a = 0;

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i] == invertedColor)
                {
                    colors.RemoveAt(i);
                    UpdateColor();
                    break;
                }
            }
        }
        public void ApplyColor(Color color)
        {
            style.backgroundColor = color;
        }

        public void OnPointerClick()
        {
            for (int i = 0; i < pieces.Count; i++) pieces[i].onClick?.Invoke();
            onClick?.Invoke(this);
        }

        public void OnPointerEnter(PointerEnterEvent eventData)
        {
            if (worldBound.Contains(eventData.position - eventData.deltaPosition) && !left) return;
            left = false;

            for (int i = 0; i < pieces.Count; i++) pieces[i].onEnter?.Invoke();
            onEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerLeaveEvent eventData)
        {
            if (worldBound.Contains(eventData.position)) return;
            left = true;

            for (int i = 0; i < pieces.Count; i++) pieces[i].onExit?.Invoke();
            onExit?.Invoke(this);
        }

        public void PlacePiece(IPiece piece)
        {
            if (pieces.Contains(piece)) return;
            pieces.Add(piece);
        }
    }
}