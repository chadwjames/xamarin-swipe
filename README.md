# xamarin-swipe
Xamarin Froms pan gesture recognizer demo.

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
