using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Yup.Core.Services
{
  public class MongoService
  {
    private MongoClient _mongoclient;
    public string CurrentUrl { get; private set; }

    public void SetUrl(string url)
    {
      _mongoclient = new MongoClient(url);
      CurrentUrl = url;
    }

    public async Task<IEnumerable<string>> GetCollectionsFrom(string dbName)
    {
      var database = _mongoclient.GetDatabase(dbName);
      var cursor = await database.ListCollectionNamesAsync();
      return cursor.ToEnumerable();
    }

    public async Task<IEnumerable<string>> GetDatabases()
    {
      var cursor = await _mongoclient.ListDatabaseNamesAsync();
      return cursor.ToEnumerable();
    }

    public async Task<(BsonElement, BsonElement)> ExecuteRawAsync(string dbName, string command)
    {
      var db = _mongoclient.GetDatabase(dbName);
      var result = await db.RunCommandAsync<BsonDocument>(command);
      result.TryGetElement("cursor", out BsonElement cursor);
      result.TryGetElement("ok", out BsonElement ok);
      return (cursor, ok);
    }
  }
}
