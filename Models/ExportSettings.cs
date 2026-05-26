namespace RevitSheetExporter.Models
{
    /// <summary>
    /// Top-level settings model — serialised to JSON in %AppData%\RevitSheetExporter\settings.json
    /// </summary>
    public class ExportSettings
    {
        // ── General ────────────────────────────────────────────────────────────
        /// <summary>
        /// Base folder template. Supports: {RevitFileDir} {ProjectNumber} {ProjectName} {Date}
        /// Example: {RevitFileDir}\Exports\{Date}_
        /// </summary>
        public string FolderTemplate { get; set; } = @"{RevitFileDir}\Exports\{Date}_";

        /// <summary>
        /// .NET date format string. Default "yyMMdd" → 260521
        /// </summary>
        public string DateFormat { get; set; } = "yyMMdd";

        // ── Format toggles ─────────────────────────────────────────────────────
        public bool ExportPdf { get; set; } = true;
        public bool ExportDwg { get; set; } = true;
        public bool ExportIfc { get; set; } = false;

        // ── Per-format settings ────────────────────────────────────────────────
        public PdfSettings Pdf { get; set; } = new();
        public DwgSettings Dwg { get; set; } = new();
        public IfcSettings Ifc { get; set; } = new();
    }

    public class PdfSettings
    {
        /// <summary>
        /// Filename template for the combined PDF (or per-sheet if not combined).
        /// Supports: {ProjectNumber} {ProjectName} {Date} {SheetNumber} {SheetName}
        /// </summary>
        public string FilenameTemplate { get; set; } = "{ProjectNumber}_{ProjectName}_{Date}";

        /// <summary>"Default" = use each sheet's paper size; otherwise e.g. "A1", "A3"</summary>
        public string PaperSize { get; set; } = "Default";

        /// <summary>Portrait | Landscape</summary>
        public string Orientation { get; set; } = "Landscape";

        /// <summary>FitToPage | Zoom</summary>
        public string ZoomType { get; set; } = "Zoom";
        public int ZoomPercentage { get; set; } = 100;

        /// <summary>Combine all selected sheets into one PDF</summary>
        public bool CombineToSingleFile { get; set; } = true;

        /// <summary>VectorProcessing | RasterProcessing</summary>
        public string HiddenLineViews { get; set; } = "VectorProcessing";

        // Options checkboxes
        public bool ViewLinksInBlue { get; set; } = false;
        public bool HideRefWorkPlanes { get; set; } = true;
        public bool HideUnreferencedViewTags { get; set; } = true;
        public bool HideScopeBoxes { get; set; } = true;
        public bool HideCropBoundaries { get; set; } = true;
        public bool ReplaceHalftoneWithThinLines { get; set; } = false;

        /// <summary>Draft | Low | Medium | High | Presentation</summary>
        public string RasterQuality { get; set; } = "High";

        /// <summary>Color | GrayScale | BlackAndWhite</summary>
        public string ColorDepth { get; set; } = "Color";
    }

    public class DwgSettings
    {
        /// <summary>
        /// Filename template per sheet.
        /// Supports: {ProjectNumber} {ProjectName} {Date} {SheetNumber} {SheetName}
        /// </summary>
        public string FilenameTemplate { get; set; } = "{ProjectNumber}_{ProjectName}_DWG_{Date}_{SheetNumber}_{SheetName}";

        /// <summary>AutoCAD file version: 2018 | 2013 | 2010 | 2007 | 2004 | 2000</summary>
        public string FileVersion { get; set; } = "2018";

        /// <summary>Export views on sheets as xrefs (OFF by default)</summary>
        public bool ViewsOnSheetsAsExternalRefs { get; set; } = false;

        /// <summary>Layer mapping standard. "fromPrinterSettings" | "AIA" | "BS1192" etc.</summary>
        public string LayerMapping { get; set; } = "fromPrinterSettings";

        public bool HideScopeBoxes { get; set; } = true;
        public bool HideRefPlanes { get; set; } = false;
        public bool HideUnreferencedViewTags { get; set; } = false;
        public bool ExportRooms { get; set; } = false;

        /// <summary>ACIS | PolymeshObject</summary>
        public string ExportSolids { get; set; } = "ACIS";

        /// <summary>Millimeter | Foot | Inch | Meter | Centimeter</summary>
        public string Units { get; set; } = "Millimeter";

        /// <summary>ProjectInternal | SharedSite</summary>
        public string CoordinateSystem { get; set; } = "ProjectInternal";

        /// <summary>IndexColors | TrueColor | TrueColorPerView</summary>
        public string ExportColors { get; set; } = "IndexColors";

        /// <summary>Exact | TextOnly</summary>
        public string TextTreatment { get; set; } = "Exact";
    }

    public class IfcSettings
    {
        /// <summary>
        /// Filename for the whole-model IFC export.
        /// Supports: {ProjectNumber} {ProjectName} {Date}
        /// (No sheet tags — IFC is always a single whole-model export)
        /// </summary>
        public string FilenameTemplate { get; set; } = "{ProjectNumber}_{ProjectName}_IFC_{Date}";

        // IFC export uses Revit's active IFC export setup.
        // Future: expose schema version, property sets etc. here.
    }

    // ── View-model helper ──────────────────────────────────────────────────────
    public class SheetItem : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isSelected;

        public string SheetNumber { get; set; } = string.Empty;
        public string SheetName { get; set; } = string.Empty;
        public bool IsInSheetList { get; set; } = true;
        public Autodesk.Revit.DB.ElementId ElementId { get; set; } = Autodesk.Revit.DB.ElementId.InvalidElementId;

        public string DisplayName => $"{SheetNumber} - {SheetName}";

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    }
}
