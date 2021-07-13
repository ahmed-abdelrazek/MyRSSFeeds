using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using MyRSSFeeds.Contracts.ViewModels;
using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Popups;

namespace MyRSSFeeds.ViewModels
{
    public class SourcesListDetailsViewModel : ObservableRecipient, INavigationAware
    {
        private readonly IRSSDataService _rssDataService;
        private readonly ISourceDataService _sourceDataService;

        public CancellationTokenSource TokenSource { get; set; } = null;

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get => _isButtonEnabled;
            set => SetProperty(ref _isButtonEnabled, value);
        }

        private string _sourceTitle;

        public string SourceTitle
        {
            get => _sourceTitle;
            set
            {
                if (SetProperty(ref _sourceTitle, value))
                {
                    AddNewSourceCommand.NotifyCanExecuteChanged();
                    UpdateSourceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _sourceUrl;

        public string SourceUrl
        {
            get => _sourceUrl;
            set
            {
                if (SetProperty(ref _sourceUrl, value))
                {
                    AddNewSourceCommand.NotifyCanExecuteChanged();
                    UpdateSourceCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _sourceDescription;

        public string SourceDescription
        {
            get => _sourceDescription;
            set => SetProperty(ref _sourceDescription, value);
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get => _isWorking;
            set => SetProperty(ref _isWorking, value);
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

        private bool _isLoadingData;

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set
            {
                if (SetProperty(ref _isLoadingData, value))
                {
                    RefreshSourcesCommand.NotifyCanExecuteChanged();
                    CancelLoadingCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private Source _selectedSource;

        public Source SelectedSource
        {
            get => _selectedSource;
            set
            {
                if (SetProperty(ref _selectedSource, value))
                {
                    UpdateSourceCommand.NotifyCanExecuteChanged();
                    DeleteSourceCommand.NotifyCanExecuteChanged();
                    ClearSelectedSourceCommand.NotifyCanExecuteChanged();

                    if (_selectedSource is not null)
                    {
                        IsButtonEnabled = true;
                        SourceTitle = _selectedSource.SiteTitle;
                        SourceUrl = _selectedSource.RssUrl.ToString();
                        SourceDescription = _selectedSource.Description;
                    }
                    else
                    {
                        IsButtonEnabled = false;
                    }
                }
            }
        }

        public ObservableCollection<Source> Sources { get; private set; } = new ObservableCollection<Source>();

        public AsyncRelayCommand AddNewSourceCommand { get; private set; }

        private bool CanAddNewSource()
        {
            return !string.IsNullOrWhiteSpace(SourceUrl);
        }

        /// <summary>
        /// Check if the source exist if not gets its info then add it to the database
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task AddNewSource()
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;

            try
            {
                string trimedUrl = SourceUrl.TrimEnd('/');
                var exist = await _sourceDataService.SourceExistAsync(trimedUrl);
                if (exist)
                {
                    await new MessageDialog("SourcesViewModelSourceExistMessageDialog".GetLocalized()).ShowAsync();
                }
                else
                {
                    try
                    {
                        var feedString = await RssRequest.GetFeedAsStringAsync(trimedUrl, TokenSource.Token);

                        var source = await _sourceDataService.GetSourceInfoFromRssAsync(feedString, trimedUrl);
                        if (source is null)
                        {
                            await new MessageDialog("SourcesViewModelSourceInfoNotValidMessageDialog".GetLocalized()).ShowAsync();
                            return;
                        }
                        Sources.Insert(0, await _sourceDataService.AddNewSourceAsync(source));

                        RefreshSourcesCommand.NotifyCanExecuteChanged();

                        await new MessageDialog("SourcesViewModelSourceAddedMessageDialog".GetLocalized()).ShowAsync();

                        ClearPopups();
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message.StartsWith("Response status code does not indicate success: 403"))
                        {
                            await new MessageDialog("HttpRequestException403MessageDialog".GetLocalized()).ShowAsync();
                        }
                        else
                        {
                            await new MessageDialog("HttpRequestExceptionMessageDialog".GetLocalized()).ShowAsync();
                        }
                        Debug.WriteLine(ex);
                    }
                    catch (XmlException ex)
                    {
                        await new MessageDialog("XmlExceptionMessageDialog".GetLocalized()).ShowAsync();
                        Debug.WriteLine(ex);
                    }
                    catch (ArgumentNullException ex)
                    {
                        await new MessageDialog("SourcesViewModelSourceUrlNullExceptionMessageDialog".GetLocalized()).ShowAsync();
                        Debug.WriteLine(ex);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsWorking = false;
            }
        }

        public AsyncRelayCommand UpdateSourceCommand { get; private set; }

        private bool CanUpdateSource()
        {
            return SelectedSource is not null && !string.IsNullOrEmpty(SourceTitle) && !string.IsNullOrWhiteSpace(SourceUrl);
        }

        /// <summary>
        /// Update selected source (Title - Url - Description)
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task UpdateSource()
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;

            try
            {
                SelectedSource.SiteTitle = SourceTitle;
                SelectedSource.RssUrl = new Uri(SourceUrl);
                SelectedSource.Description = SourceDescription;

                var source = await _sourceDataService.UpdateSourceAsync(SelectedSource);
                if (source is null)
                {
                    await new MessageDialog("SourcesViewModelSourceInfoNotValidMessageDialog".GetLocalized()).ShowAsync();
                    return;
                }
                Sources[Sources.IndexOf(SelectedSource)] = SelectedSource;

                await new MessageDialog("SourcesViewModelSourceUpdatedMessageDialog".GetLocalized()).ShowAsync();

                ClearPopups();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsWorking = false;
            }
        }

        public AsyncRelayCommand DeleteSourceCommand { get; private set; }

        private bool CanDeleteSource()
        {
            return SelectedSource is not null;
        }

        /// <summary>
        /// Delete the selected source
        /// </summary>
        /// <returns>Task Type</returns>
        private async Task DeleteSource()
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;

            try
            {
                MessageDialog showDialog = new("SourcesViewModelSourceDeleteAllRSSMessageDialog".GetLocalized());
                showDialog.Commands.Add(new UICommand("Yes".GetLocalized())
                {
                    Id = 0
                });
                showDialog.Commands.Add(new UICommand("No".GetLocalized())
                {
                    Id = 1
                });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();

                if ((int)result.Id == 0)
                {
                    await _rssDataService.DeleteManyFeedsAsync(x => x.PostSource.Id == SelectedSource.Id);
                }

                await _sourceDataService.DeleteSourceAsync(SelectedSource);
                Sources.Remove(SelectedSource);
                ClearPopups();
                RefreshSourcesCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsWorking = false;
            }
        }

        public AsyncRelayCommand RefreshSourcesCommand { get; private set; }

        private bool CanRefreshSources()
        {
            return Sources.Count > 0 && !IsLoadingData;
        }

        /// <summary>
        /// Reload the sources list by calling LoadDataAsync()
        /// </summary>
        /// <returns></returns>
        private async Task RefreshSources()
        {
            await LoadDataAsync(new Progress<int>(percent => ProgressCurrent = percent), TokenSource.Token);
        }

        /// <summary>
        /// Stop loading online data from sources
        /// </summary>
        public RelayCommand CancelLoadingCommand { get; private set; }

        private bool CanCancelLoading()
        {
            return IsLoadingData;
        }

        private void CancelLoading()
        {
            TokenSource.Cancel();
        }

        public RelayCommand ClearSelectedSourceCommand { get; private set; }

        private bool CanClearSelectedSource()
        {
            return SelectedSource is not null;
        }

        /// <summary>
        /// Clears selected source property and the popups fields
        /// </summary>
        private void ClearSelectedSource()
        {
            SelectedSource = null;
            ClearPopups();
        }

        public SourcesListDetailsViewModel(IRSSDataService rssDataService, ISourceDataService sourceDataService)
        {
            _rssDataService = rssDataService;
            _sourceDataService = sourceDataService;
            AddNewSourceCommand = new AsyncRelayCommand(AddNewSource, CanAddNewSource);
            UpdateSourceCommand = new AsyncRelayCommand(UpdateSource, CanUpdateSource);
            DeleteSourceCommand = new AsyncRelayCommand(DeleteSource, CanDeleteSource);
            RefreshSourcesCommand = new AsyncRelayCommand(RefreshSources, CanRefreshSources);
            CancelLoadingCommand = new RelayCommand(CancelLoading, CanCancelLoading);
            ClearSelectedSourceCommand = new RelayCommand(ClearSelectedSource, CanClearSelectedSource);
            TokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets all the sources from the database and checks if they still works or not
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns>Task Type</returns>
        public async Task LoadDataAsync(IProgress<int> progress, CancellationToken token)
        {
            IsLoadingData = true;
            ProgressCurrent = 0;
            Sources.Clear();

            var sourcesDataList = await _sourceDataService.GetSourcesDataAsync();

            ProgressMax = sourcesDataList.Count();
            int progressCount = 0;

            foreach (var source in sourcesDataList)
            {
                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    TokenSource = new CancellationTokenSource();
                    return;
                }

                Sources.Add(source);

                progress.Report(++progressCount);
            }

            RefreshSourcesCommand.NotifyCanExecuteChanged();

            foreach (var item in Sources)
            {
                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    TokenSource = new CancellationTokenSource();
                    return;
                }

                try
                {
                    item.IsChecking = true;
                    var task = await _sourceDataService.IsSourceWorkingAsync(item.RssUrl.AbsoluteUri);
                    item.IsWorking = task.Item1;
                    item.LastBuildDate = task.Item2;
                    item.CurrentRssItemsCount = task.Item3;

                    // Saves latest build date and rss items count to source
                    await _sourceDataService.UpdateSourceAsync(item);
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message.StartsWith("Response status code does not indicate success: 403"))
                    {
                        item.ErrorMessage = "HttpRequestException403MessageDialog".GetLocalized();
                    }
                    else
                    {
                        item.ErrorMessage = "HttpRequestExceptionMessageDialog".GetLocalized();
                    }
                    Debug.WriteLine(ex);
                    item.IsError = true;
                }
                catch (XmlException ex)
                {
                    item.ErrorMessage = "XmlExceptionMessageDialog".GetLocalized();
                    Debug.WriteLine(ex);
                    item.IsError = true;
                }
                catch (ArgumentNullException ex)
                {
                    item.ErrorMessage = "SourcesViewModelSourceUrlNullExceptionMessageDialog".GetLocalized();
                    Debug.WriteLine(ex);
                    item.IsError = true;
                }
                catch (Exception ex)
                {
                    item.ErrorMessage = "SourcesViewModelExceptionMessageDialog".GetLocalized();
                    Debug.WriteLine(ex);
                    item.IsError = true;
                }
                finally
                {
                    item.IsChecking = false;
                }
            }

            IsLoadingData = false;
        }

        /// <summary>
        /// Clears popups fields
        /// </summary>
        private void ClearPopups()
        {
            SourceTitle = string.Empty;
            SourceUrl = string.Empty;
            SourceDescription = string.Empty;
        }

        public async void OnNavigatedTo(object parameter)
        {
            await LoadDataAsync(new Progress<int>(percent => ProgressCurrent = percent), TokenSource.Token);
        }

        public void OnNavigatedFrom()
        {
        }
    }
}
