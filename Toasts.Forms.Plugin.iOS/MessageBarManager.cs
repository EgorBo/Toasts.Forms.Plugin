//
// MessageBarManager.cs
//
// Author:
//       Prashant Cholachagudda <pvc@outlook.com>
//
// Copyright (c) 2013 Prashant Cholachagudda
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Foundation;
using UIKit;

namespace Plugin.Toasts
{
    public class MessageBarManager : NSObject
    {
        private const float DismissAnimationDuration = 0.25f;
        private MessageWindow _messageWindow;
        private readonly Queue<MessageView> _messageBarQueue;
        private static MessageBarManager _instance;
        private readonly float _messageBarOffset;
        private bool _messageVisible;

        public static MessageBarManager SharedInstance
        {
            get { return _instance ?? (_instance = new MessageBarManager()); }
        }

        private MessageBarManager()
        {
            _messageBarQueue = new Queue<MessageView>();
            _messageVisible = false;
            _messageBarOffset = 20;
        }

        private UIView MessageWindowView { get { return GetMessageBarViewController().View; } }

        /// <summary>
        /// Shows the message
        /// </summary>
        /// <param name="title">Messagebar title</param>
        /// <param name="description">Messagebar description</param>
        /// <param name="type">Message type</param>
        /// <param name = "onDismiss">OnDismiss callback</param>
        /// <param name="duration"></param>
        /// <param name="styleSheet"></param>
        public void ShowMessage(string title, string description, ToastNotificationType type, Action<bool> onDismiss, TimeSpan duration, MessageBarStyleSheet styleSheet = null)
        {
            var messageView = new MessageView(title, description, type, onDismiss, duration);
            messageView.StylesheetProvider = styleSheet;
            messageView.Hidden = true;

            MessageWindowView.AddSubview(messageView);
            MessageWindowView.BringSubviewToFront(messageView);

            _messageBarQueue.Enqueue(messageView);

            if (!_messageVisible)
            {
                ShowNextMessage();
            }
        }

        private void ShowNextMessage()
        {
            if (_messageBarQueue.Count > 0)
            {
                _messageVisible = true;
                MessageView messageView = _messageBarQueue.Dequeue();
                messageView.Frame = new RectangleF(0, -messageView.Height, messageView.Width, messageView.Height);
                messageView.Hidden = false;
                messageView.SetNeedsDisplay();

                var gest = new UITapGestureRecognizer(MessageTapped);
                messageView.AddGestureRecognizer(gest);

                UIView.Animate(DismissAnimationDuration, () => messageView.Frame = new RectangleF((float)messageView.Frame.X,
                        (float)(_messageBarOffset + messageView.Frame.Y + messageView.Height), messageView.Width, messageView.Height));

                //Need a better way of dissmissing the method
                var dismiss = new Timer(DismissMessage, messageView, TimeSpan.FromSeconds(messageView.DisplayDelay), TimeSpan.FromMilliseconds(-1));
            }
        }

        /// <summary>
        /// Hides all messages
        /// </summary>
        public void HideAll()
        {
            MessageView currentMessageView = null;
            var subviews = MessageWindowView.Subviews;

            foreach (UIView subview in subviews)
            {
                var view = subview as MessageView;
                if (view != null)
                {
                    currentMessageView = view;
                    currentMessageView.RemoveFromSuperview();
                }
            }

            _messageVisible = false;
            _messageBarQueue.Clear();
            CancelPreviousPerformRequest(this);
        }

        private void MessageTapped(UIGestureRecognizer recognizer)
        {
            var view = recognizer.View as MessageView;
            if (view != null)
                DismissMessage(view, true);
        }

        private void DismissMessage(object messageView)
        {
            var view = messageView as MessageView;
            if (view != null)
                InvokeOnMainThread(() => DismissMessage(view, false));
        }

        private void DismissMessage(MessageView messageView, bool clicked)
        {
            if (messageView != null && !messageView.Hit)
            {
                messageView.Hit = true;
                UIView.Animate(DismissAnimationDuration, () => messageView.Frame =
                    new RectangleF((float)messageView.Frame.X, (float)-(messageView.Frame.Height - _messageBarOffset), (float)messageView.Frame.Width, (float)messageView.Frame.Height),
                    () =>
                    {
                        _messageVisible = false;
                        messageView.RemoveFromSuperview();

                        var action = messageView.OnDismiss;
                        if (action != null)
                            action(clicked);

                        if (_messageBarQueue.Count > 0)
                            ShowNextMessage();
                    });
            }
        }

        private MessageBarViewController GetMessageBarViewController()
        {
            if (_messageWindow == null)
            {
                _messageWindow = new MessageWindow
                {
                    Frame = UIApplication.SharedApplication.KeyWindow.Frame,
                    Hidden = false,
                    WindowLevel = UIWindowLevel.Normal,
                    BackgroundColor = UIColor.Clear,
                    RootViewController = new MessageBarViewController()
                };
            }

            return (MessageBarViewController)_messageWindow.RootViewController;
        }
    }
}
