using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GridSystem
{
    public interface IPiece: IHover
    {
        public string Name { get; set; }
        public int id { get; set; }
        public Coordinate coordinate { get; set; }
        public Action onClick { get; set; }
        public List<Characteristic> pieceCharacteristics { get; set; }

        public void Initialize(int pieceId = 1, params Characteristic[] characteristics)
        {
            pieceCharacteristics.Clear();
            for (int i = 0; i < characteristics.Length; i++)
            {
                Characteristic characteristic = characteristics[i];
                pieceCharacteristics.Add(characteristic);
                characteristic.ModifyID(ref pieceId);
                characteristic.SetUp(this);
            }
            id = pieceId;
        }

        public void PositionPiece(Vector3 position, ITile tile);

        public bool AddCharacteristic<T>(T characteristic) where T : Characteristic
        {
            //check if characteristic (or one that inherits it) exists
            for (int i = 0; i < pieceCharacteristics.Count; i++)
            {
                if (pieceCharacteristics[i] is not T) continue;
                Debug.LogWarning($"Characteristic {pieceCharacteristics[i].GetType().Name} already exists!");
                return false;
            }

            //Debug.Log($"adding {characteristic.GetType().Name}");

            int modifiedId = id;
            characteristic.ModifyID(ref modifiedId);
            id = modifiedId;
            pieceCharacteristics.Add(characteristic);
            return true;
        }

        public T GetCharacteristic<T>() where T : Characteristic
        {
            T returnValue = null;
            for (int i = 0; i < pieceCharacteristics.Count; i++)
            {
                if (pieceCharacteristics[i] is not T) continue;
                returnValue = (T)pieceCharacteristics[i];
            }

            return returnValue;
        }

        public string CharacteristicsInfo()
        {
            StringBuilder value = new();
            for (int i = 0; i < pieceCharacteristics.Count; i++)
            {
                value.Append(pieceCharacteristics[i]);
                if (i < pieceCharacteristics.Count - 1) value.Append('\n');
            }

            return value.ToString();
        }

        public string ToString()
        {
            StringBuilder value = new(Name);
            value.Append($"({coordinate.x},{coordinate.y})");
            value.Append(CharacteristicsInfo());

            return value.ToString();
        }
    }
}