using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Duathlon
{
    class Printer
    {
        private readonly float _Margin = 4;

        private Font _PrintFont = new Font("Arial", 10);
        private int _NextIndexToPrint = 0;
        private StarterWrapper _Starters;
        private PrintColumn<Starter>[] _Columns;
        private string _DocTitle;
        private Starter[] _TempStarters;
        private int _CurrentPage;

        public Printer(ref StarterWrapper starters)
        {
            _Starters = starters;
        }

        // Print the file.
        public void Print(PrintJob<Starter> printJob)
        {
            _CurrentPage = 0;
            _DocTitle = printJob.DocTitle;
            _TempStarters = printJob.CollectionFunction(_Starters.Starters);
            _Columns = printJob.Columns;

            _NextIndexToPrint = -1;

            PrintDocument doc = new PrintDocument();
            doc.PrintPage += PrintPage;

            PrintDialog dialog = new PrintDialog();
            dialog.AllowCurrentPage = false;
            dialog.AllowSelection = false;
            dialog.AllowSomePages = false;
            dialog.ShowHelp = false;
            dialog.PrinterSettings.DefaultPageSettings.Landscape = true;
            dialog.Document = doc;

            if (dialog.ShowDialog() == DialogResult.OK)
                doc.Print();
        }

        // Print multiple files.
        public void MultiPrint(params PrintJob<Starter>[] printJobs)
        {
            PrintDocument doc = new PrintDocument();
            doc.PrintPage += PrintPage;

            PrintDialog dialog = new PrintDialog();
            dialog.AllowCurrentPage = false;
            dialog.AllowSelection = false;
            dialog.AllowSomePages = false;
            dialog.ShowHelp = false;
            dialog.PrinterSettings.DefaultPageSettings.Landscape = true;
            dialog.Document = doc;

            if (dialog.ShowDialog() == DialogResult.OK)
                foreach (PrintJob<Starter> printJob in printJobs)
                {
                    _CurrentPage = 0;
                    _DocTitle = printJob.DocTitle;
                    _TempStarters = printJob.CollectionFunction(_Starters.Starters);
                    _Columns = printJob.Columns;

                    _NextIndexToPrint = -1;

                    doc.Print();
                }
        }

        // The PrintPage event is raised for each page to be printed.
        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            _CurrentPage += 1;
            float lineHeight = _Margin * 2 + _PrintFont.GetHeight(e.Graphics);
            float linesPerPage = 0;
            int count = 0;

            //calculate the width of all columns
            foreach (PrintColumn<Starter> column in _Columns)
                column.Width = column.MaxWidth(_TempStarters, _PrintFont, e);
            while (_Columns.Select(c => c.Width).Sum() > e.MarginBounds.Width)
            {
                var widestColumns = _Columns.OrderByDescending(c => c.Width).Take(3);
                foreach (var column in widestColumns)
                    column.Width *= 0.95f;
            }

            Pen pen = new Pen(Color.Black, 1);

            // Calculate the number of lines per page.
            linesPerPage = e.MarginBounds.Height / lineHeight - 1;

            float contentWidth = _Columns.Select(c => c.Width).Sum();
            float yPos = e.MarginBounds.Y;

            //print document title
            e.Graphics.DrawString(_DocTitle, _PrintFont, Brushes.Black, e.MarginBounds.X, yPos - lineHeight - _Margin);

            //print page count
            e.Graphics.DrawString($"Seite {_CurrentPage}", _PrintFont, Brushes.Black, e.MarginBounds.Right - 20, e.MarginBounds.Bottom + _Margin);

            //top horizontal line
            e.Graphics.DrawLine(pen, new PointF(e.MarginBounds.X, yPos), new PointF(e.MarginBounds.X + contentWidth, yPos));

            //print rows
            while (count < linesPerPage - 1 && _NextIndexToPrint < _TempStarters.Length - 1)
            {
                if (count > 0)
                {
                    _NextIndexToPrint += 1;

                    //ignore entry when it is flagged as empty
                    if (false == _TempStarters[_NextIndexToPrint].HasValue) continue;
                }

                float xPos = e.MarginBounds.X;

                //first vertical line
                e.Graphics.DrawLine(pen, new PointF(xPos, yPos), new PointF(xPos, yPos + lineHeight));

                //print columns
                foreach (var column in _Columns)
                {
                    string content;
                    //title for first row
                    if (count == 0) content = column.Title;
                    //content for rows after the first row
                    else content = column.GetCellString(_TempStarters[_NextIndexToPrint]);

                    //string that fits into the width of the column
                    string cellContent = MakeStringFit(content, column.Width, e);
                    float offSet = 0;
                    //exclude title row from aligning right
                    if (column.DoAlignRight && count > 0)
                        offSet = column.Width - e.Graphics.MeasureString(cellContent, _PrintFont).Width;

                    e.Graphics.DrawString(cellContent, _PrintFont, Brushes.Black, new PointF(xPos + offSet, yPos + _Margin));

                    xPos += column.Width;

                    //vertical line
                    e.Graphics.DrawLine(pen, new PointF(xPos, yPos), new PointF(xPos, yPos + lineHeight));
                }

                yPos += lineHeight;

                //bottom line
                e.Graphics.DrawLine(pen, new PointF(e.MarginBounds.X, yPos), new PointF(xPos, yPos));

                count += 1;
            }
            // If more lines exist, print another page.
            e.HasMorePages = _NextIndexToPrint < _TempStarters.Length - 1;
        }

        private string MakeStringFit(string s, float width, PrintPageEventArgs e)
        {
            while (e.Graphics.MeasureString(s, _PrintFont).Width > width)
                s = s.Substring(0, s.Length - 1);
            return s;
        }
    }

    public class PrintColumn<T>
    {
        public string Title { get; set; }
        public Func<T, string> GetCellString { get; set; }
        public float Width { get; set; }
        public bool DoAlignRight { get; set; }

        public PrintColumn(string title, Func<T, string> getString, bool doAlignRight = false)
        {
            Title = title;
            GetCellString = getString;
            DoAlignRight = doAlignRight;
        }

        public float MaxWidth(IEnumerable<T> collection, Font font, PrintPageEventArgs e)
        {
            //return collection.Select(t => e.Graphics.MeasureString(GetCellString(t), font).Width).Max();
            return collection.Select(t => GetCellString(t)).Concat(new[] { Title }).Select(s => e.Graphics.MeasureString(s, font).Width).Max();
        }
    }

    public class PrintJob<T>
    {
        public Func<T[], T[]> CollectionFunction { get; set; }
        public string DocTitle { get; set; }
        public PrintColumn<T>[] Columns { get; set; }

        public PrintJob(params PrintColumn<T>[] columns)
            :this(s => s, columns) { }

        public PrintJob(string docTitle, params PrintColumn<T>[] columns)
            :this(s => s, docTitle, columns) { }

        public PrintJob(Func<T[], T[]> starterSelection, params PrintColumn<T>[] columns)
            :this(starterSelection, null, columns) { }

        public PrintJob(Func<T[], T[]> collectionFunction, string docTitle, params PrintColumn<T>[] columns)
        {
            CollectionFunction = collectionFunction;
            DocTitle = docTitle;
            Columns = columns;
        }
    }
}