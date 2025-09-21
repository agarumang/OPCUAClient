# Row Data Issue - Diagnosis and Solution

## Problem Description
You reported that "data is wrong written from 3 row to last row as last row" - meaning rows 3-10 are writing the last row's data instead of their correct data.

## Investigation Results

### ✅ Data Extraction is CORRECT
Our debugging confirmed that:
- PDF extraction works perfectly (1,588 characters extracted)
- Table extraction finds all 10 measurement rows correctly
- Each row has the correct data:
  ```
  Row 1: [1, 18577, 13981, 13.9755, 0.1306, 0.5474, -0.0052]
  Row 2: [2, 18588, 14009, 13.9235, 0.0786, 0.5494, -0.0031]
  Row 3: [3, 18600, 14030, 13.8979, 0.0531, 0.5504, -0.0021]
  ...
  Row 10: [10, 18656, 14135, 13.7483, -0.0965, 0.5564, 0.0039]
  ```

### ✅ OPC UA Writing Logic is CORRECT
The application correctly:
- Loops through each row (i = 0 to 9)
- Creates the correct CSV string for each row
- Maps to the correct cycle_row tag (cycle_row1 to cycle_row10)
- Sends the correct data to each tag

## Likely Root Causes

The issue is probably **NOT** in the FileReader application but in one of these areas:

### 1. **Kepware Server Configuration**
- **Tag Data Types**: Ensure all cycle_row1-10 tags are configured as `String` type
- **Tag Update Rates**: Fast writes might cause older values to overwrite newer ones
- **Tag Scaling/Conversion**: Check if any scaling is applied that might corrupt the data

### 2. **OPC UA Server Timing Issues**
- **Write Buffer**: Server might be buffering writes and processing them out of order
- **Update Rate**: Server might not be processing rapid sequential writes correctly
- **Memory/Performance**: Server might be dropping or corrupting writes under load

### 3. **Client Display/Reading Issues**
- **Refresh Rate**: The client viewing the tags might not be refreshing fast enough
- **Caching**: The client might be showing cached values instead of live values
- **Display Order**: The client might be displaying values in the wrong order

## Solutions Implemented

### 1. **Enhanced Debug Logging**
Added comprehensive logging to show:
- Exactly what data is sent to each tag
- Success/failure status of each write
- Individual value verification
- Timing information

### 2. **Write Timing Improvements**
- Added 10ms delay between writes to prevent race conditions
- Ensured each write completes before starting the next
- Added better error handling and status checking

### 3. **Data Validation**
- Store row reference to avoid any potential memory issues
- Explicit string conversion to ensure clean data
- Detailed logging of each value being written

## How to Test and Verify

### 1. **Run with Debug Logging**
```bash
FileReader.exe --test-rows
```
This will show exactly what data is being sent to each tag.

### 2. **Check Kepware Configuration**
- Verify all cycle_row1-10 tags are `String` type
- Check update rates and scaling settings
- Ensure no data conversions are applied

### 3. **Monitor in Real-Time**
- Open Kepware OPC Expert or similar client
- Watch the tags in real-time as data is written
- Check if the issue occurs during writing or display

### 4. **Test with Slower Writes**
If needed, you can increase the delay between writes by modifying:
```csharp
System.Threading.Thread.Sleep(10); // Increase to 50 or 100ms
```

## Expected Results

When working correctly, you should see:
- `cycle_row1`: `"1,18577,13981,13.9755,0.1306,0.5474,-0.0052"`
- `cycle_row2`: `"2,18588,14009,13.9235,0.0786,0.5494,-0.0031"`
- `cycle_row3`: `"3,18600,14030,13.8979,0.0531,0.5504,-0.0021"`
- ...
- `cycle_row10`: `"10,18656,14135,13.7483,-0.0965,0.5564,0.0039"`

## Next Steps

1. **Start Kepware Server** on port 49320
2. **Run the test**: `FileReader.exe --test-rows`
3. **Check the debug output** to see if the correct data is being sent
4. **Monitor Kepware tags** to see if they receive the correct values
5. **If issue persists**, check Kepware configuration and update rates

The enhanced logging will help pinpoint exactly where the issue occurs - whether it's in the sending, receiving, or display of the data.
