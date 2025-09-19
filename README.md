# PDF Data Extractor

A console application that extracts specific data from PDF files and displays the results in a formatted output.

## Features

### Standard Data Extraction
The application can automatically extract the following types of data from PDF files:
- **Email addresses** - Finds all valid email addresses
- **Phone numbers** - Detects various phone number formats
- **Timestamps** - Extracts start/completion times with full date-time
- **Dates** - Identifies dates in multiple formats (MM/DD/YYYY, DD/MM/YYYY, Month DD, YYYY, etc.)
- **Measurements** - Captures values with units (mass, volume, temperature, pressure)
- **Densities** - Specific extraction for density values with units
- **Status values** - Extracts status/condition information
- **URLs** - Extracts web URLs (http/https)
- **Numbers** - Finds numeric values (excluding single digits)
- **Tables** - Detects and extracts tabular data (measurement cycles, etc.)

### Custom Pattern Extraction
You can define your own regular expression patterns to search for specific data types such as:
- Social Security Numbers
- Credit Card Numbers
- Invoice Numbers
- Product Codes
- Custom IDs
- Any pattern you can define with regex

### Table Extraction & Excel Export
The application can detect and extract tabular data from PDFs:
- **Measurement Cycles Tables** - Automatically detects tables with cycle data
- **Excel Export** - Exports extracted tables to Excel (.xlsx) format
- **Formatted Output** - Preserves table structure with headers and data types
- **Auto-naming** - Creates Excel files named after the source PDF

## How to Use

### Prerequisites
- .NET Framework 4.8 or later
- PDF files to analyze

### Running the Application

1. **Build the project:**
   ```bash
   dotnet build
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Select your PDF file:**
   - The application will automatically open a Windows file dialog
   - Browse and select your PDF file using the familiar Windows interface
   - **Fallback**: If the dialog fails, you can manually enter the file path
   - **Tip**: You can drag and drop the PDF file into the console window (manual mode)
   - Type 'exit' to quit the application (manual mode)

4. **Automatic data extraction:**
   - The application automatically extracts standard data (emails, phones, dates, URLs, numbers)
   - Results are displayed immediately in a formatted output

5. **Additional options:**
   - **Option 1**: Process another PDF file
   - **Option 2**: Extract custom patterns from the current file
   - **Option 3**: Show full text from the current file
   - **Option 4**: Export tables to Excel
   - **Option 5**: Exit the application

### Example Usage

#### Streamlined Workflow with File Dialog
```
=== PDF Data Extractor ===
This application extracts data from PDF files.

📁 PDF File Selection
Opening file dialog to select your PDF file...

✅ PDF file selected: sample.pdf
📍 Location: C:\Documents\sample.pdf

Processing PDF file...
Data extraction completed!

=== PDF DATA EXTRACTION RESULTS ===

EMAIL ADDRESSES:
----------------
  • john.doe@example.com
  • support@company.com

PHONE NUMBERS:
--------------
  • (555) 123-4567
  • +1-800-555-0199

DATES:
------
  • 12/25/2023
  • January 15, 2024

URLS:
-----
  • https://www.example.com
  • http://support.company.com

NUMBERS:
--------
  • 12345
  • 99.99
  • 2024

TABLES FOUND:
-------------
  📊 Measurement Cycles: 10 rows, 7 columns

Options:
1. Process another PDF file
2. Extract custom patterns from this file
3. Show full text from this file
4. Export tables to Excel
5. Exit

Select an option (1-5): 4

✅ Tables exported successfully!
📁 Excel file saved: Multiple Reports_Tables.xlsx
📍 Location: C:\Documents\Multiple Reports_Tables.xlsx
📊 Exported 1 table(s) with 10 total rows
```

#### Custom Pattern Extraction
```

Enter custom regex patterns to search for:
(Enter empty pattern name to finish)

Pattern name: Invoice Number
Regex pattern for 'Invoice Number': INV-\d{4,6}
✓ Pattern 'Invoice Number' added successfully.

Pattern name: SSN
Regex pattern for 'SSN': \d{3}-\d{2}-\d{4}
✓ Pattern 'SSN' added successfully.

Pattern name: 

Processing PDF file with custom patterns...
Do you want to include the full text in the output? (y/n): n

=== PDF DATA EXTRACTION RESULTS ===

CUSTOM PATTERNS:
----------------
INVOICE NUMBER:
---------------
  • INV-123456
  • INV-789012

SSN:
----
  • 123-45-6789
```

## Dependencies

The application uses the following NuGet packages:
- **itext7** (v7.2.5) - PDF processing library compatible with .NET Framework 4.8
- **itext7.bouncy-castle-adapter** (v7.2.5) - Cryptographic support for iText7
- **EPPlus** (v6.2.10) - Excel file creation and manipulation library

## Common Regex Patterns

Here are some useful regex patterns for custom extraction:

| Data Type | Regex Pattern | Description |
|-----------|---------------|-------------|
| SSN | `\d{3}-\d{2}-\d{4}` | Social Security Numbers (XXX-XX-XXXX) |
| Credit Card | `\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}` | Credit card numbers |
| ZIP Code | `\d{5}(-\d{4})?` | US ZIP codes (5 or 9 digits) |
| Invoice Number | `INV-\d{4,6}` | Invoice numbers starting with "INV-" |
| Product Code | `[A-Z]{2,3}-\d{3,5}` | Product codes (letters-numbers) |
| Currency | `\$\d+(?:\.\d{2})?` | Dollar amounts |
| Time | `\d{1,2}:\d{2}(?::\d{2})?\s?(?:AM|PM)?` | Time formats |

## Error Handling

The application includes comprehensive error handling for:
- File not found errors
- Invalid PDF files
- Corrupted PDF files
- Invalid regex patterns
- File access permissions

## Tips

1. **File Paths**: Use full file paths for best results. Drag and drop the file into the console to get the path automatically.

2. **Regex Testing**: Test your custom regex patterns on small samples first to ensure they work correctly.

3. **Large Files**: For very large PDF files, consider not including the full text output to improve readability.

4. **Multiple Files**: Run the application multiple times for different files, or modify the code to process multiple files in batch.

## Troubleshooting

- **"File not found"**: Ensure the file path is correct and the file exists
- **"Invalid PDF"**: The file may be corrupted or password-protected
- **"Access denied"**: Check file permissions or close the PDF if it's open in another application
- **"Invalid regex"**: Verify your regular expression syntax is correct
