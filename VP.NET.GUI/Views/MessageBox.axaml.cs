using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace VP.NET.GUI.Views
{
    public partial class MessageBox : Window
    {
        public enum MessageBoxButtons
        {
            OK,
            OKCancel,
            YesNo,
            YesNoCancel,
            Continue,
            ContinueCancel,
            Details,
            DetailsOKCancel,
            DetailsContinueCancel,
            DontWarnAgainOK,
            ContinueCancelSkipVersion,
            YesCancel
        }

        public enum MessageBoxResult
        {
            OK,
            Cancel,
            Yes,
            No,
            Continue,
            Details,
            DontWarnAgain,
            SkipVersion
        }

        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);//not thread safe!
        }

        //Messagebox is not thread safe!
        public static Task<MessageBoxResult> Show(Window? parent, string text, string title, MessageBoxButtons buttons)
        {
            var msgbox = new MessageBox()
            {
                Title = title
            };

            msgbox.FindControl<TextBlock>("Text")!.Text = text;

            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons")!;

            var result = MessageBoxResult.OK;

            void AddButton(string caption, MessageBoxResult r, bool def = false, string classes = "", int buttonWidth = -1)
            {                
                var button = new Button { Content = caption, Width = 100};
                button.Click += (_, __) => {
                    result = r;
                    msgbox.Close();
                };
                if(classes != "")
                {
                    button.Classes.Add(classes);
                }
                if (buttonWidth != -1)
                {
                    button.Width = buttonWidth;
                }
                buttonPanel.Children.Add(button);
                if (def)
                {
                    result = r;
                }
            }

            if (buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OKCancel || buttons == MessageBoxButtons.DetailsOKCancel)
            {
                AddButton("OK", MessageBoxResult.OK, true, "");
            }

            if (buttons == MessageBoxButtons.YesCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes, false, "");
                AddButton("Cancel", MessageBoxResult.Cancel, true, "");
            }

            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes, false, "");
                AddButton("No", MessageBoxResult.No, true, "");
            }

            if (buttons == MessageBoxButtons.Continue || buttons == MessageBoxButtons.ContinueCancel || buttons == MessageBoxButtons.DetailsContinueCancel || buttons == MessageBoxButtons.ContinueCancelSkipVersion)
            {
                AddButton("Continue", MessageBoxResult.Continue, false, "");
            }

            if (buttons == MessageBoxButtons.OKCancel || buttons == MessageBoxButtons.YesNoCancel || buttons == MessageBoxButtons.ContinueCancel || buttons == MessageBoxButtons.DetailsOKCancel || buttons == MessageBoxButtons.DetailsContinueCancel || buttons == MessageBoxButtons.ContinueCancelSkipVersion)
            {
                AddButton("Cancel", MessageBoxResult.Cancel, true, "");
            }

            if (buttons == MessageBoxButtons.Details || buttons == MessageBoxButtons.DetailsOKCancel || buttons == MessageBoxButtons.DetailsContinueCancel)
            {
                AddButton("Details", MessageBoxResult.Details, false, "");
            }

            if (buttons == MessageBoxButtons.DontWarnAgainOK)
            {
                AddButton("OK", MessageBoxResult.OK, true, "");
                AddButton("Don't warn again", MessageBoxResult.DontWarnAgain, false, "", 150);
            }

            if (buttons == MessageBoxButtons.ContinueCancelSkipVersion)
            {
                AddButton("Skip this version", MessageBoxResult.SkipVersion, false, "", 150);
            }

            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += delegate { tcs.TrySetResult(result); };

            if (parent != null && parent.IsVisible)
            {
                msgbox.ShowDialog(parent);
            }
            else
            {
                msgbox.Show();
            }

            return tcs.Task;
        }
    }
}
