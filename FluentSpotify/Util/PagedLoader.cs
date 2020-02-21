using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FluentSpotify.Util
{
    public class PagedLoader<T> : IScrollNotify
    {
        public double Threshold { get; set; } = 256;

        private PagedRequest<T> request;

        private ListView listView;

        private double lastMax;

        private volatile bool isLoading;

        public PagedLoader(PagedRequest<T> request, ListView listView)
        {
            this.request = request;
            this.listView = listView;
        }

        public async Task Begin()
        {
            await LoadMore();
        }

        public async void OnScroll(double current, double max)
        {
            if (max - current < Threshold && max != lastMax)
            {
                await LoadMore();
                lastMax = max;
            }
        }

        private async Task LoadMore()
        {
            if (isLoading)
                return;

            if (request.HasNext)
            {
                isLoading = true;
                var tracks = await request.Next();
                foreach (var track in tracks)
                    listView.Items.Add(track);
                isLoading = false;
            }
        }
    }
}
