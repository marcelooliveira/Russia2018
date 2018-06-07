using Windows.Foundation;

namespace Russia2018.Model
{
    public class GhostBall
    {
        public GhostBall(Player player, Point point, double player2BallDistance, double ball2GoalDistance, int difficulty)
        {
            this.Player = player;
            this.Point = point;
            this.Player2BallDistance = player2BallDistance;
            this.Ball2GoalDistance = ball2GoalDistance;
            this.Difficulty = difficulty;
        }

        public Player Player { get; set; }
        public Point Point { get; set; }
        public double Player2BallDistance { get; set; }
        public double Ball2GoalDistance { get; set; }
        public int Difficulty { get; set; }
    }
}
