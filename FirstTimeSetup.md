# First Time Setup Guide

## Quick Start

1. **Install Kepware Server** and start it on port 49320
2. **Run the application** - it will automatically setup certificates
3. **Test the connection** using `FileReader.exe --diagnostic`

## What Happens Automatically

- Creates certificate directories
- Sets up OPC UA client configuration  
- Auto-accepts server certificates
- Connects to Kepware server

## If You Have Issues

1. **Run as Administrator** (first time only)
2. **Check Kepware is running** on `opc.tcp://127.0.0.1:49320`
3. **Allow through firewall** - port 49320
4. **Use diagnostic mode** - `FileReader.exe --diagnostic`

## Manual Setup (if needed)

Run: `FileReader.exe --setup`

This will:
- Create certificates
- Test connection
- Show any remaining issues

That's it! The application handles everything else automatically.