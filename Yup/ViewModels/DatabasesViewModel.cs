using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using Windows.UI.Xaml.Controls;
using Yup.Core.Models;
using Yup.Core.Services;

namespace Yup.ViewModels
{
  public class DatabasesViewModel : Screen
  {
    private readonly MongoService _mongoservice;
    private ObservableCollection<DatabaseEntry> databases = new ObservableCollection<DatabaseEntry>();
    private string selecteddb;
    private string _queryError;
    private string queryStatement;
    private bool _isLoadingDatabases = true;
    private bool isLoadingEditor = false;
    private bool isLoadingResults = false;
    private ObservableCollection<string> _results = new ObservableCollection<string>();
    private ObservableCollection<string> _headerResults = new ObservableCollection<string>();

    public string SelectedDb { get => selecteddb; set => Set(ref selecteddb, value); }
    public string QueryError { get => _queryError; set => Set(ref _queryError, value); }
    public string QueryStatement { get => queryStatement; set => Set(ref queryStatement, value); }

    public ObservableCollection<DatabaseEntry> Databases { get => databases; set => Set(ref databases, value); }
    public ObservableCollection<string> QueryResults { get => _results; set => Set(ref _results, value); }
    public ObservableCollection<string> HeaderResults { get => _headerResults; set => Set(ref _headerResults, value); }


    public bool IsLoadingDatabases { get => _isLoadingDatabases; set => Set(ref _isLoadingDatabases, value); }
    public bool IsLoadingEditor { get => isLoadingEditor; set => Set(ref isLoadingEditor, value); }
    public bool IsLoadingResults { get => isLoadingResults; set => Set(ref isLoadingResults, value); }

    public event EventHandler<DatabaseEntry> OnEntrySelected;

    public DatabasesViewModel(MongoService ms)
    {
      _mongoservice = ms;
    }

    protected override async void OnViewReady(object view)
    {
      base.OnViewReady(view);
      IsLoadingDatabases = true;
      var databases = await _mongoservice.GetDatabases();
      Databases.Clear();
      foreach (var database in databases)
      {
        Databases.Add(new DatabaseEntry()
        {
          Database = database,
          Name = database,
          EntryType = EntryType.Database,
          Children = new ObservableCollection<DatabaseEntry>()
        }); ;
      }
      IsLoadingDatabases = false;
    }

    public async void OnItemInvoked(Microsoft.UI.Xaml.Controls.TreeViewItemInvokedEventArgs args)
    {
      var entry = args.InvokedItem as DatabaseEntry;

      if (entry.EntryType == EntryType.Collection) return;
      var selected = Databases.First(database => database.Name == entry.Name);
      SelectedDb = selected.Name;
      selected.IsActive = true;
      selected.IsExpanded = true;
      await AddCollectionsTo(selected);
      UpdateDatabasesWith(selected);
    }

    public void OnNavigationStarting()
    {
      IsLoadingEditor = true;
    }

    public void OnWebViewLoaded(WebViewNavigationCompletedEventArgs args)
    {
      IsLoadingEditor = false;
    }

    public void OnDoubleTapped(DatabaseEntry entry)
    {
      IsLoadingEditor = true;
      OnEntrySelected?.Invoke(this, entry);
      SelectedDb = entry.Database;
    }

    public async void OnScriptNotify(NotifyEventArgs args)
    {
      var execution = args.Value.Split(';');
      var command = execution[0];
      var commandValue = execution[1];
      QueryStatement = commandValue;
      switch (command)
      {
        // Shouldn't but... Will add more if necessary
        case "ExecuteInEditor":
          await OnExecuteStatement();
          break;
      }
    }

    public async Task OnExecuteStatement()
    {
      IsLoadingResults = true;
      QueryError = "";
      HeaderResults.Clear();
      QueryResults.Clear();
      try
      {
        var (cursor, ok) = await _mongoservice.ExecuteRawAsync(SelectedDb, QueryStatement);
        cursor.Value.AsBsonDocument.TryGetValue("firstBatch", out BsonValue firstBatch);
        var rows = firstBatch.AsBsonArray;
        foreach (var item in rows?.FirstOrDefault().AsBsonDocument.ToDictionary()?.Keys)
        {
          HeaderResults.Add(item);
        }
        foreach (var row in rows)
        {
          var result = row.AsBsonDocument.ToJson(new JsonWriterSettings { Indent = true, IndentChars = "  " });
          QueryResults.Add(result);
        }

      }
      catch (Exception e)
      {
        QueryError = e.Message;
        Debug.WriteLine($"{e.Message}");
      }
      IsLoadingResults = false;
    }

    private async Task AddCollectionsTo(DatabaseEntry entry)
    {
      var collections = await _mongoservice.GetCollectionsFrom(entry.Name);
      var entries = collections
        .Select(collection =>
          new DatabaseEntry()
          {
            Database = entry.Database,
            IsActive = false,
            IsExpanded = false,
            Name = collection,
            EntryType = EntryType.Collection
          });
      entry.Children.Clear();
      foreach (var collection in entries)
      {
        entry.Children.Add(collection);
      }
    }

    private void UpdateDatabasesWith(DatabaseEntry entry)
    {
      var holder = new DatabaseEntry[Databases.Count];
      holder = Databases.Select(e =>
      {
        if (e.Name != entry.Name)
          return e;
        else
          return entry;
      }).ToArray();
      Databases.Clear();
      foreach (var item in holder)
      {
        Databases.Add(item);
      }
    }
  }
}
