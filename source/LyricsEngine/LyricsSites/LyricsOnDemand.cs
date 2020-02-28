using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LyricsEngine.LyricsSites
{
  public class LyricsOnDemand : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "LyricsOnDemand";

    // Base url
    private const string SiteBaseUrl = "https://www.lyricsondemand.com";

    #endregion

    #region patterns

    // lyrics mark pattern 
    private const string LyricsMarkPattern = @".*<div class=""lcontent"".*?>(.*?)<\/div";

    #endregion patterns

    public LyricsOnDemand(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      var artist = LyricUtil.RemoveFeatComment(Artist);
      artist = LyricUtil.DeleteSpecificChars(artist);
      artist = artist.Replace(" ", "");
      artist = artist.Replace("The ", "");
      artist = artist.Replace("the ", "");
      artist = artist.Replace("-", "");

      artist = artist.ToLower();

      // Cannot find lyrics containing non-English letters!

      var title = LyricUtil.TrimForParenthesis(Title);
      title = LyricUtil.DeleteSpecificChars(title);
      title = title.Replace(" ", "");
      title = title.Replace("#", "");
      artist = artist.Replace("-", "");

      // Danish letters
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");
      title = title.Replace("�", "");

      title = title.ToLower();

      // Validation
      if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
      {
        return;
      }

      var firstLetter = artist[0].ToString(CultureInfo.InvariantCulture);

      int firstNumber;
      if (int.TryParse(firstLetter, out firstNumber))
      {
        firstLetter = "0";
      }

      var urlString = SiteBaseUrl + "/" + firstLetter + "/" + artist + "lyrics/" + title + "lyrics.html";

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
          Thread.Sleep(100);
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
      return SiteSpeed.Fast;
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
      LyricText = LyricText.Replace("<br>", " ");
      LyricText = LyricText.Replace("</font></p>", " \r\n");
      LyricText = LyricText.Replace("</p>", "");
      LyricText = LyricText.Replace("<p>", "");
      LyricText = LyricText.Replace("<i>", "");
      LyricText = LyricText.Replace("</i>", "");
      LyricText = LyricText.Replace("*", "");
      LyricText = LyricText.Replace("?s", "'s");
      LyricText = LyricText.Replace("?t", "'t");
      LyricText = LyricText.Replace("?m", "'m");
      LyricText = LyricText.Replace("?l", "'l");
      LyricText = LyricText.Replace("?v", "'v");
      LyricText = LyricText.Replace("<p>", " ");
      LyricText = LyricText.Replace("<BR>", " ");
      LyricText = LyricText.Replace("<br />", " ");
      LyricText = LyricText.Replace("&#039;", "'");
      LyricText = LyricText.Replace("&amp;", "&");
      LyricText = LyricText.Replace("%quot;", "\"");
      LyricText = LyricText.Trim();
    }

    #endregion private methods
  }
}