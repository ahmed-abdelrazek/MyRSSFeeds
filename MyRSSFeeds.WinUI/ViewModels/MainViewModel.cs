using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MyRSSFeeds.Core.Contracts.Services;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Web.Syndication;

namespace MyRSSFeeds.ViewModels
{
    public class MainViewModel : ObservableRecipient
    {
        private readonly IRSSDataService _rssDataService = Ioc.Default.GetService<IRSSDataService>();
        private readonly ISourceDataService _sourceDataService = Ioc.Default.GetService<ISourceDataService>();

        public CancellationTokenSource TokenSource { get; set; } = null;

        private RSS _selectedRSS;

        public RSS SelectedRSS
        {
            get => _selectedRSS;
            set
            {
                if (SetProperty(ref _selectedRSS, value))
                {
                    if (_selectedRSS is not null)
                    {
                        if (!_selectedRSS.IsRead)
                        {
                            _selectedRSS.IsRead = true;
                            new RSSDataService().UpdateFeedAsync(_selectedRSS).FireAndGet();
                        }
                    }
                    ClearSelectedRSSCommand.NotifyCanExecuteChanged();
                };
            }
        }

        public ObservableCollection<RSS> Feeds { get; private set; } = new ObservableCollection<RSS>();

        private bool _isLoadingData;

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set
            {
                if (SetProperty(ref _isLoadingData, value))
                {
                    ((AsyncRelayCommand)RefreshFeedsCommand).NotifyCanExecuteChanged();
                };
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    if (value)
                    {
                        IsShowingFailedMessage = false;
                    }
                    IsLoadingVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                };
            }
        }

        private Visibility _isLoadingVisibility;

        public Visibility IsLoadingVisibility
        {
            get => _isLoadingVisibility;
            set => SetProperty(ref _isLoadingVisibility, value);
        }

        private bool _isShowingFailedMessage;

        public bool IsShowingFailedMessage
        {
            get => _isShowingFailedMessage;
            set
            {
                if (SetProperty(ref _isShowingFailedMessage, value))
                {

                    if (value)
                    {
                        IsLoading = false;
                    }
                };
                FailedMesageVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _failedMesageVisibility;

        public Visibility FailedMesageVisibility
        {
            get => _failedMesageVisibility;
            set => SetProperty(ref _failedMesageVisibility, value);
        }

        public ObservableCollection<Source> FilterSources { get; private set; } = new ObservableCollection<Source>();

        private Source _filterSelectedSource;

        public Source FilterSelectedSource
        {
            get => _filterSelectedSource;
            set
            {
                if (SetProperty(ref _filterSelectedSource, value))
                {
                    ((RelayCommand)ClearFilterSourceCommand).NotifyCanExecuteChanged();
                };
            }
        }

        private string _filterTitle;

        public string FilterTitle
        {
            get => _filterTitle;
            set => SetProperty(ref _filterTitle, value);
        }

        private string _filterCreator;

        public string FilterCreator
        {
            get => _filterCreator;
            set => SetProperty(ref _filterCreator, value);
        }

        private bool _filterIsUnreadOnly;

        public bool FilterIsUnreadOnly
        {
            get => _filterIsUnreadOnly;
            set => SetProperty(ref _filterIsUnreadOnly, value);
        }

        private int _progressMax;

        public int ProgressMax
        {
            get => _progressMax;
            set => SetProperty(ref _progressMax, value);
        }

        private int _progressCurrent;

        public int ProgressCurrent
        {
            get => _progressCurrent;
            set => SetProperty(ref _progressCurrent, value);
        }

        public ICommand MarkAsReadCommand { get; private set; }

        public ICommand FilterCommand { get; private set; }

        /// <summary>
        /// Clears selected source from the Filter ComboBox
        /// </summary>
        public ICommand ClearFilterSourceCommand { get; private set; }

        /// <summary>
        /// Reloads the Feeds
        /// </summary>
        public ICommand RefreshFeedsCommand { get; private set; }

        /// <summary>
        /// Stop loading online data from sources
        /// </summary>
        public ICommand CancelLoadingCommand { get; private set; }

        /// <summary>
        /// Clear the Selected rss property from the list and retun the built-in browser to blank
        /// </summary>
        public RelayCommand ClearSelectedRSSCommand { get; private set; }

        public MainViewModel()
        {
            IsLoading = true;
            TokenSource = new CancellationTokenSource();
            LoadCommands();
        }

        private void LoadCommands()
        {
            CancelLoadingCommand = new RelayCommand(CancelLoading, CanCancelLoading);
            FilterCommand = new AsyncRelayCommand(Filter);
            ClearFilterSourceCommand = new RelayCommand(ClearFilterSource, CanClearFilterSource);
            MarkAsReadCommand = new AsyncRelayCommand(MarkAsReadAsync, CanMarkAsRead);
            ClearSelectedRSSCommand = new RelayCommand(ClearSelectedRSS, CanClearSelectedRSS);
            RefreshFeedsCommand = new AsyncRelayCommand(RefreshFeedAsync, CanRefreshFeeds);
        }


        /// <summary>
        /// Gets all the feeds from database (with-in limits in settings)
        /// the try to gets all the new stuff from your sources
        /// add the new ones to the database if there is any
        /// then show the latest (with-in limits in settings)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns>Task Type</returns>
        public async Task LoadDataAsync(IProgress<int> progress, CancellationToken token)
        {
            IsLoadingData = true;
            FilterSources.Clear();
            Feeds.Clear();
            ProgressCurrent = 0;
            bool hasLoadedFeedNewItems = false;

            // Set Httpclient userAgent to the user selected one
            await RssRequest.SetCustomUserAgentAsync();

            foreach (var rss in (await _rssDataService.GetFeedsDataAsync(await ApplicationData.Current.LocalSettings.ReadAsync<int>("FeedsLimit"))).ToList())
            {
                Feeds.Add(rss);
            }

            SyndicationFeed feed = null;

            var sourcesDataList = await _sourceDataService.GetSourcesDataAsync();

            ProgressMax = sourcesDataList.Count();
            int progressCount = 0;

            foreach (var source in sourcesDataList.ToList())
            {
                FilterSources.Add(source);

                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    TokenSource = new CancellationTokenSource();
                    ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
                    return;
                }
            }
            // if there is no internet just cut our loses and get out of here we already loaded the local data
            //if (!new NetworkInformationHelper().HasInternetAccess)
            //{
            //    await new MessageDialog("CheckInternetMessageDialog".GetLocalized()).ShowAsync();
            //    return;
            //}
            var WaitAfterLastCheckInMinutes = await ApplicationData.Current.LocalSettings.ReadAsync<int>("WaitAfterLastCheck");

            foreach (var sourceItem in FilterSources)
            {
                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    TokenSource = new CancellationTokenSource();
                    ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
                    return;
                }

                // don't get source feed if x number of minutes haven't passed since the last one - default is 2 hours
                var checkSourceAfter = sourceItem.LastBuildCheck.AddMinutes(WaitAfterLastCheckInMinutes);

                if (checkSourceAfter >= DateTimeOffset.Now && Feeds.Count > 0)
                {
                    continue;
                }

                //if (!new NetworkInformationHelper().HasInternetAccess)
                //{
                //    continue;
                //}

                progress.Report(++progressCount);

                //if getting the feed crushed for (internet - not xml rss - other reasons)
                //move to the next source on the list to try it instead of stopping every thing
                try
                {
                    var feedString = await RssRequest.GetFeedAsStringAsync(sourceItem.RssUrl, token);
                    feed = new SyndicationFeed();

                    if (string.IsNullOrWhiteSpace(feedString))
                    {
                        continue;
                    }
                    else
                    {
                        feed.Load(feedString);

                        // Saves rss items count and last check time to source 
                        sourceItem.CurrentRssItemsCount = feed.Items.Count;
                        sourceItem.LastBuildCheck = DateTimeOffset.Now;
                        await new SourceDataService().UpdateSourceAsync(sourceItem);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    continue;
                }

                // Iterate through each feed item.
                foreach (SyndicationItem syndicationItem in feed.Items)
                {
                    if (token.IsCancellationRequested)
                    {
                        IsLoadingData = false;
                        TokenSource = new CancellationTokenSource();
                        ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
                        return;
                    }

                    //handle edge cases like when they don't send that stuff or misplace them like freaking reddit r/worldnews
                    if (syndicationItem.Title is null)
                    {
                        syndicationItem.Title = new SyndicationText("MainViewModelNoTitleFound".GetLocalized());
                    }
                    if (syndicationItem.Summary is null)
                    {
                        syndicationItem.Summary = new SyndicationText("MainViewModelNoSummaryFound".GetLocalized());
                    }
                    if (syndicationItem.PublishedDate.Year < 2000)
                    {
                        syndicationItem.PublishedDate = syndicationItem.LastUpdatedTime.Year > 2000 ? syndicationItem.LastUpdatedTime : DateTimeOffset.Now;
                    }

                    Uri itemNewUri = syndicationItem.ItemUri;
                    if (itemNewUri is null)
                    {
                        if (syndicationItem.Links.Count > 0)
                        {
                            itemNewUri = syndicationItem.Links.FirstOrDefault().Uri;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(syndicationItem.Id))
                    {
                        syndicationItem.Id = itemNewUri.ToString();
                    }

                    RSS rss = new()
                    {
                        PostTitle = syndicationItem.Title.Text,
                        Description = syndicationItem.Summary.Text,
                        Authors = new List<Author>(),
                        URL = itemNewUri,
                        CreatedAt = syndicationItem.PublishedDate.DateTime,
                        ItemGuid = syndicationItem.Id,
                        PostSource = sourceItem
                    };

                    foreach (var author in syndicationItem.Authors)
                    {
                        rss.Authors.Add(new Author
                        {
                            Name = author.Name,
                            Email = author.Email,
                            Uri = author.Uri
                        });
                    }

                    if (!await _rssDataService.FeedExistAsync(rss))
                    {
                        var newRss = await _rssDataService.AddNewFeedAsync(rss);
                        Feeds.Add(newRss);
                        hasLoadedFeedNewItems = true;
                    }
                }
            }

            if (hasLoadedFeedNewItems)
            {
                Feeds.Clear();
                foreach (var rss in (await _rssDataService.GetFeedsDataAsync(await ApplicationData.Current.LocalSettings.ReadAsync<int>("FeedsLimit"))).ToList())
                {
                    Feeds.Add(rss);
                }
            }
            IsLoadingData = false;
            ((AsyncRelayCommand)MarkAsReadCommand).NotifyCanExecuteChanged();
        }

        private bool CanCancelLoading()
        {
            return IsLoadingData;
        }

        private void CancelLoading()
        {
            TokenSource.Cancel();
        }

        private bool CanRefreshFeeds()
        {
            return !IsLoadingData;
        }

        /// <summary>
        /// Reloads the feed by calling LoadDataAsync()
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task RefreshFeedAsync()
        {
            await LoadDataAsync(new Progress<int>(percent => ProgressCurrent = percent), TokenSource.Token);
        }

        private bool CanMarkAsRead()
        {
            return IsLoadingData == false && Feeds.Any(x => x.IsRead == false);
        }

        /// <summary>
        /// Gets every item in the feed and change any unread item to read
        /// then updateing the database with it
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task MarkAsReadAsync()
        {
            foreach (var item in Feeds.Where(x => x.IsRead == false))
            {
                item.IsRead = true;
                await _rssDataService.UpdateFeedAsync(item);
            }
        }

        private bool CanClearFilterSource()
        {
            return FilterSelectedSource is not null;
        }

        private void ClearFilterSource()
        {
            FilterSelectedSource = null;
        }

        private bool CanClearSelectedRSS()
        {
            return SelectedRSS is not null;
        }

        private void ClearSelectedRSS()
        {
            SelectedRSS = null;
        }

        private async Task Filter()
        {
            TokenSource.Cancel();

            var query = await _rssDataService.GetFeedsDataAsync();

            if (FilterSelectedSource is not null)
            {
                query = query.Where(x => x.PostSource.Id == FilterSelectedSource.Id);
            }
            if (!string.IsNullOrWhiteSpace(FilterTitle))
            {
                query = query.Where(x => x.PostTitle.ToLower().Contains(FilterTitle.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(FilterCreator))
            {
                query = query.Where(x => x.Authors.Any(x => x.Email.ToLower() == FilterCreator.ToLower()));
            }
            if (FilterIsUnreadOnly)
            {
                query = query.Where(x => x.IsRead == false);
            }

            Feeds.Clear();

            foreach (var item in query.OrderByDescending(x => x.CreatedAt).ToList())
            {
                Feeds.Add(item);
            }
        }

        /// <summary>
        /// return back a string from the start to entered postion
        /// then adding "…" at the end of it 
        /// </summary>
        /// <param name="txt">the test to substring</param>
        /// <param name="endindex">intger for the end index</param>
        /// <returns>new shorted string</returns>
        private string ShortenText(string txt, int endindex)
        {
            return txt.Length > endindex ? string.Concat(txt.Substring(0, endindex), "…") : txt;
        }
    }
}
