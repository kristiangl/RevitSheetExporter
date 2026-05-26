using Autodesk.Revit.DB;
using RevitSheetExporter.Models;
using System.IO;

namespace RevitSheetExporter.Services
{
    /// <summary>
    /// Orchestrates PDF, DWG, and IFC exports.
    /// Each format writes into a subfolder under the resolved base folder:
    ///   <base>\PDF\
    ///   <base>\DWG\
    ///   <base>\IFC\
    /// </summary>
    public class ExportService
    {
        private readonly Document _doc;
        private readonly ExportSettings _settings;
        private readonly TagResolver _tags;

        public ExportService(Document doc, ExportSettings settings)
        {
            _doc = doc;
            _settings = settings;
            _tags = new TagResolver(doc, settings.DateFormat);
        }

        public List<string> Export(IList<ViewSheet> sheets, IProgress<string>? progress = null)
        {
            var errors = new List<string>();
            var baseFolder = _tags.Resolve(_settings.FolderTemplate);

            if (_settings.ExportPdf)
            {
                progress?.Report("Exporting PDF…");
                try { ExportPdf(sheets, baseFolder); }
                catch (Exception ex) { errors.Add($"PDF: {ex.Message}"); }
            }

            if (_settings.ExportDwg)
            {
                progress?.Report("Exporting DWG…");
                try { ExportDwg(sheets, baseFolder); }
                catch (Exception ex) { errors.Add($"DWG: {ex.Message}"); }
            }

            if (_settings.ExportIfc)
            {
                progress?.Report("Exporting IFC…");
                try { ExportIfc(baseFolder); }
                catch (Exception ex) { errors.Add($"IFC: {ex.Message}"); }
            }

            return errors;
        }

        // ── PDF ───────────────────────────────────────────────────────────────

        private void ExportPdf(IList<ViewSheet> sheets, string baseFolder)
        {
            var pdfFolder = Path.Combine(baseFolder, "PDF");
            Directory.CreateDirectory(pdfFolder);

            var options = BuildPdfOptions();
            var viewIds = sheets.Select(s => s.Id).ToList();

            if (_settings.Pdf.CombineToSingleFile)
            {
                options.Combine = true;
                // FileName must be set on the options object for combined exports
                options.FileName = _tags.Resolve(_settings.Pdf.FilenameTemplate);
                _doc.Export(pdfFolder, viewIds, options);
            }
            else
            {
                options.Combine = false;
                foreach (var sheet in sheets)
                {
                    // Per-sheet: set filename per iteration
                    options.FileName = _tags.Resolve(_settings.Pdf.FilenameTemplate, sheet);
                    _doc.Export(pdfFolder, new List<ElementId> { sheet.Id }, options);
                }
            }
        }

        private PDFExportOptions BuildPdfOptions()
        {
            var opt = new PDFExportOptions
            {
                Combine                      = _settings.Pdf.CombineToSingleFile,
                ViewLinksInBlue              = _settings.Pdf.ViewLinksInBlue,
                HideReferencePlane           = _settings.Pdf.HideRefWorkPlanes,
                HideUnreferencedViewTags     = _settings.Pdf.HideUnreferencedViewTags,
                HideScopeBoxes               = _settings.Pdf.HideScopeBoxes,
                HideCropBoundaries           = _settings.Pdf.HideCropBoundaries,
                ReplaceHalftoneWithThinLines = _settings.Pdf.ReplaceHalftoneWithThinLines,
                ZoomType                     = _settings.Pdf.ZoomType == "FitToPage"
                                                   ? ZoomType.FitToPage
                                                   : ZoomType.Zoom,
                ZoomPercentage               = _settings.Pdf.ZoomPercentage,
            };

            opt.RasterQuality = _settings.Pdf.RasterQuality switch
            {
                "Low"          => RasterQualityType.Low,
                "Medium"       => RasterQualityType.Medium,
                "Presentation" => RasterQualityType.Presentation,
                _              => RasterQualityType.High
            };

            opt.ColorDepth = _settings.Pdf.ColorDepth switch
            {
                "GrayScale" => ColorDepthType.GrayScale,
                _           => ColorDepthType.Color
            };

            return opt;
        }

        // ── DWG ───────────────────────────────────────────────────────────────

        private void ExportDwg(IList<ViewSheet> sheets, string baseFolder)
        {
            var dwgFolder = Path.Combine(baseFolder, "DWG");
            Directory.CreateDirectory(dwgFolder);

            var options = BuildDwgOptions();

            foreach (var sheet in sheets)
            {
                var fileName = _tags.Resolve(_settings.Dwg.FilenameTemplate, sheet);
                _doc.Export(dwgFolder, fileName, new List<ElementId> { sheet.Id }, options);
            }
        }

        private DWGExportOptions BuildDwgOptions()
        {
            // ACADVersion.R2000 and R2004 were removed in Revit 2026 API; minimum is R2007
            var fileVersion = _settings.Dwg.FileVersion switch
            {
                "2013" => ACADVersion.R2013,
                "2010" => ACADVersion.R2010,
                _      => ACADVersion.R2018   // default and handles 2018, 2004, 2000 fallback
            };

            // R2007 needs its own case
            if (_settings.Dwg.FileVersion == "2007")
                fileVersion = ACADVersion.R2007;

            var opt = new DWGExportOptions
            {
                FileVersion    = fileVersion,
                ExportingAreas = _settings.Dwg.ExportRooms,
                SharedCoords   = _settings.Dwg.CoordinateSystem == "SharedSite",
                // MergedViews=true embeds views into the sheet DWG (no xrefs).
                // MergedViews=false creates separate xref DWGs for each view on the sheet.
                MergedViews    = !_settings.Dwg.ViewsOnSheetsAsExternalRefs,
            };

            // HideUnreferenceViewTags — note the Revit API spelling (missing 'd')
            opt.HideUnreferenceViewTags = _settings.Dwg.HideUnreferencedViewTags;

            return opt;
        }

        // ── IFC ───────────────────────────────────────────────────────────────

        private void ExportIfc(string baseFolder)
        {
            var ifcFolder = Path.Combine(baseFolder, "IFC");
            Directory.CreateDirectory(ifcFolder);

            var fileName = _tags.Resolve(_settings.Ifc.FilenameTemplate);
            using var tx = new Transaction(_doc, "IFC Export");
            tx.Start();
            _doc.Export(ifcFolder, fileName, new IFCExportOptions());
            tx.Commit();
        }
    }
}
