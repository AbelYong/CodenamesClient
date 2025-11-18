using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CodenamesClient.GameUI.BoardUI
{
    public static class TextAnimator
    {
        public static readonly DependencyProperty AnimatedTextProperty =
            DependencyProperty.RegisterAttached(
                "AnimatedText",
                typeof(string),
                typeof(TextAnimator),
                new PropertyMetadata(null, OnAnimatedTextChanged));

        public static string GetAnimatedText(DependencyObject obj)
        {
            return (string)obj.GetValue(AnimatedTextProperty);
        }

        public static void SetAnimatedText(DependencyObject obj, string value)
        {
            obj.SetValue(AnimatedTextProperty, value);
        }

        private static readonly DependencyProperty CancellationTokenSourceProperty =
        DependencyProperty.RegisterAttached(
            "CancellationTokenSource",
            typeof(CancellationTokenSource),
            typeof(TextAnimator),
            new PropertyMetadata(null));

        private static async void OnAnimatedTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TextBlock textBlock)
            {
                // Cancel any text animation currently running
                var oldCancelationToken = (CancellationTokenSource)obj.GetValue(CancellationTokenSourceProperty);
                oldCancelationToken?.Cancel();

                var newCancelationToken = new CancellationTokenSource();
                obj.SetValue(CancellationTokenSourceProperty, newCancelationToken);

                const int taskDelayMiliseconds = 50;
                var newText = (string)e.NewValue ?? string.Empty;
                var delay = taskDelayMiliseconds;

                try
                {
                    textBlock.Text = string.Empty;

                    for (int i = 0; i < newText.Length; i++)
                    {
                        await Task.Delay(delay, newCancelationToken.Token);
                        textBlock.Text = newText.Substring(0, i + 1);
                    }
                }
                catch (TaskCanceledException)
                {
                    // The animation was canceled because the text changed
                    // Do nothing, let new animation play out.
                }
                finally
                {
                    // Null animation so its not wrongly cancelled 
                    if (newCancelationToken == obj.GetValue(CancellationTokenSourceProperty))
                    {
                        obj.SetValue(CancellationTokenSourceProperty, null);
                    }
                }
            }
        }
    }
}
