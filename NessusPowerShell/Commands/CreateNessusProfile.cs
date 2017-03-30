using System.IO;
using System.Management.Automation;

namespace NessusPowerShell.Commands
{
    [Cmdlet(VerbsCommon.New, "NessusProfile")]
    public class NewNessusProfile: Cmdlet
    {
        [Parameter( Mandatory = true, ValueFromPipelineByPropertyName = true)]        
        public string Server { get; set; }

        [Parameter( ValueFromPipelineByPropertyName = true)]        
        public int Port { get; set; } = 8834;

        

        [Parameter( ValueFromPipelineByPropertyName = true)]
        public string OutFile { get; set; }

        [Parameter(Mandatory = true)]
        public PSCredential Credential { get; set; }



        protected override void ProcessRecord()
        {                        
            var profile =
                new NessusProfile {Server = Server, UserName = Credential.UserName, Port = Port, Password = Credential.Password}
                    .ToProtectedString();

            NessusProfile.FromProtectedString(profile);

            if (!string.IsNullOrWhiteSpace(OutFile))
            {                
                File.WriteAllText(OutFile, profile);

                WriteVerbose($"Profile successfully saved to {Path.GetFullPath(OutFile)}");
            }
            else
            {
                WriteObject(profile);
            }
            
        }

       
    }
}