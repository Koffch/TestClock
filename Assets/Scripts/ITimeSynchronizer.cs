using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITimeSynchronizer
{
    event Action<string> Finish;
    List<string> Servers { get; }
    Task<DateTime> GetTime(int serverIndex);
}