# First Time Setup Guide

## Quick Start

1. **Download and Extract** the application files
2. **Configure** the `appsettings.json` file
3. **Run Setup** to initialize certificates
4. **Test Connection** to verify everything works
5. **Process PDF** files

## Step-by-Step Setup

### 1. Configure OPC UA Connection

Edit `appsettings.json`:

```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://YOUR_SERVER:49320",
    "ApplicationName": "PDF Data Extractor OPC UA Client"
  }
}
```

Replace `YOUR_SERVER` with your OPC UA server address.

### 2. Run Certificate Setup

Open Command Prompt/PowerShell and run:

```bash
FileReader.exe --setup
```

This will:
- Create necessary certificates
- Configure certificate stores
- Test the connection to your OPC UA server

### 3. Test Connection

Verify everything is working:

```bash
FileReader.exe --diagnostic
```

You should see:
- ✅ Connection successful!
- List of available OPC UA nodes

### 4. Configure Node Mappings

Update the `NodeMappings` section in `appsettings.json` to match your OPC UA server structure:

```json
{
  "NodeMappings": {
    "StartedTime": "ns=2;s=YOUR_NODE_PATH.started",
    "CompletedTime": "ns=2;s=YOUR_NODE_PATH.completed"
  }
}
```

### 5. Test with PDF

Run the application normally:

```bash
FileReader.exe
```

Select a PDF file and verify:
- CSV file is created in the `output/` folder
- Data is written to your OPC UA server

## Common Setup Issues

### Certificate Problems
**Error**: Certificate validation failed
**Solution**: 
- Run `FileReader.exe --setup`
- Check OPC UA server certificate settings
- Set `AutoAcceptUntrustedCertificates: true` for testing

### Connection Refused
**Error**: Connection refused or timeout
**Solution**:
- Verify OPC UA server is running
- Check firewall settings
- Confirm endpoint URL is correct

### Node Write Failures
**Error**: Write operations fail
**Solution**:
- Verify node IDs in configuration
- Check OPC UA server permissions
- Use `--diagnostic` to browse available nodes

### Security Issues
**Error**: Authentication failed
**Solution**:
- Set `UseSecurity: false` for testing
- Configure proper username/password if required
- Check OPC UA server security policy

## Verification Checklist

- [ ] `appsettings.json` configured with correct endpoint
- [ ] Certificate setup completed successfully
- [ ] Diagnostic test passes
- [ ] Node mappings configured
- [ ] PDF processing works end-to-end
- [ ] CSV files are generated
- [ ] OPC UA data writes successfully

## Next Steps

Once setup is complete:

1. **Process PDF Files**: Use the application to extract data from your PDF reports
2. **Monitor OPC UA**: Verify data appears in your OPC UA client
3. **Customize Mappings**: Adjust node mappings as needed for your system
4. **Automate**: Consider scheduling or integrating with your workflow

## Support

If you encounter issues:

1. Run diagnostic tools (`--diagnostic`, `--setup`)
2. Check the console output for detailed error messages
3. Verify your OPC UA server configuration
4. Review the Configuration Guide for advanced settings

## Security Considerations

For production use:
- Enable security: `"UseSecurity": true`
- Use proper certificates
- Configure authentication
- Restrict network access
- Regular security updates