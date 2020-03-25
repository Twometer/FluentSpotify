using FluentSpotify.API;
using FluentSpotify.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentSpotify.UI.Controller
{
    internal class DeviceListController : ControllerBase<MenuFlyout>
    {
        public IEnumerable<Device> DeviceList { get; private set; }

        public DeviceListController(Page parent, MenuFlyout control) : base(parent, control)
        {
        }

        public async Task ReloadDeviceList()
        {
            var DeviceFlyout = this.control;
            DeviceFlyout.Items.Clear();
            DeviceFlyout.Items.Add(new MenuFlyoutSeparator());

            var devices = await Spotify.Account.GetDevices();
            DeviceList = devices;

            foreach (var device in devices)
            {
                if (device.Id == Spotify.Playback.LocalPlayer?.PlayerId)
                    continue;

                var glyph = 0;
                switch (device.Type)
                {
                    case DeviceType.Computer:
                        glyph = 59767;
                        break;
                    case DeviceType.Smartphone:
                        glyph = 59626;
                        break;
                    case DeviceType.Speaker:
                        glyph = 59239;
                        break;
                    case DeviceType.GameConsole:
                        glyph = 59792;
                        break;
                        // TODO more icons
                }

                DeviceFlyout.Items.Add(CreateItem(device.Name, glyph, device.Id, device.IsActive));
            }

            DeviceFlyout.Items.Insert(0, CreateItem("This computer", 59767, Spotify.Playback.LocalPlayer.PlayerId, !IsOtherDeviceActive()));
        }

        public string GetCurrentlyActivePlayer()
        {
            foreach (var device in DeviceList)
                if (device.IsActive)
                    return device.Id;
            return Spotify.Playback.LocalPlayer.PlayerId;
        }

        public bool IsOtherDeviceActive()
        {
            foreach (var device in DeviceList)
                if (device.IsActive)
                    return true;
            return false;
        }

        private MenuFlyoutItem CreateItem(string text, int iconGlyph, string playerId, bool active)
        {
            var icon = new FontIcon() { Glyph = ((char)iconGlyph).ToString() };
            var item = new MenuFlyoutItem() { Text = text, Icon = icon, Tag = playerId };

            if (active)
            {
                item.Foreground = new SolidColorBrush((Color)parent.Resources["SystemAccentColor"]);
            }
            item.Click += DeviceListItem_Click;

            return item;
        }

        private async void DeviceListItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedDevice = (sender as MenuFlyoutItem).Tag as string;
            await Spotify.Playback.TransferPlayback(selectedDevice);

            // Wait for Spotify API to apply the change, then reload the list.
            // Because well why should it be applied after the request returns, unreasonable...
            await Task.Delay(250);
            await ReloadDeviceList();
        }

    }
}
