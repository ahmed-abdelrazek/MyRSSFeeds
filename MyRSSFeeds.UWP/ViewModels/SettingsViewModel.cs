using LiteDB;
using MyRSSFeeds.Core.Data;
using MyRSSFeeds.Core.Models;
using MyRSSFeeds.Core.Services;
using MyRSSFeeds.UWP.Helpers;
using MyRSSFeeds.UWP.Services;
using OPMLCore.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace MyRSSFeeds.UWP.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings.md
    public class SettingsViewModel : Observable
    {
        private readonly RSSDataService rssDataService;
        private readonly SourceDataService sourceDataService;
        private readonly UserAgentService userAgentService;

        public bool IsLoaded { get; set; }

        private int _feedsLimit;

        public int FeedsLimit
        {
            get => _feedsLimit;
            set
            {
                Set(ref _feedsLimit, value, nameof(FeedsLimit), () =>
                {
                    ApplicationData.Current.LocalSettings.SaveAsync("FeedsLimit", value).ConfigureAwait(false);
                });
            }
        }

        private int _waitAfterLastCheck;

        public int WaitAfterLastCheck
        {
            get => _waitAfterLastCheck;
            set
            {
                Set(ref _waitAfterLastCheck, value, nameof(WaitAfterLastCheck), () =>
                {
                    ApplicationData.Current.LocalSettings.SaveAsync("WaitAfterLastCheck", value).ConfigureAwait(false);
                });
            }
        }

        private string _userAgentName;

        public string UserAgentName
        {
            get => _userAgentName;
            set
            {
                Set(ref _userAgentName, value, nameof(UserAgentName), () =>
                {
                    AddUserAgentCommand.OnCanExecuteChanged();
                });
            }
        }

        private string _userAgentValue;

        public string UserAgentValue
        {
            get => _userAgentValue;
            set
            {
                Set(ref _userAgentValue, value, nameof(UserAgentValue), () =>
                {
                    AddUserAgentCommand.OnCanExecuteChanged();
                });
            }
        }

        public ObservableCollection<UserAgent> UserAgents { get; private set; } = new ObservableCollection<UserAgent>();

        private UserAgent _selectedUserAgent;

        public UserAgent SelectedUserAgent
        {
            get => _selectedUserAgent;
            set
            {
                Set(ref _selectedUserAgent, value, nameof(SelectedUserAgent), () =>
                {
                    DeleteUserAgentCommand.OnCanExecuteChanged();

                    if (_selectedUserAgent != null)
                    {
                        userAgentService.ResetAgentUse();
                        SelectedUserAgent.IsUsed = true;
                        userAgentService.UpdateAgent(SelectedUserAgent);
                    }
                });
            }
        }

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        private ICommand _exportSourceAsJsonCommand;

        public ICommand ExportSourceAsJsonCommand
        {
            get
            {
                if (_exportSourceAsJsonCommand == null)
                {
                    _exportSourceAsJsonCommand = new RelayCommand(
                        async () =>
                        {
                            var savePicker = new Windows.Storage.Pickers.FileSavePicker
                            {
                                SuggestedStartLocation =
                                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                            };
                            // Dropdown of file types the user can save the file as
                            savePicker.FileTypeChoices.Add("JSON File", new List<string>() { ".json" });
                            // Default file name if the user does not type one in or select a file to replace
                            savePicker.SuggestedFileName = "ExportedRssSources";
                            StorageFile file = await savePicker.PickSaveFileAsync();
                            if (file != null)
                            {
                                try
                                {
                                    // Prevent updates to the remote version of the file until
                                    // we finish making changes and call CompleteUpdatesAsync.
                                    CachedFileManager.DeferUpdates(file);
                                    // write to file
                                    var jsonContent = await Core.Helpers.Json.StringifyAsync(sourceDataService.GetSourcesData());
                                    await FileIO.WriteTextAsync(file, jsonContent);
                                    // Let Windows know that we're finished changing the file so
                                    // the other app can update the remote version of the file.
                                    // Completing updates may require Windows to ask for user input.
                                    Windows.Storage.Provider.FileUpdateStatus status =
                                        await CachedFileManager.CompleteUpdatesAsync(file);
                                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                                    {
                                        await new MessageDialog(string.Format("Settings_ExportSuccessfulMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                                    }
                                    else
                                    {
                                        await new MessageDialog(string.Format("Settings_ExportFailedMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                    await new MessageDialog(string.Format("Settings_ExportErrorMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                                }
                            }
                            else
                            {
                                await new MessageDialog("Settings_ExportCanceledMessageDialog".GetLocalized()).ShowAsync();
                            }
                        });
                }

                return _exportSourceAsJsonCommand;
            }
        }

        private ICommand _importSourceAsJsonCommand;

        public ICommand ImportSourceAsJsonCommand
        {
            get
            {
                if (_importSourceAsJsonCommand == null)
                {
                    _importSourceAsJsonCommand = new RelayCommand(
                        async () =>
                        {
                            var picker = new Windows.Storage.Pickers.FileOpenPicker
                            {
                                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                            };
                            picker.FileTypeFilter.Add(".json");

                            StorageFile file = await picker.PickSingleFileAsync();
                            if (file != null)
                            {
                                try
                                {
                                    // Application now has read/write access to the picked file
                                    var sourcesList = await Core.Helpers.Json.ToObjectAsync<List<Source>>(await FileIO.ReadTextAsync(file));

                                    foreach (var source in sourcesList)
                                    {
                                        if (sourceDataService.SourceExist(source.RssUrl.ToString()))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            sourceDataService.AddNewSource(source);
                                        }
                                    }
                                    await new MessageDialog(string.Format("Settings_ImportSuccessfulMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex);
                                    await new MessageDialog(string.Format("Settings_ImportErrorMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                                }
                            }
                            else
                            {
                                await new MessageDialog("Settings_ImportCanceledMessageDialog".GetLocalized()).ShowAsync();
                            }
                        });
                }

                return _importSourceAsJsonCommand;
            }
        }

        public RelayCommand AddUserAgentCommand { get; private set; }

        public RelayCommand DeleteUserAgentCommand { get; private set; }

        public RelayCommand ImportSourcesAsOPMLCommand { get; private set; }
        public RelayCommand ExportSourcesAsOPMLCommand { get; private set; }

        public SettingsViewModel()
        {
            AddUserAgentCommand = new RelayCommand(async () => await AddUserAgent(), CanAddUserAgent);
            DeleteUserAgentCommand = new RelayCommand(async () => await DeleteUserAgent(), CanDeleteUserAgent);
            ImportSourcesAsOPMLCommand = new RelayCommand(async () => await ImportSourcesAsOPML(), CanImportSourcesAsOPML);
            ExportSourcesAsOPMLCommand = new RelayCommand(async () => await ExportSourcesAsOPML(), CanExportSourcesAsOPML);

            var db = new LiteDatabase(LiteDbContext.ConnectionString);
            rssDataService = new RSSDataService(db);
            sourceDataService = new SourceDataService(db);
            userAgentService = new UserAgentService(db);
        }

        private bool CanImportSourcesAsOPML()
        {
            return true;
        }

        private async Task ImportSourcesAsOPML()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".opml");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    // Application now has read/write access to the picked file
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(await FileIO.ReadTextAsync(file));

                    var opmlFile = new Opml(xmlDoc);

                    foreach (var source in opmlFile.Body.Outlines)
                    {
                        if (sourceDataService.SourceExist(source.XMLUrl))
                        {
                            continue;
                        }
                        else
                        {
                            Uri.TryCreate(source.XMLUrl, UriKind.Absolute, out Uri rssUrl);
                            Uri.TryCreate(source.HTMLUrl, UriKind.Absolute, out Uri baseUrl);

                            sourceDataService.AddNewSource(new Source
                            {
                                SiteTitle = source.Title,
                                BaseUrl = baseUrl ?? new Uri(rssUrl.GetLeftPart(UriPartial.Authority)),
                                RssUrl = rssUrl,
                                Description = source.Description
                            });
                        }
                    }
                    await new MessageDialog(string.Format("Settings_ImportSuccessfulMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    await new MessageDialog(string.Format("Settings_ImportErrorMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("Settings_ImportCanceledMessageDialog".GetLocalized()).ShowAsync();
            }
        }

        private bool CanExportSourcesAsOPML()
        {
            return true;
        }

        private async Task ExportSourcesAsOPML()
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("OPML File", new List<string>() { ".opml" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "ExportedOPMLRssSources";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                try
                {
                    // Prevent updates to the remote version of the file until
                    // we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    // write to file
                    var allSourcesList = sourceDataService.GetSourcesData();
                    var SourcesAsOutlinesList = allSourcesList.Select(x => new Outline
                    {
                        Title = x.SiteTitle,
                        Text = x.SiteTitle,
                        Type = "rss",
                        XMLUrl = x.RssUrl.ToString(),
                        HTMLUrl = x.BaseUrl?.ToString(),
                        //Description = x.Description,
                        //Created = x.LastBuildDate.DateTime
                    });

                    var opmlFile = new Opml(new Head("My RSS Feed Sources"), new Body(SourcesAsOutlinesList.ToList()));

                    await FileIO.WriteTextAsync(file, opmlFile.ToString());
                    // Let Windows know that we're finished changing the file so
                    // the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    Windows.Storage.Provider.FileUpdateStatus status =
                        await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        await new MessageDialog(string.Format("Settings_ExportSuccessfulMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                    }
                    else
                    {
                        await new MessageDialog(string.Format("Settings_ExportFailedMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    await new MessageDialog(string.Format("Settings_ExportErrorMessageDialog".GetLocalized(), file.Name)).ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("Settings_ExportCanceledMessageDialog".GetLocalized()).ShowAsync();
            }
        }

        private bool CanAddUserAgent()
        {
            return !string.IsNullOrWhiteSpace(UserAgentName) && !string.IsNullOrWhiteSpace(UserAgentValue);
        }

        private async Task AddUserAgent()
        {
            try
            {
                userAgentService.AddNewAgent(new UserAgent
                {
                    Name = UserAgentName,
                    AgentString = UserAgentValue,
                    IsDeletable = true,
                    IsUsed = false
                });

                await new MessageDialog("SettingsViewModelAddNewAgentMessageDialog".GetLocalized()).ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private bool CanDeleteUserAgent()
        {
            return SelectedUserAgent != null;
        }

        private async Task DeleteUserAgent()
        {
            try
            {
                if (SelectedUserAgent.IsDeletable)
                {
                    userAgentService.DeleteAgent(SelectedUserAgent);

                    await new MessageDialog("SettingsViewModelDeleteAgentMessageDialog".GetLocalized()).ShowAsync();

                    SelectedUserAgent = UserAgents.SingleOrDefault(x => x.IsDeletable == false);
                }
                else
                {
                    await new MessageDialog("SettingsViewModelUnDeletableAgentMessageDialog".GetLocalized()).ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task InitializeAsync()
        {
            FeedsLimit = await ApplicationData.Current.LocalSettings.ReadAsync<int>("FeedsLimit");
            WaitAfterLastCheck = await ApplicationData.Current.LocalSettings.ReadAsync<int>("WaitAfterLastCheck");
            VersionDescription = GetVersionDescription();
            IsLoaded = true;
            UserAgents.Clear();
            foreach (var item in userAgentService.GetAgentsData())
            {
                UserAgents.Add(item);
            }
            SelectedUserAgent = UserAgents.SingleOrDefault(x => x.IsUsed);
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
