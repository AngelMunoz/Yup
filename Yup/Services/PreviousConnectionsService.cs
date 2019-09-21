using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Yup.Core.Helpers;
using Yup.Core.Models;
using Yup.Helpers;

namespace Yup.Services
{
  public class PreviousConnectionsService
  {
    public Task<PreviousConnection[]> GetPreviousConnectionsAsync()
    {
      var connections = ApplicationData
          .Current
          .RoamingSettings
          .Values
          .Where(kv => kv.Key.Contains("previous:mongodb:"))
          .Select(kv => Json.ToObjectAsync<PreviousConnection>(kv.Value as string));
      return Task.WhenAll(connections);
    }

    public Task SetActiveConnectionAsync(PreviousConnection toSet)
    {
      var keys = ApplicationData
        .Current
        .RoamingSettings
        .Values
        .Where(kv => kv.Key.Contains("previous:mongodb:"))
        .Select(entry =>
          Json.ToObjectAsync<PreviousConnection>(entry.Value as string)
            .ContinueWith(prevTask =>
            {
              var result = prevTask.Result;
              if (result.KeyName == toSet.KeyName)
              {
                result.IsActive = true;
              }
              else
              {
                result.IsActive = false;
              }
              return ApplicationData.Current.RoamingSettings.SaveAsync(entry.Key, result);
            })
        );
      return Task.WhenAll(keys);
    }

    public Task<PreviousConnection> GetActiveConnectionAsync()
    {
      var values = ApplicationData
        .Current
        .RoamingSettings
        .Values
        .Where(kv =>
        {
          var obj = Json.ToObject<PreviousConnection>(kv.Value as string);
          return obj.IsActive;
        })
        .FirstOrDefault();
      return Json.ToObjectAsync<PreviousConnection>(values.Value as string);
    }

    public PreviousConnection GetActiveConnection()
    {
      var values = ApplicationData
        .Current
        .RoamingSettings
        .Values
        .Where(kv =>
        {
          var obj = Json.ToObject<PreviousConnection>(kv.Value as string);
          return obj.IsActive;
        })
        .FirstOrDefault();
      return Json.ToObject<PreviousConnection>(values.Value as string);
    }

    public Task<PreviousConnection> GetPreviousConnectionByKeyAsync(string key)
    {
      var previous = ApplicationData
          .Current
          .RoamingSettings
          .Values
          .FirstOrDefault(kv => kv.Key.Contains($"previous:mongodb:{key}"))
          .Value;
      return Json.ToObjectAsync<PreviousConnection>(previous as string);
    }

    public Task SaveConnection(string keyName, PreviousConnection value)
    {
      if (ApplicationData.Current.IsRoamingStorageAvailable())
      {
        return ApplicationData.Current.RoamingSettings.SaveAsync($"previous:mongodb:{keyName}", value);
      }
      throw new Exception("Out of Roaming Storage");
    }

    public void RemoveConnection(PreviousConnection toRemove)
    {
      ApplicationData.Current.RoamingSettings.RemoveKeyValue($"previous:mongodb:{toRemove.KeyName}");
      return;
    }
  }
}
