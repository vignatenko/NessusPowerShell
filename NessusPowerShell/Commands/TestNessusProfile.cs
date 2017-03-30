using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using NessusClient;

namespace NessusPowerShell.Commands
{
    [Cmdlet(VerbsDiagnostic.Test, "NessusProfile"), OutputType(typeof(PrpofileValidationResult))]
    public class TestNessusProfile: Cmdlet
    {
        
        

        [Parameter( Position = 0, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string ProfileFile { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true)]
        public  SwitchParameter TryLoginToServer { get; set; }





        protected override void ProcessRecord()
        {
            try
            {


                var profile = NessusProfile.FromProtectedString(File.ReadAllText(ProfileFile));
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
                    Server = $"Unable to read",
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