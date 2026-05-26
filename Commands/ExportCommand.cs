using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitSheetExporter.UI;

namespace RevitSheetExporter.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDoc = commandData.Application.ActiveUIDocument;
                if (uiDoc == null)
                {
                    message = "Please open a Revit project before using Sheet Exporter.";
                    return Result.Failed;
                }

                var doc = uiDoc.Document;
                if (doc == null || doc.IsFamilyDocument)
                {
                    message = "Sheet Exporter only works with project files (.rvt), not family files.";
                    return Result.Failed;
                }

                var window = new SheetSelectionWindow(doc);
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
