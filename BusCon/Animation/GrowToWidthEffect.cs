using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace BusCon.Animation
{
    public class GrowToWidthEffect : DynamicEffect
    {
        readonly double _from;
        readonly double _to;
        readonly double _speed;
        readonly int _msdelay;

        public GrowToWidthEffect(double from, double to, double speed, int msdelay)
        {
            _from = from;
            _to = to;
            _speed = speed;
            _msdelay = msdelay;
        }

        protected override Storyboard CreateStoryboard(FrameworkElement target)
        {
            var result = new Storyboard();
            var animation = new DoubleAnimation
            {
                From = _from,
                To = _to,
                SpeedRatio = _speed,
                BeginTime =
                new TimeSpan(0, 0, 0, 0, _msdelay),
                EasingFunction =
                new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));
            result.Children.Add(animation);
            return result;
        }
    }
}