using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RevitSheetExporter
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            const string tabName = "Sheet Exporter";

            try { application.CreateRibbonTab(tabName); }
            catch { /* Tab may already exist on reload */ }

            var panel = application.CreateRibbonPanel(tabName, "Export");

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var buttonData = new PushButtonData(
                "ExportSheets",
                "Export\nSheets",
                assemblyPath,
                "RevitSheetExporter.Commands.ExportCommand"
            )
            {
                ToolTip = "Bulk export sheets to PDF, DWG, and/or IFC",
                LongDescription = "Select sheets from the project sheet list, then configure export formats and locations.",
                LargeImage = LoadIcon("Resources/icon32.png"),
                Image      = LoadIcon("Resources/icon32.png")   // 16×16 slot — Revit scales it down
            };

            panel.AddItem(buttonData);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        private static BitmapImage? LoadIcon(string relPath)
        {
            try
            {
                var uri = new Uri(
                    $"pack://application:,,,/RevitSheetExporter;component/{relPath}",
                    UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                return null;
            }
        }
    }
}
