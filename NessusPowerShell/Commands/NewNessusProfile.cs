using System.IO;
using System.Management.Automation;

namespace NessusPowerShell.Commands
{
    /// <summary>
    /// <para type="synopsis">Creates connection profile to Nessus server and saves it in encrypted file or in memory.</para>
    /// <para type="description">In order to get list of vulnerabilities the cmdlet does export, download and parse of downloaded file.</para>    
    /// </summary>
    /// <example>
    /// <para>Create new profile in current folder:</para>
    /// <para>New-NessusProfile -OutFile ./nessus-profile.txt</para>
    /// </example>
    /// <example>
    /// <para>Create new profile in memory:</para>
    /// <para>$profile = New-NessusProfile -InMemoryOnly</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "NessusProfile")]
    public class NewNessusProfile: Cmdlet
    {
        /// <summary>        
        /// <para type="description">Host name or IP of Nessus Server</para>    
        /// </summary>
        [Parameter( Mandatory = true, ValueFromPipelineByPropertyName = true)]        
        public string Server { get; set; }

        /// <summary>        
        /// <para type="description">Optional port. Default value is 8834</para>    
        /// </summary>
        [Parameter( ValueFromPipelineByPropertyName = true)]        
        public int Port { get; set; } = 8834;


        /// <summary>        
        /// <para type="description">Path to file where save profile </para>    
        /// </summary>
        [Parameter(ParameterSetName = "file")]
        public string OutFile { get; set; }

        /// <summary>        
        /// <para type="description">User name and password. If not specified explicitelly the UI dialog will be shown.</para>    
        /// </summary>
        [Parameter(Mandatory = true)]
        public PSCredential Credential { get; set; }

        /// <summary>        
        /// <para type="description">Create profile only in memory.</para>    
        /// <para type="description">Can be used for temporary, not persistent profiles.</para>        
        /// </summary>
        [Parameter(ParameterSetName = "memory")]
        public SwitchParameter InMemoryOnly { get; set; }


        protected override void ProcessRecord()
        {
            var profile = new NessusProfile
            {
                Server = Server,
                UserName = Credential.UserName,
                Port = Port,
                Password = Credential.Password
            };
            if (InMemoryOnly)
            {
                WriteObject(profile);
                return;
            }

            var protectedProfile = profile.ToProtectedString();

            NessusProfile.FromProtectedString(protectedProfile);

            if (!string.IsNullOrWhiteSpace(OutFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

                File.WriteAllText(OutFile, protectedProfile);

                WriteVerbose($"Profile successfully saved to {Path.GetFullPath(OutFile)}");
            }
            else
            {
                WriteObject(protectedProfile);
            }

        }


    }
}