using System;
using System.Collections.Generic;

namespace FileReader.Models
{
    /// <summary>
    /// Represents comprehensive data extracted from a PDF report
    /// </summary>
    public class ExtractedReportData
    {
        public ReportInfo ReportInfo { get; set; } = new ReportInfo();
        public InstrumentInfo Instrument { get; set; } = new InstrumentInfo();
        public SampleInfo Sample { get; set; } = new SampleInfo();
        public MeasurementParameters Parameters { get; set; } = new MeasurementParameters();
        public MeasurementResults Results { get; set; } = new MeasurementResults();
        public List<MeasurementCycle> MeasurementCycles { get; set; } = new List<MeasurementCycle>();
        public string FullText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Report metadata information
    /// </summary>
    public class ReportInfo
    {
        public DateTime Generated { get; set; } = DateTime.Now;
        public string SourceFile { get; set; } = string.Empty;
        public string ReportDate { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Instrument information
    /// </summary>
    public class InstrumentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sample information
    /// </summary>
    public class SampleInfo
    {
        public string Record { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string Submitter { get; set; } = string.Empty;
        public DateTime? StartedTime { get; set; }
        public DateTime? CompletedTime { get; set; }
        public DateTime? ReportTime { get; set; }
        public double? SampleMass { get; set; } // in grams
        public double? AbsoluteDensity { get; set; } // in g/cm³
        public string SampleMassText { get; set; } = string.Empty;
        public string AbsoluteDensityText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Measurement parameters
    /// </summary>
    public class MeasurementParameters
    {
        public double? ChamberDiameter { get; set; } // in mm
        public int? PreparationCycles { get; set; }
        public int? MeasurementCycles { get; set; }
        public string BlankData { get; set; } = string.Empty;
        public double? ConsolidationForce { get; set; } // in N
        public double? ConversionFactor { get; set; } // in cm³/mm
        public double? ZeroDepth { get; set; } // in mm
        
        // Text representations for display
        public string ChamberDiameterText { get; set; } = string.Empty;
        public string ConsolidationForceText { get; set; } = string.Empty;
        public string ConversionFactorText { get; set; } = string.Empty;
        public string ZeroDepthText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Measurement results
    /// </summary>
    public class MeasurementResults
    {
        public double? AverageEnvelopeVolume { get; set; } // in cm³
        public double? AverageEnvelopeDensity { get; set; } // in g/cm³
        public double? SpecificPoreVolume { get; set; } // in cm³/g
        public double? Porosity { get; set; } // percentage
        public double? PercentSampleVolume { get; set; } // percentage
        public double? StandardDeviationVolume { get; set; } // in cm³
        public double? StandardDeviationDensity { get; set; } // in g/cm³
        
        // Text representations for display
        public string AverageEnvelopeVolumeText { get; set; } = string.Empty;
        public string AverageEnvelopeDensityText { get; set; } = string.Empty;
        public string SpecificPoreVolumeText { get; set; } = string.Empty;
        public string PorosityText { get; set; } = string.Empty;
        public string PercentSampleVolumeText { get; set; } = string.Empty;
        public string StandardDeviationVolumeText { get; set; } = string.Empty;
        public string StandardDeviationDensityText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual measurement cycle data
    /// </summary>
    public class MeasurementCycle
    {
        public int CycleNumber { get; set; }
        public int BlankCounts { get; set; }
        public int SampleCounts { get; set; }
        public double Volume { get; set; } // in cm³
        public double VolumeDeviation { get; set; } // in cm³
        public double Density { get; set; } // in g/cm³
        public double DensityDeviation { get; set; } // in g/cm³

        /// <summary>
        /// Returns the cycle data as a comma-separated string for OPC UA
        /// </summary>
        public string ToCommaSeparatedString()
        {
            return $"{CycleNumber},{BlankCounts},{SampleCounts},{Volume},{VolumeDeviation},{Density},{DensityDeviation}";
        }

        /// <summary>
        /// Returns the numeric values as a double array (excluding cycle number)
        /// </summary>
        public double[] ToDoubleArray()
        {
            return new double[] { BlankCounts, SampleCounts, Volume, VolumeDeviation, Density, DensityDeviation };
        }
    }

    /// <summary>
    /// DTO for CSV export data
    /// </summary>
    public class CsvDataItem
    {
        public string Category { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public CsvDataItem() { }

        public CsvDataItem(string category, string field, string value)
        {
            Category = category;
            Field = field;
            Value = value;
        }
    }

    /// <summary>
    /// DTO for OPC UA write operations
    /// </summary>
    public class OpcUaWriteItem
    {
        public string NodeId { get; set; } = string.Empty;
        public object Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public OpcUaWriteItem() { }

        public OpcUaWriteItem(string nodeId, object value, string description = "")
        {
            NodeId = nodeId;
            Value = value;
            Description = description;
        }
    }
}

