using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LyricsEngine.LyricsSites
{
  public class LyricsNet : AbstractSite
  {
    #region const

    // Name
    private const string SiteName = "LyricsNet";

    // Base url
    private const string SiteBaseUrl = "https://www.lyrics.com";

    private const string SearchPathQuery = "/artist/";

    #endregion

    #region patterns

    //////////////////////////
    // First phase patterns //
    //////////////////////////

    // RegEx to extract lyrics page out of a search on Artist
    private const string FindLyricsPagePatternPrefix = @"<a href=""([^""]*)"">";
    private const string FindLyricsPagePatternSuffix = "</a>";

    ///////////////////////////
    // Second phase patterns //
    ///////////////////////////

    // Lyrics RegEx
    // Lyrics start RegEx
    private const string LyricsSearchPattern = @"<pre id=""lyric-body-text"".*?>(.*?)<\/pre>";

    #endregion

    // step 1 output
    private string _lyricsIndex;


    public LyricsNet(string artist, string title, WaitHandle mEventStopSiteSearches, int timeLimit) : base(artist, title, mEventStopSiteSearches, timeLimit)
    {
    }

    #region interface implemetation

    protected override void FindLyricsWithTimer()
    {
      var artist = FixEscapeCharacters(Artist);

      // 1st step - find lyrics page
      var firstUrlString = BaseUrl + SearchPathQuery + artist;
      var res = GetWebResult(firstUrlString);
      if (res.Item1 == CompletionResult.Completed)
      {
        var findLyricsPagePattern = FindLyricsPagePatternPrefix + Title + FindLyricsPagePatternSuffix;
        var match = Regex.Match(res.Item2, findLyricsPagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
          _lyricsIndex = match.Groups[1].Value;
        }
        else
        {
          _lyricsIndex = null;
        }
      }
      else
      {
        _lyricsIndex = null;
      }

      if (_lyricsIndex == null)
      {
        LyricText = NotFound;
        return;
      }
      // 2nd step - find lyrics
      var secondUrlString = BaseUrl + _lyricsIndex;
      res = GetWebResult(secondUrlString);
      if (res.Item1 == CompletionResult.Completed)
      {
        var match = Regex.Match(res.Item2, LyricsSearchPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
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
      else
      {
        LyricText = NotFound;
      }

    }

    public override string Name => SiteName;

    public override string BaseUrl => SiteBaseUrl;


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
      return SiteComplexity.TwoSteps;
    }

    public override SiteSpeed GetSiteSpeed()
    {
      return SiteSpeed.Slow;
    }

    public override bool SiteActive()
    {
      return true;
    }

    #endregion interface implemetation

    #region private methods

    private void CleanLyrics()
    {
      while (LyricText.IndexOf("<a ") > -1)
      {
        var startIndex = LyricText.IndexOf("<a ");
        var length = LyricText.IndexOf("\">") - startIndex + 2;
        LyricText = LyricText.Remove(startIndex, length);
      }

      LyricText = LyricText.Replace("</a>", "");
      LyricText = LyricText.Replace("<br>", "");
      LyricText = LyricText.Replace("<br/>", "");
      LyricText = LyricText.Replace("&quot;", "\"");

      LyricText = LyricText.Trim();
    }

    private static string FixEscapeCharacters(string text)
    {
      text = text.Replace("(", "");
      text = text.Replace(")", "");
      text = text.Replace("#", "");
      text = text.Replace("/", "");

      text = text.Replace("%", "%25");

      text = text.Replace(" ", "%20");
      text = text.Replace("$", "%24");
      text = text.Replace("&", "%26");
      text = text.Replace("'", "%27");
      text = text.Replace("+", "%2B");
      text = text.Replace(",", "%2C");
      text = text.Replace(":", "%3A");
      text = text.Replace(";", "%3B");
      text = text.Replace("=", "%3D");
      text = text.Replace("?", "%3F");
      text = text.Replace("@", "%40");
      text = text.Replace("&amp;", "&");

      return text;
    }

    #endregion private methods
  }
}
