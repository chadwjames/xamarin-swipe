using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Xamarin.Forms;

namespace XamarinSwipe
{
    public class App : Application
    {
        private readonly DiagnosticsView _leftContent = new DiagnosticsView();
        private readonly WebView _rightContent;
        private readonly AbsoluteLayout _mainLayout = new AbsoluteLayout();

        public App()
        {
            /* The perpouse of this app is to help me undersand Pan 
             * Gestures in Xamarin forms. There seams to be some inconsistancies between iOS and Android.
             * Questions:
	         *     The swipe left feature works for iOS, but not Android. The OnUpdated event does not fire on android. 
             *     Why doesn't iOS and Android implement the the pan gesture the same? 
             *     (Note: It does work on android of the view has no child elements. 
             *     Try changing the _rightContent from a webview to a boxview and it will work almost as expected.)
	         *
             *     The pan gesture doesn't work at all for webviews. This is the case for iOS and Android. 
             *     Why can’t I attach a pan gesture recognizer to a webview?
             */

            var swipeLeftGestureRecognizer = new PanGestureRecognizer();
            swipeLeftGestureRecognizer.PanUpdated += OnSwipeLeftGestureUpdated;

            var swipeRightGestureRcognizer = new PanGestureRecognizer();
            swipeRightGestureRcognizer.PanUpdated += OnSwipeRightGestureUpdated;

            var swipLeftCloseGestureRocognizer = new PanGestureRecognizer();
            swipLeftCloseGestureRocognizer.PanUpdated += OnSwipeLeftCloseGestureUpdated;

            var swipeRightCloseGestureRecognizer = new PanGestureRecognizer();
            swipeRightCloseGestureRecognizer.PanUpdated += OnSwipeRightCloseGestureUpdated;

            _leftContent.GestureRecognizers.Add(swipLeftCloseGestureRocognizer);
            _mainLayout.Children.Add(_leftContent, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            _rightContent = new WebView {Source = "https://developer.xamarin.com"};
            _rightContent.GestureRecognizers.Add(swipeRightCloseGestureRecognizer);
            _mainLayout.Children.Add(_rightContent, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            var mainContent = new StackLayout {BackgroundColor = Color.FromHex("#00afef")};
            mainContent.Children.Add(new Label
            {
                Text = "Swipe from the left or right edges.",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center
            });
            _mainLayout.Children.Add(mainContent, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);

            var swipeRightGesture = new ContentView {Opacity = .5, BackgroundColor = Color.Black};
            swipeRightGesture.GestureRecognizers.Add(swipeRightGestureRcognizer);
            _mainLayout.Children.Add(swipeRightGesture, new Rectangle(1, 0, .1, 1), AbsoluteLayoutFlags.All);

            var swipeLeftGesture = new ContentView {Opacity = .5, BackgroundColor = Color.Black};
            swipeLeftGesture.GestureRecognizers.Add(swipeLeftGestureRecognizer);
            _mainLayout.Children.Add(swipeLeftGesture, new Rectangle(0, 0, .1, 1), AbsoluteLayoutFlags.All);

            MainPage = new ContentPage {Content = _mainLayout, Padding = new Thickness(0,Device.OnPlatform(20,0,0),0,0)};
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private async void OnSwipeLeftGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't pan beyond the wrapped user interface element bounds.
                    if (e.TotalX <= _leftContent.Width)
                        _leftContent.TranslationX = e.TotalX - _leftContent.Width;
                    break;

                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    if (_leftContent.TranslationX + _leftContent.Width > (_mainLayout.Width/4))
                    {
                        await _leftContent.TranslateTo(0, 0, 250, Easing.SinInOut);
                    }
                    else
                    {
                        await _leftContent.TranslateTo(-_leftContent.Width, 0, 250, Easing.SinInOut);
                    }
                    break;

                case GestureStatus.Started:
                    _mainLayout.RaiseChild(_leftContent);
                    break;

                case GestureStatus.Canceled:
                    await _leftContent.TranslateTo(-_leftContent.Width, 0, 250, Easing.SinInOut);
                    _mainLayout.LowerChild(_leftContent);
                    break;
            }
        }

        private async void OnSwipeLeftCloseGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't pan beyond the wrapped user interface element bounds.                    
                    if (e.TotalX <= 0)
                        _leftContent.TranslationX = e.TotalX;
                    break;

                case GestureStatus.Completed:
                    // move the menu all the way.
                    if (Math.Abs(_leftContent.TranslationX) < (_mainLayout.Width / 4))
                    {
                        await _leftContent.TranslateTo(0, 0, 250, Easing.SinInOut);
                    }
                    else
                    {
                        await _leftContent.TranslateTo(-_leftContent.Width, 0, 250, Easing.SinInOut);
                        _mainLayout.LowerChild(_leftContent);
                    }
                    break;

                case GestureStatus.Started:
                    break;

                case GestureStatus.Canceled:
                    await _leftContent.TranslateTo(0, 0, 250, Easing.SinInOut);
                    break;
            }
        }

        private async void OnSwipeRightGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't pan beyond the wrapped user interface element bounds.
                    if (Math.Abs(e.TotalX) <= _mainLayout.Width)
                        _rightContent.TranslationX = _rightContent.Width + e.TotalX;
                    break;

                case GestureStatus.Completed:
                   // Store the translation applied during the pan
                    if (_rightContent.Width - _rightContent.TranslationX > _mainLayout.Width / 4)
                    {
                        await _rightContent.TranslateTo(0, 0, 250, Easing.SinInOut);
                    }
                    else
                    {
                        await _rightContent.TranslateTo(_rightContent.Width, 0, 250, Easing.SinInOut);
                    }
                    break;

                case GestureStatus.Started:
                    _mainLayout.RaiseChild(_rightContent);
                    break;

                case GestureStatus.Canceled:
                    await _rightContent.TranslateTo(-_rightContent.Width, 0, 250, Easing.SinInOut);
                    _mainLayout.LowerChild(_rightContent);
                    break;
            }
        }

        private async void OnSwipeRightCloseGestureUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    // Translate and ensure we don't pan beyond the wrapped user interface element bounds.                    
                    if (e.TotalX >= 0)
                        _rightContent.TranslationX = e.TotalX;
                    break;

                case GestureStatus.Completed:
                    // move the menu all the way.
                    if (_rightContent.TranslationX > (_mainLayout.Width / 4))
                    {
                        await _rightContent.TranslateTo(_rightContent.Width, 0, 250, Easing.SinInOut);
                        _mainLayout.LowerChild(_rightContent);
                    }
                    else
                    {
                        await _rightContent.TranslateTo(0, 0, 250, Easing.SinInOut);                      
                    }
                    break;

                case GestureStatus.Started:
                    break;

                case GestureStatus.Canceled:
                    await _rightContent.TranslateTo(0, 0, 250, Easing.SinInOut);
                    break;
            }
        }
    }
}
