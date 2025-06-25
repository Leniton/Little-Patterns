using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridSystem
{
    public interface ITile
    {
        public List<IPiece> pieces { get; set; }

        public int pieceID { get; }

        [Flags]
        public enum State
        {
            Generic = 1,
            Selectable = 2,
            Valid = 4,
            Invalid = 8
        }

        public State state { get; set; }

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

        public void SetState(State newState)
        {
            state = newState;
            UpdateStateVisual();
        }
        
        public void AddState(State newState)
        {
            state |= newState;
            UpdateStateVisual();
        }
        
        public void ToggleState(State newState)
        {
            state ^= newState;
            UpdateStateVisual();
        }
        
        public void RemoveState(State newState)
        {
            state &= ~newState;
            UpdateStateVisual();
        }

        public void UpdateStateVisual();
    }
}
