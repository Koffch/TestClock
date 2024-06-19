using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TimeSynchronizer : ITimeSynchronizer
{
    private readonly Dictionary<string, string> _timeServers = new()
    {
        ["worldtimeapi.org"] = "https://worldtimeapi.org/api/timezone/Etc/UTC",
        ["yandex.com"] = "https://yandex.com/time/sync.json",
        ["timeapi.io"] = "https://timeapi.io/api/Time/current/zone?timeZone=UTC"
    };

    public TimeSynchronizer()
    {
        Servers = _timeServers.Keys.ToList();
    }

    public event Action<string> Finish;
    public List<string> Servers { get; }

    public async Task<DateTime> GetTime(int serverIndex)
    {
        var serverId = Servers[serverIndex];
        var request = UnityWebRequest.Get(_timeServers[serverId]);
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Finish?.Invoke($"Error contacting {serverId}: {request.error}");
            return new DateTime();
        }

        Finish?.Invoke("");
        return ParseTime(request.downloadHandler.text, serverIndex);
    }

    private static DateTime ParseTime(string response, int serverIndex)
    {
        switch (serverIndex)
        {
            case 0:
            {
                var timeData = JsonUtility.FromJson<WorldTimeApiResponse>(response);
                return DateTime.Parse(timeData.utc_datetime);
            }
            case 1:
            {
                var timeData = JsonUtility.FromJson<YandexResponse>(response);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return dateTime.AddMilliseconds(timeData.time);
            }
            case 2:
            {
                var timeData = JsonUtility.FromJson<TimeApiIoResponse>(response);
                return DateTime.Parse(timeData.dateTime);
            }
            default:
                throw new Exception("Unsupported server response format");
        }
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class WorldTimeApiResponse
    {
        public string utc_datetime;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class YandexResponse
    {
        public long time;
    }

    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class TimeApiIoResponse
    {
        public string dateTime;
    }
}