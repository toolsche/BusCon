using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BusCon.Animation
{
    public abstract class DynamicEffect
    {
        FrameworkElement _target;
        Storyboard _storyboard;

        public DynamicEffect()
        {
        }

        protected abstract Storyboard CreateStoryboard(FrameworkElement target);

        protected virtual void Add(FrameworkElement frameworkElement)
        {
            _target = frameworkElement;
            _storyboard = CreateStoryboard(frameworkElement);
            _storyboard.Completed += new EventHandler(OnCompleted);
            frameworkElement.Resources.Add("storyboard", _storyboard);
        }

        protected virtual void Remove()
        {
            if (_target == null)
                return;

            if (_storyboard == null)
                return;

            _target.Resources.Remove("storyboard");
            _target =  null;
            _storyboard = null;
        }
        protected virtual void OnCompleted(object sender, EventArgs e)
        {
            Remove();

            if (Completed != null)
                Completed(sender, e);
        }

        public virtual void Start(FrameworkElement frameworkElement)
        {
            if (frameworkElement.Resources.Contains("storyboard")) 
                return;

            Add(frameworkElement);
            _storyboard.Begin();
        }

        public virtual void Stop()
        {
            if (_target == null || _storyboard == null)
                return;

            _storyboard.Stop();
            Remove();
        }

        public event EventHandler Completed;
    }
}
