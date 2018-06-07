using Windows.Foundation;

namespace Russia2018.Model
{
    public class Shot
    {
        public Shot(Player player, Point target, double strength, int value)
        {
            this.Player = player;
            this.Target = target;
            this.Strength = strength;
            this.Value = value;
        }
        public Player Player { get; set; }
        public Point Target { get; set; }
        public double Strength { get; set; }
        public int Value { get; set; }
    }
}
