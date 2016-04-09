using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Configuration;
using Helper.IOHelper;
using Helper.RegexHelper;

namespace PlayParser
{
    public class PlayParser
    {
        protected string id;
        protected string hostLanguage;
        protected string page;
        protected string appName;
        protected string appIconURI;
        protected stringWithURLs appDescription;
        protected string appUpdateDescription;
        protected List<string> appScreenShotURIs;

        protected string proxyURI = "";
        protected int proxyPort = 80;
        protected string storeURL = "https://play.google.com/store/apps/details?id={0}&hl={1}";
        protected string regexMatch = "match";
        protected string regexAppName = "<div class=\"id-app-title\" tabindex=\"0\">(?<match>[a-zA-Z0-9\\./% ]*)</div>";
        protected string regexAppIconURI = "<img class=\"cover-image\" src=\"(?<match>[^\"]*)\"";
        protected string regexAppUpdateDescription = "(?<=<div class=\"recent-change\">)[^<]+(?=</div>)";
        protected string regexAppScreenShotURIs = "<img class=\"full-screenshot[a-z ]*\" data-expand-fit-to=\"container\" data-expand-target=\"full-screenshot-[0-9]+\" data-expand-to=\"[a-z0-9 \\-]+\" src=\"(?<match>[^\"]*)\"";
        protected string regexAppDescription = "<div class=\"show-more-content text-body\" itemprop=\"description\"[a-z:; \"=\\u0025]*> <div jsname=\"[A-Za-z0-9]+\">(?<match>.+?(?=</div>))";

        public PlayParser(string PlayStoreID, string Language = "en")
        {
            this.id = PlayStoreID;
            this.hostLanguage = Language;
            getSetConfig();
            retrievePage();
            getAppName();
            getAppIconURI();
            getAppScreenShotURIs();
            getAppDescription();
            getAppUpdateDescription();
        }

        protected void retrievePage()
        {
            this.page = WebHelper.retrievePageStatic(string.Format(storeURL, this.id, this.hostLanguage), this.proxyURI, this.proxyPort);
        }

        protected void getSetConfig()
        {
            string tempProxyPort = this.proxyPort.ToString();
            setIfNotNull(ref tempProxyPort, ConfigurationManager.AppSettings["proxyPort"]);
            int.TryParse(tempProxyPort, out this.proxyPort);
            setIfNotNull(ref this.storeURL, ConfigurationManager.AppSettings["storeURL"]);
            setIfNotNull(ref this.proxyURI, ConfigurationManager.AppSettings["proxyURI"]);
            setIfNotNull(ref this.regexAppName, ConfigurationManager.AppSettings["regexAppName"]);
            setIfNotNull(ref this.regexAppIconURI, ConfigurationManager.AppSettings["regexAppIconURI"]);
            setIfNotNull(ref this.regexAppDescription, ConfigurationManager.AppSettings["regexAppDescription"]);
            setIfNotNull(ref this.regexAppUpdateDescription, ConfigurationManager.AppSettings["regexAppUpdateDescription"]);
            setIfNotNull(ref this.regexAppScreenShotURIs, ConfigurationManager.AppSettings["regexAppScreenShotURIs"]);
        }

        protected void setIfNotNull<T>(ref T variable, T value)
        {
            if(value != null)
            {
                variable = value;
            }
        }

        protected string getAppItem(string regex)
        {
            List<string> matches = getAppItems(regex);
            if (matches.Count > 0)
            {
                return matches.First();
            }
            throw new EntityNotFoundException();
        }

        protected List<string> getAppItems(string regex)
        {
            List<string> matches;
            if (regex.Contains(this.regexMatch))
            {
                matches = RegexHelper.getAllMatches(this.page, regex, this.regexMatch);
            }
            else
            {
                matches = RegexHelper.getAllMatches(this.page, regex);
            }
            if (matches.Count > 0)
            {
                return matches;
            }
            throw new EntityNotFoundException();

        }

        protected void getAppName()
        {
            this.appName = getAppItem(this.regexAppName);
        }

        protected void getAppDescription()
        {
            this.appDescription = new stringWithURLs(getAppItem(regexAppDescription), new string[] { });
        }

        protected void getAppUpdateDescription()
        {
            this.appUpdateDescription = getAppItem(regexAppUpdateDescription);
        }

        protected void getAppIconURI()
        {
            this.appIconURI = getAppItem(this.regexAppIconURI);
        }

        protected void getAppScreenShotURIs()
        {
            this.appScreenShotURIs = getAppItems(regexAppScreenShotURIs);
        }
    }

    public class stringWithURLs
    {
        private string formattedText;
        private List<Uri> uris;
        private string uriStringFormat = "[a href=\"{0}\"]{1}[/a]";

        public stringWithURLs(string formatString, string[] pURLs)
        {
            this.formattedText = formatString;
            uris = null;
            if(pURLs.Length > 0)
            {
                uris = new List<Uri>();
                foreach(string url in pURLs)
                {
                    uris.Add(new Uri(url));
                }
            }
        }

        public stringWithURLs(string formatString, Uri[] pURLs)
        {
            this.formattedText = formatString;
            this.uris = pURLs.ToList();
        }

        public string toString()
        {
            return string.Format(this.formattedText, this.uris);
        }
    }

    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base("No match was found.")
        {
        }
        public EntityNotFoundException(string message) : base(message)
        {
        }
        public EntityNotFoundException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
