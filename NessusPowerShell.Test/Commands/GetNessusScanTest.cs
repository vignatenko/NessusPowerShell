using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using NessusPowerShell.Commands;
using NUnit.Framework;

namespace NessusPowerShell.Test.Commands
{
    [TestFixture]
    public class GetNessusScanTest
    {
        private RunspaceConfiguration _config;

        [SetUp]
        public void Initialize()
        {
            _config = RunspaceConfiguration.Create();
            
            _config.Cmdlets.Append(new CmdletConfigurationEntry(
                "Get-NessusScan",
                typeof(GetNessusScan),
                null));
        }
        

        [Test]
        
        public void GetScanList()
        {
            using (var rs = RunspaceFactory.CreateRunspace(_config))
            {
                rs.Open();
                using (var p = rs.CreatePipeline("$passwd = ConvertTo-SecureString -String \"1\" -AsPlainText -Force; " +
                                                 "$cred = New-Object -TypeName \"System.Management.Automation.PSCredential\" -ArgumentList admin, $passwd;" +
                                                 "$profile = New-Object -TypeName \"NessusPowerShell.NessusProfile\"; " +
                                                 "$profile.Server = \"w2012r2-dc\";" +
                                                 "$profile.UserName = $cred.UserName;" +
                                                 "$profile.Password = $cred.Password;" +
                                                 "get-nessusscan -profile $profile"))
                {
                    var objs = p.Invoke();
                    
                }
            }
        }
       
    }
}
