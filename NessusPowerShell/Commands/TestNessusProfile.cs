using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using NessusClient;

namespace NessusPowerShell.Commands
{
    /// <summary>
    /// <para type="synopsis">Validates Nessus connection profile created by New-NessusProfile and outputs profile information.</para>    
    /// </summary>
    /// <example>
    /// <para>Do validate profile in current folder and test if credentials are valid:</para>
    /// <para>Test-NessusProfile ./nessus-profile.txt -TryLoginToServer</para>
    /// </example>
    /// <example>
    /// <para>Do validate in-memory profile:</para>
    /// <para>Test-NessusProfile -Profile $profile</para>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "NessusProfile"), OutputType(typeof(PrpofileValidationResult))]
    public class TestNessusProfile: Cmdlet
    {


        /// <summary>
        /// <para type="description">Path to profile file created with New-NessusProfile.</para>        
        /// <para type="description">At least on parameter -ProfileFile or -Profile must be specified</para>   
        /// </summary>

        [Parameter( Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true, ParameterSetName = "file")]
        public string ProfileFile { get; set; }

        /// <summary>
        /// <para type="description">Optional profile data created with New-NessusProfile -InMemoryOnly.</para>        
        /// <para type="description">At least on parameter -ProfileFile or -Profile must be specified</para>        
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, Mandatory = true, ParameterSetName = "memory")]
        public NessusProfile Profile { get; set; }

        /// <summary>
        /// <para type="description">If specified, cmdlet will try to logon to Nessus server.</para>                
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public  SwitchParameter TryLoginToServer { get; set; }


        protected override void ProcessRecord()
        {
            try
            {


                var profile = Profile ?? NessusProfile.FromProtectedString(File.ReadAllText(ProfileFile));
                if (TryLoginToServer)
                {
                    using (
                        var c = new NessusConnection(profile.Server, profile.Port, profile.UserName, profile.Password))
                    {
                        try
                        {
                            c.OpenAsync(new CancellationToken()).Wait();
                            WriteObject(new PrpofileValidationResult
                            {
                                Profile = ProfileFile,
                                Server = $"{profile.Server}:{profile.Port}",
                                Status = "Login Successful"
                            });
                        }
                        catch (AggregateException e)
                        {

                            WriteObject(new PrpofileValidationResult
                            {
                                Profile = ProfileFile,
                                Server = $"{profile.Server}:{profile.Port}",
                                Status = $"{e.Flatten().InnerException.Message}"
                            });
                        }
                    }
                }
                else
                {
                    WriteObject(new PrpofileValidationResult
                    {
                        Profile = ProfileFile,
                        Server = $"{profile.Server}:{profile.Port}",
                        Status = "OK"
                    });
                }
            }
            catch (Exception e)
            {
                WriteObject(new PrpofileValidationResult
                {
                    Profile = ProfileFile,
                    Server = "Unable to read",
                    Status = e.Message
                });
            }

        }


    }
    class PrpofileValidationResult
    {
        public string Profile { get; set; }
        public string Server { get; set; }
        public string Status { get; set; }
    }
}