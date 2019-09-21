using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Caliburn.Micro;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Yup.Core.Models;
using Yup.Core.Services;
using Yup.Services;

namespace Yup.ViewModels
{
  public class MainViewModel : Screen, IDeactivate
  {
    private readonly PreviousConnectionsService _prevConnService;
    private readonly MongoService _mongoservice;
    private PreviousConnection _selecteditem;
    public bool _showAddForm;
    private readonly INavigationService _navigation;

    private ObservableCollection<PreviousConnection> _prevConnections = new ObservableCollection<PreviousConnection>();
    private bool _isLoading = true;

    public ObservableCollection<PreviousConnection> PrevConnections { get => _prevConnections; set { Set(ref _prevConnections, value); } }
    public PreviousConnection SelectedItem { get => _selecteditem; set { Set(ref _selecteditem, value); } }
    public bool ShowAddForm { get => _showAddForm; set { Set(ref _showAddForm, value); } }
    public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }


    public MainViewModel(MongoService ms, PreviousConnectionsService pcs, INavigationService nav)
    {
      _prevConnService = pcs;
      _mongoservice = ms;
      _navigation = nav;
    }

    protected override void OnDeactivate(bool close)
    {
      var active = _prevConnService.GetActiveConnection();
      _mongoservice.SetUrl(active.MongoUrl);
      Clipboard.ContentChanged -= Clipboard_ContentChanged;
      base.OnDeactivate(close);
    }

    protected override void OnViewReady(object view)
    {
      base.OnViewReady(view);
      LoadPreviousConnections();
      Clipboard.ContentChanged += Clipboard_ContentChanged;
    }

    private async void Clipboard_ContentChanged(object sender, object e)
    {
      var package = Clipboard.GetContent();
      if (!package.Contains(StandardDataFormats.Text)) { return; }
      try
      {
        var content = await package.GetTextAsync();
        if (content.StartsWith("mongodb://"))
        {
          SelectedItem = new PreviousConnection() { IsActive = false, KeyName = SelectedItem.KeyName, MongoUrl = content };
        }
      }
      catch (Exception err)
      {
        Debug.WriteLine(err.Message);
      }
    }

    public void OnAdd()
    {
      ShowAddForm = !ShowAddForm;
      SelectedItem = new PreviousConnection();
    }

    public void OnDelete()
    {
      DeleteConnection(SelectedItem);
      SelectedItem = null;
      ShowAddForm = false;
      LoadPreviousConnections();
    }

    public void OnCancel()
    {
      SelectedItem = null;
      ShowAddForm = false;
    }

    public async void OnSelectedItem(ItemClickEventArgs args)
    {
      SelectedItem = args.ClickedItem as PreviousConnection;
      SelectedItem.IsActive = true;
      await AddConnection(SelectedItem);
    }

    public async void OnConnect()
    {
      if (SelectedItem == null || string.IsNullOrEmpty(SelectedItem.MongoUrl)) return;

      _mongoservice.SetUrl(SelectedItem.MongoUrl);
      await _prevConnService.SetActiveConnectionAsync(SelectedItem);
      _navigation.NavigateToViewModel<DatabasesViewModel>();
      IsLoading = true;
    }

    public async void OnSaveCurrent(string keyName, string partial)
    {
      var connection = new PreviousConnection() { KeyName = keyName, MongoUrl = partial, IsActive = true };
      await AddConnection(connection);
      SelectedItem = null;
      ShowAddForm = false; ;
      LoadPreviousConnections();
    }

    public async void OnPaperClip()
    {
      var package = Clipboard.GetContent();

      try
      {
        var content = await package.GetTextAsync();
        SelectedItem = new PreviousConnection() { IsActive = false, KeyName = SelectedItem.KeyName, MongoUrl = content };
      }
      catch (Exception e)
      {
        Debug.WriteLine(e.Message);
      }
    }

    protected async void LoadPreviousConnections()
    {
      IsLoading = true;
      PrevConnections.Clear();
      var conns = await _prevConnService.GetPreviousConnectionsAsync();
      foreach (var conn in conns)
      {
        PrevConnections.Add(conn);
      }
      IsLoading = false;
    }

    protected async Task AddConnection(PreviousConnection toAdd)
    {
      IsLoading = true;
      await _prevConnService.SaveConnection(toAdd.KeyName, toAdd);
      IsLoading = false;
    }

    protected void DeleteConnection(PreviousConnection connection)
    {
      IsLoading = true;
      _prevConnService.RemoveConnection(connection);
      IsLoading = false;
    }
  }
}
