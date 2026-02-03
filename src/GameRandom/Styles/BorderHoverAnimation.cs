using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using GameRandom.Scr.Service;
using Colors = GameRandom.SteamSDK.Colors;

namespace GameRandom.Styles;

public class BorderHoverAnimation
{
    private readonly Button _currentButton;
    private readonly LinearGradientBrush _gradientBrush;
    private IBrush? _savedBrush;
    private DispatcherTimer _timer;
    private bool _isHovering;
    
    public BorderHoverAnimation(Button button, Color topColor, Color bottomColor)
    {
        _currentButton = button;
        
        _gradientBrush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            
            GradientStops = new GradientStops
            {
                new GradientStop
                {
                    Color = topColor,
                    Offset = 0
                },
                
                new GradientStop
                {
                    Color = bottomColor,
                    Offset = 1
                }
            }
        };
        
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        _currentButton.PointerEntered += (sender, args) =>
        {
            _isHovering = true;
            PointerEntered();
        };

        _currentButton.PointerExited += (sender, args) =>
        {
            _isHovering = false;
            ResetState();
        };
    }

    private void PointerEntered()
    {
        var border = _currentButton.GetTemplateChildren().OfType<Border>().FirstOrDefault();

        if (border == null)
        {
            Console.WriteLine("PointerEntered: Border is null. Returning");
            return;
        }
        
        _savedBrush = border.BorderBrush;
        border.BorderBrush = _gradientBrush;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMicroseconds(50) };
        _timer.Tick += (_, _) =>
        {
            if (!_isHovering) return;

            foreach (var stop in _gradientBrush.GradientStops)
            {
                var offset = stop.Offset + 1;
                stop.Offset = offset > 1 ? offset - 1 : offset;
            }
        };
        
        _timer.Start();
    }

    private void ResetState()
    {
        if (_isHovering) return;
        
        _timer.Stop();
        _currentButton.BorderBrush = _savedBrush;
    }
}