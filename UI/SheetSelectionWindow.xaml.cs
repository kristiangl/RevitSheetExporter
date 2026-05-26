using Autodesk.Revit.DB;
using RevitSheetExporter.Models;
using System.Collections.ObjectModel;
using System.Windows;
// Disambiguate WPF/WinForms/Revit types
using CheckBox = System.Windows.Controls.CheckBox;
using MessageBox = System.Windows.MessageBox;
using RevitView = Autodesk.Revit.DB.View;

namespace RevitSheetExporter.UI
{
    public partial class SheetSelectionWindow : Window
    {
        private readonly Document _doc;
        private List<SheetItem> _allItems = new();
        private ObservableCollection<SheetItem> _visibleItems = new();

        // Guard against event handlers firing before InitializeComponent finishes
        private bool _ready = false;

        public SheetSelectionWindow(Document doc)
        {
            _doc = doc;
            InitializeComponent();
            _ready = true;
            SheetListView.ItemsSource = _visibleItems;
            LoadItems();
            ApplyFilters();
        }

        // ── Data loading ───────────────────────────────────────────────────────

        private void LoadItems()
        {
            _allItems.Clear();

            if (RadioSheets?.IsChecked == true)
                LoadSheets();
            else
                LoadViews();
        }

        private void LoadSheets()
        {
            var sheets = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(s => !s.IsPlaceholder)
                .OrderBy(s => s.SheetNumber)
                .ToList();

            foreach (var sheet in sheets)
            {
                var inSheetList = sheet.LookupParameter("Appears In Sheet List")?.AsInteger() == 1;

                _allItems.Add(new SheetItem
                {
                    SheetNumber = sheet.SheetNumber,
                    SheetName = sheet.Name,
                    IsInSheetList = inSheetList,
                    ElementId = sheet.Id,
                    IsSelected = false
                });
            }
        }

        private void LoadViews()
        {
            var views = new FilteredElementCollector(_doc)
                .OfClass(typeof(RevitView))
                .Cast<RevitView>()
                .Where(v => !v.IsTemplate && v.CanBePrinted)
                .OrderBy(v => v.Name)
                .ToList();

            foreach (var view in views)
            {
                _allItems.Add(new SheetItem
                {
                    SheetNumber = view.ViewType.ToString(),
                    SheetName = view.Name,
                    IsInSheetList = true,
                    ElementId = view.Id,
                    IsSelected = false
                });
            }
        }

        // ── Filtering ──────────────────────────────────────────────────────────

        private void ApplyFilters()
        {
            if (!_ready) return;

            var filterBySheetList = ChkShowInSheetList?.IsChecked == true
                                    && RadioSheets?.IsChecked == true;
            var search = TxtSearch?.Text.Trim().ToLowerInvariant() ?? string.Empty;

            var filtered = _allItems.AsEnumerable();

            if (filterBySheetList)
                filtered = filtered.Where(i => i.IsInSheetList);

            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(i =>
                    i.SheetNumber.ToLowerInvariant().Contains(search) ||
                    i.SheetName.ToLowerInvariant().Contains(search));

            _visibleItems.Clear();
            foreach (var item in filtered)
                _visibleItems.Add(item);

            UpdateSelectionCount();
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void ModeChanged(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            if (ChkShowInSheetList != null)
                ChkShowInSheetList.IsEnabled = RadioSheets?.IsChecked == true;
            LoadItems();
            ApplyFilters();
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            ApplyFilters();
        }

        private void SearchChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!_ready) return;
            ApplyFilters();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in _visibleItems) item.IsSelected = true;
            UpdateSelectionCount();
        }

        private void DeselectAll(object sender, RoutedEventArgs e)
        {
            foreach (var item in _visibleItems) item.IsSelected = false;
            UpdateSelectionCount();
        }

        private void ItemCheckChanged(object sender, RoutedEventArgs e) => UpdateSelectionCount();

        private void HeaderCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var chk = (CheckBox)sender;
            if (chk.IsChecked == true) SelectAll(sender, e);
            else DeselectAll(sender, e);
        }

        private void UpdateSelectionCount()
        {
            if (!_ready) return;
            var count = _allItems.Count(i => i.IsSelected);
            TxtSelectionCount.Text = $"{count} sheet{(count == 1 ? "" : "s")} selected";
            BtnExport.IsEnabled = count > 0;
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            var selectedIds = _allItems.Where(i => i.IsSelected).Select(i => i.ElementId).ToList();

            var sheets = selectedIds
                .Select(id => _doc.GetElement(id) as ViewSheet)
                .Where(s => s != null)
                .Cast<ViewSheet>()
                .ToList();

            if (sheets.Count == 0)
            {
                MessageBox.Show("No sheets selected.", "Sheet Exporter",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var settingsWindow = new ExportSettingsWindow(_doc, sheets);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void CancelClick(object sender, RoutedEventArgs e) => Close();
    }
}
