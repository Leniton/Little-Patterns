using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridSystem
{
    public interface ITile
    {
        public List<IPiece> pieces { get; set; }

        public int pieceID { get; }

        public Color defaultColor { get; }
        public Color selectableColor { get; }
        public Color validColor { get; }
        public Color invalidColor { get; }

        [Flags]
        public enum State
        {
            Generic = 1,
            Selectable = 2,
            Valid = 4,
            Invalid = 8
        }

        public State state { get; set; }

        public List<Color> colors { get; set; }

        //callbacks
        public Action<ITile> onClick { get; set; }
        public Action<ITile> onEnter { get; set; }
        public Action<ITile> onExit { get; set; }

        public void PlacePiece(IPiece piece);

        public void RemovePiece(IPiece piece) => pieces.Remove(piece);

        public IPiece GetFirstPiece() => pieces[0];

        public IPiece GetPiece(int id = 0) => id < pieces.Count ? pieces[id] : null;

        public T GetFirstWith<T>() where T : Characteristic
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                IPiece piece = pieces[i];
                T c = piece.GetCharacteristic<T>();
                if (c != null) return c;
            }

            return null;
        }

        public T GetLastWith<T>() where T : Characteristic
        {
            for (int i = pieces.Count - 1; i >= 0; i--)
            {
                IPiece piece = pieces[i];
                T c = piece.GetCharacteristic<T>();
                if (c != null) return c;
            }

            return null;
        }

        public List<T> GetPiecesWith<T>() where T : Characteristic
        {
            List<T> returnValue = new();
            for (int i = 0; i < pieces.Count; i++)
            {
                IPiece piece = pieces[i];
                T c = piece.GetCharacteristic<T>();
                if (c != null) returnValue.Add(c);
            }

            return returnValue;
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

        /// <summary>
        /// Set color without altering the color list. Use SetColor instead
        /// </summary>
        /// <param name="color"></param>
        public void ApplyColor(Color color);

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
    }
}
