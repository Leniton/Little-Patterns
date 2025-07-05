using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem.UI
{
    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField] private Image image;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public int id { get; set; } = (int)PieceType.generic;

        private Coordinate _coordinate;

        public Coordinate coordinate
        {
            get => _coordinate;
            set => _coordinate = value;
        }

        public Action onEnter { get; set; }
        public Action onExit { get; set; }
        public Action onClick { get; set; }
        public List<Characteristic> pieceCharacteristics { get; set; } = new();

        public void StylePiece(Sprite sprite, Color color)
        {
            image.sprite = sprite;
            image.color = color;
        }

        public void PositionPiece(Vector3 position, ITile tile)
        {
            //place him at new space
            transform.localPosition = position;
        }
    }
}