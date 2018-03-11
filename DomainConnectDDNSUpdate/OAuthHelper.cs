﻿using System;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Net;

namespace OAuthHelper
{
    public class OAuthHelper
    {
        const string providerId = "exampleservice.domainconnect.org";

        static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        //
        // GetTokens
        //
        // Given an input of either a response code from an oauth authorization, or a refresh token,
        // will fetch the access_token and a refresh_token using oauth
        //
        static public bool GetTokens(string input)
        {
            int status = 0;

            string domain = ConfigurationManager.AppSettings["domain"];
            string host = ConfigurationManager.AppSettings["host"];
            string dns_provider = ConfigurationManager.AppSettings["dns_provider"];
            string urlAPI = ConfigurationManager.AppSettings["urlAPI"];

            string redirect_url = "http://exampleservice.domainconnect.org/async_oauth_response?domain=" + domain + "&hosts=" + host + "&dns_provider=" + dns_provider;

            string url = urlAPI + "/v2/oauth/access_token?code=" + input + "&grant_type=authorization_code&client_id=" + providerId + "&client_secret=DomainConnectGeheimnisSecretString&redirect_uri=" + WebUtility.UrlEncode(redirect_url);

            string json = RestAPIHelper.RestAPIHelper.POST(url, out status);
            if (status >= 300)
            {
                return false;
            }

            var jss = new JavaScriptSerializer();
            var table = jss.Deserialize<dynamic>(json);

            AddUpdateAppSettings("access_token", table["access_token"]);
            AddUpdateAppSettings("refresh_token", table["refresh_token"]);
            AddUpdateAppSettings("expires_in", table["expires_in"]);
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            AddUpdateAppSettings("iat", unixTimestamp.ToString());

            return true;
        }

    }
}