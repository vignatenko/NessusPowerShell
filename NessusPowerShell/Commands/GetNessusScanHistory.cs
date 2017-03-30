using System;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using NessusClient;
using NessusClient.Scans;

namespace NessusPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "NessusScanHistory"), OutputType(typeof(ScanHistory))]
    public class GetNessusScanHistory : NessusCmdletBase
    {
        [Parameter( ValueFromPipelineByPropertyName = true, HelpMessage = "The Scan Id from Get-NessusScan or Get-NessusScanHistory")]
        public int Id { get; set; }

        

        protected override void ProcessRecord(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            
            WriteVerbose("Getting scan history records from the server...");

            try
            {
                var task = Id == default(int) ? nessusConnection.GetAllScanHistoriesAsync(cancellationToken) : nessusConnection.GetScanHistoryAsync(Id, cancellationToken);

                task.Wait(cancellationToken);

                var historyRecords = task.Result.OrderByDescending(x => x.LastUpdateDate).ToList();

                WriteVerbose($"{historyRecords.Count} history record(s) found");

                WriteObject(historyRecords, true);
            }
            catch (AggregateException e)
            {
                WriteError(new ErrorRecord(e.Flatten().InnerException, string.Empty, ErrorCategory.ConnectionError, nessusConnection));
            }
            
            
        }
    }
}