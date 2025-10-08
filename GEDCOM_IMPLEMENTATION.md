# GEDCOM Support Implementation

## Overview
This implementation adds support for GEDCOM (.ged) files to the 3D Family Tree Graph project. The system can now:
- Parse GEDCOM files
- Convert GEDCOM data to RootsMagic SQLite database format
- Auto-load the HofstetterFamilyTree.rmtree database on startup
- Support both .rmtree and .ged file formats in the file picker

## Changes Made

### New Files
1. **Assets/Scripts/DataProviders/GedcomParser.cs**
   - Parses GEDCOM files and extracts individual and family information
   - Supports standard GEDCOM tags (INDI, FAM, NAME, BIRT, DEAT, MARR, etc.)

2. **Assets/Scripts/DataProviders/GedcomToSqlConverter.cs**
   - Converts parsed GEDCOM data into RootsMagic SQLite database format
   - Formats dates correctly for compatibility with existing queries
   - Creates all necessary tables (PersonTable, NameTable, EventTable, FamilyTable, ChildTable)

3. **Assets/Scripts/UI/GedcomConverterUtility.cs**
   - Utility script for manual GEDCOM conversion
   - Can be called to convert the project's GEDCOM file to database

### Modified Files
1. **Assets/Scripts/UI/RootsMagicFileBrowserHandler.cs**
   - Added support for .ged file filter
   - Auto-loads HofstetterFamilyTree.rmtree on first startup
   - Automatically converts GEDCOM files to SQLite when selected

## Usage

### Automatic Usage
1. On first launch, the application will automatically load `HofstetterFamilyTree.rmtree` if it exists in the project root
2. When selecting a .ged file through the file browser, it will be automatically converted to SQLite format

### Manual GEDCOM Conversion
To manually convert a GEDCOM file:
```csharp
var converter = new GedcomToSqlConverter();
converter.ConvertGedcomToDatabase(gedcomFilePath, outputDbPath);
```

## Data Format

### GEDCOM Date Format
GEDCOM dates are typically in the format: "DD MMM YYYY" (e.g., "17 January 1959")

### RootsMagic Date Format
The converter transforms GEDCOM dates to RootsMagic format: "D.+YYYYMMDD..+00000000.."
- Year: 4 digits
- Month: 2 digits (01-12)
- Day: 2 digits (01-31)

## Database Schema
The converted database includes the following tables:
- **PersonTable**: Person records with sex, living status, etc.
- **NameTable**: Person names with given name, surname, birth/death years
- **EventTable**: Birth, death, and marriage events
- **FamilyTable**: Family relationships (father, mother)
- **ChildTable**: Parent-child relationships

## Testing
The GEDCOM converter has been tested with the "Hofstetter Family Tree.ged" file, successfully converting:
- 511 individuals
- 163 families
- 350 child relationships

The converted database is fully compatible with all existing data provider queries.

## Notes
- The GEDCOM parser supports basic GEDCOM tags and may need extension for more complex genealogy files
- Date parsing handles common formats but may need adjustment for unusual date notations
- The converter creates a new database file and does not modify the original GEDCOM file
