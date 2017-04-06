using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Threading;
using NessusClient;
using NessusClient.Scans;

namespace NessusPowerShell.Commands
{
    /// <summary>
    /// <para type="synopsis">Exports nessus scan.</para>
    /// </summary>
    /// <example>
    /// <para>Export all scans for the past 10 days as HTML.</para>
    /// <para>File name of exported scan is constructed from name of scan and date of scan.</para>
    /// <para> </para>    
    /// <para>$allScans = Get-NessusScanHistory</para>
    /// 
    /// <para>$allScans</para>
    /// <para>| where {$_.LastUpdateDate -GT [DateTimeOffset]::UtcNow.AddDays(-10)}</para>
    /// <para>| select Id,HistoryId, @{Name="OutFile"; Expression={"{0}-{1:yyyyMMddHHmm}" -f($_.Name, $_.LastUpdateDate.ToLocalTime())}}</para>
    /// <para>| Export-NessusScan -Format Html</para>    
    /// </example>   
    [Cmdlet(VerbsData.Export, "NessusScan")]
    public class ExportNessusScan : NessusCmdletBase
    {

        /// <summary>
        /// <para type="description">Scan ID as it returned by /scans endpoint (or Get-NessusScanHistory cmdlet).</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The Scan Id (returned by Get-NessusScanHistory)")]        
        public int Id { get; set; }

        /// <summary>
        /// <para type="description">History ID as it returned by /scans/:id endpoint (or Get-NessusScanHistory cmdlet).</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "The History Scan Id (returned by Get-NessusScanHistory)")]
        public int HistoryId { get; set; }

        /// <summary>
        /// <para type="description">Path to file where to save the exported repot. Subfolders will be created automatically if required.</para>
        /// <para type="description">If not specified, standard output will be used.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "Output file path")]
        public string OutFile { get; set; }

        /// <summary>
        /// <para type="description">Export format.</para>
        /// <para type="description">If not specified, Nessus XML format will be used.</para>
        /// </summary>
        [Parameter(HelpMessage = "Export Format: Nessus(default), CSV, HTML, PDF")]
        public ExportFormat Format { get; set; } = ExportFormat.Nessus;


        protected override void ProcessRecord(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            Stream outStream;

            var filePath = OutFile;
            if (!string.IsNullOrWhiteSpace(OutFile))
            {
                
                if (string.IsNullOrWhiteSpace(Path.GetExtension(OutFile)))
                    filePath = $"{OutFile}.{Enum.GetName(typeof(ExportFormat), Format).ToLowerInvariant()}";

                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                outStream = File.OpenWrite(filePath);
            }
            else 
            {
                outStream = new MemoryStream();            
            }
            try
            {
                using (outStream)
                {
                    nessusConnection.ExportAsync(Id, HistoryId, Format, outStream, cancellationToken).Wait(cancellationToken);
                    var ms = outStream as MemoryStream;
                    if (ms != null)
                    {
                        WriteObject(Encoding.UTF8.GetString(ms.ToArray()));
                    }
                    else
                    {
                        WriteVerbose($"Nessus scan saved into ${Path.GetFullPath(filePath)}");
                    }
                }
            }
            catch (AggregateException e)
            {
                WriteError(new ErrorRecord(e.Flatten().InnerException, string.Empty, ErrorCategory.ConnectionError, nessusConnection));
            }            
        }       
    }

}