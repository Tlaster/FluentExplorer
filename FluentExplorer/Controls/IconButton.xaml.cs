using Windows.UI.Xaml;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FluentExplorer.Controls
{
    public sealed partial class IconButton
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            nameof(Icon), typeof(object), typeof(IconButton), new PropertyMetadata(default, OnPropertyChangedCallback));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(object), typeof(IconButton), new PropertyMetadata(default, OnPropertyChangedCallback));

        public IconButton()
        {
            InitializeComponent();
        }

        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IconProperty)
                (d as IconButton).IconContentPresenter.Content = e.NewValue;
            else if (e.Property == TextProperty) (d as IconButton).TextContentPresenter.Content = e.NewValue;
        }
    }
}