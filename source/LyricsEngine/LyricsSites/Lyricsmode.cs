using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LyricsEngine.LyricsSites
{
public class Lyricsmode : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "Lyricsmode";

    // Base url
    private const string SiteBaseUrl = "https://www.lyricsmode.com";

    #endregion

    #region patterns

    // lyrics mark pattern 
    private const string LyricsMarkPattern = @".*<div id=""lyrics_text"" .*?"">(.*?)<div";

    #endregion patterns

    public Lyricsmode(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      var artist = Artist.ToLower();
      artist = ClearName(artist);

      var title = Title.ToLower();
      title = ClearName(title);

      // Validation
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
      {
        return;
      }

      var firstLetter = artist[0].ToString(CultureInfo.InvariantCulture);

      var urlString = SiteBaseUrl + "/lyrics/" + firstLetter + "/" + artist + "/" + title + ".html";

      var client = new LyricsWebClient();

      var uri = new Uri(urlString);
      client.OpenReadCompleted += CallbackMethod;
      client.OpenReadAsync(uri);

      while (Complete == false)
      {
        if (MEventStopSiteSearches.WaitOne(1, true))
        {
          Complete = true;
        }
        else
        {
          Thread.Sleep(300);
        }
      }
    }


    public override LyricType GetLyricType()
    {
      return LyricType.UnsyncedLyrics;
    }

    public override SiteType GetSiteType()
    {
      return SiteType.Scrapper;
    }

    public override SiteComplexity GetSiteComplexity()
    {
      return SiteComplexity.OneStep;
    }

    public override SiteSpeed GetSiteSpeed()
    {
      return SiteSpeed.Medium;
    }

    public override bool SiteActive()
    {
      return true;
    }

    public override string Name => SiteName;

    public override string BaseUrl => SiteBaseUrl;

    #endregion interface implemetation

    #region private methods

    private void CallbackMethod(object sender, OpenReadCompletedEventArgs e)
    {
      Stream reply = null;
      StreamReader reader = null;

      try
      {
        reply = e.Result;
        reader = new StreamReader(reply, Encoding.UTF8);

        var line = reader.ReadToEnd();
        var match = Regex.Match(line, LyricsMarkPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
          LyricText = match.Groups[1].Value;
        }

        if (LyricText.Length > 0)
        {
          CleanLyrics();
        }
        else
        {
          LyricText = NotFound;
        }
      }
      catch
      {
        LyricText = NotFound;
      }
      finally
      {
        reader?.Close();
        reply?.Close();
        Complete = true;
      }
    }

    // Cleans the lyrics
    private void CleanLyrics()
    {
      LyricText = LyricText.Replace("&quot;", "\"");
      LyricText = LyricText.Replace("<br>", " ");
      LyricText = LyricText.Replace("<br />", " ");
      LyricText = LyricText.Replace("<BR>", " ");
      LyricText = LyricText.Replace("&amp;", "&");
      LyricText = LyricText.Trim();
    }


    private static string ClearName(string name)
    {
      // Spaces and special characters
      name = name.Replace(" ", "_");
      name = name.Replace("#", "_");
      name = name.Replace("%", "_");
      name = name.Replace("'", "");
      name = name.Replace("(", "%28");
      name = name.Replace(")", "%29");
      name = name.Replace("+", "%2B");
      name = name.Replace(",", "");
      name = name.Replace(".", "_");
      name = name.Replace(":", "_");
      name = name.Replace("=", "%3D");
      name = name.Replace("?", "_");

      // German letters
      name = name.Replace("�", "%FC");
      name = name.Replace("�", "%DC");
      name = name.Replace("�", "%E4");
      name = name.Replace("�", "%C4");
      name = name.Replace("�", "%F6");
      name = name.Replace("�", "%D6");
      name = name.Replace("�", "%DF");

      // Danish letters
      name = name.Replace("�", "%E5");
      name = name.Replace("�", "%C5");
      name = name.Replace("�", "%E6");
      name = name.Replace("�", "%F8");

      // French letters
      name = name.Replace("�", "%E9");


      return name;
    }

    #endregion private methods
  }
}