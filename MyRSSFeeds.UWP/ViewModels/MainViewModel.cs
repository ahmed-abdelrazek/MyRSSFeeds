using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.Helpers;
using MyRSSFeeds.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;

namespace MyRSSFeeds.ViewModels
{
    public class MainViewModel : Observable
    {
        private readonly ResourceLoader _loader = new ResourceLoader();
        private const string _defaultUrl = "about:blank";
        Windows.UI.ViewManagement.UISettings _systemTheme;
        ElementTheme _appTheme;
        string _uiTheme;

        public CancellationTokenSource TokenSource { get; set; } = null;

        private RSS _selectedRSS;

        public RSS SelectedRSS
        {
            get { return _selectedRSS; }
            set
            {
                Set(ref _selectedRSS, value, nameof(SelectedRSS), () =>
                {
                    ClearSelectedRSSCommand.OnCanExecuteChanged();
                    OpenInBrowserCommand.OnCanExecuteChanged();
                    OpenPostInAppCommand.OnCanExecuteChanged();

                    if (_selectedRSS != null)
                    {
                        if (!_selectedRSS.IsRead)
                        {
                            _selectedRSS.IsRead = true;
                            RSSDataService.UpdateFeedAsync(_selectedRSS).FireAndGet();
                        }
                        GetTheme();

                        var hyperLinkDescription = SelectedRSS.Description.Replace("[…]", $"<a href=\"{SelectedRSS.LaunchURL}\">[Open the full post]</a>");
                        if (_uiTheme == "#FF000000" && (_appTheme == ElementTheme.Default || _appTheme == ElementTheme.Dark))
                        {
                            _webView.NavigateToString($"<html><head><title>{SelectedRSS.PostTitle}</title><style> body {{ background:black}} h1 {{ color: white;}} h4 {{ color: white;}}</style></head><body><h1>{SelectedRSS.PostTitle} &#91;{SelectedRSS.PostSource.SiteTitle}&#93;</h1><h4>{hyperLinkDescription}</h4></body></html>");
                        }
                        else
                        {
                            _webView.NavigateToString(hyperLinkDescription);
                        }
                    }
                });
            }
        }

        public ObservableCollection<RSS> Feeds { get; private set; } = new ObservableCollection<RSS>();

        private Uri _webViewSource;

        public Uri WebViewSource
        {
            get { return _webViewSource; }
            set
            {
                Set(ref _webViewSource, value, nameof(WebViewSource), () =>
                {
                    OpenInBrowserCommand.OnCanExecuteChanged();
                    OpenPostInAppCommand.OnCanExecuteChanged();
                });
            }
        }

        private bool _isLoadingData;

        public bool IsLoadingData
        {
            get
            {
                return _isLoadingData;
            }
            set
            {
                Set(ref _isLoadingData, value);
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                Set(ref _isLoading, value, nameof(IsLoading), () =>
                {
                    if (value)
                    {
                        IsShowingFailedMessage = false;
                    }
                    IsLoadingVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        private Visibility _isLoadingVisibility;

        public Visibility IsLoadingVisibility
        {
            get { return _isLoadingVisibility; }
            set { Set(ref _isLoadingVisibility, value); }
        }

        private bool _isShowingFailedMessage;

        public bool IsShowingFailedMessage
        {
            get
            {
                return _isShowingFailedMessage;
            }
            set
            {
                Set(ref _isShowingFailedMessage, value, nameof(IsShowingFailedMessage), () =>
                {

                    if (value)
                    {
                        IsLoading = false;
                    }
                });
                FailedMesageVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private Visibility _failedMesageVisibility;

        public Visibility FailedMesageVisibility
        {
            get { return _failedMesageVisibility; }
            set { Set(ref _failedMesageVisibility, value); }
        }

        public ObservableCollection<Source> FilterSources { get; private set; } = new ObservableCollection<Source>();

        private Source _filterSelectedSource;

        public Source FilterSelectedSource
        {
            get { return _filterSelectedSource; }
            set
            {
                Set(ref _filterSelectedSource, value);
            }
        }

        private string _filterTitle;

        public string FilterTitle
        {
            get { return _filterTitle; }
            set
            {
                Set(ref _filterTitle, value);
            }
        }

        private string _filterCreator;

        public string FilterCreator
        {
            get { return _filterCreator; }
            set
            {
                Set(ref _filterCreator, value);
            }
        }

        private int _progressMax;

        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                Set(ref _progressMax, value);
            }
        }

        private int _progressCurrent;

        public int ProgressCurrent
        {
            get { return _progressCurrent; }
            set
            {
                Set(ref _progressCurrent, value);
            }
        }

        private ICommand _navCompleted;

        public ICommand NavCompletedCommand
        {
            get
            {
                if (_navCompleted == null)
                {
                    _navCompleted = new RelayCommand<WebViewNavigationCompletedEventArgs>(NavCompleted);
                }

                return _navCompleted;
            }
        }

        private void NavCompleted(WebViewNavigationCompletedEventArgs e)
        {
            IsLoading = false;
        }

        private ICommand _navFailed;

        public ICommand NavFailedCommand
        {
            get
            {
                if (_navFailed == null)
                {
                    _navFailed = new RelayCommand<WebViewNavigationFailedEventArgs>(NavFailed);
                }

                return _navFailed;
            }
        }

        private void NavFailed(WebViewNavigationFailedEventArgs e)
        {
            // Use `e.WebErrorStatus` to vary the displayed message based on the error reason
            IsShowingFailedMessage = true;
        }

        private ICommand _onNewWindowRequestedCommand;

        public ICommand OnNewWindowRequestedCommand
        {
            get
            {
                if (_onNewWindowRequestedCommand == null)
                {
                    _onNewWindowRequestedCommand = new RelayCommand<WebViewNewWindowRequestedEventArgs>(OnNewWindowRequested);
                }

                return _onNewWindowRequestedCommand;
            }
        }

        private void OnNewWindowRequested(WebViewNewWindowRequestedEventArgs e)
        {
            // Block all requests to open a new window
            e.Handled = true;
        }

        private ICommand _retryCommand;

        public ICommand RetryCommand
        {
            get
            {
                if (_retryCommand == null)
                {
                    _retryCommand = new RelayCommand(Retry);
                }

                return _retryCommand;
            }
        }

        //reload the built-in browser
        private void Retry()
        {
            IsShowingFailedMessage = false;
            IsLoading = true;

            _webView?.Refresh();
        }

        private ICommand _refreshCommand;

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(() => _webView?.Refresh());
                }

                return _refreshCommand;
            }
        }

        public RelayCommand OpenInBrowserCommand { get; private set; }

        private bool CanOpenInBrowser()
        {
            return SelectedRSS != null && SelectedRSS.LaunchURL != null;
        }

        /// <summary>
        /// Open selected rss as full post in the user default browser
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task OpenInBrowser()
        {
            await Windows.System.Launcher.LaunchUriAsync(SelectedRSS.LaunchURL);
        }

        public RelayCommand MarkAsReadCommand { get; private set; }

        private bool CanMarkAsRead()
        {
            return FilterSources.Count > 0;
        }

        /// <summary>
        /// Gets every item in the feed and change any unread item to read
        /// then updateing the database with it
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task MarkAsRead()
        {
            foreach (var item in Feeds.Where(x => x.IsRead == false))
            {
                item.IsRead = true;
                await RSSDataService.UpdateFeedAsync(item);
            }
        }

        private ICommand _filterCommand;

        public ICommand FilterCommand
        {
            get
            {
                _filterCommand = new RelayCommand(async () => await Filter());
                return _filterCommand;
            }
        }

        private async Task Filter()
        {
            await new MessageDialog("Filtring the feed is coming soon").ShowAsync();
        }

        public ICommand RefreshFeedsCommand { get; private set; }

        /// <summary>
        /// Reloads the feed by calling LoadDataAsync()
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task RefreshFeeds()
        {
            await LoadDataAsync(new Progress<int>(percent => ProgressCurrent = percent), TokenSource.Token);
        }

        public RelayCommand ClearSelectedRSSCommand { get; private set; }

        private bool CanClearSelectedRSS()
        {
            return SelectedRSS != null;
        }

        /// <summary>
        /// Clear the Selected rss property from the list and retun the built-in browser to blank
        /// </summary>
        private void ClearSelectedRSS()
        {
            SelectedRSS = null;
            GetTheme();
            if (_uiTheme == "#FF000000" && (_appTheme == ElementTheme.Default || _appTheme == ElementTheme.Dark))
            {
                //dark
                _webView.NavigateToString($"<html><head><title>blank</title><style> body {{ background:black}} h1 {{ color: white;}} h4 {{ color: white;}}</style></head><body></body></html>");
            }
            else
            {
                _webView.NavigateToString($"<html><head><title>blank</title></head><body></body></html>");
            }
        }

        public RelayCommand OpenPostInAppCommand { get; private set; }

        private bool CanOpenPostInApp()
        {
            return SelectedRSS != null && (WebViewSource != SelectedRSS.LaunchURL);
        }

        private void OpenPostInApp()
        {
            IsLoading = true;
            WebViewSource = SelectedRSS.LaunchURL;
        }

        private WebView _webView;

        public MainViewModel()
        {
            LoadCommands();
            IsLoading = true;
            WebViewSource = new Uri(_defaultUrl);
            TokenSource = new CancellationTokenSource();
        }

        private void LoadCommands()
        {
            OpenInBrowserCommand = new RelayCommand(async () => await OpenInBrowser(), CanOpenInBrowser);
            MarkAsReadCommand = new RelayCommand(async () => await MarkAsRead(), CanMarkAsRead);
            ClearSelectedRSSCommand = new RelayCommand(ClearSelectedRSS, CanClearSelectedRSS);
            RefreshFeedsCommand = new RelayCommand(async () => await RefreshFeeds());
            OpenPostInAppCommand = new RelayCommand(OpenPostInApp, CanOpenPostInApp);
        }

        /// <summary>
        /// Gets all the feeds from database (with-in limits in settings)
        /// the try to gets all the new stuff from your sources
        /// add the new ones to the database if there is any
        /// then show the latest (with-in limits in settings)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns>Task Type</returns>
        public async Task LoadDataAsync(IProgress<int> progress, CancellationToken token)
        {
            IsLoadingData = true;
            FilterSources.Clear();
            Feeds.Clear();

            foreach (var rss in await RSSDataService.GetFeedsDataAsync(await ApplicationData.Current.LocalSettings.ReadAsync<int>("FeedsLimit")))
            {
                Feeds.Add(rss);
            }

            SyndicationFeed feed = new SyndicationFeed();

            var sourcesDataList = await SourceDataService.GetSourcesDataAsync();

            ProgressMax = sourcesDataList.Count();
            ProgressCurrent = 0;
            int progressCount = 0;

            foreach (var source in sourcesDataList)
            {
                FilterSources.Add(source);

                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    return;
                }
            }

            try
            {
                bool isNetworkConnected = NetworkInterface.GetIsNetworkAvailable();
                if (!isNetworkConnected)
                {
                    await new MessageDialog(_loader.GetString("CheckInternetMessageDialog")).ShowAsync();
                }

                foreach (var sourceItem in FilterSources)
                {
                    if (token.IsCancellationRequested)
                    {
                        IsLoadingData = false;
                        return;
                    }

                    progress.Report(progressCount++);

                    if (!isNetworkConnected)
                    {
                        break;
                    }

                    var feedString = await RssRequest.GetFeedAsStringAsync(sourceItem.RssUrl);

                    if (string.IsNullOrWhiteSpace(feedString))
                    {
                        return;
                    }
                    else
                    {
                        feed.Load(feedString);
                    }

                    // Iterate through each feed item.
                    foreach (SyndicationItem syndicationItem in feed.Items)
                    {
                        if (token.IsCancellationRequested)
                        {
                            IsLoadingData = false;
                            return;
                        }

                        //handle edge cases like when they don't send that stuff or misplace them like freaking reddit r/worldnews
                        if (syndicationItem.Title == null)
                        {
                            syndicationItem.Title = new SyndicationText(_loader.GetString("MainViewModelNoTitleFound"));
                        }
                        if (syndicationItem.Summary == null)
                        {
                            syndicationItem.Summary = new SyndicationText(_loader.GetString("MainViewModelNoSummaryFound"));
                        }
                        if (syndicationItem.PublishedDate.Year < 2000)
                        {
                            syndicationItem.PublishedDate = syndicationItem.LastUpdatedTime.Year > 2000 ? syndicationItem.LastUpdatedTime : DateTimeOffset.Now;
                        }

                        Uri itemNewUri = syndicationItem.ItemUri;
                        if (itemNewUri == null)
                        {
                            if (syndicationItem.Links.Count > 0)
                            {
                                itemNewUri = syndicationItem.Links.FirstOrDefault().Uri;
                            }
                        }

                        var rss = new RSS
                        {
                            PostTitle = syndicationItem.Title.Text,
                            Description = syndicationItem.Summary.Text,
                            Authors = new List<Author>(),
                            URL = itemNewUri,
                            CreatedAt = syndicationItem.PublishedDate.DateTime,
                            Guid = syndicationItem.Id,
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

                        if (!await RSSDataService.FeedExistAsync(rss))
                        {
                            var g = await RSSDataService.AddNewFeedAsync(rss);
                        }
                    }

                    //shorten the text for windows 10 Live Tile
                    Singleton<LiveTileService>.Instance.SampleUpdate(feed.Title.Text, ShortenText(feed.Items.FirstOrDefault()?.Title.Text, 80), ShortenText(feed.Items.FirstOrDefault()?.Summary.Text, 95));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Feeds.Clear();
                foreach (var rss in await RSSDataService.GetFeedsDataAsync(await ApplicationData.Current.LocalSettings.ReadAsync<int>("FeedsLimit")))
                {
                    Feeds.Add(rss);
                }
                MarkAsReadCommand.OnCanExecuteChanged();
                IsLoadingData = false;
            }
        }

        public void Initialize(WebView webView)
        {
            _webView = webView;
            GetTheme();
            if (_uiTheme == "#FF000000" && (_appTheme == ElementTheme.Default || _appTheme == ElementTheme.Dark))
            {
                //dark
                _webView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(100, 0, 0, 0);
            }
        }

        private void GetTheme()
        {
            _systemTheme = new Windows.UI.ViewManagement.UISettings();
            _uiTheme = _systemTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
            _appTheme = ThemeSelectorService.Theme;
        }

        /// <summary>
        /// return back a string from the start to entered postion
        /// then adding "..." at the end of it 
        /// </summary>
        /// <param name="txt">the test to substring</param>
        /// <param name="endindex">intger for the end index</param>
        /// <returns>new shorted string</returns>
        private string ShortenText(string txt, int endindex)
        {
            return txt.Length > endindex ? string.Concat(txt.Substring(0, endindex), "...") : txt;
        }
    }
}
