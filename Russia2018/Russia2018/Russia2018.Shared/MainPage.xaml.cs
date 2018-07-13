using Russia2018.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
        bool fallenBallsProcessed = false;
        PlayerState playerState = PlayerState.None;
        PlayerState lastPlayerState = PlayerState.None;
        List<Discoid> strokenDiscoids = new List<Discoid>();
        static List<string> logList = new List<string>();
        GameState currentGameState = GameState.None;
        List<Ball> pottedBalls = new List<Ball>();
        bool afterTurnProcessed = false;
        Random random = new Random(DateTime.Now.Millisecond);
        bool started = false;
        Game currentGame = null;
        List<Group> groups;
        Dictionary<string, SoccerTeam> teamsDictionary;
        static List<SoccerTeam> teams = new List<SoccerTeam>();
        List<TeamPlayer> teamPlayers = new List<TeamPlayer>();
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

        bool calculatingPositions = true;
        Grid scoreGrid = new Grid();
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
        string selectedTeamID;

        public MainPage() //: base(parameters)
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            selectedTeamID = e.Parameter as string;

            if (string.IsNullOrEmpty(selectedTeamID))
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

            LoadTeamPlayers(teamPlayers);

            LoadGameTable(gameTable);

            DateTime lastGameDate = new DateTime(2010, 06, 01);

            LoadBall();

            currentGame = GetNextGame(selectedTeamID, lastGameDate);

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

        private static void LoadTeamPlayers(List<TeamPlayer> teamPlayers)
        {
            //teamPlayers.Add(new TeamPlayer("ALG", 1, 185015, "Lounes GAOUAOUI"));

            teamPlayers.Add(new TeamPlayer("ARG", 1, 200999, "Nahuel GUZMAN"));
            teamPlayers.Add(new TeamPlayer("ARG", 12, 398422, "Franco ARMANI"));
            teamPlayers.Add(new TeamPlayer("ARG", 23, 181440, "Wilfredo CABALLERO"));
            teamPlayers.Add(new TeamPlayer("ARG", 2, 266796, "Gabriel MERCADO"));
            teamPlayers.Add(new TeamPlayer("ARG", 3, 308322, "Nicolas TAGLIAFICO"));
            teamPlayers.Add(new TeamPlayer("ARG", 4, 298593, "Cristian ANSALDI"));
            teamPlayers.Add(new TeamPlayer("ARG", 6, 266784, "Federico FAZIO"));
            teamPlayers.Add(new TeamPlayer("ARG", 8, 401204, "Marcos ACUNA"));
            teamPlayers.Add(new TeamPlayer("ARG", 14, 182372, "Javier MASCHERANO"));
            teamPlayers.Add(new TeamPlayer("ARG", 16, 318688, "Marcos ROJO"));
            teamPlayers.Add(new TeamPlayer("ARG", 17, 310116, "Nicolas OTAMENDI"));
            teamPlayers.Add(new TeamPlayer("ARG", 18, 268992, "Eduardo SALVIO"));
            teamPlayers.Add(new TeamPlayer("ARG", 5, 200133, "Lucas BIGLIA"));
            teamPlayers.Add(new TeamPlayer("ARG", 7, 266790, "Ever BANEGA"));
            teamPlayers.Add(new TeamPlayer("ARG", 11, 266800, "Angel DI MARIA"));
            teamPlayers.Add(new TeamPlayer("ARG", 13, 411433, "Maximiliano MEZA"));
            teamPlayers.Add(new TeamPlayer("ARG", 15, 316997, "Enzo PEREZ"));
            teamPlayers.Add(new TeamPlayer("ARG", 20, 395414, "Giovani LO CELSO"));
            teamPlayers.Add(new TeamPlayer("ARG", 22, 371828, "Cristian PAVON"));
            teamPlayers.Add(new TeamPlayer("ARG", 9, 271550, "Gonzalo HIGUAIN"));
            teamPlayers.Add(new TeamPlayer("ARG", 10, 229397, "Lionel MESSI"));
            teamPlayers.Add(new TeamPlayer("ARG", 19, 228528, "Sergio AGUERO"));
            teamPlayers.Add(new TeamPlayer("ARG", 21, 392905, "Paulo DYBALA"));

            teamPlayers.Add(new TeamPlayer("AUS", 1, 339117, "Mathew RYAN"));
            teamPlayers.Add(new TeamPlayer("AUS", 12, 181467, "Brad JONES"));
            teamPlayers.Add(new TeamPlayer("AUS", 18, 197419, "Danny VUKOVIC"));
            teamPlayers.Add(new TeamPlayer("AUS", 2, 331149, "Milos DEGENEK"));
            teamPlayers.Add(new TeamPlayer("AUS", 3, 397636, "James MEREDITH"));
            teamPlayers.Add(new TeamPlayer("AUS", 5, 197411, "Mark MILLIGAN"));
            teamPlayers.Add(new TeamPlayer("AUS", 6, 306404, "Matthew JURMAN"));
            teamPlayers.Add(new TeamPlayer("AUS", 16, 349342, "Aziz BEHICH"));
            teamPlayers.Add(new TeamPlayer("AUS", 19, 397717, "Joshua RISDON"));
            teamPlayers.Add(new TeamPlayer("AUS", 20, 339118, "Trent SAINSBURY"));
            teamPlayers.Add(new TeamPlayer("AUS", 8, 339116, "Massimo LUONGO"));
            teamPlayers.Add(new TeamPlayer("AUS", 13, 312252, "Aaron MOOY"));
            teamPlayers.Add(new TeamPlayer("AUS", 15, 197514, "Mile JEDINAK"));
            teamPlayers.Add(new TeamPlayer("AUS", 22, 355775, "Jackson IRVINE"));
            teamPlayers.Add(new TeamPlayer("AUS", 23, 367641, "Tom ROGIC"));
            teamPlayers.Add(new TeamPlayer("AUS", 4, 213001, "Tim CAHILL"));
            teamPlayers.Add(new TeamPlayer("AUS", 7, 321697, "Mathew LECKIE"));
            teamPlayers.Add(new TeamPlayer("AUS", 9, 379785, "Tomi JURIC"));
            teamPlayers.Add(new TeamPlayer("AUS", 10, 229043, "Robbie KRUSE"));
            teamPlayers.Add(new TeamPlayer("AUS", 11, 411241, "Andrew NABBOUT"));
            teamPlayers.Add(new TeamPlayer("AUS", 14, 368899, "Jamie MacLAREN"));
            teamPlayers.Add(new TeamPlayer("AUS", 17, 389503, "Daniel ARZANI"));
            teamPlayers.Add(new TeamPlayer("AUS", 21, 333076, "Dimitrios PETRATOS"));

            teamPlayers.Add(new TeamPlayer("BEL", 1, 358106, "Thibaut COURTOIS"));
            teamPlayers.Add(new TeamPlayer("BEL", 12, 298738, "Simon MIGNOLET"));
            teamPlayers.Add(new TeamPlayer("BEL", 13, 373315, "Koen CASTEELS"));
            teamPlayers.Add(new TeamPlayer("BEL", 2, 290864, "Toby ALDERWEIRELD"));
            teamPlayers.Add(new TeamPlayer("BEL", 3, 216880, "Thomas VERMAELEN"));
            teamPlayers.Add(new TeamPlayer("BEL", 4, 216004, "Vincent KOMPANY"));
            teamPlayers.Add(new TeamPlayer("BEL", 5, 290904, "Jan VERTONGHEN"));
            teamPlayers.Add(new TeamPlayer("BEL", 15, 358122, "Thomas MEUNIER"));
            teamPlayers.Add(new TeamPlayer("BEL", 20, 358118, "Dedryck BOYATA"));
            teamPlayers.Add(new TeamPlayer("BEL", 23, 402015, "Leander DENDONCKER"));
            teamPlayers.Add(new TeamPlayer("BEL", 6, 290821, "Axel WITSEL"));
            teamPlayers.Add(new TeamPlayer("BEL", 7, 358120, "Kevin DE BRUYNE"));
            teamPlayers.Add(new TeamPlayer("BEL", 8, 290902, "Marouane FELLAINI"));
            teamPlayers.Add(new TeamPlayer("BEL", 11, 398653, "Yannick CARRASCO"));
            teamPlayers.Add(new TeamPlayer("BEL", 16, 378834, "Thorgan HAZARD"));
            teamPlayers.Add(new TeamPlayer("BEL", 17, 401444, "Youri TIELEMANS"));
            teamPlayers.Add(new TeamPlayer("BEL", 19, 290825, "Moussa DEMBELE"));
            teamPlayers.Add(new TeamPlayer("BEL", 22, 358108, "Nacer CHADLI"));
            teamPlayers.Add(new TeamPlayer("BEL", 9, 358112, "Romelu LUKAKU"));
            teamPlayers.Add(new TeamPlayer("BEL", 10, 273996, "Eden HAZARD"));
            teamPlayers.Add(new TeamPlayer("BEL", 14, 358114, "Dries MERTENS"));
            teamPlayers.Add(new TeamPlayer("BEL", 18, 379910, "Adnan JANUZAJ"));
            teamPlayers.Add(new TeamPlayer("BEL", 21, 378835, "Michy BATSHUAYI"));

            teamPlayers.Add(new TeamPlayer("BRA", 1, 308370, "ALISSON"));
            teamPlayers.Add(new TeamPlayer("BRA", 16, 266773, "CASSIO"));
            teamPlayers.Add(new TeamPlayer("BRA", 23, 395427, "EDERSON"));
            teamPlayers.Add(new TeamPlayer("BRA", 2, 289964, "THIAGO SILVA"));
            teamPlayers.Add(new TeamPlayer("BRA", 3, 289958, "MIRANDA"));
            teamPlayers.Add(new TeamPlayer("BRA", 4, 398286, "PEDRO GEROMEL"));
            teamPlayers.Add(new TeamPlayer("BRA", 6, 217161, "FILIPE LUIS"));
            teamPlayers.Add(new TeamPlayer("BRA", 12, 218284, "MARCELO"));
            teamPlayers.Add(new TeamPlayer("BRA", 13, 332946, "MARQUINHOS"));
            teamPlayers.Add(new TeamPlayer("BRA", 14, 335656, "DANILO"));
            teamPlayers.Add(new TeamPlayer("BRA", 22, 266774, "FAGNER"));
            teamPlayers.Add(new TeamPlayer("BRA", 5, 308386, "CASEMIRO"));
            teamPlayers.Add(new TeamPlayer("BRA", 8, 218292, "RENATO AUGUSTO"));
            teamPlayers.Add(new TeamPlayer("BRA", 11, 308366, "PHILIPPE COUTINHO"));
            teamPlayers.Add(new TeamPlayer("BRA", 15, 362727, "PAULINHO"));
            teamPlayers.Add(new TeamPlayer("BRA", 17, 208016, "FERNANDINHO"));
            teamPlayers.Add(new TeamPlayer("BRA", 18, 394018, "FRED"));
            teamPlayers.Add(new TeamPlayer("BRA", 19, 218306, "WILLIAN"));
            teamPlayers.Add(new TeamPlayer("BRA", 7, 305070, "DOUGLAS COSTA"));
            teamPlayers.Add(new TeamPlayer("BRA", 9, 386559, "GABRIEL JESUS"));
            teamPlayers.Add(new TeamPlayer("BRA", 10, 314197, "NEYMAR"));
            teamPlayers.Add(new TeamPlayer("BRA", 20, 344547, "ROBERTO FIRMINO"));
            teamPlayers.Add(new TeamPlayer("BRA", 21, 329039, "TAISON"));

            teamPlayers.Add(new TeamPlayer("COL", 1, 228686, "David OSPINA"));
            teamPlayers.Add(new TeamPlayer("COL", 12, 360642, "Camilo VARGAS"));
            teamPlayers.Add(new TeamPlayer("COL", 22, 411340, "Jose CUADRADO"));
            teamPlayers.Add(new TeamPlayer("COL", 2, 200209, "Cristian ZAPATA"));
            teamPlayers.Add(new TeamPlayer("COL", 3, 228688, "Oscar MURILLO"));
            teamPlayers.Add(new TeamPlayer("COL", 4, 315614, "Santiago ARIAS"));
            teamPlayers.Add(new TeamPlayer("COL", 13, 395552, "Yerry MINA"));
            teamPlayers.Add(new TeamPlayer("COL", 17, 394500, "Johan MOJICA"));
            teamPlayers.Add(new TeamPlayer("COL", 18, 394417, "Farid DIAZ"));
            teamPlayers.Add(new TeamPlayer("COL", 23, 386013, "Davinson SANCHEZ"));
            teamPlayers.Add(new TeamPlayer("COL", 5, 394377, "Wilmar BARRIOS"));
            teamPlayers.Add(new TeamPlayer("COL", 6, 280487, "Carlos SANCHEZ"));
            teamPlayers.Add(new TeamPlayer("COL", 8, 198243, "Abel AGUILAR"));
            teamPlayers.Add(new TeamPlayer("COL", 10, 269058, "James RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("COL", 11, 309714, "Juan CUADRADO"));
            teamPlayers.Add(new TeamPlayer("COL", 15, 398409, "Mateus URIBE"));
            teamPlayers.Add(new TeamPlayer("COL", 16, 395550, "Jefferson  LERMA"));
            teamPlayers.Add(new TeamPlayer("COL", 20, 356704, "Juan QUINTERO"));
            teamPlayers.Add(new TeamPlayer("COL", 7, 349512, "Carlos BACCA"));
            teamPlayers.Add(new TeamPlayer("COL", 9, 229444, "Radamel FALCAO"));
            teamPlayers.Add(new TeamPlayer("COL", 14, 339731, "Luis MURIEL"));
            teamPlayers.Add(new TeamPlayer("COL", 19, 369538, "Miguel BORJA"));
            teamPlayers.Add(new TeamPlayer("COL", 21, 411341, "Jose IZQUIERDO"));

            teamPlayers.Add(new TeamPlayer("COS", 1, 199175, "Keylor NAVAS"));
            teamPlayers.Add(new TeamPlayer("COS", 18, 214874, "Patrick PEMBERTON"));
            teamPlayers.Add(new TeamPlayer("COS", 23, 270136, "Leonel MOREIRA"));
            teamPlayers.Add(new TeamPlayer("COS", 2, 339447, "Johnny ACOSTA"));
            teamPlayers.Add(new TeamPlayer("COS", 3, 268821, "Giancarlo GONZALEZ"));
            teamPlayers.Add(new TeamPlayer("COS", 4, 389725, "Ian SMITH"));
            teamPlayers.Add(new TeamPlayer("COS", 6, 312987, "Oscar DUARTE"));
            teamPlayers.Add(new TeamPlayer("COS", 8, 270149, "Bryan OVIEDO"));
            teamPlayers.Add(new TeamPlayer("COS", 15, 339797, "Francisco CALVO"));
            teamPlayers.Add(new TeamPlayer("COS", 16, 307026, "Cristian GAMBOA"));
            teamPlayers.Add(new TeamPlayer("COS", 19, 236536, "Kendall WASTON"));
            teamPlayers.Add(new TeamPlayer("COS", 22, 307016, "Kenner GUTIERREZ"));
            teamPlayers.Add(new TeamPlayer("COS", 5, 236530, "Celso BORGES"));
            teamPlayers.Add(new TeamPlayer("COS", 7, 183796, "Christian BOLANOS"));
            teamPlayers.Add(new TeamPlayer("COS", 9, 361029, "Daniel COLINDRES"));
            teamPlayers.Add(new TeamPlayer("COS", 10, 214876, "Bryan RUIZ"));
            teamPlayers.Add(new TeamPlayer("COS", 13, 356986, "Rodney WALLACE"));
            teamPlayers.Add(new TeamPlayer("COS", 14, 183794, "Randall AZOFEIFA"));
            teamPlayers.Add(new TeamPlayer("COS", 17, 307541, "Yeltsin TEJEDA"));
            teamPlayers.Add(new TeamPlayer("COS", 20, 270143, "David GUZMAN"));
            teamPlayers.Add(new TeamPlayer("COS", 11, 395354, "Johan VENEGAS"));
            teamPlayers.Add(new TeamPlayer("COS", 12, 307529, "Joel CAMPBELL"));
            teamPlayers.Add(new TeamPlayer("COS", 21, 270144, "Marcos URENA"));

            teamPlayers.Add(new TeamPlayer("CRO", 1, 369029, "Dominik LIVAKOVIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 12, 376287, "Lovre KALINIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 23, 299887, "Danijel SUBASIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 2, 336485, "Sime VRSALJKO"));
            teamPlayers.Add(new TeamPlayer("CRO", 3, 357988, "Ivan STRINIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 5, 297373, "Vedran CORLUKA"));
            teamPlayers.Add(new TeamPlayer("CRO", 6, 312432, "Dejan LOVREN"));
            teamPlayers.Add(new TeamPlayer("CRO", 13, 372987, "Tin JEDVAJ"));
            teamPlayers.Add(new TeamPlayer("CRO", 15, 372424, "Duje CALETA-CAR"));
            teamPlayers.Add(new TeamPlayer("CRO", 21, 299896, "Domagoj VIDA"));
            teamPlayers.Add(new TeamPlayer("CRO", 22, 375261, "Josip PIVARIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 7, 296633, "Ivan RAKITIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 8, 339987, "Mateo KOVACIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 10, 241559, "Luka MODRIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 11, 380000, "Marcelo BROZOVIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 14, 402149, "Filip BRADARIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 19, 357991, "Milan BADELJ"));
            teamPlayers.Add(new TeamPlayer("CRO", 4, 359381, "Ivan PERISIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 9, 336472, "Andrej KRAMARIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 16, 297379, "Nikola KALINIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 17, 375518, "Mario MANDZUKIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 18, 369058, "Ante REBIC"));
            teamPlayers.Add(new TeamPlayer("CRO", 20, 369057, "Marko PJACA"));

            teamPlayers.Add(new TeamPlayer("DEN", 1, 278388, "Kasper  SCHMEICHEL"));
            teamPlayers.Add(new TeamPlayer("DEN", 16, 318462, "Jonas LOSSL"));
            teamPlayers.Add(new TeamPlayer("DEN", 22, 401283, "Frederik RONNOW"));
            teamPlayers.Add(new TeamPlayer("DEN", 3, 358302, "Jannik VESTERGAARD"));
            teamPlayers.Add(new TeamPlayer("DEN", 4, 309962, "Simon KJAER"));
            teamPlayers.Add(new TeamPlayer("DEN", 5, 365738, "Jonas KNUDSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 6, 401281, "Andreas CHRISTENSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 13, 318485, "Mathias  JORGENSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 14, 318477, "Henrik DALSGAARD"));
            teamPlayers.Add(new TeamPlayer("DEN", 17, 395455, "Jens Stryger LARSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 2, 302550, "Michael KROHN-DEHLI"));
            teamPlayers.Add(new TeamPlayer("DEN", 7, 299421, "William KVIST"));
            teamPlayers.Add(new TeamPlayer("DEN", 8, 372855, "Thomas DELANEY"));
            teamPlayers.Add(new TeamPlayer("DEN", 10, 321716, "Christian ERIKSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 18, 405205, "Lukas LERAGER"));
            teamPlayers.Add(new TeamPlayer("DEN", 19, 313907, "Lasse SCHONE"));
            teamPlayers.Add(new TeamPlayer("DEN", 9, 360184, "Nicolai JORGENSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 11, 372857, "Martin BRAITHWAITE"));
            teamPlayers.Add(new TeamPlayer("DEN", 12, 402096, "Kasper DOLBERG"));
            teamPlayers.Add(new TeamPlayer("DEN", 15, 336131, "Viktor FISCHER"));
            teamPlayers.Add(new TeamPlayer("DEN", 20, 336133, "Yussuf Yurary POULSEN"));
            teamPlayers.Add(new TeamPlayer("DEN", 21, 360188, "Andreas CORNELIUS"));
            teamPlayers.Add(new TeamPlayer("DEN", 23, 395448, "Pione SISTO"));

            teamPlayers.Add(new TeamPlayer("EGY", 1, 155050, "ESSAM ELHADARY"));
            teamPlayers.Add(new TeamPlayer("EGY", 16, 184382, "SHERIF EKRAMY"));
            teamPlayers.Add(new TeamPlayer("EGY", 23, 408948, "MOHAMED ELSHENAWY"));
            teamPlayers.Add(new TeamPlayer("EGY", 2, 401838, "ALI GABR"));
            teamPlayers.Add(new TeamPlayer("EGY", 3, 256311, "AHMED ELMOHAMADY"));
            teamPlayers.Add(new TeamPlayer("EGY", 6, 344640, "AHMED HEGAZY"));
            teamPlayers.Add(new TeamPlayer("EGY", 7, 303682, "AHMED FATHI"));
            teamPlayers.Add(new TeamPlayer("EGY", 12, 306460, "AYMAN ASHRAF"));
            teamPlayers.Add(new TeamPlayer("EGY", 13, 356053, "MOHAMED ABDELSHAFY"));
            teamPlayers.Add(new TeamPlayer("EGY", 15, 411607, "MAHMOUD HAMDY"));
            teamPlayers.Add(new TeamPlayer("EGY", 20, 312951, "SAMIR SAAD"));
            teamPlayers.Add(new TeamPlayer("EGY", 4, 344652, "OMAR GABER"));
            teamPlayers.Add(new TeamPlayer("EGY", 5, 407023, "SAM MORSY"));
            teamPlayers.Add(new TeamPlayer("EGY", 8, 395858, "TAREK HAMED"));
            teamPlayers.Add(new TeamPlayer("EGY", 17, 332777, "MOHAMED ELNENY"));
            teamPlayers.Add(new TeamPlayer("EGY", 19, 230099, "ABDALLA SAID"));
            teamPlayers.Add(new TeamPlayer("EGY", 21, 363863, "TREZIGUET"));
            teamPlayers.Add(new TeamPlayer("EGY", 9, 339900, "MARWAN MOHSEN"));
            teamPlayers.Add(new TeamPlayer("EGY", 10, 344654, "MOHAMED SALAH"));
            teamPlayers.Add(new TeamPlayer("EGY", 11, 369215, "KAHRABA"));
            teamPlayers.Add(new TeamPlayer("EGY", 14, 401841, "RAMADAN SOBHY"));
            teamPlayers.Add(new TeamPlayer("EGY", 18, 230098, "SHIKABALA"));
            teamPlayers.Add(new TeamPlayer("EGY", 22, 363867, "AMR WARDA"));

            teamPlayers.Add(new TeamPlayer("ENG", 1, 336022, "Jordan PICKFORD"));
            teamPlayers.Add(new TeamPlayer("ENG", 13, 344536, "Jack BUTLAND"));
            teamPlayers.Add(new TeamPlayer("ENG", 23, 411304, "Nick POPE"));
            teamPlayers.Add(new TeamPlayer("ENG", 2, 356750, "Kyle WALKER"));
            teamPlayers.Add(new TeamPlayer("ENG", 3, 274036, "Danny ROSE"));
            teamPlayers.Add(new TeamPlayer("ENG", 5, 369434, "John STONES"));
            teamPlayers.Add(new TeamPlayer("ENG", 6, 407498, "Harry MAGUIRE"));
            teamPlayers.Add(new TeamPlayer("ENG", 12, 306504, "Kieran TRIPPIER"));
            teamPlayers.Add(new TeamPlayer("ENG", 15, 305764, "Gary CAHILL"));
            teamPlayers.Add(new TeamPlayer("ENG", 16, 358009, "Phil JONES"));
            teamPlayers.Add(new TeamPlayer("ENG", 17, 306520, "Fabian DELPH"));
            teamPlayers.Add(new TeamPlayer("ENG", 18, 299962, "Ashley  YOUNG"));
            teamPlayers.Add(new TeamPlayer("ENG", 22, 390761, "Trent ALEXANDER-ARNOLD"));
            teamPlayers.Add(new TeamPlayer("ENG", 4, 369400, "Eric  DIER"));
            teamPlayers.Add(new TeamPlayer("ENG", 7, 398743, "Jesse  LINGARD"));
            teamPlayers.Add(new TeamPlayer("ENG", 8, 356189, "Jordan HENDERSON"));
            teamPlayers.Add(new TeamPlayer("ENG", 20, 401298, "Dele ALLI"));
            teamPlayers.Add(new TeamPlayer("ENG", 21, 411303, "Ruben LOFTUS-CHEEK"));
            teamPlayers.Add(new TeamPlayer("ENG", 9, 369419, "Harry KANE"));
            teamPlayers.Add(new TeamPlayer("ENG", 10, 336043, "Raheem STERLING"));
            teamPlayers.Add(new TeamPlayer("ENG", 11, 400820, "Jamie VARDY"));
            teamPlayers.Add(new TeamPlayer("ENG", 14, 274034, "Danny WELBECK"));
            teamPlayers.Add(new TeamPlayer("ENG", 19, 401470, "Marcus RASHFORD"));

            teamPlayers.Add(new TeamPlayer("FRA", 1, 297105, "Hugo LLORIS"));
            teamPlayers.Add(new TeamPlayer("FRA", 16, 254133, "Steve MANDANDA"));
            teamPlayers.Add(new TeamPlayer("FRA", 23, 368840, "Alphonse AREOLA"));
            teamPlayers.Add(new TeamPlayer("FRA", 2, 411471, "Benjamin PAVARD"));
            teamPlayers.Add(new TeamPlayer("FRA", 3, 401459, "Presnel KIMPEMBE"));
            teamPlayers.Add(new TeamPlayer("FRA", 4, 359440, "Raphael VARANE"));
            teamPlayers.Add(new TeamPlayer("FRA", 5, 368846, "Samuel UMTITI"));
            teamPlayers.Add(new TeamPlayer("FRA", 17, 299876, "Adil RAMI"));
            teamPlayers.Add(new TeamPlayer("FRA", 19, 398682, "Djibril SIDIBE"));
            teamPlayers.Add(new TeamPlayer("FRA", 21, 411470, "Lucas HERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("FRA", 22, 335995, "Benjamin MENDY"));
            teamPlayers.Add(new TeamPlayer("FRA", 6, 367388, "Paul POGBA"));
            teamPlayers.Add(new TeamPlayer("FRA", 12, 404566, "Corentin TOLISSO"));
            teamPlayers.Add(new TeamPlayer("FRA", 13, 398681, "Ngolo KANTE"));
            teamPlayers.Add(new TeamPlayer("FRA", 14, 358014, "Blaise MATUIDI"));
            teamPlayers.Add(new TeamPlayer("FRA", 15, 319327, "Steven NZONZI"));
            teamPlayers.Add(new TeamPlayer("FRA", 7, 336435, "Antoine GRIEZMANN"));
            teamPlayers.Add(new TeamPlayer("FRA", 8, 402049, "Thomas LEMAR"));
            teamPlayers.Add(new TeamPlayer("FRA", 9, 358015, "Olivier GIROUD"));
            teamPlayers.Add(new TeamPlayer("FRA", 10, 389867, "Kylian MBAPPE"));
            teamPlayers.Add(new TeamPlayer("FRA", 11, 398680, "Ousmane DEMBELE"));
            teamPlayers.Add(new TeamPlayer("FRA", 18, 401458, "Nabil FEKIR"));
            teamPlayers.Add(new TeamPlayer("FRA", 20, 368965, "Florian THAUVIN"));

            teamPlayers.Add(new TeamPlayer("GER", 1, 228912, "Manuel NEUER"));
            teamPlayers.Add(new TeamPlayer("GER", 12, 274179, "Kevin TRAPP"));
            teamPlayers.Add(new TeamPlayer("GER", 22, 309302, "Marc-Andre TER STEGEN"));
            teamPlayers.Add(new TeamPlayer("GER", 2, 309312, "Marvin PLATTENHARDT"));
            teamPlayers.Add(new TeamPlayer("GER", 3, 401377, "Jonas HECTOR"));
            teamPlayers.Add(new TeamPlayer("GER", 4, 379736, "Matthias GINTER"));
            teamPlayers.Add(new TeamPlayer("GER", 5, 311150, "Mats HUMMELS"));
            teamPlayers.Add(new TeamPlayer("GER", 15, 395488, "Niklas SUELE"));
            teamPlayers.Add(new TeamPlayer("GER", 16, 379955, "Antonio RUEDIGER"));
            teamPlayers.Add(new TeamPlayer("GER", 17, 299442, "Jerome BOATENG"));
            teamPlayers.Add(new TeamPlayer("GER", 18, 386413, "Joshua KIMMICH"));
            teamPlayers.Add(new TeamPlayer("GER", 6, 196900, "Sami KHEDIRA"));
            teamPlayers.Add(new TeamPlayer("GER", 7, 358692, "Julian DRAXLER"));
            teamPlayers.Add(new TeamPlayer("GER", 8, 275162, "Toni KROOS"));
            teamPlayers.Add(new TeamPlayer("GER", 10, 305036, "Mesut OEZIL"));
            teamPlayers.Add(new TeamPlayer("GER", 13, 321722, "Thomas MUELLER"));
            teamPlayers.Add(new TeamPlayer("GER", 14, 379953, "Leon GORETZKA"));
            teamPlayers.Add(new TeamPlayer("GER", 19, 275630, "Sebastian RUDY"));
            teamPlayers.Add(new TeamPlayer("GER", 20, 385947, "Julian BRANDT"));
            teamPlayers.Add(new TeamPlayer("GER", 21, 358690, "Ilkay GUENDOGAN"));
            teamPlayers.Add(new TeamPlayer("GER", 9, 404357, "Timo WERNER"));
            teamPlayers.Add(new TeamPlayer("GER", 11, 352394, "Marco REUS"));
            teamPlayers.Add(new TeamPlayer("GER", 23, 216784, "Mario GOMEZ"));

            teamPlayers.Add(new TeamPlayer("ICE", 1, 359843, "Hannes HALLDORSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 12, 411385, "Frederik SCHRAM"));
            teamPlayers.Add(new TeamPlayer("ICE", 13, 406732, "Runar RUNARSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 2, 300344, "Birkir SAEVARSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 5, 401143, "Sverrir INGASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 6, 300343, "Ragnar SIGURDSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 14, 231598, "Kari ARNASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 15, 300369, "Holmar EYJOLFSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 18, 401146, "Hordur MAGNUSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 23, 300371, "Ari SKULASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 3, 411383, "Samuel FRIDJONSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 4, 411384, "Albert GUDMUNDSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 7, 300351, "Johann GUDMUNDSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 8, 300376, "Birkir BJARNASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 10, 300377, "Gylfi SIGURDSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 16, 216678, "Olafur SKULASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 17, 300382, "Aron GUNNARSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 19, 300380, "Rurik GISLASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 20, 300348, "Emil HALLFREDSSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 21, 401147, "Arnor TRAUSTASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 9, 359845, "Bjorn SIGURDARSON"));
            teamPlayers.Add(new TeamPlayer("ICE", 11, 359847, "Alfred FINNBOGASON"));
            teamPlayers.Add(new TeamPlayer("ICE", 22, 401145, "Jon BODVARSSON"));

            teamPlayers.Add(new TeamPlayer("IRN", 1, 380007, "Ali BEIRANVAND"));
            teamPlayers.Add(new TeamPlayer("IRN", 12, 397792, "Rashid MAZAHERI"));
            teamPlayers.Add(new TeamPlayer("IRN", 22, 411643, "Amir ABEDZADEH"));
            teamPlayers.Add(new TeamPlayer("IRN", 3, 288961, "Ehsan HAJI SAFI"));
            teamPlayers.Add(new TeamPlayer("IRN", 4, 406595, "Roozbeh CHESHMI"));
            teamPlayers.Add(new TeamPlayer("IRN", 5, 390537, "Milad MOHAMMADI"));
            teamPlayers.Add(new TeamPlayer("IRN", 8, 314249, "Morteza POURALIGANJI"));
            teamPlayers.Add(new TeamPlayer("IRN", 13, 379885, "Mohammad Reza KHANZADEH"));
            teamPlayers.Add(new TeamPlayer("IRN", 15, 251481, "Pejman MONTAZERI"));
            teamPlayers.Add(new TeamPlayer("IRN", 19, 371735, "Majid HOSSEINI"));
            teamPlayers.Add(new TeamPlayer("IRN", 23, 384797, "Ramin REZAEIAN"));
            teamPlayers.Add(new TeamPlayer("IRN", 2, 390535, "Mahdi TORABI"));
            teamPlayers.Add(new TeamPlayer("IRN", 6, 371736, "Saeid EZATOLAHI"));
            teamPlayers.Add(new TeamPlayer("IRN", 7, 213170, "Masoud SHOJAEI"));
            teamPlayers.Add(new TeamPlayer("IRN", 9, 346735, "Omid EBRAHIMI"));
            teamPlayers.Add(new TeamPlayer("IRN", 11, 384795, "Vahid AMIRI"));
            teamPlayers.Add(new TeamPlayer("IRN", 10, 330659, "Karim ANSARIFARD"));
            teamPlayers.Add(new TeamPlayer("IRN", 14, 411644, "Saman GHODDOS"));
            teamPlayers.Add(new TeamPlayer("IRN", 16, 362641, "Reza GHOOCHANNEJHAD"));
            teamPlayers.Add(new TeamPlayer("IRN", 17, 388475, "Mehdi TAREMI"));
            teamPlayers.Add(new TeamPlayer("IRN", 18, 379886, "Alireza JAHANBAKHSH"));
            teamPlayers.Add(new TeamPlayer("IRN", 20, 379887, "Sardar AZMOUN"));
            teamPlayers.Add(new TeamPlayer("IRN", 21, 196812, "Ashkan DEJAGAH"));

            teamPlayers.Add(new TeamPlayer("JPN", 1, 198117, "Eiji KAWASHIMA"));
            teamPlayers.Add(new TeamPlayer("JPN", 12, 356269, "Masaaki HIGASHIGUCHI"));
            teamPlayers.Add(new TeamPlayer("JPN", 23, 331184, "Kosuke NAKAMURA"));
            teamPlayers.Add(new TeamPlayer("JPN", 2, 331166, "Naomichi UEDA"));
            teamPlayers.Add(new TeamPlayer("JPN", 3, 384847, "Gen SHOJI"));
            teamPlayers.Add(new TeamPlayer("JPN", 5, 291372, "Yuto NAGATOMO"));
            teamPlayers.Add(new TeamPlayer("JPN", 6, 395318, "Wataru ENDO"));
            teamPlayers.Add(new TeamPlayer("JPN", 19, 350003, "Hiroki SAKAI"));
            teamPlayers.Add(new TeamPlayer("JPN", 20, 268477, "Tomoaki MAKINO"));
            teamPlayers.Add(new TeamPlayer("JPN", 21, 321736, "Gotoku SAKAI"));
            teamPlayers.Add(new TeamPlayer("JPN", 22, 271253, "Maya YOSHIDA"));
            teamPlayers.Add(new TeamPlayer("JPN", 4, 233500, "Keisuke HONDA"));
            teamPlayers.Add(new TeamPlayer("JPN", 7, 307719, "Gaku SHIBASAKI"));
            teamPlayers.Add(new TeamPlayer("JPN", 8, 347718, "Genki HARAGUCHI"));
            teamPlayers.Add(new TeamPlayer("JPN", 10, 268596, "Shinji KAGAWA"));
            teamPlayers.Add(new TeamPlayer("JPN", 11, 307739, "Takashi USAMI"));
            teamPlayers.Add(new TeamPlayer("JPN", 14, 271255, "Takashi INUI"));
            teamPlayers.Add(new TeamPlayer("JPN", 16, 356466, "Hotaru YAMAGUCHI"));
            teamPlayers.Add(new TeamPlayer("JPN", 17, 289027, "Makoto HASEBE"));
            teamPlayers.Add(new TeamPlayer("JPN", 18, 395327, "Ryota OHSHIMA"));
            teamPlayers.Add(new TeamPlayer("JPN", 9, 286278, "Shinji OKAZAKI"));
            teamPlayers.Add(new TeamPlayer("JPN", 13, 384846, "Yoshinori MUTO"));
            teamPlayers.Add(new TeamPlayer("JPN", 15, 275136, "Yuya OSAKO"));

            teamPlayers.Add(new TeamPlayer("KOR", 1, 274281, "KIM Seunggyu"));
            teamPlayers.Add(new TeamPlayer("KOR", 21, 268426, "KIM Jinhyeon"));
            teamPlayers.Add(new TeamPlayer("KOR", 23, 397753, "JO Hyeonwoo"));
            teamPlayers.Add(new TeamPlayer("KOR", 2, 363578, "LEE Yong"));
            teamPlayers.Add(new TeamPlayer("KOR", 3, 395087, "JUNG Seunghyun"));
            teamPlayers.Add(new TeamPlayer("KOR", 4, 411413, "OH Bansuk"));
            teamPlayers.Add(new TeamPlayer("KOR", 5, 329895, "YUN Youngsun"));
            teamPlayers.Add(new TeamPlayer("KOR", 6, 268411, "PARK Jooho"));
            teamPlayers.Add(new TeamPlayer("KOR", 12, 274285, "KIM Minwoo"));
            teamPlayers.Add(new TeamPlayer("KOR", 14, 329912, "HONG Chul"));
            teamPlayers.Add(new TeamPlayer("KOR", 19, 375754, "KIM Younggwon"));
            teamPlayers.Add(new TeamPlayer("KOR", 20, 336682, "JANG Hyunsoo"));
            teamPlayers.Add(new TeamPlayer("KOR", 22, 375627, "GO Yohan"));
            teamPlayers.Add(new TeamPlayer("KOR", 8, 390529, "JU Sejong"));
            teamPlayers.Add(new TeamPlayer("KOR", 10, 390081, "LEE Seungwoo"));
            teamPlayers.Add(new TeamPlayer("KOR", 13, 291393, "KOO Jacheol"));
            teamPlayers.Add(new TeamPlayer("KOR", 15, 356534, "JUNG Wooyoung"));
            teamPlayers.Add(new TeamPlayer("KOR", 16, 268406, "KI Sungyueng"));
            teamPlayers.Add(new TeamPlayer("KOR", 17, 390525, "LEE Jaesung"));
            teamPlayers.Add(new TeamPlayer("KOR", 18, 314722, "MOON Seonmin"));
            teamPlayers.Add(new TeamPlayer("KOR", 7, 307849, "SON Heungmin"));
            teamPlayers.Add(new TeamPlayer("KOR", 9, 321745, "KIM Shinwook"));
            teamPlayers.Add(new TeamPlayer("KOR", 11, 395083, "HWANG Heechan"));

            teamPlayers.Add(new TeamPlayer("MEX", 1, 195231, "Jose CORONA"));
            teamPlayers.Add(new TeamPlayer("MEX", 12, 175546, "Alfredo TALAVERA"));
            teamPlayers.Add(new TeamPlayer("MEX", 13, 215285, "Guillermo OCHOA"));
            teamPlayers.Add(new TeamPlayer("MEX", 2, 309592, "Hugo AYALA"));
            teamPlayers.Add(new TeamPlayer("MEX", 3, 395518, "Carlos SALCEDO"));
            teamPlayers.Add(new TeamPlayer("MEX", 4, 178119, "Rafael MARQUEZ"));
            teamPlayers.Add(new TeamPlayer("MEX", 15, 238112, "Hector MORENO"));
            teamPlayers.Add(new TeamPlayer("MEX", 16, 329092, "Hector HERRERA"));
            teamPlayers.Add(new TeamPlayer("MEX", 21, 400634, "Edson ALVAREZ"));
            teamPlayers.Add(new TeamPlayer("MEX", 5, 386332, "Erick GUTIERREZ"));
            teamPlayers.Add(new TeamPlayer("MEX", 6, 318612, "Jonathan DOS SANTOS"));
            teamPlayers.Add(new TeamPlayer("MEX", 7, 371044, "Miguel LAYUN"));
            teamPlayers.Add(new TeamPlayer("MEX", 10, 234551, "Giovani DOS SANTOS"));
            teamPlayers.Add(new TeamPlayer("MEX", 17, 314918, "Jesus CORONA"));
            teamPlayers.Add(new TeamPlayer("MEX", 18, 251352, "Andres GUARDADO"));
            teamPlayers.Add(new TeamPlayer("MEX", 20, 350124, "Javier AQUINO"));
            teamPlayers.Add(new TeamPlayer("MEX", 23, 402772, "Jesus GALLARDO"));
            teamPlayers.Add(new TeamPlayer("MEX", 8, 319311, "Marco FABIAN"));
            teamPlayers.Add(new TeamPlayer("MEX", 9, 356731, "Raul JIMENEZ"));
            teamPlayers.Add(new TeamPlayer("MEX", 11, 234552, "Carlos VELA"));
            teamPlayers.Add(new TeamPlayer("MEX", 14, 228599, "Javier HERNANDEZ"));
            teamPlayers.Add(new TeamPlayer("MEX", 19, 227851, "Oribe PERALTA"));
            teamPlayers.Add(new TeamPlayer("MEX", 22, 386337, "Hirving  LOZANO"));

            teamPlayers.Add(new TeamPlayer("MAR", 1, 356956, "Yassine BOUNOU"));
            teamPlayers.Add(new TeamPlayer("MAR", 12, 395755, "Monir EL KAJOUI"));
            teamPlayers.Add(new TeamPlayer("MAR", 22, 371553, "Ahmed TAGNAOUTI"));
            teamPlayers.Add(new TeamPlayer("MAR", 2, 400721, "Achraf HAKIMI"));
            teamPlayers.Add(new TeamPlayer("MAR", 3, 401882, "Hamza MENDYL"));
            teamPlayers.Add(new TeamPlayer("MAR", 4, 299073, "Manuel DA COSTA"));
            teamPlayers.Add(new TeamPlayer("MAR", 5, 305954, "Mehdi BENATIA"));
            teamPlayers.Add(new TeamPlayer("MAR", 6, 398599, "Ghanem SAISS"));
            teamPlayers.Add(new TeamPlayer("MAR", 17, 296750, "Nabil DIRAR"));
            teamPlayers.Add(new TeamPlayer("MAR", 7, 395759, "Hakim ZIYACH"));
            teamPlayers.Add(new TeamPlayer("MAR", 8, 232674, "Karim EL AHMADI"));
            teamPlayers.Add(new TeamPlayer("MAR", 10, 353205, "Younes BELHANDA"));
            teamPlayers.Add(new TeamPlayer("MAR", 11, 395756, "Faycal FAJR"));
            teamPlayers.Add(new TeamPlayer("MAR", 14, 296799, "Mbark BOUSSOUFA"));
            teamPlayers.Add(new TeamPlayer("MAR", 15, 372130, "Youssef AIT BENNASSER"));
            teamPlayers.Add(new TeamPlayer("MAR", 16, 291414, "Noureddine AMRABAT"));
            teamPlayers.Add(new TeamPlayer("MAR", 18, 372114, "Amine HARIT"));
            teamPlayers.Add(new TeamPlayer("MAR", 21, 372266, "Sofyan AMRABAT"));
            teamPlayers.Add(new TeamPlayer("MAR", 23, 290868, "Mehdi CARCELA"));
            teamPlayers.Add(new TeamPlayer("MAR", 9, 411678, "Ayoub EL KAABI"));
            teamPlayers.Add(new TeamPlayer("MAR", 13, 401884, "Khalid BOUTAIB"));
            teamPlayers.Add(new TeamPlayer("MAR", 19, 401885, "Youssef EN NESYRI"));
            teamPlayers.Add(new TeamPlayer("MAR", 20, 407125, "Aziz BOUHADDOUZ"));

            teamPlayers.Add(new TeamPlayer("NIG", 1, 273287, "Ikechukwu EZENWA"));
            teamPlayers.Add(new TeamPlayer("NIG", 16, 230229, "Daniel AKPEYI"));
            teamPlayers.Add(new TeamPlayer("NIG", 23, 372607, "Francis UZOHO"));
            teamPlayers.Add(new TeamPlayer("NIG", 2, 411700, "Bryan IDOWU"));
            teamPlayers.Add(new TeamPlayer("NIG", 3, 267647, "Elderson ECHIEJILE"));
            teamPlayers.Add(new TeamPlayer("NIG", 5, 395529, "William EKONG"));
            teamPlayers.Add(new TeamPlayer("NIG", 6, 402251, "Leon BALOGUN"));
            teamPlayers.Add(new TeamPlayer("NIG", 12, 369428, "Abdullahi SHEHU"));
            teamPlayers.Add(new TeamPlayer("NIG", 20, 407163, "Chidozie AWAZIEM"));
            teamPlayers.Add(new TeamPlayer("NIG", 21, 407161, "Tyronne EBUEHI"));
            teamPlayers.Add(new TeamPlayer("NIG", 22, 315424, "Kenneth OMERUO"));
            teamPlayers.Add(new TeamPlayer("NIG", 4, 369512, "Wilfred NDIDI"));
            teamPlayers.Add(new TeamPlayer("NIG", 8, 395531, "Oghenekaro ETEBO"));
            teamPlayers.Add(new TeamPlayer("NIG", 10, 234463, "John Obi MIKEL"));
            teamPlayers.Add(new TeamPlayer("NIG", 15, 329076, "Joel OBI"));
            teamPlayers.Add(new TeamPlayer("NIG", 17, 315429, "Ogenyi ONAZI"));
            teamPlayers.Add(new TeamPlayer("NIG", 19, 367504, "John OGU"));
            teamPlayers.Add(new TeamPlayer("NIG", 7, 344714, "Ahmed MUSA"));
            teamPlayers.Add(new TeamPlayer("NIG", 9, 312997, "Odion IGHALO"));
            teamPlayers.Add(new TeamPlayer("NIG", 11, 274090, "Victor MOSES"));
            teamPlayers.Add(new TeamPlayer("NIG", 13, 411702, "Simeon NWANKWO"));
            teamPlayers.Add(new TeamPlayer("NIG", 14, 372542, "Kelechi IHEANACHO"));
            teamPlayers.Add(new TeamPlayer("NIG", 18, 395533, "Alex IWOBI"));

            teamPlayers.Add(new TeamPlayer("PAN", 1, 213770, "Jaime PENEDO"));
            teamPlayers.Add(new TeamPlayer("PAN", 12, 198478, "Jose CALDERON"));
            teamPlayers.Add(new TeamPlayer("PAN", 22, 357006, "Alex RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("PAN", 2, 385031, "Michael MURILLO"));
            teamPlayers.Add(new TeamPlayer("PAN", 3, 337231, "Harold CUMMINGS"));
            teamPlayers.Add(new TeamPlayer("PAN", 4, 385030, "Fidel ESCOBAR"));
            teamPlayers.Add(new TeamPlayer("PAN", 5, 198481, "Roman TORRES"));
            teamPlayers.Add(new TeamPlayer("PAN", 13, 295901, "Adolfo MACHADO"));
            teamPlayers.Add(new TeamPlayer("PAN", 15, 337232, "Eric DAVIS"));
            teamPlayers.Add(new TeamPlayer("PAN", 17, 267688, "Luis OVALLE"));
            teamPlayers.Add(new TeamPlayer("PAN", 23, 201720, "Felipe BALOY"));
            teamPlayers.Add(new TeamPlayer("PAN", 6, 197912, "Gabriel GOMEZ"));
            teamPlayers.Add(new TeamPlayer("PAN", 8, 400416, "Edgar BARCENAS"));
            teamPlayers.Add(new TeamPlayer("PAN", 11, 267700, "Armando COOPER"));
            teamPlayers.Add(new TeamPlayer("PAN", 14, 392859, "Valentin PIMENTEL"));
            teamPlayers.Add(new TeamPlayer("PAN", 19, 411177, "Ricardo AVILA"));
            teamPlayers.Add(new TeamPlayer("PAN", 20, 269514, "Anibal GODOY"));
            teamPlayers.Add(new TeamPlayer("PAN", 21, 411176, "Jose Luis RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("PAN", 7, 202039, "Blas PEREZ"));
            teamPlayers.Add(new TeamPlayer("PAN", 9, 239360, "Gabriel TORRES"));
            teamPlayers.Add(new TeamPlayer("PAN", 10, 372342, "Ismael DIAZ"));
            teamPlayers.Add(new TeamPlayer("PAN", 16, 337225, "Abdiel ARROYO"));
            teamPlayers.Add(new TeamPlayer("PAN", 18, 213780, "Luis TEJADA"));

            teamPlayers.Add(new TeamPlayer("ARG", 1, 275261, "Pedro GALLESE"));
            teamPlayers.Add(new TeamPlayer("PER", 12, 275431, "Carlos CACEDA"));
            teamPlayers.Add(new TeamPlayer("PER", 21, 229480, "Jose CARVALLO"));
            teamPlayers.Add(new TeamPlayer("PER", 2, 202638, "Alberto RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("PER", 3, 306194, "Aldo CORZO"));
            teamPlayers.Add(new TeamPlayer("PER", 4, 398457, "Anderson SANTAMARIA"));
            teamPlayers.Add(new TeamPlayer("PER", 5, 368000, "Miguel ARAUJO"));
            teamPlayers.Add(new TeamPlayer("PER", 6, 397791, "Miguel TRAUCO"));
            teamPlayers.Add(new TeamPlayer("PER", 15, 228728, "Christian RAMOS"));
            teamPlayers.Add(new TeamPlayer("PER", 17, 349697, "Luis ADVINCULA"));
            teamPlayers.Add(new TeamPlayer("PER", 22, 398459, "Nilson LOYOLA"));
            teamPlayers.Add(new TeamPlayer("PER", 7, 349685, "Paolo HURTADO"));
            teamPlayers.Add(new TeamPlayer("PER", 8, 349700, "Christian CUEVA"));
            teamPlayers.Add(new TeamPlayer("PER", 13, 392903, "Renato TAPIA"));
            teamPlayers.Add(new TeamPlayer("PER", 14, 394981, "Andy POLO"));
            teamPlayers.Add(new TeamPlayer("PER", 16, 401945, "Wilder CARTAGENA"));
            teamPlayers.Add(new TeamPlayer("PER", 19, 349698, "Yoshimar YOTUN"));
            teamPlayers.Add(new TeamPlayer("PER", 23, 397696, "Pedro AQUINO"));
            teamPlayers.Add(new TeamPlayer("PER", 9, 225497, "Paolo GUERRERO"));
            teamPlayers.Add(new TeamPlayer("PER", 10, 201748, "Jefferson FARFAN"));
            teamPlayers.Add(new TeamPlayer("PER", 11, 349703, "Raul RUIDIAZ"));
            teamPlayers.Add(new TeamPlayer("PER", 18, 349696, "Andre CARRILLO"));
            teamPlayers.Add(new TeamPlayer("PER", 20, 394797, "Edison FLORES"));

            teamPlayers.Add(new TeamPlayer("POL", 1, 269746, "Wojciech SZCZESNY"));
            teamPlayers.Add(new TeamPlayer("POL", 12, 270890, "Bartosz BIALKOWSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 22, 216931, "Lukasz FABIANSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 2, 297356, "Michal PAZDAN"));
            teamPlayers.Add(new TeamPlayer("POL", 3, 270895, "Artur JEDRZEJCZYK"));
            teamPlayers.Add(new TeamPlayer("POL", 4, 398594, "Thiago CIONEK"));
            teamPlayers.Add(new TeamPlayer("POL", 5, 406995, "Jan BEDNAREK"));
            teamPlayers.Add(new TeamPlayer("POL", 15, 269695, "Kamil GLIK"));
            teamPlayers.Add(new TeamPlayer("POL", 18, 370435, "Bartosz BERESZYNSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 20, 216955, "Lukasz PISZCZEK"));
            teamPlayers.Add(new TeamPlayer("POL", 6, 402079, "Jacek GORALSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 8, 401277, "Karol LINETTY"));
            teamPlayers.Add(new TeamPlayer("POL", 10, 269735, "Grzegorz KRYCHOWIAK"));
            teamPlayers.Add(new TeamPlayer("POL", 11, 269706, "Kamil GROSICKI"));
            teamPlayers.Add(new TeamPlayer("POL", 13, 299687, "Maciej RYBUS"));
            teamPlayers.Add(new TeamPlayer("POL", 16, 216944, "Jakub BLASZCZYKOWSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 17, 216954, "Slawomir PESZKO"));
            teamPlayers.Add(new TeamPlayer("POL", 19, 370436, "Piotr ZIELINSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 21, 411483, "Rafal KURZAWA"));
            teamPlayers.Add(new TeamPlayer("POL", 7, 362823, "Arkadiusz MILIK"));
            teamPlayers.Add(new TeamPlayer("POL", 9, 299688, "Robert LEWANDOWSKI"));
            teamPlayers.Add(new TeamPlayer("POL", 14, 367469, "Lukasz TEODORCZYK"));
            teamPlayers.Add(new TeamPlayer("POL", 23, 411482, "Dawid KOWNACKI"));

            teamPlayers.Add(new TeamPlayer("POR", 1, 269768, "RUI PATRICIO"));
            teamPlayers.Add(new TeamPlayer("POR", 12, 373077, "ANTHONY LOPES"));
            teamPlayers.Add(new TeamPlayer("POR", 22, 214404, "BETO"));
            teamPlayers.Add(new TeamPlayer("POR", 2, 210213, "BRUNO ALVES"));
            teamPlayers.Add(new TeamPlayer("POR", 3, 275931, "PEPE"));
            teamPlayers.Add(new TeamPlayer("POR", 5, 395213, "RAPHAEL GUERREIRO"));
            teamPlayers.Add(new TeamPlayer("POR", 6, 398511, "JOSE FONTE"));
            teamPlayers.Add(new TeamPlayer("POR", 13, 384752, "RUBEN DIAS"));
            teamPlayers.Add(new TeamPlayer("POR", 15, 368706, "RICARDO"));
            teamPlayers.Add(new TeamPlayer("POR", 19, 336510, "MARIO RUI"));
            teamPlayers.Add(new TeamPlayer("POR", 21, 336491, "CEDRIC"));
            teamPlayers.Add(new TeamPlayer("POR", 4, 201099, "MANUEL FERNANDES"));
            teamPlayers.Add(new TeamPlayer("POR", 8, 200199, "JOAO MOUTINHO"));
            teamPlayers.Add(new TeamPlayer("POR", 10, 368700, "JOAO MARIO"));
            teamPlayers.Add(new TeamPlayer("POR", 11, 395205, "BERNARDO SILVA"));
            teamPlayers.Add(new TeamPlayer("POR", 14, 376349, "WILLIAM"));
            teamPlayers.Add(new TeamPlayer("POR", 16, 395206, "BRUNO FERNANDES"));
            teamPlayers.Add(new TeamPlayer("POR", 23, 319491, "ADRIEN SILVA"));
            teamPlayers.Add(new TeamPlayer("POR", 7, 201200, "CRISTIANO RONALDO"));
            teamPlayers.Add(new TeamPlayer("POR", 9, 384756, "ANDRE SILVA"));
            teamPlayers.Add(new TeamPlayer("POR", 17, 384751, "GONCALO GUEDES"));
            teamPlayers.Add(new TeamPlayer("POR", 18, 384766, "GELSON MARTINS"));
            teamPlayers.Add(new TeamPlayer("POR", 20, 189810, "RICARDO QUARESMA"));

            teamPlayers.Add(new TeamPlayer("RUS", 1, 215017, "Igor AKINFEEV"));
            teamPlayers.Add(new TeamPlayer("RUS", 12, 411573, "Andrei LUNEV"));
            teamPlayers.Add(new TeamPlayer("RUS", 20, 186785, "Vladimir GABULOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 2, 312866, "MARIO FERNANDES"));
            teamPlayers.Add(new TeamPlayer("RUS", 3, 404425, "Ilya KUTEPOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 4, 186787, "Sergey IGNASHEVICH"));
            teamPlayers.Add(new TeamPlayer("RUS", 5, 379985, "Andrey SEMENOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 13, 404424, "Fedor KUDRIASHOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 14, 358046, "Vladimir GRANAT"));
            teamPlayers.Add(new TeamPlayer("RUS", 23, 375357, "Igor SMOLNIKOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 6, 358883, "Denis CHERYSHEV"));
            teamPlayers.Add(new TeamPlayer("RUS", 7, 411572, "Daler KUZIAEV"));
            teamPlayers.Add(new TeamPlayer("RUS", 8, 379988, "Iury GAZINSKY"));
            teamPlayers.Add(new TeamPlayer("RUS", 9, 302539, "Alan DZAGOEV"));
            teamPlayers.Add(new TeamPlayer("RUS", 11, 404429, "Roman ZOBNIN"));
            teamPlayers.Add(new TeamPlayer("RUS", 15, 371765, "Alexey MIRANCHUK"));
            teamPlayers.Add(new TeamPlayer("RUS", 16, 411574, "Anton MIRANCHUK"));
            teamPlayers.Add(new TeamPlayer("RUS", 17, 371639, "Aleksandr GOLOVIN"));
            teamPlayers.Add(new TeamPlayer("RUS", 18, 216432, "Yury ZHIRKOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 19, 216435, "Alexander SAMEDOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 21, 404421, "Aleksandr EROKHIN"));
            teamPlayers.Add(new TeamPlayer("RUS", 10, 358871, "Fedor SMOLOV"));
            teamPlayers.Add(new TeamPlayer("RUS", 22, 358889, "Artem DZYUBA"));

            teamPlayers.Add(new TeamPlayer("KSA", 1, 404372, "ABDULLAH ALMUAIOUF"));
            teamPlayers.Add(new TeamPlayer("KSA", 21, 198514, "YASSER ALMOSAILEM"));
            teamPlayers.Add(new TeamPlayer("KSA", 22, 396885, "MOHAMMED ALOWAIS"));
            teamPlayers.Add(new TeamPlayer("KSA", 2, 347316, "MANSOUR ALHARBI"));
            teamPlayers.Add(new TeamPlayer("KSA", 3, 278167, "OSAMA HAWSAWI"));
            teamPlayers.Add(new TeamPlayer("KSA", 4, 407983, "ALI ALBULAYHI"));
            teamPlayers.Add(new TeamPlayer("KSA", 5, 385091, "OMAR HAWSAWI"));
            teamPlayers.Add(new TeamPlayer("KSA", 6, 397857, "MOHAMMED ALBURAYK"));
            teamPlayers.Add(new TeamPlayer("KSA", 13, 339482, "YASSER ALSHAHRANI"));
            teamPlayers.Add(new TeamPlayer("KSA", 23, 339477, "MOTAZ HAWSAWI"));
            teamPlayers.Add(new TeamPlayer("KSA", 7, 352891, "SALMAN ALFARAJ"));
            teamPlayers.Add(new TeamPlayer("KSA", 8, 347165, "YAHIA ALSHEHRI"));
            teamPlayers.Add(new TeamPlayer("KSA", 9, 339485, "HATAN BAHBRI"));
            teamPlayers.Add(new TeamPlayer("KSA", 11, 295184, "ABDULMALEK ALKHAIBRI"));
            teamPlayers.Add(new TeamPlayer("KSA", 12, 407993, "MOHAMED KANNO"));
            teamPlayers.Add(new TeamPlayer("KSA", 14, 339474, "ABDULLAH OTAYF"));
            teamPlayers.Add(new TeamPlayer("KSA", 15, 411624, "ABDULLAH ALKHAIBARI"));
            teamPlayers.Add(new TeamPlayer("KSA", 16, 390000, "HUSSAIN ALMOQAHWI"));
            teamPlayers.Add(new TeamPlayer("KSA", 17, 218083, "TAISEER ALJASSAM"));
            teamPlayers.Add(new TeamPlayer("KSA", 18, 339745, "SALEM ALDAWSARI"));
            teamPlayers.Add(new TeamPlayer("KSA", 10, 288078, "MOHAMMED ALSAHLAWI"));
            teamPlayers.Add(new TeamPlayer("KSA", 19, 339470, "FAHAD ALMUWALLAD"));
            teamPlayers.Add(new TeamPlayer("KSA", 20, 330914, "MUHANNAD ASIRI"));

            teamPlayers.Add(new TeamPlayer("SEN", 1, 395924, "Abdoulaye DIALLO"));
            teamPlayers.Add(new TeamPlayer("SEN", 16, 353111, "Khadim NDIAYE"));
            teamPlayers.Add(new TeamPlayer("SEN", 23, 408940, "Alfred GOMIS"));
            teamPlayers.Add(new TeamPlayer("SEN", 2, 408938, "Adama MBENGUE"));
            teamPlayers.Add(new TeamPlayer("SEN", 3, 339820, "Kalidou KOULIBALY"));
            teamPlayers.Add(new TeamPlayer("SEN", 4, 353758, "Kara MBODJI"));
            teamPlayers.Add(new TeamPlayer("SEN", 12, 368848, "Youssouf SABALY"));
            teamPlayers.Add(new TeamPlayer("SEN", 21, 319348, "Lamine GASSAMA"));
            teamPlayers.Add(new TeamPlayer("SEN", 22, 386256, "Moussa WAGUE"));
            teamPlayers.Add(new TeamPlayer("SEN", 5, 332314, "Idrissa Gana GUEYE"));
            teamPlayers.Add(new TeamPlayer("SEN", 6, 370169, "Salif SANE"));
            teamPlayers.Add(new TeamPlayer("SEN", 8, 353765, "Cheikhou KOUYATE"));
            teamPlayers.Add(new TeamPlayer("SEN", 11, 395925, "Cheikh NDOYE"));
            teamPlayers.Add(new TeamPlayer("SEN", 13, 274154, "Alfred NDIAYE"));
            teamPlayers.Add(new TeamPlayer("SEN", 17, 339919, "Pape Alioune NDIAYE"));
            teamPlayers.Add(new TeamPlayer("SEN", 7, 327043, "Moussa SOW"));
            teamPlayers.Add(new TeamPlayer("SEN", 9, 330687, "Mame DIOUF"));
            teamPlayers.Add(new TeamPlayer("SEN", 10, 353790, "Sadio MANE"));
            teamPlayers.Add(new TeamPlayer("SEN", 14, 353782, "Moussa KONATE"));
            teamPlayers.Add(new TeamPlayer("SEN", 15, 408939, "Diafra SAKHO"));
            teamPlayers.Add(new TeamPlayer("SEN", 18, 401889, "Ismaila SARR"));
            teamPlayers.Add(new TeamPlayer("SEN", 19, 336000, "Mbaye NIANG"));
            teamPlayers.Add(new TeamPlayer("SEN", 20, 401888, "Keita BALDE"));

            teamPlayers.Add(new TeamPlayer("SRB", 1, 214386, "Vladimir STOJKOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 12, 336393, "Predrag RAJKOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 23, 401177, "Marko DMITROVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 2, 291447, "Antonio RUKAVINA"));
            teamPlayers.Add(new TeamPlayer("SRB", 3, 212308, "Dusko TOSIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 5, 404632, "Uros SPAJIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 6, 214388, "Branislav IVANOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 11, 291438, "Aleksandar KOLAROV"));
            teamPlayers.Add(new TeamPlayer("SRB", 13, 385852, "Milos VELJKOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 14, 411548, "Milan RODIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 15, 411544, "Nikola MILENKOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 4, 365641, "Luka MILIVOJEVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 7, 375538, "Andrija ZIVKOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 10, 298698, "Dusan TADIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 16, 385537, "Marko GRUJIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 17, 401178, "Filip KOSTIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 20, 385545, "Sergej MILINKOVIC-SAVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 21, 310082, "Nemanja MATIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 22, 319554, "Adem LJAJIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 8, 294103, "Aleksandar PRIJOVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 9, 370456, "Aleksandar MITROVIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 18, 385893, "Nemanja RADONJIC"));
            teamPlayers.Add(new TeamPlayer("SRB", 19, 385536, "Luka JOVIC"));

            teamPlayers.Add(new TeamPlayer("ESP", 1, 269859, "David DE GEA"));
            teamPlayers.Add(new TeamPlayer("ESP", 13, 369110, "Kepa ARRIZABALAGA"));
            teamPlayers.Add(new TeamPlayer("ESP", 23, 175413, "Pepe REINA"));
            teamPlayers.Add(new TeamPlayer("ESP", 2, 380048, "Dani CARVAJAL"));
            teamPlayers.Add(new TeamPlayer("ESP", 3, 216973, "Gerard PIQUE"));
            teamPlayers.Add(new TeamPlayer("ESP", 4, 400711, "NACHO"));
            teamPlayers.Add(new TeamPlayer("ESP", 12, 407625, "Alvaro ODRIOZOLA"));
            teamPlayers.Add(new TeamPlayer("ESP", 14, 270948, "Cesar AZPILICUETA"));
            teamPlayers.Add(new TeamPlayer("ESP", 15, 216814, "Sergio RAMOS"));
            teamPlayers.Add(new TeamPlayer("ESP", 16, 299369, "Nacho MONREAL"));
            teamPlayers.Add(new TeamPlayer("ESP", 18, 306954, "Jordi ALBA"));
            teamPlayers.Add(new TeamPlayer("ESP", 5, 303034, "Sergio BUSQUETS"));
            teamPlayers.Add(new TeamPlayer("ESP", 6, 183857, "Andres INIESTA"));
            teamPlayers.Add(new TeamPlayer("ESP", 7, 369190, "SAUL"));
            teamPlayers.Add(new TeamPlayer("ESP", 8, 313410, "KOKE"));
            teamPlayers.Add(new TeamPlayer("ESP", 10, 375512, "THIAGO"));
            teamPlayers.Add(new TeamPlayer("ESP", 20, 400715, "Marco ASENSIO"));
            teamPlayers.Add(new TeamPlayer("ESP", 22, 313374, "ISCO"));
            teamPlayers.Add(new TeamPlayer("ESP", 9, 337175, "RODRIGO"));
            teamPlayers.Add(new TeamPlayer("ESP", 11, 400713, "Lucas VAZQUEZ"));
            teamPlayers.Add(new TeamPlayer("ESP", 17, 402138, "Iago ASPAS"));
            teamPlayers.Add(new TeamPlayer("ESP", 19, 367920, "Diego COSTA"));
            teamPlayers.Add(new TeamPlayer("ESP", 21, 200176, "David SILVA"));

            teamPlayers.Add(new TeamPlayer("SWE", 1, 398547, "Robin OLSEN"));
            teamPlayers.Add(new TeamPlayer("SWE", 12, 398556, "Karl-Johan JOHNSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 23, 360496, "Kristoffer NORDFELDT"));
            teamPlayers.Add(new TeamPlayer("SWE", 2, 300251, "Mikael LUSTIG"));
            teamPlayers.Add(new TeamPlayer("SWE", 3, 395283, "Victor LINDELOF"));
            teamPlayers.Add(new TeamPlayer("SWE", 4, 214667, "Andreas GRANQVIST"));
            teamPlayers.Add(new TeamPlayer("SWE", 5, 358053, "Martin OLSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 6, 395264, "Ludwig AUGUSTINSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 14, 395270, "Filip HELANDER"));
            teamPlayers.Add(new TeamPlayer("SWE", 16, 398561, "Emil KRAFTH"));
            teamPlayers.Add(new TeamPlayer("SWE", 18, 373312, "Pontus JANSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 7, 214675, "Sebastian LARSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 8, 360340, "Albin EKDAL"));
            teamPlayers.Add(new TeamPlayer("SWE", 10, 398550, "Emil FORSBERG"));
            teamPlayers.Add(new TeamPlayer("SWE", 13, 406882, "Gustav SVENSSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 15, 372881, "Oscar HILJEMARK"));
            teamPlayers.Add(new TeamPlayer("SWE", 17, 401465, "Viktor CLAESSON"));
            teamPlayers.Add(new TeamPlayer("SWE", 19, 398549, "Marcus ROHDEN"));
            teamPlayers.Add(new TeamPlayer("SWE", 21, 365630, "Jimmy DURMAZ"));
            teamPlayers.Add(new TeamPlayer("SWE", 9, 297243, "Marcus BERG"));
            teamPlayers.Add(new TeamPlayer("SWE", 11, 398551, "John GUIDETTI"));
            teamPlayers.Add(new TeamPlayer("SWE", 20, 300255, "Ola TOIVONEN"));
            teamPlayers.Add(new TeamPlayer("SWE", 22, 395273, "Isaac Kiese THELIN"));

            teamPlayers.Add(new TeamPlayer("SUI", 1, 319460, "Yann SOMMER"));
            teamPlayers.Add(new TeamPlayer("SUI", 12, 402022, "Yvon MVOGO"));
            teamPlayers.Add(new TeamPlayer("SUI", 21, 356676, "Roman BUERKI"));
            teamPlayers.Add(new TeamPlayer("SUI", 2, 196605, "Stephan LICHTSTEINER"));
            teamPlayers.Add(new TeamPlayer("SUI", 3, 398508, "Francois MOUBANDJE"));
            teamPlayers.Add(new TeamPlayer("SUI", 4, 398509, "Nico ELVEDI"));
            teamPlayers.Add(new TeamPlayer("SUI", 5, 405178, "Manuel AKANJI"));
            teamPlayers.Add(new TeamPlayer("SUI", 6, 356407, "Michael LANG"));
            teamPlayers.Add(new TeamPlayer("SUI", 13, 313559, "Ricardo RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("SUI", 20, 216645, "Johan DJOUROU"));
            teamPlayers.Add(new TeamPlayer("SUI", 22, 356411, "Fabian SCHAER"));
            teamPlayers.Add(new TeamPlayer("SUI", 8, 402021, "Remo FREULER"));
            teamPlayers.Add(new TeamPlayer("SUI", 10, 311558, "Granit XHAKA"));
            teamPlayers.Add(new TeamPlayer("SUI", 11, 216627, "Valon BEHRAMI"));
            teamPlayers.Add(new TeamPlayer("SUI", 14, 356415, "Steven ZUBER"));
            teamPlayers.Add(new TeamPlayer("SUI", 15, 216646, "Blerim DZEMAILI"));
            teamPlayers.Add(new TeamPlayer("SUI", 16, 216650, "Gelson FERNANDES"));
            teamPlayers.Add(new TeamPlayer("SUI", 17, 401448, "Denis ZAKARIA"));
            teamPlayers.Add(new TeamPlayer("SUI", 23, 321653, "Xherdan SHAQIRI"));
            teamPlayers.Add(new TeamPlayer("SUI", 7, 393480, "Breel EMBOLO"));
            teamPlayers.Add(new TeamPlayer("SUI", 9, 311554, "Haris SEFEROVIC"));
            teamPlayers.Add(new TeamPlayer("SUI", 18, 319470, "Mario GAVRANOVIC"));
            teamPlayers.Add(new TeamPlayer("SUI", 19, 356399, "Josip DRMIC"));

            teamPlayers.Add(new TeamPlayer("TUN", 1, 320518, "Farouk BEN MUSTAPHA"));
            teamPlayers.Add(new TeamPlayer("TUN", 16, 212413, "Aymen MATHLOUTHI"));
            teamPlayers.Add(new TeamPlayer("TUN", 22, 411654, "Mouez HASSEN"));
            teamPlayers.Add(new TeamPlayer("TUN", 2, 329717, "Syam BEN YOUSSEF"));
            teamPlayers.Add(new TeamPlayer("TUN", 3, 326645, "Yohan BEN ALOUANE"));
            teamPlayers.Add(new TeamPlayer("TUN", 4, 395998, "Yassine MERIAH"));
            teamPlayers.Add(new TeamPlayer("TUN", 5, 401801, "Oussama HADDADI"));
            teamPlayers.Add(new TeamPlayer("TUN", 6, 273822, "Rami BEDOUI"));
            teamPlayers.Add(new TeamPlayer("TUN", 11, 411653, "Dylan BRONN"));
            teamPlayers.Add(new TeamPlayer("TUN", 12, 332643, "Ali MAALOUL"));
            teamPlayers.Add(new TeamPlayer("TUN", 21, 401797, "Hamdi NAGUEZ"));
            teamPlayers.Add(new TeamPlayer("TUN", 9, 407145, "Anice BADRI"));
            teamPlayers.Add(new TeamPlayer("TUN", 13, 370575, "Ferjani SASSI"));
            teamPlayers.Add(new TeamPlayer("TUN", 14, 401798, "Mohamed BEN AMOR"));
            teamPlayers.Add(new TeamPlayer("TUN", 17, 411658, "Ellyes SKHIRI"));
            teamPlayers.Add(new TeamPlayer("TUN", 7, 411656, "Saifeddine KHAOUI"));
            teamPlayers.Add(new TeamPlayer("TUN", 8, 364668, "Fakhreddine BEN YOUSSEF"));
            teamPlayers.Add(new TeamPlayer("TUN", 10, 364669, "Wahbi KHAZRI"));
            teamPlayers.Add(new TeamPlayer("TUN", 15, 411655, "Ahmed KHALIL"));
            teamPlayers.Add(new TeamPlayer("TUN", 18, 407144, "Bassem SRARFI"));
            teamPlayers.Add(new TeamPlayer("TUN", 19, 329718, "Saber KHALIFA"));
            teamPlayers.Add(new TeamPlayer("TUN", 20, 407141, "Ghaylen CHAALELI"));
            teamPlayers.Add(new TeamPlayer("TUN", 23, 402235, "Naim SLITI"));

            teamPlayers.Add(new TeamPlayer("URU", 1, 229498, "Fernando MUSLERA"));
            teamPlayers.Add(new TeamPlayer("URU", 12, 312993, "Martin CAMPANA"));
            teamPlayers.Add(new TeamPlayer("URU", 23, 175629, "Martin SILVA"));
            teamPlayers.Add(new TeamPlayer("URU", 2, 368655, "Jose GIMENEZ"));
            teamPlayers.Add(new TeamPlayer("URU", 3, 229499, "Diego GODIN"));
            teamPlayers.Add(new TeamPlayer("URU", 4, 368660, "Guillermo VARELA"));
            teamPlayers.Add(new TeamPlayer("URU", 13, 332883, "Gaston SILVA"));
            teamPlayers.Add(new TeamPlayer("URU", 16, 286481, "Maximiliano PEREIRA"));
            teamPlayers.Add(new TeamPlayer("URU", 19, 305382, "Sebastian COATES"));
            teamPlayers.Add(new TeamPlayer("URU", 22, 267829, "Martin CACERES"));
            teamPlayers.Add(new TeamPlayer("URU", 5, 392673, "Carlos SANCHEZ"));
            teamPlayers.Add(new TeamPlayer("URU", 6, 386538, "Rodrigo BENTANCUR"));
            teamPlayers.Add(new TeamPlayer("URU", 7, 216567, "Cristian RODRIGUEZ"));
            teamPlayers.Add(new TeamPlayer("URU", 8, 386284, "Nahitan NANDEZ"));
            teamPlayers.Add(new TeamPlayer("URU", 14, 371670, "Lucas TORREIRA"));
            teamPlayers.Add(new TeamPlayer("URU", 15, 337196, "Matias VECINO"));
            teamPlayers.Add(new TeamPlayer("URU", 17, 368657, "Diego LAXALT"));
            teamPlayers.Add(new TeamPlayer("URU", 9, 270775, "Luis SUAREZ"));
            teamPlayers.Add(new TeamPlayer("URU", 10, 368652, "Giorgian DE ARRASCAETA"));
            teamPlayers.Add(new TeamPlayer("URU", 11, 229506, "Cristhian STUANI"));
            teamPlayers.Add(new TeamPlayer("URU", 18, 407384, "Maximiliano GOMEZ"));
            teamPlayers.Add(new TeamPlayer("URU", 20, 305384, "Jonathan URRETAVISCAYA"));
            teamPlayers.Add(new TeamPlayer("URU", 21, 267834, "Edinson CAVANI"));
        }

        void PlayerFace_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            PlayerFace pf = (PlayerFace)sender;
            Player player = pf.Player;
            ShowPlayerInfo(player);
        }

        private void ShowPlayerInfo(Player player)
        {
            var query = from tp in teamPlayers
                        where tp.TeamID == currentGame.PlayingTeamID
                        && tp.Number == Convert.ToInt32(player.Id)
                        && player.Team.TeamID == currentGame.PlayingTeamID
                        select tp;

            if (query.Any())
            {
                TeamPlayer teamPlayer = query.First();
                imgPlayer.ImageSource = new BitmapImage(new Uri(string.Format("https://api.fifa.com/api/v1/picture/players/2018fwc/{0}_sq-300_jpg?allowDefaultPicture=true", teamPlayer.ID), UriKind.Absolute));
                numPlayer.Text = teamPlayer.Number.ToString();
                txtPlayerName.Text = teamPlayer.Name.ToString();
                grdPlayerInfo.Visibility = Visibility.Visible;
            }
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

            fallenBallsProcessed = false;
        }

        private MoveResult MoveDiscoids()
        {
            if (!started)
                return null;

            MoveResult moveResult = new MoveResult() { DiscoidPositions = new List<DiscoidPosition>(), IsTurnOver = false };

            //Flag indicating that the program is still calculating
            //the positions, that is, the balls are still in an inconsistent state.
            calculatingPositions = true;

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

            calculatingPositions = false;

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
                    currentGame = GetNextGame(selectedTeamID, currentGame.Date);

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
            afterTurnProcessed = true;
            currentGame.Fouls[currentGame.PlayingTeamID] = 0;

            int redCount = 0;
            int fallenRedCount = 0;
            int wonPoints = 0;
            int lostPoints = 0;
            bool someInTable = false;

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

            //if (!someInTable && currentGameState != GameState.TestShot)
            //{
            //    UpdateGameState(GameState.GameOver);
            //    outgoingShot.GameOver = true;
            //    return;
            //}

            //int fallenBallsCount = fallenBalls.Count;
            //for (int i = fallenBallsCount - 1; i >= 0; i--)
            //{
            //    if (!fallenBalls[i].IsBallInPocket)
            //    {
            //        fallenBalls.RemoveAt(i);
            //    }
            //}

            //teams[awaitingTeamID - 1].JustSwapped = true;
            //teams[playingTeamID - 1].JustSwapped = swappedPlayers;

            if (swappedPlayers)
            {
                playerState = PlayerState.Aiming;
            }
            else
            {
                playerState = PlayerState.Calling;
                //if (playerState == PlayerState.Aiming)
                //{
                //    if (fallenRedCount < redCount)
                //    {
                //        if (teams[playingTeamID - 1].BallOn.Points == 1)
                //        {
                //            playerState = PlayerState.Calling;
                //        }
                //    }
                //}
            }

            //if (currentGameState != GameState.TestShot)
            //{
            //    teams[playingTeamID - 1].BallOn = GetNextBallOn(swappedPlayers, teams[playingTeamID - 1].BallOn);

            //    Texture2D ballOnTexture = null;
            //    switch (teams[playingTeamID - 1].BallOn.Points)
            //    {
            //        case (int)BallValues.Red:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\RedBall");
            //            break;
            //        case (int)BallValues.Yellow:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\YellowBall");
            //            break;
            //        case (int)BallValues.Green:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\GreenBall");
            //            break;
            //        case (int)BallValues.Brown:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BrownBall");
            //            break;
            //        case (int)BallValues.Blue:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BlueBall");
            //            break;
            //        case (int)BallValues.Pink:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\PinkBall");
            //            break;
            //        case (int)BallValues.Black:
            //            ballOnTexture = Content.Load<Texture2D>(@"Images\BlackBall");
            //            break;
            //    }

            //    if (teams[playingTeamID - 1].Id == 1)
            //        ballOn1.Texture = ballOnTexture;
            //    else
            //        ballOn2.Texture = ballOnTexture;
            //}

            //targetVector = new Vector2(0, 0);

            if (currentGameState == GameState.Play)
            {
                //teams[playingTeamID - 1].Attempts =
                //teams[awaitingTeamID - 1].Attempts = 0;

                //teams[playingTeamID - 1].AttemptsToWin =
                //teams[awaitingTeamID - 1].AttemptsToWin = 0;

                //teams[playingTeamID - 1].AttemptsNotToLose =
                //teams[awaitingTeamID - 1].AttemptsNotToLose = 0;

                //teams[playingTeamID - 1].AttemptsOfDespair =
                //teams[awaitingTeamID - 1].AttemptsOfDespair = 0;

                //teams[playingTeamID - 1].BestShot.LostPoints =
                //teams[awaitingTeamID - 1].BestShot.LostPoints = 1000;

                //teams[playingTeamID - 1].BestShot.WonPoints =
                //teams[awaitingTeamID - 1].BestShot.WonPoints = 0;

                //teams[playingTeamID - 1].BestShotSelected =
                //teams[awaitingTeamID - 1].BestShotSelected = false;
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



    public enum PlayerState
    {
        None,
        SelectingNumberOfPlayers,
        SelectingHost,
        Connecting,
        ReceivingInvitation,
        Aiming,
        Calling
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
