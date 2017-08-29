using System;
using System.Collections.Generic;
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
                if (string.IsNullOrWhiteSpace(ProfileFile))
                {
                    var defaultProfile = FindProfileInDefaultLocations();

                    if (defaultProfile == null)
                    {
                        throw new FileNotFoundException(
                            $@"Profile file cannot be found. Please use -{
                                    nameof(ProfileFile)
                                } parameter or create it using New-NessusProfile -OutFile <path to file> in one of default locations:{
                                    Environment.NewLine
                                }{string.Join(Environment.NewLine, DefaultProfileLocations)}");
                    }

                    ProfileFile = defaultProfile.Path;
                    Profile = defaultProfile.Profile;
                }
                else
                {
                    ProfileFile = Environment.ExpandEnvironmentVariables(ProfileFile);
                    Profile = NessusProfile.FromProtectedString(File.ReadAllText(ProfileFile));
                }

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

        private static DefaultProfileInfo FindProfileInDefaultLocations()
        {
            var paths = DefaultProfileLocations;
            var profile = paths.Select(x =>
            {
                try
                {
                    return new DefaultProfileInfo { Path = x, Profile = NessusProfile.FromProtectedString(File.ReadAllText(x))};
                }
                catch
                {
                    return new DefaultProfileInfo() ;
                }
            }).FirstOrDefault(x => x.Profile != null);
            return profile;
        }

        private static IEnumerable<string> DefaultProfileLocations
        {
            get
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
                return paths;
            }
        }
        private class DefaultProfileInfo
        {
            public string Path { get; set; }
            public NessusProfile Profile { get; set; }
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