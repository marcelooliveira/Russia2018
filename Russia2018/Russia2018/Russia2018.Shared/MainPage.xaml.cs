using Russia2018.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Russia2018
{
    public sealed partial class MainPage : Page, IGoalObserver
    {
        double ballStrength = 50;
        int PlayersByTeam = 11;
        List<TurnEvent> turnEvents = new List<TurnEvent>();
        List<Discoid> strokenDiscoids = new List<Discoid>();
        static List<string> logList = new List<string>();
        GameState currentGameState = GameState.None;
        List<Ball> pottedBalls = new List<Ball>();
        Random random = new Random(DateTime.Now.Millisecond);
        bool started = false;
        Game currentGame = null;
        List<Group> groups;
        Dictionary<string, SoccerTeam> teamsDictionary;
        static List<SoccerTeam> teams = new List<SoccerTeam>();
        DispatcherTimer movementTimer = new DispatcherTimer();
        DispatcherTimer clockTimer = new DispatcherTimer();
        BallFace bf;

        Ball ball;

        List<PlayerFace> playerFaces = new List<PlayerFace>();
        List<Discoid> discoids = new List<Discoid>();
        List<Discoid> strokenPlayers = new List<Discoid>();
        List<TableBorder> tableBorders = new List<TableBorder>();
        List<GoalPost> goalPosts = new List<GoalPost>();

        Point targetPoint = new Point(0, 0);
        Vector2D targetVector = new Vector2D(0, 0);
        Point strengthPointNW = new Point(0, 0);
        Point strengthPointSE = new Point(0, 0);

        ScoreControl scoreControl = new ScoreControl();
        List<Goal> goals = new List<Goal>();
        double lastTotalVelocity = 0;
        bool hasPendingGoalResolution = false;
        double fieldWidth;
        double fieldHeight;
        Point goalPost00Point;
        Point goalPost01Point;
        Point goalPost10Point;
        Point goalPost11Point;
        Storyboard sbStadiumScreen;
        List<Game> gameTable = new List<Game>();
        DateTime totalTime = new DateTime(1, 1, 1, 0, 30, 0);
        SoccerTeam selectedTeam;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            selectedTeam = e.Parameter as SoccerTeam;

            if (string.IsNullOrEmpty(selectedTeam.TeamID))
            {
                return;
            }

            GameHelper.Instance.IsMovingDiscoids = false;

            clockTimer.Tick += clockTimer_Tick;
            clockTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);

            movementTimer.Tick += movementTimer_Tick;
            movementTimer.Interval = new TimeSpan(0, 0, 0, 0, 3);
            movementTimer.Start();

            clockTimer.Start();

            groups = GameHelper.Instance.Groups;

            teamsDictionary = GameHelper.Instance.TeamsDictionary;

            LoadGameTable(gameTable);

            DateTime lastGameDate = new DateTime(2010, 06, 01);

            LoadBall();

            currentGame = GetNextGame(selectedTeam.TeamID, lastGameDate);

            strengthPointNW = new Point
            (
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value +
                this.colRightPosts.Width.Value,
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value +
                this.rowTopGoalArea.Height.Value
            );

            strengthPointSE = new Point
            (
                strengthPointNW.X + colMenu.Width.Value,
                strengthPointNW.Y +
                rowBottomGoalArea.Height.Value +
                rowBottomPenaltyArea.Height.Value
            );

            fieldWidth =
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value +
                this.colRightPosts.Width.Value;

            fieldHeight =
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value +
                this.rowTopGoalArea.Height.Value +
                this.rowBottomGoalArea.Height.Value +
                this.rowBottomPenaltyArea.Height.Value +
                this.rowBottomFieldLine.Height.Value;

            goalPost00Point = new Point(
                this.colLeftEscapeArea.Width.Value,
                this.rowTopEscapeArea.Height.Value +
                this.rowTopFieldLine.Height.Value +
                this.rowTopPenaltyArea.Height.Value);

            goalPost01Point = new Point(goalPost00Point.X, goalPost00Point.Y + rowTopGoalArea.Height.Value);

            goalPost10Point = new Point(
                this.colLeftEscapeArea.Width.Value +
                this.colLeftPosts.Width.Value +
                this.colLeftGoalArea.Width.Value +
                this.colLeftPenaltyArea.Width.Value +
                this.colHalfWay.Width.Value +
                this.colRightPenaltyArea.Width.Value +
                this.colRightGoalArea.Width.Value,
                goalPost00Point.Y);

            goalPost11Point = new Point(goalPost10Point.X, goalPost10Point.Y + rowTopGoalArea.Height.Value);

            discoids.Add(new GoalPost(new Vector2D(goalPost00Point.X, goalPost00Point.Y), new Vector2D(8, 8), "1001"));
            discoids.Add(new GoalPost(new Vector2D(goalPost01Point.X, goalPost01Point.Y), new Vector2D(8, 8), "1002"));
            discoids.Add(new GoalPost(new Vector2D(goalPost10Point.X, goalPost10Point.Y), new Vector2D(8, 8), "1003"));
            discoids.Add(new GoalPost(new Vector2D(goalPost11Point.X, goalPost11Point.Y), new Vector2D(8, 8), "1004"));

            goals.Add(new Goal(this, 1, new Point(0, goalPost00Point.Y), goalPost00Point, new Point(0, goalPost01Point.Y), goalPost01Point));
            goals.Add(new Goal(this, 2, goalPost10Point, new Point(goalPost10Point.X + colRightGoalArea.Width.Value, goalPost10Point.Y), goalPost11Point, new Point(goalPost11Point.X + colRightGoalArea.Width.Value, goalPost11Point.Y)));

            ball = new Ball(new Vector2D(550, 310), new Vector2D(36, 36), "9");
            ball.X = (goalPost00Point.X + goalPost10Point.X) / 2;
            ball.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;

            rootCanvas.Children.Add(bf);

            discoids.Add(ball);

            LoadPlayerFaces();

            ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);

            LayoutRoot.Background = new ImageBrush()
            {
                ImageSource =
                new BitmapImage(new Uri("ms-appx:///Images/Grass.png"))
            };

            tableBorders.Add(new TableBorder(0, -20, (int)fieldWidth, 20));
            tableBorders.Add(new TableBorder(0, (int)fieldHeight, (int)fieldWidth, 200));
            tableBorders.Add(new TableBorder(-20, 0, 20, (int)fieldHeight));
            tableBorders.Add(new TableBorder((int)fieldWidth, 0, 20, (int)fieldHeight));
            tableBorders.Add(new TableBorder(0, (int)goalPost00Point.Y, (int)colLeftEscapeArea.Width.Value, 10));
            tableBorders.Add(new TableBorder(0, (int)goalPost01Point.Y, (int)colLeftEscapeArea.Width.Value, 10));
            tableBorders.Add(new TableBorder((int)goalPost10Point.X, (int)goalPost10Point.Y, (int)colRightPosts.Width.Value, 10));
            tableBorders.Add(new TableBorder((int)goalPost11Point.X, (int)goalPost11Point.Y, (int)colRightPosts.Width.Value, 10));

            scoreControl.SetValue(Grid.ColumnProperty, 1);
            scoreControl.SetValue(Grid.RowProperty, 0);
            scoreControl.SetValue(Grid.ColumnSpanProperty, 3);
            scoreControl.SetValue(Grid.RowSpanProperty, 1);
            LayoutRoot.Children.Add(scoreControl);

            scoreControl.Team1Name = currentGame.Team1ID;
            scoreControl.Team1Score = currentGame.Scores[currentGame.Team1ID];
            scoreControl.Team2Name = currentGame.Team2ID;
            scoreControl.Team2Score = currentGame.Scores[currentGame.Team2ID];
            scoreControl.PlayingTeamID = currentGame.PlayingTeamID;

            this.Background = new SolidColorBrush(Colors.Black);

            sbStadiumScreen = new Storyboard()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 10))
            };

            DoubleAnimation translateAnimation = new DoubleAnimation()
            {
                From = 800,
                To = 0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3))
            };

            Storyboard.SetTarget(translateAnimation, lettersXTranslate);
            Storyboard.SetTargetProperty(translateAnimation, "X");

            DoubleAnimation lettersOpacityAnimation = new DoubleAnimation()
            {
                From = 0.8,
                To = 1.0,
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(lettersOpacityAnimation, grdBrightness);
            Storyboard.SetTargetProperty(lettersOpacityAnimation, "Opacity");

            DoubleAnimation screenOpacityAnimation = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                BeginTime = new TimeSpan(0, 0, 0, 0),
                Duration = new Duration(new TimeSpan(0, 0, 0, 4))
            };

            sbStadiumScreen.Children.Add(translateAnimation);
            sbStadiumScreen.Children.Add(lettersOpacityAnimation);
            sbStadiumScreen.Children.Add(screenOpacityAnimation);

            Storyboard.SetTarget(screenOpacityAnimation, grdStadiumScreen);
            Storyboard.SetTargetProperty(screenOpacityAnimation, "Opacity");

            Storyboard sbBallStrength = new Storyboard()
            {
                RepeatBehavior = RepeatBehavior.Forever
            };

            DoubleAnimation ballStrengthAngle = new DoubleAnimation()
            {
                From = 0.0,
                To = 360,
                Duration = new Duration(new TimeSpan(0, 0, 0, 3, 0)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            sbBallStrength.Children.Add(ballStrengthAngle);
            Storyboard.SetTarget(ballStrengthAngle, rtBallStrength);
            Storyboard.SetTargetProperty(ballStrengthAngle, "Angle");
            sbBallStrength.Begin();

        }

        private void LoadPlayerFaces()
        {
            int rootCanvasChildrenCount = rootCanvas.Children.Count();
            for (int i = rootCanvasChildrenCount - 1; i >= 0; i--)
            {
                if (rootCanvas.Children[i] is PlayerFace)
                {
                    PlayerFace pf = rootCanvas.Children[i] as PlayerFace;

                    pf.PointerReleased -= LayoutRoot_MouseLeftButtonUp;
                    pf.PointerEntered -= PlayerFace_MouseEnter;
                    pf.PointerExited -= PlayerFace_MouseLeave;

                    Player p = pf.Player;

                    discoids.Remove(pf.Player);
                    pf.Player = null;
                    playerFaces.Remove(pf);
                    rootCanvas.Children.RemoveAt(i);
                    currentGame.Teams[currentGame.Team1ID].players.Clear();
                    currentGame.Teams[currentGame.Team2ID].players.Clear();
                }
            }

            int[] classicalPlayerPositions = GetClassicalPlayerPositions();

            PlayersByTeam = classicalPlayerPositions.Length;

            for (int i = 0; i <= PlayersByTeam - 1; i++)
            {
                Player p1 = new Player(currentGame.Teams[currentGame.Team1ID], classicalPlayerPositions[i]);
                p1.Position.X = 0;
                p1.Position.Y = 0;

                if (i <= 11)
                    p1.IsPlaying = true;

                currentGame.Teams[currentGame.Team1ID].players.Add(p1);

                Player p2 = new Player(currentGame.Teams[currentGame.Team2ID], classicalPlayerPositions[i]);
                p2.Position.X = 0;
                p2.Position.Y = 0;

                if (i <= 11)
                    p2.IsPlaying = true;

                currentGame.Teams[currentGame.Team2ID].players.Add(p2);

                PlayerFace pf1 = new PlayerFace(p1);
                pf1.PointerReleased += LayoutRoot_MouseLeftButtonUp;
                pf1.PointerEntered += PlayerFace_MouseEnter;
                pf1.PointerExited += PlayerFace_MouseLeave;
                playerFaces.Add(pf1);

                PlayerFace pf2 = new PlayerFace(p2);
                pf2.PointerReleased += LayoutRoot_MouseLeftButtonUp;
                pf2.PointerEntered += PlayerFace_MouseEnter;
                pf2.PointerExited += PlayerFace_MouseLeave;
                playerFaces.Add(pf2);

                rootCanvas.Children.Add(pf1);
                rootCanvas.Children.Add(pf2);

                discoids.Add(p1);
                discoids.Add(p2);
            }
        }

        private static int[] GetClassicalPlayerPositions()
        {
            var teamLineUp = new Random(DateTime.Now.Millisecond).Next(0, 2);

            if (teamLineUp == 0)
            {
                return new int[] { 1, 3, 5, 7, 10, 11 };
            }
            else
            {
                return new int[] { 1, 4, 2, 8, 6, 9 };
            }
        }

        private void LoadBall()
        {
            bf = new BallFace("Jabulani", 06, Colors.White, 0x40, 0x00, 0x00, 0x8D, 0x00, 0x00, 0x8D, 0x00, 0x00, 1.0);
            bf.SetValue(Canvas.LeftProperty, (double)900);
            bf.SetValue(Canvas.TopProperty, (double)120);
            bf.Height = 13;
            bf.Width = 13;
        }

        private Game GetNextGame(string selectedTeamID, DateTime lastGameDate)
        {
            Game result = null;

            var gameQuery = from game in gameTable
                            where
                            (
                                (game.Team1.TeamID == selectedTeamID) ||
                                (game.Team2.TeamID == selectedTeamID)
                            ) &&
                            game.Date > lastGameDate
                            select game;

            result = gameQuery.OrderBy(e => e.Date).First();

            if (result.Team1ID != selectedTeamID)
            {
                result.SwapTeams();
            }

            return result;
        }

        private void LoadGameTable(List<Game> gameTable)
        {
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.a.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.b.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.c.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.d.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.e.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.f.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.g.matches);
            AddMatches(gameTable, GameHelper.Instance.WorldCupData.groups.h.matches);
        }

        private void AddMatches(List<Game> gameTable, List<Shared.Model.Match> matches)
        {
            var teams = GameHelper.Instance.WorldCupData.teams;
            var stadiums = GameHelper.Instance.WorldCupData.stadiums;
            foreach (var match in matches)
            {
                var team1 = teams.Where(t => t.id == match.home_team).Single();
                var team2 = teams.Where(t => t.id == match.away_team).Single();
                var stadium = stadiums.Where(s => s.id == match.stadium).Single();
                gameTable.Add(new Game("GRP", match.date, teamsDictionary[team1.fifaCode], teamsDictionary[team2.fifaCode], stadium.name, stadium.city));
            }
        }

        void PlayerFace_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            PlayerFace pf = (PlayerFace)sender;
            Player player = pf.Player;
            ShowPlayerInfo(player);
        }

        private void ShowPlayerInfo(Player player)
        {
            //var query = from tp in teamPlayers
            //            where tp.TeamID == currentGame.PlayingTeamID
            //            && tp.Number == Convert.ToInt32(player.Id)
            //            && player.Team.TeamID == currentGame.PlayingTeamID
            //            select tp;

            //if (query.Any())
            //{
            //    TeamPlayer teamPlayer = query.First();
            //    imgPlayer.ImageSource = new BitmapImage(new Uri(string.Format("https://api.fifa.com/api/v1/picture/players/2018fwc/{0}_sq-300_jpg?allowDefaultPicture=true", teamPlayer.ID), UriKind.Absolute));
            //    numPlayer.Text = teamPlayer.Number.ToString();
            //    txtPlayerName.Text = teamPlayer.Name.ToString();
            //    grdPlayerInfo.Visibility = Visibility.Visible;
            //}
        }

        void PlayerFace_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            grdPlayerInfo.Visibility = Visibility.Collapsed;
        }

        private void ResetPlayerPositions(SoccerTeam xt1, SoccerTeam xt2, Canvas xrootCanvas, List<Discoid> xdiscoids, double leftEndX, double rightEndX, double topEndY, double bottomEndY)
        {
            hasPendingGoalResolution = false;

            int columnCount1 = currentGame.Teams[currentGame.Team1ID].Formation.Length;
            int columnCount2 = currentGame.Teams[currentGame.Team2ID].Formation.Length;
            int currentColumn1 = 0;
            int currentColumn2 = 0;
            int currentRow1 = 0;
            int currentRow2 = 0;
            int rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
            int rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
            double columnWidth1 = ((rightEndX - leftEndX) / 2) / columnCount1;
            double columnWidth2 = ((rightEndX - leftEndX) / 2) / columnCount2;
            double rowHeight1 = 2 * ((bottomEndY - topEndY) / 2) / rowCount1;
            double rowHeight2 = 2 * ((bottomEndY - topEndY) / 2) / rowCount2;
            for (int i = 0; i <= PlayersByTeam - 1; i++)
            {
                rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
                rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
                double vOffset1 = ((bottomEndY - topEndY) / 2) - (rowCount1 * rowHeight1) / 2;
                double vOffset2 = ((bottomEndY - topEndY) / 2) - (rowCount2 * rowHeight2) / 2;

                Player p1 = currentGame.Teams[currentGame.Team1ID].players[i];
                Player p2 = currentGame.Teams[currentGame.Team2ID].players[i];

                p1.Position.X = leftEndX + columnWidth1 * (0.5) + columnWidth1 * currentColumn1;
                p1.Position.Y = topEndY + rowHeight1 * (0.5) + rowHeight1 * currentRow1 + vOffset1;

                currentRow1++;
                if (currentRow1 == rowCount1)
                {
                    currentRow1 = 0;
                    currentColumn1++;

                    if (currentColumn1 < currentGame.Teams[currentGame.Team1ID].Formation.Length)
                    {
                        rowCount1 = currentGame.Teams[currentGame.Team1ID].Formation[currentColumn1];
                        columnWidth1 = ((rightEndX - leftEndX) / 2) / columnCount1;
                        rowHeight1 = 2 * ((bottomEndY - topEndY) / 2) / rowCount1;
                    }
                }

                p2.Position.X = rightEndX - columnWidth2 * (0.5) - columnWidth2 * currentColumn2;
                p2.Position.Y = topEndY + rowHeight2 * (0.5) + rowHeight2 * currentRow2 + vOffset2;

                currentRow2++;
                if (currentRow2 == rowCount2)
                {
                    currentRow2 = 0;
                    currentColumn2++;

                    if (currentColumn2 < currentGame.Teams[currentGame.Team2ID].Formation.Length)
                    {
                        rowCount2 = currentGame.Teams[currentGame.Team2ID].Formation[currentColumn2];
                        columnWidth2 = ((rightEndX - leftEndX) / 2) / columnCount2;
                        rowHeight2 = 2 * ((bottomEndY - topEndY) / 2) / rowCount2;
                    }
                }
            }
        }

        void movementTimer_Tick(object sender, object e)
        {
            movementTimer.Stop();

            MoveResult moveResult = MoveDiscoids();

            if (moveResult != null)
            {
                if (moveResult.IsTurnOver && currentGame.PlayingTeamID == currentGame.Team2ID)
                {
                    GenerateBestShot();
                }
            }

            movementTimer.Start();

            //y = ax + b
            double a = (targetPoint.Y - (double)playerFaces[1].GetValue(Canvas.TopProperty)) / (targetPoint.X - (double)playerFaces[1].GetValue(Canvas.LeftProperty));
            //b = y - ax
            double b = targetPoint.Y - a * targetPoint.X;

            for (int i = 0; i < playerFaces.Count; i++)
            {
                Vector2D v = discoids[i].TranslateVelocity;
                double angle = (double)playerFaces[1].Angle;
                angle = (double)playerFaces[i].Angle + (v.Dot(v)) / 50;
                playerFaces[i].Angle = angle;

                double width = (double)playerFaces[i].GetValue(Canvas.WidthProperty);
                double height = (double)playerFaces[i].GetValue(Canvas.HeightProperty);
                playerFaces[i].SetValue(Canvas.LeftProperty, discoids[5 + i].Position.X - discoids[5 + i].Radius);
                playerFaces[i].SetValue(Canvas.TopProperty, discoids[5 + i].Position.Y - discoids[5 + i].Radius);
            }

            double ballAngle = (double)playerFaces[1].Angle;
            Vector2D v2 = ball.TranslateVelocity;
            ballAngle = (double)bf.Angle + (v2.Dot(v2)) / 10;
            bf.Angle = ballAngle;

            bf.SetValue(Canvas.LeftProperty, ball.Position.X - ball.Radius);
            bf.SetValue(Canvas.TopProperty, ball.Position.Y - ball.Radius);
        }

        void clockTimer_Tick(object sender, object e)
        {
            scoreControl.Time = scoreControl.Time.AddSeconds(1);
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            var position = e.GetCurrentPoint(rootCanvas).Position;

            if ((position.X > strengthPointNW.X) &&
                (position.X < strengthPointSE.X) &&
                (position.Y > strengthPointNW.Y) &&
                (position.Y < strengthPointSE.Y))
            {
                e.Handled = true;
                double relativeY = strengthPointSE.Y - position.Y;
                currentGame.Team1BallStrength = ((strengthPointSE.Y - position.Y) / (strengthPointSE.Y - strengthPointNW.Y)) * 100.0;

                imgBallStrength.Margin = new Thickness(0, position.Y - strengthPointNW.Y - imgBallStrength.ActualHeight / 2.0, 0, 0);
                brdStrength.Margin = new Thickness(8, position.Y - strengthPointNW.Y + imgBallStrength.ActualHeight / 2.0, 8, 8);
            }
            else if (GameHelper.Instance.CurrentMousePlayer == null && GameHelper.Instance.CurrentSelectedPlayer != null && !GameHelper.Instance.IsMovingDiscoids)
            {
                HitPlayer(position.X, position.Y);
            }
            else
            {
                if (GameHelper.Instance.CurrentMousePlayer != null)
                {
                    if (currentGame.PlayingTeamID == GameHelper.Instance.CurrentMousePlayer.Team.TeamID)
                    {
                        Player newSelectedPlayer = GameHelper.Instance.CurrentMousePlayer;
                        ChangePlayer(newSelectedPlayer);
                        GameHelper.Instance.CurrentSelectedPlayer = GameHelper.Instance.CurrentMousePlayer;
                    }
                }
            }
        }

        private void GenerateBestShot()
        {
            double x = 0;
            double y = 0;
            List<GhostBall> ghostBalls = GetGhostBalls(teamsDictionary[currentGame.PlayingTeamID], ball, false);

            GhostBall bestGhostBall = null;

            var ghostBallQuery = from gb in ghostBalls
                                 select gb;

            if (ghostBallQuery.Any())
            {
                bestGhostBall = ghostBallQuery.OrderBy(p => p.Difficulty).OrderBy(p => p.Player2BallDistance).First();
            }

            if (bestGhostBall != null)
            {
                x = bestGhostBall.Point.X;
                y = bestGhostBall.Point.Y;
                GameHelper.Instance.CurrentSelectedPlayer = bestGhostBall.Player;

                currentGame.Team2BallStrength = bestGhostBall.Player2BallDistance / 2;
                HitPlayer(x, y);
            }
        }

        private void ChangePlayer(Player newSelectedPlayer)
        {
            foreach (PlayerFace pf in playerFaces)
            {
                if (pf.Player != newSelectedPlayer)
                    pf.UnSelect();
                else
                    pf.Select();
            }
        }

        void HitPlayer(double x, double y)
        {
            if (scoreControl.Time.Minute == 0 && scoreControl.Time.Second == 0)
            {
                scoreControl.Time = new DateTime(1, 1, 1, 0, 0, 1);
                clockTimer.Start();
            }

            GameHelper.Instance.IsMovingDiscoids = true;
            turnEvents.Clear();
            foreach (PlayerFace pf in playerFaces)
            {
                pf.UnSelect();
            }

            started = true;

            if (currentGame.Team1ID == currentGame.PlayingTeamID)
            {
                ballStrength = currentGame.Team1BallStrength;
            }
            else
            {
                ballStrength = currentGame.Team2BallStrength;
            }

            double v = (ballStrength / 100.0) * 30.0;

            Player selectedPlayer = GameHelper.Instance.CurrentSelectedPlayer;

            ShowPlayerInfo(selectedPlayer);

            double dx = x - GameHelper.Instance.CurrentSelectedPlayer.Position.X;
            double dy = y - GameHelper.Instance.CurrentSelectedPlayer.Position.Y;
            double h = (float)(Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
            double sin = dy / h;
            double cos = dx / h;
            selectedPlayer.IsBallInGoal = false;
            selectedPlayer.TranslateVelocity = new Vector2D(v * cos, v * sin);
            Vector2D normalVelocity = new Vector2D(selectedPlayer.TranslateVelocity.X, selectedPlayer.TranslateVelocity.Y);
            normalVelocity.Normalize();

            //Calculates the top spin/back spin velocity, in the same direction as the normal velocity, but in opposite angle
            double topBottomVelocityRatio = selectedPlayer.TranslateVelocity.Length() * (targetVector.Y / 100.0f);
            selectedPlayer.VSpinVelocity = new Vector2D(-1.0f * topBottomVelocityRatio * normalVelocity.X, -1.0f * topBottomVelocityRatio * normalVelocity.Y);

            //xSound defines if the sound is coming from the left or the right
            double xSound = (double)(selectedPlayer.Position.X - 300.0f) / 300.0f;

        }

        private MoveResult MoveDiscoids()
        {
            if (!started)
                return null;

            MoveResult moveResult = new MoveResult() { DiscoidPositions = new List<DiscoidPosition>(), IsTurnOver = false };

            //Flag indicating that the program is still calculating
            //the positions, that is, the balls are still in an inconsistent state.

            foreach (Discoid discoid in discoids)
            {
                if (Math.Abs(discoid.Position.X) < 5 && Math.Abs(discoid.Position.Y) < 5 && Math.Abs(discoid.TranslateVelocity.X) < 10 && Math.Abs(discoid.TranslateVelocity.Y) < 10)
                {
                    discoid.Position.X =
                    discoid.Position.Y = 0;

                    discoid.TranslateVelocity = new Vector2D(0, 0);
                }
            }

            bool conflicted = true;

            //process this loop as long as some balls are still colliding
            while (conflicted)
            {
                conflicted = false;

                bool someCollision = true;
                while (someCollision)
                {
                    foreach (Goal goal in goals)
                    {
                        bool inGoal = goal.IsBallInGoal(ball);
                    }

                    someCollision = false;
                    foreach (Discoid discoidA in discoids)
                    {
                        if (discoidA is Player)
                        {
                            if (!((Player)discoidA).IsPlaying)
                                break;
                        }

                        //Resolve collisions between balls and each of the 6 borders in the table
                        RectangleCollision borderCollision = RectangleCollision.None;
                        foreach (TableBorder tableBorder in tableBorders)
                        {
                            borderCollision = tableBorder.Colliding(discoidA);

                            //if (borderCollision != RectangleCollision.None && !discoidA.IsBallInGoal)
                            if (borderCollision != RectangleCollision.None)
                            {
                                someCollision = true;
                                tableBorder.ResolveCollision(discoidA, borderCollision);
                            }
                        }

                        //Resolve collisions between players
                        foreach (Discoid discoidB in discoids)
                        {
                            if (discoidB is Player)
                            {
                                if (!((Player)discoidB).IsPlaying)
                                    break;
                            }

                            if (discoidA != discoidB)
                            {
                                if (discoidA.Colliding(discoidB))
                                {
                                    if ((discoidA is Player && discoidB is Ball) ||
                                        (discoidB is Player && discoidA is Ball))
                                    {
                                        Player p = null;
                                        Ball b = null;
                                        discoidA.HSpinVelocity =
                                        discoidB.HSpinVelocity =
                                        discoidA.VSpinVelocity =
                                        discoidB.VSpinVelocity = 
                                        discoidA.TranslateVelocity =
                                        discoidB.TranslateVelocity = Vector2D.Zero;
                                        if (discoidA is Player)
                                        {
                                            p = (Player)discoidA;
                                            b = (Ball)discoidB;
                                        }
                                        else
                                        {
                                            p = (Player)discoidB;
                                            b = (Ball)discoidA;
                                        }
                                        b.Y = p.Y;
                                        b.X = p.X + p.Radius * 1.5;

                                        //turnEvents.Add(new PlayerToBallContact(currentGame.PlayingTeamID, p, new Point(ball.Position.X, ball.Position.Y)));
                                    }
                                    else if (discoidA is Player && discoidB is Player)
                                    {
                                        if (
                                            (((Player)discoidA).Team.TeamID != currentGame.PlayingTeamID) ||
                                            ((Player)discoidB).Team.TeamID != currentGame.PlayingTeamID)
                                        {
                                            PlayerToPlayerContact p2p = new PlayerToPlayerContact(currentGame.PlayingTeamID, (Player)discoidA, (Player)discoidB, new Point(discoidA.Position.X, discoidA.Position.Y));

                                            var q = from te in turnEvents
                                                    where te is PlayerToBallContact
                                                    where ((PlayerToBallContact)te).Player.Team.TeamID == currentGame.PlayingTeamID
                                                    select te;

                                            if (!q.Any())
                                            {
                                                turnEvents.Add(p2p);
                                                Player p = null;
                                                if (((Player)discoidA).Team.TeamID == currentGame.PlayingTeamID)
                                                {
                                                    p = (Player)discoidA;
                                                }
                                                else
                                                {
                                                    p = (Player)discoidB;
                                                }
                                            }
                                        }
                                    }


                                    if (discoidA.Points == 0)
                                    {
                                        strokenPlayers.Add(discoidB);
                                    }
                                    else if (discoidB.Points == 0)
                                    {
                                        strokenPlayers.Add(discoidA);
                                    }

                                    while (discoidA.Colliding(discoidB))
                                    {
                                        someCollision = true;
                                        discoidA.ResolveCollision(discoidB);
                                    }
                                }
                            }
                        }

                        //Calculate ball's translation velocity (movement) as well as the spin velocity.
                        //The friction coefficient is used to decrease ball's velocity
                        if (discoidA.TranslateVelocity.X != 0.0d ||
                            discoidA.TranslateVelocity.Y != 0.0d)
                        {
                            double signalXVelocity = discoidA.TranslateVelocity.X >= 0.0f ? 1.0f : -1.0f;
                            double signalYVelocity = discoidA.TranslateVelocity.Y >= 0.0f ? 1.0f : -1.0f;
                            double absXVelocity = Math.Abs(discoidA.TranslateVelocity.X);
                            double absYVelocity = Math.Abs(discoidA.TranslateVelocity.Y);

                            Vector2D absVelocity = new Vector2D(absXVelocity, absYVelocity);

                            Vector2D normalizedDiff = new Vector2D(absVelocity.X, absVelocity.Y);
                            normalizedDiff.Normalize();

                            absVelocity.X = absVelocity.X * (1.0f - discoidA.Friction) - normalizedDiff.X * discoidA.Friction;
                            absVelocity.Y = absVelocity.Y * (1.0f - discoidA.Friction) - normalizedDiff.Y * discoidA.Friction;

                            if (absVelocity.X < 0f)
                                absVelocity.X = 0f;

                            if (absVelocity.Y < 0f)
                                absVelocity.Y = 0f;

                            double vx = absVelocity.X * signalXVelocity;
                            double vy = absVelocity.Y * signalYVelocity;

                            if (double.IsNaN(vx))
                                vx = 0;

                            if (double.IsNaN(vy))
                                vy = 0;

                            discoidA.TranslateVelocity = new Vector2D(vx, vy);
                        }

                        //Calculate ball's translation velocity (movement) as well as the spin velocity.
                        //The friction coefficient is used to decrease ball's velocity
                        if (discoidA.VSpinVelocity.X != 0.0d || discoidA.VSpinVelocity.Y != 0.0d)
                        {
                            double signalXVelocity = discoidA.VSpinVelocity.X >= 0.0f ? 1.0f : -1.0f;
                            double signalYVelocity = discoidA.VSpinVelocity.Y >= 0.0f ? 1.0f : -1.0f;
                            double absXVelocity = Math.Abs(discoidA.VSpinVelocity.X);
                            double absYVelocity = Math.Abs(discoidA.VSpinVelocity.Y);

                            Vector2D absVelocity = new Vector2D(absXVelocity, absYVelocity);

                            Vector2D normalizedDiff = new Vector2D(absVelocity.X, absVelocity.Y);
                            normalizedDiff.Normalize();

                            absVelocity.X = absVelocity.X - normalizedDiff.X * discoidA.Friction / 1.2f;
                            absVelocity.Y = absVelocity.Y - normalizedDiff.Y * discoidA.Friction / 1.2f;

                            if (absVelocity.X < 0f)
                                absVelocity.X = 0f;

                            if (absVelocity.Y < 0f)
                                absVelocity.Y = 0f;

                            discoidA.VSpinVelocity = new Vector2D(absVelocity.X * signalXVelocity, absVelocity.Y * signalYVelocity);
                        }
                    }

                    //Calculate the ball position based on both the ball's translation velocity and vertical spin velocity.
                    foreach (Discoid discoid in discoids)
                    {
                        discoid.Position.X += discoid.TranslateVelocity.X + discoid.VSpinVelocity.X;
                        discoid.Position.Y += discoid.TranslateVelocity.Y + discoid.VSpinVelocity.Y;
                    }
                }

                conflicted = false;
            }


            double totalVelocity = 0;
            foreach (Discoid discoid in discoids)
            {
                totalVelocity += discoid.TranslateVelocity.X;
                totalVelocity += discoid.TranslateVelocity.Y;
            }

            bool isTurnOver = false;
            if (Math.Abs(totalVelocity) < 0.005 && Math.Abs(lastTotalVelocity) > 0)
            {
                totalVelocity = 0;
                foreach (Discoid d in discoids)
                {
                    d.TranslateVelocity = new Vector2D(0, 0);
                }
                //MoveDiscoids();
                //poolState = PoolState.AwaitingShot;
                if (!goals[0].IsBallInGoal(ball) && !goals[1].IsBallInGoal(ball))
                {
                    ball.IsBallInGoal = false;
                }

                if (Math.Abs(lastTotalVelocity) > 0 && hasPendingGoalResolution)
                {
                    //ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);
                    //ball.X = (goalPost00Point.X + goalPost10Point.X) / 2;
                    //ball.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;
                }
                ProcessAfterTurn();
                isTurnOver = true;
                GameHelper.Instance.IsMovingDiscoids = false;

                if (scoreControl.Time > totalTime)
                {
                    SoccerTeam selectedTeam = currentGame.Team1;
                    currentGame = GetNextGame(selectedTeam.TeamID, currentGame.Date);

                    LoadPlayerFaces();
                    ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);

                    scoreControl.Team1Name = currentGame.Team1ID;
                    scoreControl.Team1Score = currentGame.Scores[currentGame.Team1ID];
                    scoreControl.Team2Name = currentGame.Team2ID;
                    scoreControl.Team2Score = currentGame.Scores[currentGame.Team2ID];
                    scoreControl.PlayingTeamID = currentGame.PlayingTeamID;
                    scoreControl.Time = new DateTime(1, 1, 1, 0, 0, 1);

                    ball.ResetPosition();
                    bf.SetValue(Canvas.LeftProperty, ball.Position.X - ball.Radius);
                    bf.SetValue(Canvas.TopProperty, ball.Position.Y - ball.Radius);
                }
            }
            lastTotalVelocity = totalVelocity;

            //playerPositionList = GetBallPositionList();

            return new MoveResult() { DiscoidPositions = moveResult.DiscoidPositions, IsTurnOver = isTurnOver };
        }

        private void rootCanvas_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
        }

        #region IGoalObserver Members

        public void BallEnteredGoal(Goal goal, Ball ball)
        {
            turnEvents.Add(new BallEnteredGoal(currentGame.PlayingTeamID, goal, new Point(ball.Position.X, ball.Position.Y)));
            pottedBalls.Add(ball);
            //if (ball.LastGoalID == 1)
            //{
            //    scoreControl.Team2Score++;
            //}
            //else
            //{
            //    scoreControl.Team1Score++;
            //}
            //letterContainer.Visibility = Visibility.Visible;
            //sbLetters.Begin();
            //hasPendingGoalResolution = true;
        }

        public void BallLeftGoal(Goal goal, Ball ball)
        {

        }

        #endregion

        private List<GhostBall> GetGhostBalls(SoccerTeam team, Ball ball, bool despair)
        {
            List<GhostBall> ghostBalls = new List<GhostBall>();

            int i = 0;
            foreach (Player player in team.players)
            {
                if (player.IsPlaying)
                {
                    foreach (Goal goal in goals)
                    {
                        List<Point> hotSpots = goal.HotSpots;

                        foreach (Point hotSpot in hotSpots)
                        {
                            //distances between goal hotspot and ball center
                            double dxGoalBallOn = hotSpot.X - ball.Position.X;
                            double dyGoalBallOn = hotSpot.Y - ball.Position.Y;
                            double hGoalBallOn = Math.Sqrt(dxGoalBallOn * dxGoalBallOn + dyGoalBallOn * dyGoalBallOn);
                            double a = dyGoalBallOn / dxGoalBallOn;

                            //distances between ball on center and ghost ball center
                            double hBallOnGhost = (ball.Radius - 1.5) * 2.0;
                            double dxBallOnGhost = hBallOnGhost * (dxGoalBallOn / hGoalBallOn);
                            double dyBallOnGhost = hBallOnGhost * (dyGoalBallOn / hGoalBallOn);

                            //ghost ball coordinates
                            double gX = ball.Position.X - dxBallOnGhost;
                            double gY = ball.Position.Y - dyBallOnGhost;
                            double dxGhostPlayer = player.Position.X - gX;
                            double dyGhostPlayer = player.Position.Y - gY;
                            double hGhostPlayer = Math.Sqrt(dxGhostPlayer * dxGhostPlayer + dyGhostPlayer * dyGhostPlayer);

                            //distances between ball center and player center
                            double dxBallOnPlayer = ball.Position.X - player.Position.X;
                            double dyBallOnPlayer = ball.Position.Y - player.Position.Y;
                            double hBallOnPlayer = Math.Sqrt(dxBallOnPlayer * dxBallOnPlayer + dyBallOnPlayer * dyBallOnPlayer);

                            //if (((Math.Sign(dxGoalBallOn) == Math.Sign(dxBallOnPlayer) || dxGoalBallOn == 0) && (Math.Sign(dyGoalBallOn) == Math.Sign(dyBallOnPlayer) || dyGoalBallOn == 0)))
                            //{
                            //    GhostBall ghostBall = new GhostBall(player, new Point(gX, gY), hBallOnPlayer, a, 0);
                            //    ghostBalls.Add(ghostBall);
                            //    i++;
                            //}

                            int value = 0;
                            if (
                                !(((Math.Sign(dxGoalBallOn) == Math.Sign(dxBallOnPlayer) || dxGoalBallOn == 0) &&
                                (Math.Sign(dyGoalBallOn) == Math.Sign(dyBallOnPlayer) || dyGoalBallOn == 0)))
                                )
                            {
                                value += 10;
                            }

                            GhostBall ghostBall = new GhostBall(player, new Point(gX, gY), hBallOnPlayer, a, value);
                            ghostBalls.Add(ghostBall);
                            i++;
                        }
                    }
                }
            }

            return ghostBalls;
        }

        private void ProcessAfterTurn()
        {
            currentGame.Fouls[currentGame.PlayingTeamID] = 0;

            //List<Discoid> players = from d in discoids.Where( e => e is Player).ToList();

            foreach (Ball ball in pottedBalls)
            {
                if (ball.Points == 0)
                {
                    //ball.ResetPositionAt(ball.InitPosition.X, ball.InitPosition.Y);
                    ball.IsBallInGoal = false;
                }
                else if (ball.Points > 1)
                {
                    //if (fallenRedCount < redCount || teams[playingTeamID - 1].BallOn.Points != ball.Points)
                    //{
                    //if (currentGameState != GameState.TestShot)
                    //{
                    //    if (fallenRedCount < redCount)
                    //        logList.Add(string.Format("{0} is back to table (there are still red balls on the table)", ball.Id));
                    //    else if (teams[playingTeamID - 1].BallOn.Points != ball.Points)
                    //        logList.Add(string.Format("{0} is back to table (expected: {1})", ball.Id, teams[playingTeamID - 1].BallOn.Id));
                    //}

                    //for (int points = ball.Points; points > 1; points--)
                    //{
                    //    Ball candidateBall = GetCandidateBall(ball, points);
                    //    if (candidateBall != null)
                    //    {
                    //        ball.ResetPositionAt(candidateBall.InitPosition.X, candidateBall.InitPosition.Y);
                    //        ball.IsBallInPocket = false;
                    //        break;
                    //    }
                    //}
                    //}
                }
            }

            //if (teams[playingTeamID - 1].BallOn == null)
            //    teams[playingTeamID - 1].BallOn = ballSprites[1];

            bool touchedOpponentBeforeBall = false;
            bool touchedBall = false;

            int strokenDiscoidsCount = 0;
            foreach (Discoid strokenDiscoid in strokenDiscoids)
            {
                if (strokenDiscoid is Ball)
                {
                    touchedBall = true;
                    break;
                }
                else
                {
                    Player strokenPlayer = (Player)strokenDiscoid;
                    //causing the player to first hit an opponent player before hitting the ball
                    if ((strokenPlayer.Team.TeamID != currentGame.PlayingTeamID) && !touchedBall)
                    {
                        touchedOpponentBeforeBall = true;
                        break;
                    }
                }

                //if (strokenDiscoidsCount == 0)
                //{
                //    if (ball.Points != teams[playingTeamID - 1].BallOn.Points)
                //    {
                //        if (ball.Points == 1 || teams[playingTeamID - 1].BallOn.Points == 1 || fallenRedCount == redCount)
                //        {
                //            if (currentGameState != GameState.TestShot)
                //                logList.Add(string.Format("foul: {0} was touched first, while {1} was expected", ball.Id, (fallenRedCount < redCount && teams[playingTeamID - 1].BallOn.Points > 1) ? "some color ball" : teams[playingTeamID - 1].BallOn.Id));

                //            teams[playingTeamID - 1].FoulList.Add((teams[playingTeamID - 1].BallOn.Points < 4 ? 4 : teams[playingTeamID - 1].BallOn.Points));
                //            break;
                //        }
                //    }
                //}

                strokenDiscoidsCount++;
            }

            ////Foul: causing the cue ball to miss all object balls
            //if (strokenDiscoidsCount == 0)
            //    teams[playingTeamID - 1].FoulList.Add(4);

            foreach (Ball ball in pottedBalls)
            {
                //causing the cue ball to enter a pocket
                if (!touchedOpponentBeforeBall)
                    currentGame.Scores[currentGame.PlayingTeamID]++;
            }

            //if (teams[playingTeamID - 1].FoulList.Count == 0)
            //{
            //    foreach (Ball ball in pottedBalls)
            //    {
            //        //legally potting reds or colors
            //        wonPoints += ball.Points;

            //        if (currentGameState != GameState.TestShot)
            //            logList.Add(string.Format("Player {0} won {1} points", teams[playingTeamID - 1].CurrentPlayer.Name, wonPoints));
            //    }
            //}
            //else
            //{
            //    teams[playingTeamID - 1].FoulList.Sort();
            //    lostPoints = teams[playingTeamID - 1].FoulList[teams[playingTeamID - 1].FoulList.Count - 1];

            //    if (currentGameState != GameState.TestShot)
            //        logList.Add(string.Format("Player {0} lost {1} points", teams[playingTeamID - 1].CurrentPlayer.Name, lostPoints));
            //}

            //teams[playingTeamID - 1].Points += wonPoints;
            //teams[awaitingTeamID - 1].Points += lostPoints;

            bool swappedPlayers = false;
            //check if it's other player's turn
            if ((!touchedBall || touchedOpponentBeforeBall) && currentGameState != GameState.TestShot)
            {
                swappedPlayers = true;
                //outgoingShot.HasFinishedTurn = true;
                //if (currentGameState != GameState.TestShot)
                //    logList.Add(string.Format("Player {0} has finished turn", teams[playingTeamID - 1].CurrentPlayer.Name));

                //Turnover();

                //cueSprite.Texture = (teams[playingTeamID - 1].Id == 1 ? cueTexture1 : cueTexture2);
            }

            foreach (TurnEvent turnEvent in turnEvents)
            {
                if (turnEvent is BallEnteredGoal)
                {
                    grdStadiumScreen.Visibility = Visibility.Visible;
                    sbStadiumScreen.Begin();
                    hasPendingGoalResolution = true;

                    if (ball.LastGoalID == 1)
                    {
                        scoreControl.Team2Score++;
                    }
                    else
                    {
                        scoreControl.Team1Score++;
                    }
                    Turnover(null);
                    ResetPlayerPositions(currentGame.Teams[currentGame.Team1ID], currentGame.Teams[currentGame.Team2ID], rootCanvas, discoids, goalPost00Point.X, goalPost10Point.X, rowTopEscapeArea.Height.Value, fieldHeight - rowTopEscapeArea.Height.Value);
                    ball.Position.X = (goalPost00Point.X + goalPost10Point.X) / 2;
                    ball.Position.Y = (rowTopEscapeArea.Height.Value + fieldHeight - rowTopEscapeArea.Height.Value) / 2;
                    break;
                }
                else if (turnEvent is PlayerToBallContact)
                {
                    Player p = ((PlayerToBallContact)turnEvent).Player;
                    if (p.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p);
                        break;
                    }
                }
                else if (turnEvent is PlayerToPlayerContact)
                {
                    Player p1 = ((PlayerToPlayerContact)turnEvent).Player1;
                    Player p2 = ((PlayerToPlayerContact)turnEvent).Player2;
                    //Player of the playing team touched an opponent
                    //player before the ball, so it's a foul
                    if (p1.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p1);
                        break;
                    }
                    else if (p2.Team.TeamID != currentGame.PlayingTeamID)
                    {
                        Turnover(p2);
                        break;
                    }
                }
            }

            if (turnEvents.Count == 0)
            {
                Turnover(null);
            }

            strokenDiscoids.Clear();
            pottedBalls.Clear();
        }

        private void Turnover(Player newSelectedPlayer)
        {
            foreach (PlayerFace pf in playerFaces)
            {
                if (pf.Player != newSelectedPlayer)
                    pf.UnSelect();
                else
                    pf.Select();
            }

            string auxID = currentGame.PlayingTeamID;
            currentGame.PlayingTeamID = currentGame.AwaitingTeamID;
            currentGame.AwaitingTeamID = auxID;
            scoreControl.PlayingTeamID = currentGame.PlayingTeamID;
            GameHelper.Instance.CurrentSelectedPlayer = newSelectedPlayer;
        }

        private void brdBallStrengthContainer_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            double y = e.GetCurrentPoint(brdBallStrengthContainer).Position.Y;
            if (y > (imgBallStrength.ActualHeight * 2.0) && y < (brdBallStrengthContainer.ActualHeight - imgBallStrength.ActualHeight))
            {
                ballStrength = 2 * (brdBallStrengthContainer.ActualHeight - y - imgBallStrength.ActualHeight);
            }
            imgBallStrength.Margin = new Thickness(0, y - imgBallStrength.ActualHeight / 2.0, 0, 0);
            brdStrength.Margin = new Thickness(8, y + imgBallStrength.ActualHeight / 2.0, 8, 8);
        }

    }

    public enum GameState
    {
        None,
        SignIn,
        Setup,
        ShowOpponents,
        Play,
        TestShot,
        GameOver
    }

    public enum FoulTypes
    {
        DirectFreeKick = 1,
        IndirectFreeKick = 2,
        PenaltyKick = 3
    }

}
