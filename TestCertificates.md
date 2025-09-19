# Certificate Troubleshooting Guide

## The "ApplicationCertificate cannot be found" Error

This error occurs when the OPC UA client cannot find or create its application certificate. Here's how to fix it:

### Quick Fix Commands

Run your application with these commands to troubleshoot:

```bash
# Test the application with diagnostic mode
FileReader.exe --diagnostic

# Manual setup mode
FileReader.exe --setup
```

### What the Application Does Automatically

1. **Creates Certificate Directories**:
   ```
   ApplicationFolder/
   └── Certificates/
       ├── Own/           (Your application certificate)
       ├── TrustedPeers/  (Server certificates you trust)
       ├── TrustedIssuers/ (Certificate authorities)
       └── Rejected/      (Certificates that were rejected)
   ```

2. **Generates Application Certificate**:
   - Creates a self-signed certificate for your application
   - Valid for 2 years
   - Stored in the `Certificates/Own/` folder

3. **Handles Missing Certificates**:
   - If certificate creation fails, uses minimal security configuration
   - Automatically accepts server certificates
   - Continues operation with reduced security

### Manual Certificate Setup

If automatic setup fails, you can manually create the certificate structure:

1. **Create Directories**:
   ```
   mkdir Certificates
   mkdir Certificates\Own
   mkdir Certificates\TrustedPeers
   mkdir Certificates\TrustedIssuers
   mkdir Certificates\Rejected
   ```

2. **Run Application**: It will attempt to create certificates in these directories.

### Configuration for Certificate Issues

Update your `appsettings.json` to handle certificate problems:

```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "AutoAcceptUntrustedCertificates": true,
    "UseSecurity": false,
    "ApplicationName": "PDF Data Extractor OPC UA Client"
  }
}
```

### Common Solutions

#### 1. Permission Issues
**Problem**: Access denied when creating certificates
**Solution**:
- Run application as Administrator once
- Or use the portable certificate mode (default)

#### 2. Certificate Store Corruption
**Problem**: Certificates exist but cannot be read
**Solution**:
- Delete the `Certificates` folder
- Restart the application
- New certificates will be created automatically

#### 3. Firewall/Antivirus Issues
**Problem**: Security software blocks certificate creation
**Solution**:
- Add application to antivirus exceptions
- Temporarily disable real-time protection during setup

#### 4. Network Issues
**Problem**: Cannot connect to OPC UA server
**Solution**:
- Check if Kepware server is running
- Verify endpoint URL in configuration
- Test with diagnostic mode: `FileReader.exe --diagnostic`

### Test Your Setup

1. **Check Certificate Directories**:
   - Look for `Certificates` folder in application directory
   - Verify subfolders exist

2. **Run Diagnostic**:
   ```bash
   FileReader.exe --diagnostic
   ```
   This will test:
   - Network connectivity
   - Certificate setup
   - OPC UA connection
   - Server discovery

3. **Check Application Output**:
   - Look for "✅ Application certificate created" message
   - Or "✅ Application certificate exists" message

### Fallback Configuration

If all else fails, the application will use minimal configuration:

- No application certificate required
- Accepts all server certificates
- Uses anonymous authentication
- Connects without security

This allows the application to work even with certificate issues, though with reduced security.

### Debugging Certificate Issues

Enable detailed logging by running:
```bash
FileReader.exe --setup
```

This will show:
- Certificate creation attempts
- Directory permissions
- Validation errors
- Connection test results

The application is designed to be resilient and will continue operating even if certificate setup fails, ensuring your PDF processing workflow continues uninterrupted.
