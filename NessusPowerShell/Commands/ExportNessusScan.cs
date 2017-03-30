using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Threading;
using NessusClient;
using NessusClient.Scans;

namespace NessusPowerShell.Commands
{
    [Cmdlet(VerbsData.Export, "NessusScan")]
    public class ExportNessusScan : NessusCmdletBase
    {

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        
        public int Id { get; set; }

        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true)]
        public int HistoryId { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public string OutFile { get; set; }

        [Parameter]
        public ExportFormat Format { get; set; } = ExportFormat.Nessus;


        protected override void ProcessRecord(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            Stream outStream;

            var filePath = OutFile;
            if (!string.IsNullOrWhiteSpace(OutFile))
            {
                
                if (string.IsNullOrWhiteSpace(Path.GetExtension(OutFile)))
                    filePath = $"{OutFile}.{Enum.GetName(typeof(ExportFormat), Format).ToLowerInvariant()}";
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