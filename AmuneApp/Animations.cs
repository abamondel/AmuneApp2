using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace AmuneApp
{
  public  class Animations
    {
        public static void AnimateTextColor(TextBlock sentenceTextBlock)
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 0);

            // Specified colors: Red, BlueViolet, Blue
            GradientStop color1 = new GradientStop(Colors.Blue, 0.0);
            GradientStop color2 = new GradientStop(Colors.BlueViolet, 0.5);
            GradientStop color3 = new GradientStop(Colors.Red, 1.0);

            brush.GradientStops.Add(color1);
            brush.GradientStops.Add(color2);
            brush.GradientStops.Add(color3);

            sentenceTextBlock.Foreground = brush;

            // Animating the entire GradientBrush to create a wave-like effect
            PointAnimation startAnimation = new PointAnimation(new Point(-1, 0), new Point(1, 0), new Duration(TimeSpan.FromSeconds(3)));
            startAnimation.RepeatBehavior = RepeatBehavior.Forever;
            brush.BeginAnimation(LinearGradientBrush.StartPointProperty, startAnimation);

            PointAnimation endAnimation = new PointAnimation(new Point(0, 0), new Point(2, 0), new Duration(TimeSpan.FromSeconds(3)));
            endAnimation.RepeatBehavior = RepeatBehavior.Forever;
            brush.BeginAnimation(LinearGradientBrush.EndPointProperty, endAnimation);
        }
      


    }
}
