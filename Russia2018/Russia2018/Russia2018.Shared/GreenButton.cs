using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Russia2018
{
    [TemplateVisualState(Name = "Normal", GroupName = "ViewStates")]
    [TemplateVisualState(Name = "Highlighted", GroupName = "ViewStates")]
    public class GreenButton : Button
    {
        public static readonly DependencyProperty ImageContentProperty =
        DependencyProperty.Register("ImageContent", typeof(object),
        typeof(GreenButton), null);

        public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register("Text", typeof(string),
        typeof(GreenButton), null);

        public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register("IsHighlighted", typeof(bool), typeof(GreenButton), null);

        public GreenButton()
        {
            DefaultStyleKey = typeof(GreenButton);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(GreenButton_IsEnabledChanged);
        }

        void GreenButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.IsHighlighted = false;
                ChangeVisualState(true);
                this.Opacity = 0.35;
            }
            else
            {
                this.Opacity = 1.00;
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        public object ImageContent
        {
            get
            {
                return base.GetValue(ImageContentProperty);
            }
            set
            {
                base.SetValue(ImageContentProperty, value);
            }
        }

        public object Text
        {
            get
            {
                return base.GetValue(TextProperty);
            }
            set
            {
                base.SetValue(TextProperty, value);
            }
        }

        public bool IsHighlighted
        {
            get
            {
                return (bool)base.GetValue(IsHighlightedProperty);
            }
            set
            {
                base.SetValue(IsHighlightedProperty, value);
                ChangeVisualState(true);
            }
        }

        private void ChangeVisualState(bool useTransitions)
        {
            if (IsHighlighted)
            {
                VisualStateManager.GoToState(this, "Highlighted", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PointerEntered += new GreenButton_MouseEnter;
            this.PointerExited += new GreenButton_MouseLeave;
            this.PointerMoved += new GreenButton_MouseMove;

            this.HorizontalContentAlignment = HorizontalAlignment.Center;
            this.ChangeVisualState(false);
        }

        void GreenButton_MouseMove(object sender, PointerRoutedEventArgs e)
        {
            this.IsHighlighted = true;
            ChangeVisualState(true);
        }

        void GreenButton_MouseEnter(object sender, PointerRoutedEventArgs e)
        {
            this.IsHighlighted = true;
            ChangeVisualState(true);
        }

        void GreenButton_MouseLeave(object sender, PointerRoutedEventArgs e)
        {
            this.IsHighlighted = false;
            ChangeVisualState(true);
        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsHighlighted = !this.IsHighlighted;
            ChangeVisualState(true);
        }
    }
}
