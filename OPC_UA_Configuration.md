# OPC UA Integration Configuration

## Overview
This application now includes OPC UA client functionality to write extracted PDF data to a Kepware OPC UA server.

## Setup Instructions

### 1. Kepware Server Configuration
- Ensure your Kepware server is running on the default endpoint: `opc.tcp://localhost:49320`
- Create the following OPC UA nodes in your Kepware project:

#### Summary Data Nodes:
- `ns=2;s=PDF.StartedTime` (String) - Started timestamp from PDF
- `ns=2;s=PDF.CompletedTime` (String) - Completed timestamp from PDF  
- `ns=2;s=PDF.SampleMass` (Double) - Sample mass value in grams
- `ns=2;s=PDF.AbsoluteDensity` (Double) - Absolute density value in g/cm³
- `ns=2;s=PDF.LastExtracted` (String) - Timestamp when data was extracted

#### Measurement Cycle Nodes (up to 10 cycles):
- `ns=2;s=PDF.MeasurementCount` (Integer) - Total number of measurement cycles
- `ns=2;s=PDF.Cycle1.Number` (Integer) - Cycle 1 number
- `ns=2;s=PDF.Cycle1.Volume` (Double) - Cycle 1 volume in cm³
- `ns=2;s=PDF.Cycle1.Density` (Double) - Cycle 1 density in g/cm³
- ... (repeat for Cycle2 through Cycle10)

### 2. Endpoint Configuration
If your Kepware server runs on a different address/port, modify the endpoint URL in `Program.cs`:

```csharp
var opcClient = new OPCUAClient("opc.tcp://your-server-ip:port");
```

### 3. Node ID Mapping
To customize the OPC UA node mappings, modify the `WritePdfDataToOpcUa` method in `OPCUAClient.cs`:

```csharp
// Example: Map to different node IDs
nodeValues["ns=2;s=YourCustomNode.StartTime"] = summaryData.StartedTime;
```

## Application Workflow

1. **PDF Selection**: User selects PDF file via file dialog
2. **Data Extraction**: PDF content is parsed and data extracted
3. **Excel Creation**: Data is exported to Excel file in `excel/ExtractedExcel.xlsx`
4. **OPC UA Writing**: Extracted data is automatically written to OPC UA server
5. **Auto-Close**: Application closes automatically

## Error Handling

- OPC UA operations are optional - if the connection fails, the application continues
- All OPC UA errors are handled silently to maintain the streamlined user experience
- Check Kepware server logs if data is not appearing in OPC UA nodes

## Security Configuration

The OPC UA client is configured to:
- Auto-accept untrusted certificates
- Use anonymous authentication
- Connect without security (for development/testing)

For production environments, consider enabling security policies and proper certificate management.

## Troubleshooting

### Connection Issues:
1. Verify Kepware server is running
2. Check if endpoint URL is correct
3. Ensure firewall allows OPC UA traffic (port 49320 by default)
4. Verify OPC UA nodes exist in Kepware project

### Data Not Appearing:
1. Check if PDF data extraction is working (Excel file should contain data)
2. Verify node IDs match exactly between application and Kepware
3. Check Kepware server event logs for write errors
4. Ensure data types match (String, Double, Integer)

## Dependencies

- **OPCFoundation.NetStandard.Opc.Ua.Client**: Official OPC Foundation .NET client library
- Compatible with .NET Framework 4.8
- Automatically handles OPC UA protocol communication and security
