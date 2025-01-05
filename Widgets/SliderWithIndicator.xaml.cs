using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MotionDetectorNet.Widgets;

public partial class SliderWithIndicator : UserControl
{
    [Description("Caption"), Category("Common Properties")]
    public string Caption
    {
        get => (string)base.GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
        "Caption",
        typeof(string),
        typeof(SliderWithIndicator));

    [Description("Value"), Category("Common Properties")]
    public double Value
    {
        get => (double)base.GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        "Value",
        typeof(double),
        typeof(SliderWithIndicator),
        new FrameworkPropertyMetadata(0.5d, new PropertyChangedCallback(ValuePropertyChanged)));

    [Description("Is Logarithmic"), Category("Common Properties")]
    public bool IsLogarithmic
    {
        get => (bool)base.GetValue(IsLogarithmicProperty);
        set => SetValue(IsLogarithmicProperty, value);
    }

    public static readonly DependencyProperty IsLogarithmicProperty = DependencyProperty.Register(
        "IsLogarithmic",
        typeof(bool),
        typeof(SliderWithIndicator),
        new PropertyMetadata(false, new PropertyChangedCallback(IsLogarithmicPropertyChanged)));

    [Description("Slider minimum"), Category("Common Properties")]
    public double Minimum
    {
        get => (double)base.GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        "Minimum",
        typeof(double),
        typeof(SliderWithIndicator),
        new PropertyMetadata(0d, new PropertyChangedCallback(MinimumPropertyChanged)));

    [Description("Slider maximum"), Category("Common Properties")]
    public double Maximum
    {
        get => (double)base.GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        "Maximum",
        typeof(double),
        typeof(SliderWithIndicator),
        new PropertyMetadata(100d, new PropertyChangedCallback(MaximumPropertyChanged)));

    public SliderWithIndicator()
    {
        InitializeComponent();

        if (Minimum >= 0 && Maximum <= 1)
        {
            sldSlider.SmallChange = 0.001;
        }
    }

    // Internal

    private static void ValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is SliderWithIndicator instance)
        {
            if (!instance._ignorePropertyChange)
            {
                var value = instance.Value;
                instance.sldSlider.Value = instance.IsLogarithmic && value > 0 ? Math.Log10(value) : value;
                System.Diagnostics.Debug.WriteLine($"Value = {instance.Value} [{instance.sldSlider.Minimum}..{instance.sldSlider.Maximum}]");
            }
        }
    }

    private static void IsLogarithmicPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is SliderWithIndicator instance)
        {
            var min = instance.Minimum;
            var max = instance.Maximum;
            instance.sldSlider.Minimum = instance.IsLogarithmic && min > 0 ? Math.Log10(min) : min;
            instance.sldSlider.Maximum = instance.IsLogarithmic && max > 0 ? Math.Log10(max) : max;
            System.Diagnostics.Debug.WriteLine($"Min = {instance.sldSlider.Minimum}, Max = {instance.sldSlider.Maximum}");
        }
    }

    private static void MinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is SliderWithIndicator instance)
        {
            var min = instance.Minimum;
            instance.sldSlider.Minimum = instance.IsLogarithmic && min > 0 ? Math.Log10(min) : min;
            System.Diagnostics.Debug.WriteLine($"Min = {instance.sldSlider.Minimum}");
        }
    }

    private static void MaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is SliderWithIndicator instance)
        {
            var max = instance.Maximum;
            instance.sldSlider.Maximum = instance.IsLogarithmic && max > 0 ? Math.Log10(max) : max;
            System.Diagnostics.Debug.WriteLine($"Max = {instance.sldSlider.Maximum}");
        }
    }

    bool _ignorePropertyChange = false;
    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        double value = sldSlider.Value;
        if (IsLogarithmic)
        {
            value = Math.Pow(10, value);
        }

        _ignorePropertyChange = true;
        SetValue(ValueProperty, value);
        _ignorePropertyChange = false;
    }
}
