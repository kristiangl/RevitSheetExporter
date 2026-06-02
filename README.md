# Revit Sheet Exporter

Bulk export Revit sheets to PDF, DWG, and/or IFC with dynamic folder and filename templates.

> **Personal project notice:** This is a personal project, shared as-is. It will likely not receive updates and does not come with support. Use at your own risk. Issues and pull requests are welcome, but responses are not guaranteed.

---

## Quick install (no build required)

1. Go to the [Releases](../../releases) page and download the latest release Installer.zip.
2. Extract the ZIP anywhere.
3. Ensure Revit is closed. Run `Install.bat` — it will detect which Revit versions you have installed (2026 and/or 2027) and copy the files to the correct addins folder automatically.
4. Restart Revit. The **Sheet Exporter** tab will appear in the ribbon.

To remove the add-in, run `Uninstall.bat` from the same extracted folder.

---

## Quick Update - If you have a previous version installed (no build required)

1. Go to the [Releases](../../releases) page and download the latest release Installer.zip.
2. Extract the ZIP anywhere.
3. Ensure Revit is closed. Run `Install.bat` — it will detect which Revit versions you have installed (2026 and/or 2027) and copy the files to the correct addins folder automatically, replacing previous versions.
4. Start Revit.

---

## Requirements (building from source)

- Revit 2026 or 2027
- [Visual Studio Community 2022](https://visualstudio.microsoft.com/vs/community/) (free) — with the **.NET desktop development** workload installed
- .NET 8 SDK (installed automatically with Visual Studio)

---

## Build

### First time setup

1. Open `RevitSheetExporter.sln` (or `RevitSheetExporter.csproj`) in Visual Studio.
2. The project references `RevitAPI.dll` and `RevitAPIUI.dll` from `C:\Program Files\Autodesk\Revit 2026\`.
   - If Revit is installed elsewhere, update the `<HintPath>` values in `RevitSheetExporter.csproj`.
3. Right-click the project → **Manage NuGet Packages** → restore packages (Newtonsoft.Json will download automatically).
4. The project also uses `System.Windows.Forms` for the folder browser dialog.
   Add it if missing: right-click **Dependencies** → **Add COM reference** or add this to the `.csproj`:
   ```xml
   <UseWindowsForms>true</UseWindowsForms>
   ```

### Build for Revit 2026

Select **Debug** or **Release** from the configuration dropdown, then **Build → Build Solution** (Ctrl+Shift+B).

### Build for Revit 2027

Select **Debug2027** or **Release2027**, then build.

Output goes to `bin\Debug\net8.0-windows\` (or the relevant config folder).

---

## Install (from source build)

Copy both files from the build output to your Revit addins folder:

```
RevitSheetExporter.dll
RevitSheetExporter.addin
```

Destination (choose one):

| Scope | Path |
|-------|------|
| Current user only | `%APPDATA%\Autodesk\Revit\Addins\2026\` |
| All users on this PC | `C:\ProgramData\Autodesk\Revit\Addins\2026\` |

For Revit 2027, replace `2026` with `2027`.

The project's post-build targets handle this automatically — a successful **Debug** or **Release** build copies the files to `%APPDATA%\Autodesk\Revit\Addins\2026\` without any extra steps.

---

## First run

1. Open a Revit project (must have a saved `.rvt` file path — the folder template uses `{RevitFileDir}`).
2. Go to the **Sheet Exporter** ribbon tab → click **Export Sheets**.
3. The **Sheet Selection** window opens with all sheets listed.
4. Filter/select your sheets, click **Export…**.
5. Configure your settings in the **Export Settings** window.
6. Click **Save Settings** so your preferences persist for next time.
7. Click **Export**.

---

## Transferring settings between computers

Settings are saved to:
```
%APPDATA%\RevitSheetExporter\settings.json
```

To move settings to another machine, copy that file to the same path on the other computer. You can also use **Load Settings…** in the Export Settings window to load a settings file from any location.

---

## Dynamic tags

Use these in the Folder Template and Filename Template fields:

| Tag | Resolves to |
|-----|-------------|
| `{RevitFileDir}` | Folder containing the open `.rvt` file |
| `{ProjectNumber}` | Revit Project Information → Number |
| `{ProjectName}` | Revit Project Information → Name |
| `{Date}` | Today's date in your chosen format (e.g. `260521`) |
| `{SheetNumber}` | Sheet number (filename templates only) |
| `{SheetName}` | Sheet name (filename templates only) |

### Example

Folder template:
```
{RevitFileDir}\Exports\{Date}_
```

PDF filename:
```
{ProjectNumber}_{ProjectName}_{Date}
```

DWG filename:
```
{ProjectNumber}_{ProjectName}_DWG_{Date}_{SheetNumber}_{SheetName}
```

Result for a sheet "A1.01 - Site Plan" in project 367 "Cornish House" on 21 May 2026:
```
C:\Projects\CornishHouse\Revit\Exports\260521_\PDF\367_Cornish House_260521.pdf
C:\Projects\CornishHouse\Revit\Exports\260521_\DWG\367_Cornish House_DWG_260521_A1.01_Site Plan.dwg
C:\Projects\CornishHouse\Revit\Exports\260521_\IFC\367_Cornish House_IFC_260521.ifc
```

---

## Notes

- **IFC** is always a single whole-model export, not per-sheet. It uses Revit's active IFC export configuration. To change IFC schema or property sets, configure them in Revit's built-in IFC export dialog first (File → Export → IFC).
- **PDF** is combined into one file by default. Uncheck "Combined into a single file" in PDF settings to get one PDF per sheet.
- The "Show in sheet list only" filter uses Revit's built-in **Appears In Sheet List** parameter.
- If `{RevitFileDir}` resolves to Documents (fallback), it means the Revit file hasn't been saved yet — save it first.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Ribbon tab doesn't appear | Check the `.addin` file is in the correct Addins folder and Revit loaded without errors |
| "Could not load file or assembly" error | Make sure the `.dll` path in the `.addin` file matches where you put the `.dll` |
| API compile errors on specific properties | The Revit API occasionally renames properties between versions — check the Revit API docs for your version |
| Export fails silently | Check the folder template resolves to a valid path; make sure the `.rvt` file is saved |

---

## Support

This is a personal project. It is provided as-is with no warranty and no guarantee of updates or bug fixes. Issues and pull requests are welcome, but responses are not guaranteed.
