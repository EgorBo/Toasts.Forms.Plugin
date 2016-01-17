using Android.Views;
using Android.Views.Animations;

namespace Plugin.Toasts
{
    public static class DefaultAnimationsBuilder
    {
        private const long Duration = 400;
        private static Animation _slideInDownAnimation, _slideOutUpAnimation;
        private static int _lastInAnimationHeight, _lastOutAnimationHeight;

        public static Animation BuildDefaultSlideInDownAnimation(View croutonView)
        {
            if (_lastInAnimationHeight != croutonView.MeasuredHeight || (null == _slideInDownAnimation))
            {
                _slideInDownAnimation = new TranslateAnimation(0, 0, -croutonView.MeasuredHeight, 0);
                _slideInDownAnimation.Duration = Duration;
                _lastInAnimationHeight = croutonView.MeasuredHeight;
            }
            return _slideInDownAnimation;
        }
        
        public static Animation BuildDefaultSlideOutUpAnimation(View croutonView)
        {
            if (_lastOutAnimationHeight != croutonView.MeasuredHeight || (null == _slideOutUpAnimation))
            {
                _slideOutUpAnimation = new TranslateAnimation(0, 0, 0, -croutonView.MeasuredHeight);
                _slideOutUpAnimation.Duration = Duration;
                _lastOutAnimationHeight = croutonView.MeasuredHeight;
            }
            return _slideOutUpAnimation;
        }
    }
}