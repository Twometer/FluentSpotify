using FluentSpotify.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentSpotify.UI
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private string authUrl;

        public LoginDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            authUrl = Spotify.Instance.BuildAuthUrl();
            WebView.Navigate(new Uri(authUrl, UriKind.Absolute));
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri.ToString().StartsWith(Spotify.CallbackUrl))
            {
                args.Cancel = true;

            }
            else if (!args.Uri.ToString().StartsWith("https://accounts.spotify.com"))
            {
                args.Cancel = true;
                // TODO Start in external browser
            }
        }

        private async void WebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            var sb = new StringBuilder();

            sb.Append(SetCss("html", "overflow", "hidden"));
            sb.Append(SetCss(".head", "border", "none"));
            sb.Append(SetCss(".divider-title", "background", "#000"));
            sb.Append(SetCss(".divider-title", "color", "#fff"));
            sb.Append(SetCss(".checkbox", "display", "none"));
            sb.Append(SetCss(".divider", "display", "none"));
            sb.Append(SetCss(".grecaptcha-badge", "display", "none"));
            sb.Append(SetCss("#login-username", "border", "none"));
            sb.Append(SetCss("#login-password", "border", "none"));

            if (WindowsTheme.IsDarkMode())
            {
                sb.Append(SetCss("body", "background", "#000000"));
                sb.Append(SetCss("body", "color", "#ffffff"));
                sb.Append(SetCss(".spotify-logo", "filter", "invert(1)"));
                sb.Append(SetCss("#login-username", "background", "#555"));
                sb.Append(SetCss("#login-password", "background", "#555"));
            } else
            {
                sb.Append(SetCss("body", "background", "#ffffff"));
                sb.Append(SetCss("body", "color", "#000000"));
                sb.Append(SetCss("#login-username", "background", "#efefef"));
                sb.Append(SetCss("#login-password", "background", "#efefef"));
            }

            await WebView.InvokeScriptAsync("eval", new string[] { sb.ToString() });
            WebView.Opacity = 1;
        }

        private string SetCss(string name, string prop, string val)
        {
            return $"document.querySelector('{name}').style['{prop}'] = '{val}';";
        }
    }
}
