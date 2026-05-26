using Autodesk.Revit.DB;
using System.IO;

namespace RevitSheetExporter.Services
{
    /// <summary>
    /// Resolves dynamic tag placeholders in folder and filename templates.
    ///
    /// Available tags:
    ///   {RevitFileDir}   — directory containing the open .rvt file
    ///   {ProjectNumber}  — Revit Project Information > Number
    ///   {ProjectName}    — Revit Project Information > Name
    ///   {Date}           — today in the configured DateFormat (e.g. 260521)
    ///   {SheetNumber}    — sheet number (only valid in per-sheet filenames)
    ///   {SheetName}      — sheet name  (only valid in per-sheet filenames)
    /// </summary>
    public class TagResolver
    {
        private readonly Document _doc;
        private readonly string _dateFormat;

        public TagResolver(Document doc, string dateFormat)
        {
            _doc = doc;
            _dateFormat = dateFormat;
        }

        /// <summary>
        /// Resolve all tags in <paramref name="template"/>.
        /// Pass a <paramref name="sheet"/> to also resolve {SheetNumber} and {SheetName}.
        /// </summary>
        public string Resolve(string template, ViewSheet? sheet = null)
        {
            var projectInfo = _doc.ProjectInformation;

            var result = template
                .Replace("{RevitFileDir}", GetRevitFileDir())
                .Replace("{ProjectNumber}", Sanitize(projectInfo?.Number ?? "UnknownProject"))
                .Replace("{ProjectName}", Sanitize(projectInfo?.Name ?? "UnknownName"))
                .Replace("{Date}", DateTime.Now.ToString(_dateFormat));

            if (sheet != null)
            {
                result = result
                    .Replace("{SheetNumber}", Sanitize(sheet.SheetNumber))
                    .Replace("{SheetName}", Sanitize(sheet.Name));
            }

            return result;
        }

        /// <summary>
        /// Returns a preview string using placeholder values — useful for UI previews
        /// without needing a live document.
        /// </summary>
        public static string Preview(string template, string dateFormat)
        {
            return template
                .Replace("{RevitFileDir}", @"C:\Projects\MyProject")
                .Replace("{ProjectNumber}", "367")
                .Replace("{ProjectName}", "Cornish House")
                .Replace("{Date}", DateTime.Now.ToString(dateFormat))
                .Replace("{SheetNumber}", "A1.01")
                .Replace("{SheetName}", "Site Plan");
        }

        private string GetRevitFileDir()
        {
            var path = _doc.PathName;
            if (string.IsNullOrEmpty(path))
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return Path.GetDirectoryName(path) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>Replace characters that are illegal in file/folder names.</summary>
        private static string Sanitize(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c)).Trim();
        }
    }
}
