# Revit Sheet Exporter — Website Copy

---

## HERO

**Revit Sheet Exporter**

Bulk export sheets to PDF, DWG, and IFC — without the subscription.

[Download Free] [View on GitHub]

---

## INTRO

A free, open-source Revit add-in for exporting sheets in bulk. No licence fees, no cloud dependency, no annual renewal. Just install it and it works.

Supports Revit 2026 and 2027.

---

## FEATURES

**Export to PDF, DWG, and IFC in one pass**
Select your sheets, configure your formats, click Export. All three formats run together, each landing in its own subfolder.

**Custom folder and filename templates**
Use dynamic tags to build folder paths and filenames from your project data — sheet number, sheet name, project number, date, or the location of the Revit file itself. The preview updates live as you type.

**Full PDF control**
Paper size, orientation, zoom, raster or vector processing, colour depth, raster quality, and visibility options for reference planes, scope boxes, crop boundaries, and unreferenced tags.

**Full DWG control**
File version (2007 through 2018), layer mapping, text treatment, export colours, solids, units, and coordinate system. Views on sheets can be embedded or exported as external references.

**Settings that travel with you**
Settings are saved to a JSON file you can copy between machines. Load any settings file from anywhere using the Load Settings button.

---

## HOW IT WORKS

1. Open a Revit project and go to the Sheet Exporter tab in the ribbon.
2. Select the sheets you want to export — filter by name or number, or select all.
3. Configure your export folder, filenames, and format options.
4. Click Export.

---

## FILENAME TEMPLATES

Build filenames from your project data using tags:

| Tag | Resolves to |
|-----|-------------|
| `{ProjectNumber}` | Revit Project Information → Number |
| `{ProjectName}` | Revit Project Information → Name |
| `{SheetNumber}` | Sheet number |
| `{SheetName}` | Sheet name |
| `{Date}` | Today's date in your chosen format |
| `{RevitFileDir}` | Folder containing the open .rvt file |

Example filename: `{ProjectNumber}_{SheetNumber}_{SheetName}_{Date}`
Result: `367_A1.01_Site Plan_260604`

---

## INSTALL

Download the installer, extract the ZIP, and run Install.bat. It detects which versions of Revit you have installed and copies the files to the right place. Restart Revit and the Sheet Exporter tab will appear.

[Download Latest Release]

Requires Revit 2026 or 2027.

---

## OPEN SOURCE

The full source code is on GitHub. Built with C# and the Revit API — contributions and forks are welcome.

[View on GitHub →]

---

## FOOTER

Revit Sheet Exporter is a personal project, shared for free. It is provided as-is with no warranty. It will likely not receive regular updates and does not come with support.

Not affiliated with Autodesk.
