# New CSV Format - Table Format for Measurement Cycles

## Before (Old Format)
```csv
Category,Field,Value
"Measurement Cycles","Headers","Cycle #,Blank (counts),Sample (counts),Volume (cm³),Deviation (cm³),Density (g/cm³),Deviation (g/cm³)"
"Measurement Cycles","Cycle 1","1,18577,13981,13.9755,0.1306,0.5474,-0.0052"
"Measurement Cycles","Cycle 2","2,18588,14009,13.9235,0.0786,0.5494,-0.0031"
```

## After (New Table Format)
```csv
Category,Field,Value

"Report Info","Generated","2025-10-16 23:15:30"
"Report Info","Source File","Multiple Reports.pdf"
"Sample","Record","RHH646-ORA-0070 24-3 -- SOP TECH IN"
"Sample","Sample mass","7.6500 g"
... (all other data)

MEASUREMENT CYCLES TABLE
Cycle #,Blank (counts),Sample (counts),Volume (cm³),Volume Deviation (cm³),Density (g/cm³),Density Deviation (g/cm³)
1,18577,13981,13.9755,0.1306,0.5474,-0.0052
2,18588,14009,13.9235,0.0786,0.5494,-0.0031
3,18600,14030,13.8979,0.0531,0.5504,-0.0021
4,18612,14050,13.8733,0.0285,0.5514,-0.0011
5,18623,14068,13.8523,0.0075,0.5523,-0.0003
6,18630,14084,13.8241,-0.0208,0.5534,0.0008
7,18637,14099,13.7994,-0.0454,0.5544,0.0018
8,18643,14110,13.7848,-0.0600,0.5550,0.0024
9,18651,14123,13.7693,-0.0755,0.5556,0.0030
10,18656,14135,13.7483,-0.0965,0.5564,0.0039
```

## Benefits of New Table Format
- ✅ **Each cycle is a row** with values in separate columns
- ✅ **Perfect for Excel import** - opens as a proper table
- ✅ **Easy data analysis** - can sort, filter, and analyze by any column
- ✅ **Clear structure** - measurement cycles are in their own table section
- ✅ **Consistent formatting** - 4 decimal places for precision values
- ✅ **Standard CSV format** - compatible with all spreadsheet applications

## File Structure
1. **Basic Data Section**: All report info, sample data, parameters, and results in Category/Field/Value format
2. **Empty Line**: Separates basic data from measurement cycles
3. **Measurement Cycles Table**: 
   - Header row with column names
   - Each measurement cycle as a separate row
   - Each measurement value in its own column

## Usage in Excel/Spreadsheets
When you open this CSV file in Excel:
- The basic data will be in the first section (Category/Field/Value columns)
- The measurement cycles will appear as a proper table with:
  - Column headers in row 1 of the table
  - Each cycle's data in separate rows
  - Easy sorting and filtering capabilities
  - Perfect for creating charts and pivot tables

This format gives you the best of both worlds: structured metadata at the top and tabular measurement data at the bottom!