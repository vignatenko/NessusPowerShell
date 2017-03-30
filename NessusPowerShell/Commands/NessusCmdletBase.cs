using System;
using System.IO;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using NessusClient;

namespace NessusPowerShell.Commands
{
    public abstract class NessusCmdletBase: Cmdlet
    {
        
        [Parameter(HelpMessage = "Path to profile file created with New-NessusProfile")]
        public string ProfileFile { get; set; }

        [Parameter]
        public NessusProfile Profile { get; set; }

        private INessusConnection _nessusConnection;
        private CancellationTokenSource _tokenSource;        

        protected override void BeginProcessing()
        {

            _tokenSource = new CancellationTokenSource();
            if (Profile == null)
            {
                if (string.IsNullOrWhiteSpace(ProfileFile))
                {
                    ProfileFile = ".\\nessus.profile.txt";
                }
                if (!File.Exists(ProfileFile))
                {
                    throw new FileNotFoundException(
                        $"Profile file {Path.GetFullPath(ProfileFile)} cannot be found. Please use -{nameof(ProfileFile)} parameter or create {ProfileFile} using New-NessusProfile");
                }
                WriteVerbose($"Using {Path.GetFullPath(ProfileFile)} profile.");
                Profile = NessusProfile.FromProtectedString(File.ReadAllText(ProfileFile));
            }

            _nessusConnection = new NessusConnection(Profile.Server, Profile.Port, Profile.UserName, Profile.Password);
            try
            {
                WriteVerbose($"Connecting to Nessus server at {Profile.Server}:{Profile.Port}");

                _nessusConnection.OpenAsync(_tokenSource.Token).Wait(_tokenSource.Token);

                InitialLoad(_nessusConnection, _tokenSource.Token);


            }
            catch (AggregateException e)
            {
                var err = e.Flatten().InnerException;

                WriteError(new ErrorRecord(err, string.Empty, ErrorCategory.ConnectionError, _nessusConnection));
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, string.Empty, ErrorCategory.ConnectionError, _nessusConnection));
            }

        }

        


        protected override void EndProcessing()
        {
            CloseConnectionAsync().Wait(TimeSpan.FromSeconds(30));
        }
        
        protected override  void StopProcessing()
        {
            CloseConnectionAsync().Wait(TimeSpan.FromSeconds(1));
        }

        protected override void ProcessRecord()
        {
            ProcessRecord(_nessusConnection, _tokenSource.Token);
        }

        protected virtual void InitialLoad(INessusConnection nessusConnection, CancellationToken cancellationToken)
        {
            
        }
        protected abstract void ProcessRecord(INessusConnection nessusConnection, CancellationToken cancellationToken);

        private async Task CloseConnectionAsync()
        {
            if (!_tokenSource.IsCancellationRequested)
                _tokenSource.Cancel();
            await _nessusConnection.CloseAsync(CancellationToken.None);
        }

    }
}