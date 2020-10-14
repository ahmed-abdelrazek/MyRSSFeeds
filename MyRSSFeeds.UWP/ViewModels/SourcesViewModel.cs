using MyRSSFeeds.Core.Helpers;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.Helpers;
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
    public class SourcesViewModel : Observable
    {
        public CancellationTokenSource TokenSource { get; set; } = null;

        private bool _isButtonEnabled;

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set { Set(ref _isButtonEnabled, value); }
        }

        private string _sourceTitle;

        public string SourceTitle
        {
            get { return _sourceTitle; }
            set
            {
                Set(ref _sourceTitle, value, nameof(SourceTitle), () =>
                {
                    AddNewSourceCommand.OnCanExecuteChanged();
                    UpdateSourceCommand.OnCanExecuteChanged();
                });
            }
        }

        private string _sourceUrl;

        public string SourceUrl
        {
            get { return _sourceUrl; }
            set
            {
                Set(ref _sourceUrl, value, nameof(SourceUrl), () =>
                {
                    AddNewSourceCommand.OnCanExecuteChanged();
                    UpdateSourceCommand.OnCanExecuteChanged();
                });
            }
        }

        private string _sourceDescription;

        public string SourceDescription
        {
            get { return _sourceDescription; }
            set { Set(ref _sourceDescription, value); }
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set { Set(ref _isWorking, value); }
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

        private bool _isLoadingData;

        public bool IsLoadingData
        {
            get
            {
                return _isLoadingData;
            }
            set
            {
                Set(ref _isLoadingData, value, nameof(IsLoadingData), () =>
                {
                    RefreshSourcesCommand.OnCanExecuteChanged();
                    CancelLoadingCommand.OnCanExecuteChanged();
                });
            }
        }

        private Source _selectedSource;

        public Source SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                Set(ref _selectedSource, value, nameof(SelectedSource), () =>
                {
                    UpdateSourceCommand.OnCanExecuteChanged();
                    DeleteSourceCommand.OnCanExecuteChanged();
                    ClearSelectedSourceCommand.OnCanExecuteChanged();

                    if (_selectedSource != null)
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
                });
            }
        }

        public ObservableCollection<Source> Sources { get; private set; } = new ObservableCollection<Source>();

        public RelayCommand AddNewSourceCommand { get; private set; }

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
                var exist = await SourceDataService.SourceExistAsync(trimedUrl);
                if (exist)
                {
                    await new MessageDialog("SourcesViewModelSourceExistMessageDialog".GetLocalized()).ShowAsync();
                }
                else
                {
                    try
                    {
                        var feedString = await RssRequest.GetFeedAsStringAsync(trimedUrl);
                        var xmlSource = feedString.TrimStart();

                        var source = await SourceDataService.GetSourceInfoFromRssAsync(xmlSource, trimedUrl);
                        if (source == null)
                        {
                            await new MessageDialog("SourcesViewModelSourceInfoNotValidMessageDialog".GetLocalized()).ShowAsync();
                            return;
                        }
                        Sources.Insert(0, await SourceDataService.AddNewSourceAsync(source));

                        RefreshSourcesCommand.OnCanExecuteChanged();

                        await new MessageDialog("SourcesViewModelSourceAddedMessageDialog".GetLocalized()).ShowAsync();

                        ClearPopups();
                    }
                    catch (HttpRequestException ex)
                    {
                        await new MessageDialog("HttpRequestExceptionMessageDialog".GetLocalized()).ShowAsync();
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

        public RelayCommand UpdateSourceCommand { get; private set; }

        private bool CanUpdateSource()
        {
            return SelectedSource != null && !string.IsNullOrEmpty(SourceTitle) && !string.IsNullOrWhiteSpace(SourceUrl);
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

                var source = await SourceDataService.UpdateSourceAsync(SelectedSource);
                if (source == null)
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

        public RelayCommand DeleteSourceCommand { get; private set; }

        private bool CanDeleteSource()
        {
            return SelectedSource != null;
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
                await SourceDataService.DeleteSourceAsync(SelectedSource);
                Sources.Remove(SelectedSource);
                RefreshSourcesCommand.OnCanExecuteChanged();
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

        public RelayCommand RefreshSourcesCommand { get; private set; }

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
            return SelectedSource != null;
        }

        /// <summary>
        /// Clears selected source property and the popups fields
        /// </summary>
        private void ClearSelectedSource()
        {
            SelectedSource = null;
            ClearPopups();
        }

        public SourcesViewModel()
        {
            AddNewSourceCommand = new RelayCommand(async () => await AddNewSource(), CanAddNewSource);
            UpdateSourceCommand = new RelayCommand(async () => await UpdateSource(), CanUpdateSource);
            DeleteSourceCommand = new RelayCommand(async () => await DeleteSource(), CanDeleteSource);
            RefreshSourcesCommand = new RelayCommand(async () => await RefreshSources(), CanRefreshSources);
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

            var sourcesDataList = await SourceDataService.GetSourcesDataAsync();

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

            RefreshSourcesCommand.OnCanExecuteChanged();

            foreach (var item in Sources)
            {
                if (token.IsCancellationRequested)
                {
                    IsLoadingData = false;
                    TokenSource = new CancellationTokenSource();
                    return;
                }

                await item.CheckIfSourceWorking();
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
    }
}
