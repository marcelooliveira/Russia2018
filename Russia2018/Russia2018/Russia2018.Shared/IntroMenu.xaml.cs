﻿using Russia2018.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Russia2018
{
    public partial class IntroMenu : Page
    {
        const int GROUPCOUNT = 8;
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
            for (int i = 0; i < 8; i++)
            {
                Border brd = new Border();
                brd.CornerRadius = new CornerRadius(5);
                brd.Margin = new Thickness(2);
                brd.SetValue(Grid.ColumnProperty, i % 4);
                brd.SetValue(Grid.RowProperty, i / 4);

                LinearGradientBrush lgb = new LinearGradientBrush();
                lgb.StartPoint = new Point(0, 0);
                lgb.EndPoint = new Point(1, 1);
                lgb.GradientStops = new GradientStopCollection();
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
                    Text = ((char)(i + 65)).ToString(),
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

                for (int j = 0; j < 4; j++)
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

                    SoccerTeam team = GameHelper.Instance.TeamsDictionary[GameHelper.Instance.TeamCodes[i * 4 + j]];
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
                    img.Tag = team.TeamID;
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
                    txt.Tag = team.TeamID;

                    grdTeam.Tag = team.TeamID;
                    grdTeam.Children.Add(img);
                    grdTeam.Children.Add(txt);
                    grdTeam.PointerEntered += team_MouseEnter;
                    grdTeam.PointerExited += team_MouseLeave;
                    grdTeam.PointerReleased += team_MouseLeftButtonUp;

                    grdGroup.Children.Add(grdTeam);
                }

                brd.Child = grdGroup;

                grdGroupsContainer.Children.Add(brd);
            }
        }

        void team_MouseLeftButtonUp(object sender, PointerRoutedEventArgs e)
        {
            string teamID = ((FrameworkElement)e.OriginalSource).Tag.ToString();
            Grid grd = (Grid)sender;
            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("TeamID1", teamID);
            this.Frame.Navigate(typeof(MainPage), teamID);



            //((UserControlContainer)Application.Current.RootVisual).SwitchToView(typeof(MainPage), parameters);
        }

        void team_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            object tag = ((FrameworkElement)e.OriginalSource).Tag;
            if (tag == null)
            {
                return;
            }

            string teamID = tag.ToString();
            txtSelectedTeam.Text = "";
            imgSelectedTeam.Source = null;

            Grid grd = (Grid)sender;
            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            txt.FontWeight = FontWeights.Normal;
            grd.Background = null;
        }

        void team_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            string teamID = ((FrameworkElement)e.OriginalSource).Tag.ToString();
            txtSelectedTeam.Text = teamsDictionary[teamID].TeamName;
            imgSelectedTeam.Source = new BitmapImage(new Uri(string.Format(@"http://www.fifa.com/imgml/flags/reflected/m/{0}.png", teamID.ToLower()), UriKind.Absolute));

            Grid grd = (Grid)sender;
            Image img = (Image)(grd.Children[0]);
            TextBlock txt = (TextBlock)(grd.Children[1]);

            txt.FontWeight = FontWeights.Bold;
            grd.Background = lgbEven;
        }
    }
}
