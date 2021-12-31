//Program adapted from base code sourced from https://dev.to/rachelsoderberg/create-a-simple-web-scraper-in-c-1l1m, Author: Rachel Soderberg, Jul 29, 2019.
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;

namespace Scraper
{
    public partial class ScienceScraper : Form
    {
        public ScienceScraper()
        {
            InitializeComponent();
            ScrapeWebsite();
            
        }

        private string Title { get; set; }
        private string Url { get; set; }
        private string siteUrl = "https://www.science.org/journal/scirobotics";
        public string[] QueryTerms { get; } = { "sound", "abstract" };


        internal async void ScrapeWebsite()
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(siteUrl);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);
            GetScrapeResults(document);
        }

        private void GetScrapeResults(IHtmlDocument document)
        {
            IEnumerable<IElement> articleLink = null;

            foreach (var term in QueryTerms)
            {
                //Scans document contents for matching class and compares data to the specified terms.
                articleLink = document.All.Where(x => x.ClassName == "card__title" && (x.ParentElement.InnerHtml.Contains(term) || x.ParentElement.InnerHtml.Contains(term.ToLower())));
               
                if (articleLink.Any())
                {
                    PrintResults(articleLink);
                }
            }
            
        }

        public void PrintResults(IEnumerable<IElement> articleLink) 
        {
            foreach (var elem in articleLink)
            {
                CleanUpResults(elem); 
                
                //Appends elem object to results pane.
                richTextBox1.AppendText($"{Title} - {Url}{Environment.NewLine}");
            }
            
        }

        //Builds a string with html tags removed and a delimiter to separate data.
        private void CleanUpResults(IElement result)
        {
            
            string htmlResult = result.InnerHtml.ReplaceFirst("<a href=\"", "https://www.science.org");
            htmlResult = htmlResult.ReplaceFirst("\" title=\"", " ");
            htmlResult = htmlResult.ReplaceFirst("\" class=\"", "-");
            htmlResult = htmlResult.ReplaceFirst("text-reset animation-underline\">", "*");
            htmlResult = htmlResult.ReplaceFirst("</a>", "");

            SplitResults(htmlResult);
        }

        //Splits results on delimiter to separate url and title.
        private void SplitResults(string htmlResult)
        {
            string[] splitResults = htmlResult.Split('*');
            Url = splitResults[0];
            Title = splitResults[1];
        }


    }
}
