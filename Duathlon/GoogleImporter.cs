using Google.GData.Client;
using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Duathlon
{
    static class GoogleImporter
    {
        private static TextBlock _LinkWrapper;
        private static Hyperlink _Link;
        private static TextBox _Input;
        private static Button _Submit;
        private static ComboBox _SpreadsheetSelect, _WorksheetSelect;

        private static OAuth2Parameters _Parameters;
        private static SpreadsheetsService _Service;
        private static SpreadsheetFeed _SpreadsheetFeed;
        private static WorksheetFeed _WorksheetFeed;

        /*UIElementCollection grid.Children
        **          [0] TextBlock           linkwrapper
        **              [0] Hyperlink       link
        **          [1] StackPanel
        **              [0] Stackpanel
        **                  [0] TextBlock
        **                  [1] TextBox     code
        **              [1] Button          submit
        **          [2] Stackpanel
        **              [0] TextBlock
        **              [1] ComboBox        spreadsheet
        **          [3] Stackpanel
        **              [0] TextBlock
        **              [1] ComboBox        worksheet
        **          [4] Button              import
        */
        public static void SetGoogleStuff(Grid grid, Hyperlink link)
        {
            _LinkWrapper = grid.Children[0] as TextBlock;
            _Link = link;
            _Input = ((grid.Children[1] as StackPanel).Children[0] as StackPanel).Children[1] as TextBox;
            _Submit = (grid.Children[1] as StackPanel).Children[1] as Button;
            _SpreadsheetSelect = (grid.Children[2] as StackPanel).Children[1] as ComboBox;
            _WorksheetSelect = (grid.Children[3] as StackPanel).Children[1] as ComboBox;

            _Submit.Click += SubmitCodeAndGetWorkbooks;
            _Link.Click += Url_Click;
            _SpreadsheetSelect.SelectionChanged += SpreadsheetSelected;

            _Submit.IsEnabled = false;

            ProvideLink();
        }

        private static void ProvideLink()
        {
            string clientID = Statics.GoogleAuthCredentials.ClientID;
            string clientSecret = Statics.GoogleAuthCredentials.ClientSecret;
            string scope = "https://spreadsheets.google.com/feeds";
            string redirectUri = "urn:ietf:wg:oauth:2.0:oob";

            _Parameters = new OAuth2Parameters();
            _Parameters.ClientId = clientID;
            _Parameters.ClientSecret = clientSecret;
            _Parameters.RedirectUri = redirectUri;
            _Parameters.Scope = scope;

            _Link.NavigateUri = new Uri(OAuthUtil.CreateOAuth2AuthorizationUrl(_Parameters));
            _LinkWrapper.Visibility = Visibility.Visible;
        }

        private static void Url_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(_Link.NavigateUri.ToString());
            _Submit.IsEnabled = true;
        }

        private static void SubmitCodeAndGetWorkbooks(object sender, RoutedEventArgs e)
        {
            string accessCode = _Input.Text.Trim();
            if (String.IsNullOrWhiteSpace(accessCode))
            {
                MessageBox.Show("No code entered");
                return;
            }
            _Parameters.AccessCode = accessCode;

            _Submit.IsEnabled = false;
            _LinkWrapper.Visibility = Visibility.Hidden;

            OAuthUtil.GetAccessToken(_Parameters);
            GOAuth2RequestFactory requestFactory = new GOAuth2RequestFactory(null, "Duathlon Data Grabber", _Parameters);

            _Service = new SpreadsheetsService("Duathlon Data Grabber");
            _Service.RequestFactory = requestFactory;
            SpreadsheetQuery query = new SpreadsheetQuery();
            _SpreadsheetFeed = _Service.Query(query);
            _SpreadsheetSelect.ItemsSource = _SpreadsheetFeed.Entries.Select(ae => ae.Title.Text);
        }

        private static void SpreadsheetSelected(object sender, SelectionChangedEventArgs e)
        {
            int index = _SpreadsheetSelect.SelectedIndex;
            SpreadsheetEntry spreadsheet = (SpreadsheetEntry)_SpreadsheetFeed.Entries[index];
            _WorksheetFeed = spreadsheet.Worksheets;
            _WorksheetSelect.ItemsSource = _WorksheetFeed.Entries.Select(ae => ae.Title.Text);
        }

        public static string[,] GrabEntries()
        {
            int index = _WorksheetSelect.SelectedIndex;
            WorksheetEntry worksheet = (WorksheetEntry)_WorksheetFeed.Entries[index];
            AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
            CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
            CellFeed cellFeed = _Service.Query(cellQuery);

            string[,] result = new string[cellFeed.RowCount.IntegerValue, cellFeed.ColCount.IntegerValue];
            foreach (var cellEntry in cellFeed.Entries)
            {
                CellEntry.CellElement cell = ((CellEntry)cellEntry).Cell;
                result[(int)cell.Row - 1, (int)cell.Column - 1] = cell.InputValue;
            }

            return result;
        }
    }
}
