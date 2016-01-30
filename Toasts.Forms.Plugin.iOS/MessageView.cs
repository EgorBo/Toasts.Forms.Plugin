//
// MessageView.cs
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
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Plugin.Toasts
{
    public class MessageView : UIView
    {
        private static readonly UIFont TitleFont;
        private static readonly UIFont DescriptionFont;
        private static readonly UIColor TitleColor;
        private static readonly UIColor DescriptionColor;
        private const float Padding = 12.0f;
        private const float IconSize = 36.0f;
        private const float TextOffset = 2.0f;
        private float _height;
        private float _width;

        static MessageView()
        {
            TitleFont = UIFont.BoldSystemFontOfSize(16.0f);
            DescriptionFont = UIFont.SystemFontOfSize(14.0f);
            TitleColor = UIColor.FromWhiteAlpha(1.0f, 1.0f);
            DescriptionColor = UIColor.FromWhiteAlpha(1.0f, 1.0f);
        }

        internal MessageView(string title, string description, ToastNotificationType type, Action<bool> onDismiss, TimeSpan duration)
            : this((NSString)title, (NSString)description, type)
        {
            OnDismiss = onDismiss;
            DisplayDelay = duration.TotalSeconds;
        }

        private MessageView(NSString title, NSString description, ToastNotificationType type)
            : base(RectangleF.Empty)
        {
            BackgroundColor = UIColor.Clear;
            ClipsToBounds = false;
            UserInteractionEnabled = true;
            Title = title;
            Description = description;
            MessageType = type;
            Height = 0.0f;
            Width = 0.0f;
            Hit = false;

            NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, OrientationChanged);
        }

        public Action<bool> OnDismiss { get; set; }

        public bool Hit { get; set; }

        public float Height
        {
            get
            {
                if (Math.Abs(_height) < 0.0001)
                {
                    CGSize titleLabelSize = TitleSize();
                    CGSize descriptionLabelSize = DescriptionSize();
                    _height = (float) Math.Max((Padding*2) + titleLabelSize.Height + descriptionLabelSize.Height, (Padding*2) + IconSize);
                }
                return _height;
            }
            private set { _height = value; }
        }

        public float Width
        {
            get
            {
                if (Math.Abs(_width) < 0.0001)
                    _width = GetStatusBarFrame().Width;
                return _width;
            }
            private set { _width = value; }
        }
        public double DisplayDelay { get; set; }

        internal MessageBarStyleSheet StylesheetProvider { get; set; }

        private NSString Title { get; set; }

        private new NSString Description { get; set; }

        private ToastNotificationType MessageType { get; set; }

        private float AvailableWidth
        {
            get
            {
                float maxWidth = (Width - (Padding*3) - IconSize);
                return maxWidth;
            }
        }


        private void OrientationChanged(NSNotification notification)
        {
            Frame = new RectangleF((float) Frame.X, (float) Frame.Y, GetStatusBarFrame().Width, (float) Frame.Height);
            SetNeedsDisplay();
        }

        private RectangleF GetStatusBarFrame()
        {
            var windowFrame = OrientFrame(UIApplication.SharedApplication.KeyWindow.Frame);
            var statusFrame = OrientFrame(UIApplication.SharedApplication.StatusBarFrame);

            return new RectangleF((float) windowFrame.X, (float) windowFrame.Y, (float) windowFrame.Width,
                (float) statusFrame.Height);
        }

        private CGRect OrientFrame(CGRect frame)
        {
            if ((IsDeviceLandscape(UIDevice.CurrentDevice.Orientation) ||
                IsStatusBarLandscape(UIApplication.SharedApplication.StatusBarOrientation)) &&
                !IsRunningOnIOSVersionOrLater(8) /*http://stackoverflow.com/questions/24150359/is-uiscreen-mainscreen-bounds-size-becoming-orientation-dependent-in-ios8*/)
            {
                frame = new RectangleF((float) frame.X, (float) frame.Y, (float) frame.Height, (float) frame.Width);
            }
            return frame;
        }

        private bool IsDeviceLandscape(UIDeviceOrientation orientation)
        {
            return orientation == UIDeviceOrientation.LandscapeLeft || orientation == UIDeviceOrientation.LandscapeRight;
        }

        private bool IsStatusBarLandscape(UIInterfaceOrientation orientation)
        {
            return orientation == UIInterfaceOrientation.LandscapeLeft ||
                   orientation == UIInterfaceOrientation.LandscapeRight;
        }

        public override void Draw(CGRect rect)
        {
            var context = UIGraphics.GetCurrentContext ();

            MessageBarStyleSheet styleSheet = StylesheetProvider;
            context.SaveState ();

            styleSheet.BackgroundColorForMessageType (MessageType).SetColor ();
            context.FillRect (rect);
            context.RestoreState ();
            context.SaveState ();

            context.BeginPath ();
            context.MoveTo (0, rect.Size.Height);
            context.SetStrokeColor(styleSheet.StrokeColorForMessageType (MessageType).CGColor);
            context.SetLineWidth (1);
            context.AddLineToPoint (rect.Size.Width, rect.Size.Height);
            context.StrokePath ();
            context.RestoreState ();
            context.SaveState ();
            
            float xOffset = Padding;
            float yOffset = Padding;
            var icon = styleSheet.IconImageForMessageType(MessageType);
            if (icon != null)
            {
                icon.Draw(new RectangleF(xOffset, yOffset, IconSize, IconSize));
            }
            context.SaveState ();
                
            yOffset -= TextOffset;
            xOffset += (icon == null ? 0 : IconSize) + Padding;
            CGSize titleLabelSize = TitleSize();
            if (string.IsNullOrEmpty (Title) && !string.IsNullOrEmpty (Description)) {
                yOffset = (float)(Math.Ceiling ((double)rect.Size.Height * 0.5) - Math.Ceiling ((double)titleLabelSize.Height * 0.5) - TextOffset);
            }

            TitleColor.SetColor ();
                
            var titleRectangle = new RectangleF (xOffset, yOffset, (float) titleLabelSize.Width + 5, (float) titleLabelSize.Height + 5);
			Title.DrawString (titleRectangle, new UIStringAttributes {Font=TitleFont, ForegroundColor = TitleColor});
            yOffset += (float)titleLabelSize.Height;

            CGSize descriptionLabelSize = DescriptionSize();
            DescriptionColor.SetColor();
            var descriptionRectangle = new RectangleF(xOffset, yOffset, (float)descriptionLabelSize.Width + Padding, (float)descriptionLabelSize.Height);
			Description.DrawString (descriptionRectangle, new UIStringAttributes {Font=DescriptionFont, ForegroundColor = DescriptionColor});
        }

        private CGSize TitleSize()
        {
            var boundedSize = new SizeF(AvailableWidth, float.MaxValue);
            CGSize titleLabelSize;
            if (!IsRunningOnIOSVersionOrLater(7))
            {
                var attr = new UIStringAttributes(NSDictionary.FromObjectAndKey(TitleFont, (NSString) TitleFont.Name));
                titleLabelSize = Title.GetBoundingRect(boundedSize, NSStringDrawingOptions.TruncatesLastVisibleLine, attr, null).Size;
            }
            else
            {
                titleLabelSize = Title.StringSize(TitleFont, boundedSize, UILineBreakMode.TailTruncation);
            }
            return titleLabelSize;
        }

        private CGSize DescriptionSize()
        {
            var boundedSize = new SizeF(AvailableWidth, float.MaxValue);
            CGSize descriptionLabelSize;
            if (!IsRunningOnIOSVersionOrLater(7))
            {
                var attr = new UIStringAttributes(NSDictionary.FromObjectAndKey(TitleFont, (NSString) TitleFont.Name));
                descriptionLabelSize = Description.GetBoundingRect(boundedSize, NSStringDrawingOptions.TruncatesLastVisibleLine, attr, null).Size;
            }
            else
            {
                descriptionLabelSize = Description.StringSize(DescriptionFont, boundedSize, UILineBreakMode.TailTruncation);
            }
            return descriptionLabelSize;
        }

        private bool IsRunningOnIOSVersionOrLater(int majorVersion)
        {
            Version version = new Version(UIDevice.CurrentDevice.SystemVersion);
            return version.Major >= majorVersion;
        }
    }
}
