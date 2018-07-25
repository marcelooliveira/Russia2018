using Russia2018.Model;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Russia2018
{
    public partial class IntroMenu : Page
    {
        const int GROUPCOUNT = 8;
        private const int ASCII_A = 65;
        private const int TEAMS_PER_GROUP = 4;
        List<Group> groups;
        Dictionary<string, SoccerTeam> teamsDictionary;
        LinearGradientBrush lgbEven = new LinearGradientBrush();

        public IntroMenu()
        {
            this.InitializeComponent();
            GenerateGroups();
            groups = GameHelper.Instance.Groups;
            teamsDictionary = GameHelper.Instance.TeamsDictionary;
        }

        void GenerateGroups()
        {
            for (int groupIndex = 0; groupIndex < GROUPCOUNT; groupIndex++)
            {
                GenerateGroup(groupIndex);
            }
        }

        private void GenerateGroup(int groupIndex)
        {
            Border brd = new Border
            {
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(2)
            };
            brd.SetValue(Grid.ColumnProperty, groupIndex % 4);
            brd.SetValue(Grid.RowProperty, groupIndex / 4);

            LinearGradientBrush lgb = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection()
            };
            lgb.GradientStops.Add(new GradientStop() { Offset = 0.0, Color = Color.FromArgb(255, 0, 0, 0) });
            lgb.GradientStops.Add(new GradientStop() { Offset = 0.5, Color = Color.FromArgb(255, 30, 30, 30) });
            lgb.GradientStops.Add(new GradientStop() { Offset = 1.0, Color = Color.FromArgb(255, 40, 40, 40) });
            brd.Background = lgb;

            lgbEven.StartPoint = new Point(0, 0);
            lgbEven.EndPoint = new Point(1, 1);
            lgbEven.GradientStops = new GradientStopCollection();
            lgbEven.GradientStops.Add(new GradientStop() { Offset = 0.0, Color = Color.FromArgb(255, 0, 0, 0) });
            lgbEven.GradientStops.Add(new GradientStop() { Offset = 0.5, Color = Color.FromArgb(255, 30, 30, 30) });
            lgbEven.GradientStops.Add(new GradientStop() { Offset = 1.0, Color = Color.FromArgb(255, 80, 80, 80) });

            Grid grdGroup = new Grid();
            grdGroup.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(28) });
            grdGroup.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grdGroup.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grdGroup.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(18) });
            grdGroup.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(18) });
            grdGroup.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(18) });
            grdGroup.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(18) });

            TextBlock txtGroupID = new TextBlock()
            {
                Text = ((char)(groupIndex + ASCII_A)).ToString(),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };
            txtGroupID.SetValue(Grid.ColumnProperty, 0);
            txtGroupID.SetValue(Grid.RowProperty, 0);
            txtGroupID.SetValue(Grid.RowSpanProperty, 4);
            txtGroupID.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            txtGroupID.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            grdGroup.Children.Add(txtGroupID);

            for (int teamIndex = 0; teamIndex < TEAMS_PER_GROUP; teamIndex++)
            {
                AddTeamLabel(groupIndex, grdGroup, teamIndex);
            }

            brd.Child = grdGroup;

            grdGroupsContainer.Children.Add(brd);
        }

        private void AddTeamLabel(int groupIndex, Grid grdGroup, int j)
        {
            Grid grdTeam = new Grid();
            grdTeam.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grdTeam.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grdTeam.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25) });
            grdTeam.SetValue(Grid.ColumnProperty, 1);
            grdTeam.SetValue(Grid.ColumnSpanProperty, 2);
            grdTeam.SetValue(Grid.RowProperty, j);
            grdTeam.HorizontalAlignment = HorizontalAlignment.Stretch;
            grdTeam.VerticalAlignment = VerticalAlignment.Center;
            grdTeam.Width = 120;

            SoccerTeam team = GameHelper.Instance.TeamsDictionary[GameHelper.Instance.TeamCodes[groupIndex * 4 + j]];
            Image img = new Image()
            {
                Source = new BitmapImage(new Uri(string.Format(@"http://www.fifa.com/imgml/flags/reflected/m/{0}.png", team.TeamID.ToLower()), UriKind.Absolute)),

                Width = 28.5,
                Height = 25.0,
                VerticalAlignment = VerticalAlignment.Center
            };
            img.SetValue(Grid.ColumnProperty, 0);
            img.SetValue(Grid.RowProperty, j);
            img.VerticalAlignment = VerticalAlignment.Top;
            img.HorizontalAlignment = HorizontalAlignment.Stretch;
            img.Clip = new RectangleGeometry() { Rect = new Rect(new Point(0, 0), new Point(28.5, 14)) };
            img.Tag = team;
            TranslateTransform tf = new TranslateTransform()
            {
                X = 0,
                Y = 6
            };
            img.RenderTransform = tf;

            TextBlock txt = new TextBlock()
            {
                Text = team.TeamName,
                Foreground = new SolidColorBrush(Colors.White)
            };
            txt.SetValue(Grid.ColumnProperty, 1);
            txt.SetValue(Grid.RowProperty, j);
            txt.VerticalAlignment = VerticalAlignment.Center;
            txt.HorizontalAlignment = HorizontalAlignment.Stretch;
            txt.Tag = team;

            grdTeam.Tag = team;
            grdTeam.Children.Add(img);
            grdTeam.Children.Add(txt);
            grdTeam.PointerEntered += Team_MouseEnter;
            grdTeam.PointerExited += Team_MouseLeave;
            grdTeam.PointerReleased += Team_MouseLeftButtonUp;

            grdGroup.Children.Add(grdTeam);
        }

        void Team_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            if (!(((FrameworkElement)e.OriginalSource).Tag is SoccerTeam team))
            {
                return;
            }
            Grid grd = (Grid)sender;
            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("TeamID1", team.TeamID);
            this.Frame.Navigate(typeof(MainPage), team);
        }

        void Team_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            object tag = ((FrameworkElement)e.OriginalSource).Tag;
            if (tag == null)
            {
                return;
            }

            Grid grd = (Grid)sender;
            UnselectTeam(grd);
        }

        private void UnselectTeam(Grid grd)
        {
            txtSelectedTeam.Text = "";
            imgSelectedTeam.Source = null;

            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            txt.FontWeight = FontWeights.Normal;
            grd.Background = null;
        }

        void Team_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            var team = ((FrameworkElement)e.OriginalSource).Tag as SoccerTeam;
            Grid grd = (Grid)sender;
            SelectTeam(team.TeamID, grd);
        }

        private void SelectTeam(string teamID, Grid grd)
        {
            txtSelectedTeam.Text = teamsDictionary[teamID].TeamName;
            imgSelectedTeam.Source = new BitmapImage(new Uri(string.Format(@"http://www.fifa.com/imgml/flags/reflected/m/{0}.png", teamID.ToLower()), UriKind.Absolute));

            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            txt.FontWeight = FontWeights.Bold;
            grd.Background = lgbEven;
        }
    }
}
