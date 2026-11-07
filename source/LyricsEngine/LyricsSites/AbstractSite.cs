using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using System.IO;
using System.Text;

namespace LyricsEngine.LyricsSites
{
  public abstract class AbstractSite : ILyricSite
  {
    #region const

    // Not found
    public const string NotFound = "Not found";

    #endregion const

    #region members

    // Artist
    protected readonly string Artist;
    // Title
    protected readonly string Title;
    // Stop event
    protected WaitHandle MEventStopSiteSearches;
    // Time Limit
    protected readonly int TimeLimit;

    // Lyrics
    protected string LyricText = "";

    // Complete
    private Timer _searchTimer;
    private ManualResetEvent timeoutEvent;

    #endregion members


    protected AbstractSite(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit)
    {
      // Artist
      Artist = artist;
      // Title
      Title = title;
      // Stop search event
      MEventStopSiteSearches = mEventStopSiteSearches;
      timeoutEvent = new ManualResetEvent(false);
      // Time Limit
      TimeLimit = timeLimit;
    }


    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
      _searchTimer.Stop();
      _searchTimer.Close();
      _searchTimer.Dispose();
      timeoutEvent.Dispose();

      LyricText = NotFound;
      Thread.CurrentThread.Abort();
    }

    protected abstract void FindLyricsWithTimer();


    #region interface abstract methods

    public string Lyric
    {
      get { return LyricText; }
    }

    public abstract string Name { get; }
    public abstract string BaseUrl { get; }
    public void FindLyrics()
    {
      try
      {
        // timer
        _searchTimer = new Timer { Enabled = false, Interval = TimeLimit };
        _searchTimer.Elapsed += TimerElapsed;
        _searchTimer.Start();

        // Find Lyrics
        FindLyricsWithTimer();
      }
      finally
      {
        if (_searchTimer != null)
        {
          _searchTimer.Stop();
          _searchTimer.Close();
        }
      }
    }

    public enum CompletionResult { Completed, ExternallyStoppped, TimeOut };
    public Tuple<CompletionResult, string> GetWebResult(string urlString)
    {
      string result2 = "";
      using (var client = new LyricsWebClient())
      using (var completedEvent = new ManualResetEvent(false))
      {
        var uri = new Uri(urlString);
        client.OpenReadCompleted += (sender, e) =>
        {
          try
          {
            if (e.Cancelled)
              return;
            using (var reader = new StreamReader(e.Result, Encoding.UTF8))
            {
              result2 = reader.ReadToEnd();
            }
          }
          catch (Exception)
          {
            // no logger yet...
          }
          finally
          {
            try
            {
              completedEvent.Set();
            }
            catch (ObjectDisposedException)
            {
              // The calling method has already timed out or stopped.
            }
          }
        };
        client.OpenReadAsync(uri);

        var result = WaitHandle.WaitAny(new WaitHandle[]
        {
                  completedEvent,
                  MEventStopSiteSearches,
                  timeoutEvent
        });
        if (result == 1 || result == 2)
        {
          client.CancelAsync();
          return Tuple.Create(result == 1 ? CompletionResult.ExternallyStoppped : CompletionResult.TimeOut, "");
        }
        return Tuple.Create(CompletionResult.Completed, result2);
      }
    }

    public abstract LyricType GetLyricType();
    public abstract SiteType GetSiteType();
    public abstract SiteComplexity GetSiteComplexity();
    public abstract SiteSpeed GetSiteSpeed();
    public abstract bool SiteActive();

    #endregion interface abstract methods
  }
}
