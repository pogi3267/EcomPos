using Microsoft.Reporting.NETCore;
using System.Data;

public class ReportService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ReportService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public byte[] RenderReport(string dataSet, string reportPath, string reportType, List<ReportParameter> parameters, DataTable dataSource)
    {

        var rdlcFilePath = $"{_webHostEnvironment.WebRootPath}\\Reports\\{reportPath}";
        LocalReport localReport = new LocalReport();
        localReport.ReportPath = rdlcFilePath;

        localReport.DataSources.Add(new ReportDataSource(dataSet, dataSource));

        if (parameters != null)
        {
            localReport.SetParameters(parameters);
        }

        string mimeType;
        string encoding;
        string fileNameExtension;
        Warning[] warnings;
        string[] streams;

        var renderedBytes = localReport.Render(
            reportType,
            null,
            out mimeType,
            out encoding,
            out fileNameExtension,
            out streams,
            out warnings);

        return renderedBytes;
    }

    public byte[] RenderReport(string reportPath, string reportType, List<ReportParameter> parameters, Dictionary<string, DataTable> dataSets)
    {
        var rdlcFilePath = $"{_webHostEnvironment.WebRootPath}\\Reports\\{reportPath}";
        LocalReport localReport = new LocalReport();
        localReport.ReportPath = rdlcFilePath;

        // Add the datasets to the report
        foreach (var dataSet in dataSets)
        {
            // The key is the name of the dataset, the value is the DataTable
            localReport.DataSources.Add(new ReportDataSource(dataSet.Key, dataSet.Value));
        }

        // Add any parameters (if applicable)
        if (parameters != null)
        {
            localReport.SetParameters(parameters);
        }

        // Render the report
        string mimeType, encoding, fileNameExtension;
        Warning[] warnings;
        string[] streams;

        byte[] reportBytes = localReport.Render(
            reportType, // PDF, Excel, etc.
            null, // Device info
            out mimeType,
            out encoding,
            out fileNameExtension,
            out streams,
            out warnings);

        return reportBytes;
    }

}