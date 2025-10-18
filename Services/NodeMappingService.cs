using System;
using System.Collections.Generic;
using System.Linq;
using FileReader.Interfaces;
using FileReader.Models;

namespace FileReader.Services
{
    /// <summary>
    /// Service responsible for mapping extracted data to OPC UA node IDs
    /// Follows Single Responsibility Principle - only handles node mapping logic
    /// </summary>
    public class NodeMappingService : INodeMappingService
    {
        private readonly NodeMappings _nodeMappings;

        public NodeMappingService(NodeMappings nodeMappings)
        {
            _nodeMappings = nodeMappings ?? throw new ArgumentNullException(nameof(nodeMappings));
        }

        /// <summary>
        /// Maps extracted report data to OPC UA write items
        /// </summary>
        public IEnumerable<OpcUaWriteItem> MapReportDataToOpcUaItems(ExtractedReportData reportData)
        {
            if (reportData == null)
                throw new ArgumentNullException(nameof(reportData));

            var writeItems = new List<OpcUaWriteItem>();

            // Map Report Info
            writeItems.AddRange(MapReportInfo(reportData.ReportInfo));
            
            // Map Instrument Info
            writeItems.AddRange(MapInstrumentInfo(reportData.Instrument));
            
            // Map Sample Info
            writeItems.AddRange(MapSampleInfo(reportData.Sample));
            
            // Map Parameters
            writeItems.AddRange(MapMeasurementParameters(reportData.Parameters));
            
            // Map Results
            writeItems.AddRange(MapMeasurementResults(reportData.Results));
            
            // Map Measurement Cycles
            writeItems.AddRange(MapMeasurementCycles(reportData.MeasurementCycles));

            return writeItems.Where(item => !string.IsNullOrEmpty(item.NodeId));
        }

        /// <summary>
        /// Gets the node ID for a specific data category and field
        /// </summary>
        public string GetNodeId(string category, string field)
        {
            if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(field))
                return null;

            switch (category.ToLower())
            {
                case "report info":
                    return GetReportInfoNodeId(field);
                case "instrument":
                    return GetInstrumentNodeId(field);
                case "sample":
                    return GetSampleNodeId(field);
                case "parameters":
                    return GetParametersNodeId(field);
                case "results":
                    return GetResultsNodeId(field);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the node ID for a specific measurement cycle row
        /// </summary>
        public string GetCycleRowNodeId(int cycleNumber)
        {
            switch (cycleNumber)
            {
                case 1: return _nodeMappings.CycleRow1;
                case 2: return _nodeMappings.CycleRow2;
                case 3: return _nodeMappings.CycleRow3;
                case 4: return _nodeMappings.CycleRow4;
                case 5: return _nodeMappings.CycleRow5;
                case 6: return _nodeMappings.CycleRow6;
                case 7: return _nodeMappings.CycleRow7;
                case 8: return _nodeMappings.CycleRow8;
                case 9: return _nodeMappings.CycleRow9;
                case 10: return _nodeMappings.CycleRow10;
                default: return null;
            }
        }

        /// <summary>
        /// Validates that all required node mappings are configured
        /// </summary>
        public bool ValidateMappings()
        {
            var requiredMappings = new[]
            {
                _nodeMappings.StartedTime,
                _nodeMappings.CompletedTime,
                _nodeMappings.SampleMass,
                _nodeMappings.AbsoluteDensity,
                _nodeMappings.CycleRow1,
                _nodeMappings.DataImportArray
            };

            return requiredMappings.All(mapping => !string.IsNullOrEmpty(mapping));
        }

        #region Private Mapping Methods

        private IEnumerable<OpcUaWriteItem> MapReportInfo(ReportInfo reportInfo)
        {
            var items = new List<OpcUaWriteItem>();

            AddIfNotEmpty(items, _nodeMappings.ReportGenerated, reportInfo.Generated.ToString("yyyy-MM-dd HH:mm:ss"), "Report Generated");
            AddIfNotEmpty(items, _nodeMappings.SourceFile, reportInfo.SourceFile, "Source File");
            AddIfNotEmpty(items, _nodeMappings.ReportDate, reportInfo.ReportDate, "Report Date");
            AddIfNotEmpty(items, _nodeMappings.SerialNumber, reportInfo.SerialNumber, "Serial Number");
            AddIfNotEmpty(items, _nodeMappings.ReportType, reportInfo.ReportType, "Report Type");

            return items;
        }

        private IEnumerable<OpcUaWriteItem> MapInstrumentInfo(InstrumentInfo instrument)
        {
            var items = new List<OpcUaWriteItem>();

            AddIfNotEmpty(items, _nodeMappings.InstrumentName, instrument.Name, "Instrument Name");
            AddIfNotEmpty(items, _nodeMappings.InstrumentSerialNumber, instrument.SerialNumber, "Instrument Serial Number");
            AddIfNotEmpty(items, _nodeMappings.InstrumentVersion, instrument.Version, "Instrument Version");

            return items;
        }

        private IEnumerable<OpcUaWriteItem> MapSampleInfo(SampleInfo sample)
        {
            var items = new List<OpcUaWriteItem>();

            AddIfNotEmpty(items, _nodeMappings.SampleRecord, sample.Record, "Sample Record");
            AddIfNotEmpty(items, _nodeMappings.SampleOperator, sample.Operator, "Sample Operator");
            AddIfNotEmpty(items, _nodeMappings.SampleSubmitter, sample.Submitter, "Sample Submitter");
            AddIfNotEmpty(items, _nodeMappings.ReportTime, sample.ReportTime?.ToString("yyyy-MM-dd HH:mm:ss"), "Report Time");
            
            // Use text values for display, numeric values for calculations
            AddIfNotEmpty(items, _nodeMappings.StartedTime, sample.StartedTime?.ToString("yyyy-MM-dd HH:mm:ss"), "Started Time");
            AddIfNotEmpty(items, _nodeMappings.CompletedTime, sample.CompletedTime?.ToString("yyyy-MM-dd HH:mm:ss"), "Completed Time");
            AddIfNotEmpty(items, _nodeMappings.SampleMass, sample.SampleMass?.ToString() ?? sample.SampleMassText, "Sample Mass");
            AddIfNotEmpty(items, _nodeMappings.AbsoluteDensity, sample.AbsoluteDensity?.ToString() ?? sample.AbsoluteDensityText, "Absolute Density");

            return items;
        }

        private IEnumerable<OpcUaWriteItem> MapMeasurementParameters(MeasurementParameters parameters)
        {
            var items = new List<OpcUaWriteItem>();

            AddIfNotEmpty(items, _nodeMappings.ChamberDiameter, parameters.ChamberDiameter?.ToString() ?? parameters.ChamberDiameterText, "Chamber Diameter");
            AddIfNotEmpty(items, _nodeMappings.PreparationCycles, parameters.PreparationCycles?.ToString(), "Preparation Cycles");
            AddIfNotEmpty(items, _nodeMappings.MeasurementCycles, parameters.MeasurementCycles?.ToString(), "Measurement Cycles");
            AddIfNotEmpty(items, _nodeMappings.BlankData, parameters.BlankData, "Blank Data");
            AddIfNotEmpty(items, _nodeMappings.ConsolidationForce, parameters.ConsolidationForce?.ToString() ?? parameters.ConsolidationForceText, "Consolidation Force");
            AddIfNotEmpty(items, _nodeMappings.ConversionFactor, parameters.ConversionFactor?.ToString() ?? parameters.ConversionFactorText, "Conversion Factor");
            AddIfNotEmpty(items, _nodeMappings.ZeroDepth, parameters.ZeroDepth?.ToString() ?? parameters.ZeroDepthText, "Zero Depth");

            return items;
        }

        private IEnumerable<OpcUaWriteItem> MapMeasurementResults(MeasurementResults results)
        {
            var items = new List<OpcUaWriteItem>();

            AddIfNotEmpty(items, _nodeMappings.AverageEnvelopeVolume, results.AverageEnvelopeVolume?.ToString() ?? results.AverageEnvelopeVolumeText, "Average Envelope Volume");
            AddIfNotEmpty(items, _nodeMappings.AverageEnvelopeDensity, results.AverageEnvelopeDensity?.ToString() ?? results.AverageEnvelopeDensityText, "Average Envelope Density");
            AddIfNotEmpty(items, _nodeMappings.SpecificPoreVolume, results.SpecificPoreVolume?.ToString() ?? results.SpecificPoreVolumeText, "Specific Pore Volume");
            AddIfNotEmpty(items, _nodeMappings.Porosity, results.Porosity?.ToString() ?? results.PorosityText, "Porosity");
            AddIfNotEmpty(items, _nodeMappings.PercentSampleVolume, results.PercentSampleVolume?.ToString() ?? results.PercentSampleVolumeText, "Percent Sample Volume");
            AddIfNotEmpty(items, _nodeMappings.StandardDeviationVolume, results.StandardDeviationVolume?.ToString() ?? results.StandardDeviationVolumeText, "Standard Deviation Volume");
            AddIfNotEmpty(items, _nodeMappings.StandardDeviationDensity, results.StandardDeviationDensity?.ToString() ?? results.StandardDeviationDensityText, "Standard Deviation Density");

            return items;
        }

        private IEnumerable<OpcUaWriteItem> MapMeasurementCycles(List<MeasurementCycle> cycles)
        {
            var items = new List<OpcUaWriteItem>();

            // Map individual cycle rows (up to 10)
            var maxCycles = Math.Min(cycles.Count, 10);
            for (int i = 0; i < maxCycles; i++)
            {
                var cycle = cycles[i];
                var nodeId = GetCycleRowNodeId(i + 1);
                if (!string.IsNullOrEmpty(nodeId))
                {
                    items.Add(new OpcUaWriteItem(nodeId, cycle.ToCommaSeparatedString(), $"Cycle Row {i + 1}"));
                }
            }

            // Map first cycle as double array for compatibility
            if (cycles.Count > 0 && !string.IsNullOrEmpty(_nodeMappings.DataImportArray))
            {
                var firstCycleArray = cycles[0].ToDoubleArray();
                items.Add(new OpcUaWriteItem(_nodeMappings.DataImportArray, firstCycleArray, "Data Import Array"));
            }

            return items;
        }

        private string GetReportInfoNodeId(string field)
        {
            switch (field.ToLower())
            {
                case "generated": return _nodeMappings.ReportGenerated;
                case "source file": return _nodeMappings.SourceFile;
                case "report date": return _nodeMappings.ReportDate;
                case "serial number": return _nodeMappings.SerialNumber;
                case "report type": return _nodeMappings.ReportType;
                default: return null;
            }
        }

        private string GetInstrumentNodeId(string field)
        {
            switch (field.ToLower())
            {
                case "instrument": return _nodeMappings.InstrumentName;
                case "serial number": return _nodeMappings.InstrumentSerialNumber;
                case "version": return _nodeMappings.InstrumentVersion;
                default: return null;
            }
        }

        private string GetSampleNodeId(string field)
        {
            switch (field.ToLower())
            {
                case "record": return _nodeMappings.SampleRecord;
                case "operator": return _nodeMappings.SampleOperator;
                case "submitter": return _nodeMappings.SampleSubmitter;
                case "started": return _nodeMappings.StartedTime;
                case "completed": return _nodeMappings.CompletedTime;
                case "report time": return _nodeMappings.ReportTime;
                case "sample mass": return _nodeMappings.SampleMass;
                case "absolute density": return _nodeMappings.AbsoluteDensity;
                default: return null;
            }
        }

        private string GetParametersNodeId(string field)
        {
            switch (field.ToLower())
            {
                case "chamber diameter": return _nodeMappings.ChamberDiameter;
                case "preparation cycles": return _nodeMappings.PreparationCycles;
                case "measurement cycles": return _nodeMappings.MeasurementCycles;
                case "blank data": return _nodeMappings.BlankData;
                case "consolidation force": return _nodeMappings.ConsolidationForce;
                case "conversion factor": return _nodeMappings.ConversionFactor;
                case "zero depth": return _nodeMappings.ZeroDepth;
                default: return null;
            }
        }

        private string GetResultsNodeId(string field)
        {
            switch (field.ToLower())
            {
                case "average envelope volume": return _nodeMappings.AverageEnvelopeVolume;
                case "average envelope density": return _nodeMappings.AverageEnvelopeDensity;
                case "specific pore volume": return _nodeMappings.SpecificPoreVolume;
                case "porosity": return _nodeMappings.Porosity;
                case "percent sample volume": return _nodeMappings.PercentSampleVolume;
                case "standard deviation (volume)": return _nodeMappings.StandardDeviationVolume;
                case "standard deviation (density)": return _nodeMappings.StandardDeviationDensity;
                default: return null;
            }
        }

        private void AddIfNotEmpty(List<OpcUaWriteItem> items, string nodeId, string value, string description)
        {
            if (!string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(value) && value != "Not found")
            {
                items.Add(new OpcUaWriteItem(nodeId, value, description));
            }
        }

        #endregion
    }
}
