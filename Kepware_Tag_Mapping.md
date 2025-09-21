# Kepware Tag Mapping Configuration

## Overview
The application is now configured to write PDF measurement data to your specific Kepware tags as shown in your screenshot.

## Tag Mappings

### Basic Information Tags
- **started** → `ns=2;s=Channel1.Device1.started` - PDF start time
- **completed** → `ns=2;s=Channel1.Device1.completed` - PDF completion time  
- **sample_mass** → `ns=2;s=Channel1.Device1.sample_mass` - Sample mass value
- **absolute_density** → `ns=2;s=Channel1.Device1.absolute_density` - Absolute density value

### Measurement Cycle Tags (Comma-Separated Rows)
- **cycle_row1** → `ns=2;s=Channel1.Device1.cycle_row1` - First measurement row as CSV
- **cycle_row2** → `ns=2;s=Channel1.Device1.cycle_row2` - Second measurement row as CSV
- **cycle_row3** → `ns=2;s=Channel1.Device1.cycle_row3` - Third measurement row as CSV
- **cycle_row4** → `ns=2;s=Channel1.Device1.cycle_row4` - Fourth measurement row as CSV
- **cycle_row5** → `ns=2;s=Channel1.Device1.cycle_row5` - Fifth measurement row as CSV
- **cycle_row6** → `ns=2;s=Channel1.Device1.cycle_row6` - Sixth measurement row as CSV
- **cycle_row7** → `ns=2;s=Channel1.Device1.cycle_row7` - Seventh measurement row as CSV
- **cycle_row8** → `ns=2;s=Channel1.Device1.cycle_row8` - Eighth measurement row as CSV
- **cycle_row9** → `ns=2;s=Channel1.Device1.cycle_row9` - Ninth measurement row as CSV
- **cycle_row10** → `ns=2;s=Channel1.Device1.cycle_row10` - Tenth measurement row as CSV

### Array Data Tag
- **Data_import** → `ns=2;s=Channel1.Device1.Data_import` - First measurement row as double array

## Data Format Examples

### Cycle Row Format (CSV String)
Each cycle_row tag will contain a comma-separated string like:
```
"1,18577,13981,13.9755,0.1306,0.5474,-0.0052"
```
Format: `Cycle#,Blank,Sample,Volume,VolumeDeviation,Density,DensityDeviation`

### Data_import Format (Double Array)
The Data_import tag will contain the first measurement row as a double array:
```
[18577, 13981, 13.9755, 0.1306, 0.5474, -0.0052]
```
(Excludes the cycle number, contains only the 6 measurement values)

## How It Works

1. **PDF Processing**: Application extracts measurement data from PDF
2. **Excel Creation**: Creates Excel file with all data
3. **OPC UA Writing**: 
   - Writes basic info to started, completed, sample_mass, absolute_density tags
   - Writes each measurement row as CSV string to cycle_row1-10 tags
   - Writes first measurement row as double array to Data_import tag

## Testing

To test the connection:
```bash
FileReader.exe --diagnostic
```

To run normally (process PDF and write to OPC UA):
```bash
FileReader.exe
```

## Requirements

- Kepware Server running on `opc.tcp://127.0.0.1:49320`
- All tags created in Kepware as shown in your screenshot
- Tags configured as String type for cycle_row tags
- Data_import tag configured as array type for double array

The application will now write measurement data exactly as you requested - with comma-separated values in the cycle_row tags!
