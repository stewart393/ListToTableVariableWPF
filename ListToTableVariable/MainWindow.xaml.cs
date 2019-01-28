using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;


namespace ListToTableVariable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            KeyDown += new KeyEventHandler(GrdClipboardItems_KeyDown);
        }
        public  void PositionItemsToGrid(string [][] clipboardData)
        {
            // the index of the first DataGridRow          
            int startRow = grdClipboardItems.ItemContainerGenerator.IndexFromContainer(
               (DataGridRow)grdClipboardItems.ItemContainerGenerator.ContainerFromItem
               (grdClipboardItems.SelectedCells[0].Item));
            int targetRowCount = grdClipboardItems.SelectedCells.Count;

            // the destination rows 
            //  (from startRow to either end or length of clipboard rows)
            DataGridRow[] rows =
                Enumerable.Range(
                    startRow, Math.Min(grdClipboardItems.Items.Count, targetRowCount))
                .Select(rowIndex =>
                    grdClipboardItems.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow)
                .Where(a => a != null).ToArray();

            // the destination columns 
            //  (from selected row to either end or max. length of clipboard colums)
            DataGridColumn[] columns =
                grdClipboardItems.Columns.OrderBy(column => column.DisplayIndex)
                .SkipWhile(column => column != grdClipboardItems.CurrentCell.Column)
                .Take(clipboardData.Max(row => row.Length)).ToArray();

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string[] rowContent = clipboardData[rowIndex % clipboardData.Length];
                for (int colIndex = 0; colIndex < columns.Length; colIndex++)
                {
                    string cellContent =
                        colIndex >= rowContent.Length ? "" : rowContent[colIndex];
                    columns[colIndex].OnPastingCellClipboardContent(
                        rows[rowIndex].Item, cellContent);
                }
            }
        }
        public string[][] ClipBoardItemsToArray()
        {
            // 2-dim array containing clipboard data
            string[][] clipboardData =
                ((string)Clipboard.GetData(DataFormats.Text)).Split('\n')
                .Select(row =>
                    row.Split('\t')
                    .Select(cell =>
                        cell.Length > 0 && cell[cell.Length - 1] == '\r' ?
                        cell.Substring(0, cell.Length - 1) : cell).ToArray())
                .Where(a => a.Any(b => b.Length > 0)).ToArray();
            return clipboardData;
        }
        public DataTable StringArrayToDataTable(string[][] clipboardData, bool hasHeaders = true)
        {
            DataTable dt = new DataTable();
            int numRows = clipboardData.GetLength(0);
            int numCols = clipboardData[0].GetLength(0);

            if (hasHeaders == true)
            {
                // get headers from first row
                for (int col = 0; col < numCols; col++)
                {
                    dt.Columns.Add(clipboardData[0][col], typeof(string));
                }

                for (int row = 1; row < numRows-1; row++)
                {
                    DataRow dr = dt.NewRow();
                    for (int col = 0; col < clipboardData[row].GetLength(0); col++)
                    {
                        dr[col] = clipboardData[row][col];                      
                    }
                    dt.Rows.Add(dr);
                }                
            }
            if (hasHeaders == false)
            {
                // get headers from first row
                for (int col = 0; col < clipboardData[0].GetLength(0); col++)
                {
                    dt.Columns.Add("Column" + col.ToString(), typeof(string));
                }

                for (int row = 0; row < clipboardData.GetLength(0); row++)
                {
                    DataRow dr = dt.NewRow();
                    for (int col = 0; col < clipboardData[0].GetLength(0); col++)
                    {
                        dr[col] = clipboardData[row][col];
                    }
                    dt.Rows.Add(dr);
                }
               
            }
            return dt;

        }
        public void CreateSqlScriptFromDataTable()
        {
            
        }
        public void BulkCopyDataTable()
        {
            
        }

        private void GrdClipboardItems_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V &&
                     (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {

                // 2-dim array containing clipboard data
                string[][] clipboardData = ClipBoardItemsToArray();
                DataTable dt = StringArrayToDataTable(clipboardData);
                dt.TableName = "Row";
                grdClipboardItems.ItemsSource = dt.DefaultView;


                MemoryStream txtmem = new MemoryStream();
                dt.WriteXml(txtmem);
                rtfScript.AppendText(Encoding.UTF8.GetString(txtmem.ToArray()));
                System.Data.SqlTypes.SqlXml sqlXml = new System.Data.SqlTypes.SqlXml(txtmem);

            }
        }
    }
}
