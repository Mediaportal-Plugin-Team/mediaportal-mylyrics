﻿using System;
using System.Net;

namespace LyricsEngine
{
    /// <summary>
    /// Some sites need to send cookies and a valid User Agent, so we use this class to enable a cookie container.
    /// </summary>
    internal class LyricsWebClient : WebClient
    {
        private readonly string _referer;


        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            if (response != null)
            {
                ResponseUri = response.ResponseUri;
            }
            return response;
        }

        public LyricsWebClient()
        {
            Timeout = -1;
            UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";
            CookieContainer = new CookieContainer();
        }

        public LyricsWebClient(string referer)
        {
            Timeout = -1;
            _referer = referer;
            CookieContainer = new CookieContainer();
        }

        public CookieContainer CookieContainer { get; set; }

        public string UserAgent { get; set; }

        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | (SecurityProtocolType)3072 // TLS12
                                                   | SecurityProtocolType.Ssl3;

            // Skip validation of SSL/TLS certificate
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            var request = base.GetWebRequest(address);

            if (request != null && request.GetType() == typeof (HttpWebRequest))
            {
                ((HttpWebRequest) request).CookieContainer = CookieContainer;
                ((HttpWebRequest) request).UserAgent = UserAgent;
                if (_referer != null)
                {
                    ((HttpWebRequest)request).Referer = _referer;
                }

                (request).Timeout = Timeout;
                (request).Proxy.Credentials = CredentialCache.DefaultCredentials;
            }

            return request;
        }

        public Uri ResponseUri { get; private set; }
    }
}