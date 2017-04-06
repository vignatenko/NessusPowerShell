using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using NessusClient;

namespace NessusPowerShell.Commands
{
    /// <summary>
    /// Base class for NEssus cmdlets. Adds support of logon to Nessus server
    /// </summary>
    public abstract class NessusCmdletBase: Cmdlet
    {
        /// <summary>
        /// <para type="description">Optional path to profile file created with New-NessusProfile -OutFile path-to-file.</para>
        /// <para type="description">If not specified, then default profile will be searched in these paths:</para>
        /// <para type="description">.\nessus.profile.txt</para>
        /// <para type="description">%appdata%\NessusPowerShell\Profiles\nessus.profile.txt</para>
        /// <para type="description">%appdata%\nessus.profile.txt</para>
        /// <para type="description">%userprofile%\Documents\NessusPowerShell\Profiles\nessus.profile.txt</para>
        /// <para type="description">%userprofile%\Documents\nessus.profile.txt</para>
        /// <para type="description">If profile cannot be loaded and -Profile parameter is not specified, then error will be reported</para>
        /// </summary>
        [Parameter(HelpMessage = "Path to profile file created with New-NessusProfile")]
        public string ProfileFile { get; set; }

        /// <summary>
        /// <para type="description">Optional profile data created with New-NessusProfile -InMemoryOnly.</para>
        /// <para type="description">Can be used for temporary, not persistent profiles.</para>        
        /// </summary>
        [Parameter(HelpMessage = "In Memory profile created with New-NessusProfile -InMemoryOnly")]
        public NessusProfile Profile { get; set; }

        private INessusConnection _nessusConnection;
        private CancellationTokenSource _tokenSource;        

        protected override void BeginProcessing()
        {

            _tokenSource = new CancellationTokenSource();
            if (Profile == null)
            {
                
                const string defaultFileName = "nessus.profile.txt";
                var paths = new[]
                {
                    Path.GetFullPath(defaultFileName),

                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "NessusPowerShell",
                        "Profiles",
                        defaultFileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), defaultFileName),

                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "NessusPowerShell",
                        "Profiles",
                        defaultFileName),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), defaultFileName),

                };
                var profile = paths.Select(x =>
                {
                    try
                    {
                        return new {file = x, profile = NessusProfile.FromProtectedString(File.ReadAllText(x))};
                    }
                    catch
                    {
                        return new {file = x, profile = (NessusProfile) null};
                    }

                }).FirstOrDefault(x => x.profile != null);

                if (profile == null)
                {                    
                    throw new FileNotFoundException(
                        $"Profile file cannot be found. Please use -{nameof(ProfileFile)} parameter or create it using New-NessusProfile -OutFile <path to file> in one of default locations:{Environment.NewLine}{string.Join(Environment.NewLine, paths)}");
                }

                ProfileFile = profile.file;
                Profile = profile.profile;

                WriteVerbose($"Using {Path.GetFullPath(ProfileFile)} profile.");
                
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