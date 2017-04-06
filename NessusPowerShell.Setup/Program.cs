using System;
using System.Linq;
using WixSharp;

namespace NessusPowerShell.Setup
{
    class Program
    {
        private static readonly string[] Files =
       {
            "NessusClient.dll",
            "NessusPowerShell.dll",
            "NessusPowerShell.dll-Help.xml"
        };

        static void Main()
        {
#if DEBUG
            var files = Files.Select(x => new WixSharp.File(@"bin\debug\" + x)).ToArray();
#else
            var files = Files.Select(x => new WixSharp.File(@"bin\release\" + x)).ToArray();
#endif
            var project = new Project("Nessus PowerShell Module",
                new Dir(@"%ProgramFiles%\WindowsPowerShell\Modules\NessusPowerShell",
                    files))
            {
                GUID = new Guid("6fe30b47-2577-43ad-9095-1861ba25889b"),
                UI = WUI.WixUI_InstallDir,
                ControlPanelInfo = {Manufacturer = "Vladyslav Ignatenko"},
                Platform = Platform.x64,
                Version = new Version(1, 0, 2, 0),                
                LicenceFile = "license.rtf"
                
            };

            


            project.BuildMsi();
        }
    }
}