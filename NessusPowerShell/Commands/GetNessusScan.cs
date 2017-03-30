using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using NessusClient;
using NessusClient.Scans;

namespace NessusPowerShell.Commands
{
    [Cmdlet(VerbsCommon.Get, "NessusScan"), OutputType(typeof(Scan))]
    public class GetNessusScan: NessusCmdletBase
    {        
        private  IEnumerable<Scan> _scans;        

        protected override void InitialLoad(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            WriteVerbose("Getting scans from the server...");

            var task = nessusConnection.GetScansAsync(cancellationToken);
            task.Wait(cancellationToken);
            _scans = task.Result.OrderBy(x => x.Name).ToList();

            WriteVerbose($"{_scans.Count()} scan(s) found");
        }

        protected override void ProcessRecord(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            WriteObject(_scans, true);
        }
        
    }
}