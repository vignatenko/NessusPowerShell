using System;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using NessusClient;
using NessusClient.Scans;

namespace NessusPowerShell.Commands
{
    /// <summary>
    /// <para type="synopsis">Returns scan history for all scans or for particular scan.</para>
    /// </summary>
    /// <example>
    /// <para>Get histories of all scans:</para>    
    /// <para>Get-NessusScanHistory</para>
    /// </example>
    /// <example>
    /// <para>Get history of scan with ID = 10:</para> 
    /// <para>Get-NessusScanHistory -Id 10</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "NessusScanHistory"), OutputType(typeof(ScanHistory))]
    public class GetNessusScanHistory : NessusCmdletBase
    {
        /// <summary>
        /// <para type="description">Optional scan's ID.</para>
        /// </summary>
        [Parameter( ValueFromPipelineByPropertyName = true, HelpMessage = "The Scan Id from Get-NessusScan")]
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