using System;

public interface ITimeModel
{
    public float Seconds { get; }
    public bool TimeToCheck{ get; }

    public void SetTime(DateTime dateTime);
    public void SetTime(float seconds);
    public void ShiftTime(float seconds);
}