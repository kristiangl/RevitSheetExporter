using Autodesk.Revit.DB;
using RevitSheetExporter.Models;
using RevitSheetExporter.Services;
using System.IO;
using System.Windows;
using System.Windows.Controls;
// Disambiguate WPF/WinForms/Revit types (all three present due to UseWindowsForms)
using ComboBox = System.Windows.Controls.ComboBox;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace RevitSheetExporter.UI
{
    public partial class ExportSettingsWindow : Window
    {
        private readonly Document _doc;
        private readonly List<ViewSheet> _sheets;
        private readonly SettingsService _settingsService = new();
        private ExportSettings _settings;

        public ExportSettingsWindow(Document doc, List<ViewSheet> sheets)
        {
            _doc = doc;
            _sheets = sheets;
            InitializeComponent();
            _settings = _settingsService.Load();
            PopulateFromSettings(_settings);
        }

        // ── Settings → UI ──────────────────────────────────────────────────────

        private void PopulateFromSettings(ExportSettings s)
        {
            // General
            TxtFolderTemplate.Text = s.FolderTemplate;
            SelectComboByTag(CmbDateFormat, s.DateFormat);

            // Formats
            ChkPdf.IsChecked = s.ExportPdf;
            ChkDwg.IsChecked = s.ExportDwg;
            ChkIfc.IsChecked = s.ExportIfc;

            // PDF
            TxtPdfFilename.Text = s.Pdf.FilenameTemplate;
            RdoPdfCombined.IsChecked = s.Pdf.CombineToSingleFile;
            RdoPdfSeparate.IsChecked = !s.Pdf.CombineToSingleFile;
            SelectComboByTag(CmbPdfPaperSize, s.Pdf.PaperSize);
            RdoLandscape.IsChecked = s.Pdf.Orientation == "Landscape";
            RdoPortrait.IsChecked  = s.Pdf.Orientation == "Portrait";
            RdoZoom.IsChecked      = s.Pdf.ZoomType == "Zoom";
            RdoFitToPage.IsChecked = s.Pdf.ZoomType == "FitToPage";
            TxtZoomPct.Text        = s.Pdf.ZoomPercentage.ToString();
            RdoVector.IsChecked    = s.Pdf.HiddenLineViews == "VectorProcessing";
            RdoRaster.IsChecked    = s.Pdf.HiddenLineViews == "RasterProcessing";
            ChkViewLinksBlue.IsChecked   = s.Pdf.ViewLinksInBlue;
            ChkHideRefPlanes.IsChecked   = s.Pdf.HideRefWorkPlanes;
            ChkHideUnrefTags.IsChecked   = s.Pdf.HideUnreferencedViewTags;
            ChkHideScopeBoxes.IsChecked  = s.Pdf.HideScopeBoxes;
            ChkHideCropBounds.IsChecked  = s.Pdf.HideCropBoundaries;
            ChkReplaceHalftone.IsChecked = s.Pdf.ReplaceHalftoneWithThinLines;
            SelectComboByContent(CmbRasterQuality, s.Pdf.RasterQuality);
            SelectComboByContent(CmbColorDepth,    s.Pdf.ColorDepth);

            // DWG
            TxtDwgFilename.Text = s.Dwg.FilenameTemplate;
            SelectComboByTag(CmbLayerMapping,  s.Dwg.LayerMapping);
            SelectComboByTag(CmbDwgVersion,    s.Dwg.FileVersion);
            SelectComboByTag(CmbTextTreatment, s.Dwg.TextTreatment);
            SelectComboByTag(CmbExportColors,  s.Dwg.ExportColors);
            SelectComboByTag(CmbExportSolids,  s.Dwg.ExportSolids);
            SelectComboByTag(CmbUnits,         s.Dwg.Units);
            SelectComboByTag(CmbCoordinates,   s.Dwg.CoordinateSystem);
            ChkDwgExternalRefs.IsChecked  = s.Dwg.ViewsOnSheetsAsExternalRefs;
            ChkDwgHideScopeBoxes.IsChecked = s.Dwg.HideScopeBoxes;
            ChkDwgHideRefPlanes.IsChecked  = s.Dwg.HideRefPlanes;
            ChkDwgHideUnrefTags.IsChecked  = s.Dwg.HideUnreferencedViewTags;

            // IFC
            TxtIfcFilename.Text = s.Ifc.FilenameTemplate;

            UpdatePreviews();
        }

        // ── UI → Settings ──────────────────────────────────────────────────────

        private ExportSettings CollectSettings()
        {
            var s = new ExportSettings
            {
                FolderTemplate = TxtFolderTemplate.Text,
                DateFormat     = GetComboTag(CmbDateFormat) ?? "yyMMdd",
                ExportPdf      = ChkPdf.IsChecked == true,
                ExportDwg      = ChkDwg.IsChecked == true,
                ExportIfc      = ChkIfc.IsChecked == true,

                Pdf = new PdfSettings
                {
                    FilenameTemplate         = TxtPdfFilename.Text,
                    CombineToSingleFile      = RdoPdfCombined.IsChecked == true,
                    PaperSize                = GetComboTag(CmbPdfPaperSize) ?? "Default",
                    Orientation              = RdoLandscape.IsChecked == true ? "Landscape" : "Portrait",
                    ZoomType                 = RdoFitToPage.IsChecked == true ? "FitToPage" : "Zoom",
                    ZoomPercentage           = int.TryParse(TxtZoomPct.Text, out var pct) ? pct : 100,
                    HiddenLineViews          = RdoVector.IsChecked == true ? "VectorProcessing" : "RasterProcessing",
                    ViewLinksInBlue          = ChkViewLinksBlue.IsChecked == true,
                    HideRefWorkPlanes        = ChkHideRefPlanes.IsChecked == true,
                    HideUnreferencedViewTags = ChkHideUnrefTags.IsChecked == true,
                    HideScopeBoxes           = ChkHideScopeBoxes.IsChecked == true,
                    HideCropBoundaries       = ChkHideCropBounds.IsChecked == true,
                    ReplaceHalftoneWithThinLines = ChkReplaceHalftone.IsChecked == true,
                    RasterQuality            = (CmbRasterQuality.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "High",
                    ColorDepth               = (CmbColorDepth.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Color" ? "Color"
                                             : (CmbColorDepth.SelectedItem as ComboBoxItem)?.Content?.ToString() == "Grayscale" ? "GrayScale"
                                             : "BlackAndWhite",
                },

                Dwg = new DwgSettings
                {
                    FilenameTemplate          = TxtDwgFilename.Text,
                    LayerMapping              = GetComboTag(CmbLayerMapping) ?? "fromPrinterSettings",
                    FileVersion               = GetComboTag(CmbDwgVersion) ?? "2018",
                    TextTreatment             = GetComboTag(CmbTextTreatment) ?? "Exact",
                    ExportColors              = GetComboTag(CmbExportColors) ?? "IndexColors",
                    ExportSolids              = GetComboTag(CmbExportSolids) ?? "ACIS",
                    Units                     = GetComboTag(CmbUnits) ?? "Millimeter",
                    CoordinateSystem          = GetComboTag(CmbCoordinates) ?? "ProjectInternal",
                    ViewsOnSheetsAsExternalRefs = ChkDwgExternalRefs.IsChecked == true,
                    HideScopeBoxes            = ChkDwgHideScopeBoxes.IsChecked == true,
                    HideRefPlanes             = ChkDwgHideRefPlanes.IsChecked == true,
                    HideUnreferencedViewTags  = ChkDwgHideUnrefTags.IsChecked == true,
                },

                Ifc = new IfcSettings
                {
                    FilenameTemplate = TxtIfcFilename.Text,
                }
            };

            return s;
        }

        // ── Preview updates ────────────────────────────────────────────────────

        private void PreviewChanged(object sender, object e) => UpdatePreviews();

        private void UpdatePreviews()
        {
            var dateFormat = GetComboTag(CmbDateFormat) ?? "yyMMdd";
            var resolver = new TagResolver(_doc, dateFormat);

            // Folder: live doc values (no sheet tags in folder template)
            TxtFolderPreview.Text = $"Preview: {resolver.Resolve(TxtFolderTemplate.Text)}";

            // Filenames: use first selected sheet for sheet-level tags; fall back to placeholders if none
            var sampleSheet = _sheets.FirstOrDefault();
            if (sampleSheet != null)
            {
                TxtPdfFilenamePreview.Text = $"Preview: {resolver.Resolve(TxtPdfFilename.Text, sampleSheet)}.pdf";
                TxtDwgFilenamePreview.Text = $"Preview: {resolver.Resolve(TxtDwgFilename.Text, sampleSheet)}.dwg";
            }
            else
            {
                TxtPdfFilenamePreview.Text = $"Preview: {TagResolver.Preview(TxtPdfFilename.Text, dateFormat)}.pdf";
                TxtDwgFilenamePreview.Text = $"Preview: {TagResolver.Preview(TxtDwgFilename.Text, dateFormat)}.dwg";
            }
            TxtIfcFilenamePreview.Text = $"Preview: {resolver.Resolve(TxtIfcFilename.Text)}.ifc";
        }

        private void FormatCheckChanged(object sender, RoutedEventArgs e)
        {
            // Enable/disable the Export button if at least one format is checked
            if (BtnExport != null)
                BtnExport.IsEnabled = ChkPdf.IsChecked == true || ChkDwg.IsChecked == true || ChkIfc.IsChecked == true;
        }

        // ── Folder browse ──────────────────────────────────────────────────────

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {
            // Note: WPF doesn't have a folder browser built in; use the legacy one via FolderBrowserDialog interop
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select base export folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TxtFolderTemplate.Text = dialog.SelectedPath;
        }

        // ── Load / Save settings ───────────────────────────────────────────────

        private void LoadSettings(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON settings (*.json)|*.json|All files (*.*)|*.*",
                Title  = "Load export settings",
                InitialDirectory = System.IO.Path.GetDirectoryName(SettingsService.DefaultSettingsPath)
            };

            if (dialog.ShowDialog() == true)
            {
                _settings = _settingsService.Load(dialog.FileName);
                PopulateFromSettings(_settings);
            }
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            _settings = CollectSettings();
            _settingsService.Save(_settings);
            MessageBox.Show($"Settings saved to:\n{SettingsService.DefaultSettingsPath}",
                "Sheet Exporter", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Export ─────────────────────────────────────────────────────────────

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            _settings = CollectSettings();

            if (!_settings.ExportPdf && !_settings.ExportDwg && !_settings.ExportIfc)
            {
                MessageBox.Show("Please select at least one export format.",
                    "Sheet Exporter", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var progress = new Progress<string>(msg => Title = $"Exporting — {msg}");
            var exportService = new ExportService(_doc, _settings);

            List<string> errors;
            try
            {
                errors = exportService.Export(_sheets, progress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed:\n{ex.Message}", "Sheet Exporter",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Title = "Export Settings";
                return;
            }

            Title = "Export Settings";

            if (errors.Count > 0)
            {
                MessageBox.Show($"Export completed with errors:\n\n{string.Join("\n", errors)}",
                    "Sheet Exporter", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                var baseFolder = new TagResolver(_doc, _settings.DateFormat).Resolve(_settings.FolderTemplate);
                MessageBox.Show($"Export complete!\n\nFiles saved to:\n{baseFolder}",
                    "Sheet Exporter", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e) => Close();

        // ── ComboBox helpers ───────────────────────────────────────────────────

        private static void SelectComboByTag(ComboBox cmb, string tag)
        {
            foreach (ComboBoxItem item in cmb.Items)
                if (item.Tag?.ToString() == tag) { cmb.SelectedItem = item; return; }
        }

        private static void SelectComboByContent(ComboBox cmb, string content)
        {
            foreach (ComboBoxItem item in cmb.Items)
                if (item.Content?.ToString()?.Equals(content, StringComparison.OrdinalIgnoreCase) == true)
                { cmb.SelectedItem = item; return; }
        }

        private static string? GetComboTag(ComboBox cmb) =>
            (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString();
    }
}
