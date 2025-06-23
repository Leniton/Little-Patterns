
namespace GridSystem
{
    public abstract class Characteristic
    {
        public IPiece Piece { get; private set; }
        public virtual int idModifier => 0;
        public void ModifyID(ref int id) => id |= idModifier;
        public virtual void SetUp(IPiece _piece) => Piece = _piece;
    }
}