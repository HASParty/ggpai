namespace Boardgame.GDL {
    public class Cell {
        public Cell(string id, string type, int count = 0) {
            ID = id;
            Type = type;
            Count = count;
        }
        public string ID;
        public string Type;
        public int Count;

        public override string ToString() {
            return string.Format("( Cell id: {0}, type: {1}, count: {2} )", ID, Type, Count);
        }
    }
}
