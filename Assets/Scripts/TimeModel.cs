using System;

public class TimeModel : ITimeModel
{
    private double _recheckSeconds;

    public float Seconds { get; private set; }
    public bool TimeToCheck => _recheckSeconds <= Seconds;

    public void SetTime(DateTime dateTime)
    {
        Seconds = (dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second + dateTime.Millisecond / 1000f;
        SetTime(Seconds);
    }

    public void SetTime(float seconds)
    {
        Seconds = seconds;
        _recheckSeconds = Seconds + 3600;
    }

    public void ShiftTime(float seconds)
    {
        Seconds += seconds;
    }
}