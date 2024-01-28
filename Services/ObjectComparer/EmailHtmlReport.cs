using global::KellermanSoftware.CompareNetObjects;
using global::KellermanSoftware.CompareNetObjects.Reports;
using System.Diagnostics;
using System.Text;

namespace ChannelsDVR_Log_Monitor.Services.ObjectComparer;

/// <summary>
/// Create an HTML file of the differences and launch the default HTML handler
/// </summary>
public class EmailHtmlReport : ISingleFileReport
{
    /// <summary>
    /// Default constructor, sets up Config object
    /// </summary>
    public EmailHtmlReport()
    {
        Config = new HtmlConfig();
    }

    /// <summary>
    /// EmailHtmlReport Configuration
    /// </summary>
    public HtmlConfig Config { get; set; }

    /// <summary>
    /// Output the differences to a file
    /// </summary>
    /// <param name="differences">A list of differences</param>
    /// <param name="filePath">The file path</param>
    public void OutputFile(List<Difference> differences, string filePath)
    {
        if (String.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            filePath = Path.Combine(FileHelper.GetCurrentDirectory(), filePath);

        using FileStream fileStream =
            new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        using TextWriter writer = new StreamWriter(fileStream);

        WriteItOut(differences, writer);
    }

    private void WriteItOut(List<Difference> differences, TextWriter writer)
    {
        if (Config.GenerateFullHtml)
        {
            Config.Style = string.Empty;
            writer.Write(Config.HtmlHeader);
        }

        StringBuilder rows = new();
        foreach (var difference in differences)
        {
            rows.AppendLine(
                $@"
                    <tr>
                        <td style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{EscapeString(difference.GetShortItem())}</td>
                        <td style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{EscapeString(difference.Object1Value)}</td>
                        <td style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{EscapeString(difference.Object2Value)}</td>
                    </tr>"
            );
        }

        var table =
            $@"
                <table style=""width: 100%; border-collapse: collapse;"">
                    <thead>
                        <tr>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{Config.BreadCrumbColumName}</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{Config.ExpectedColumnName}</th>
                            <th style=""border: 1px solid #ddd; padding: 8px; text-align: left;"">{Config.ActualColumnName}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>";

        writer.Write(table);

        if (Config.GenerateFullHtml)
        {
            writer.Write(Config.HtmlFooter);
        }
    }

    /// <summary>
    /// Launch the HTML Report
    /// </summary>
    /// <param name="filePath">The differences file</param>
    public void LaunchApplication(string filePath)
    {
        if (!EnvironmentHelper.IsWindows())
            throw new NotSupportedException();

        ProcessHelper.Shell(filePath, string.Empty, ProcessWindowStyle.Normal, false);
    }

    /// <summary>
    /// Output the differences to a stream
    /// </summary>
    /// <param name="differences">A list of differences</param>
    /// <param name="stream">An output stream</param>
    public void OutputStream(List<Difference> differences, Stream stream)
    {
        TextWriter writer = new StreamWriter(stream);
        WriteItOut(differences, writer);
        writer.Flush();

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Output the differences to a string
    /// </summary>
    /// <param name="differences">A list of differences</param>
    /// <returns>A string</returns>
    public string OutputString(List<Difference> differences)
    {
        StringBuilder sb = new(differences.Count * 40);
        TextWriter writer = new StringWriter(sb);
        WriteItOut(differences, writer);

        return sb.ToString();
    }

    /// <summary>
    /// Escape special characters
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static string EscapeString(object value)
    {
        if (value == null)
            return string.Empty;

        return WebHelper.HtmlEncode(value.ToString());
    }
}
