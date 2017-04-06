# Nessus PowerShell Module

PowerShell module for [Tenable Nessus 6.x](http://www.tenable.com/products/nessus-vulnerability-scanner).

Cmdlets in this initial release:
* [New-NessusProfile](#New-NessusProfile)
* [Test-NessusProfile](#Test-NessusProfile)
* [Get-NessusScanHistory](#Get-NessusScanHistory)
* [Get-NessusVulnerability](#Get-NessusVulnerability)
* [Export-NessusScan](#Export-NessusScan)
* Get-NessusScan (```Get-Help Get-NessusScan -Full```)


Fill free to post feature requests.


### Installing

Download [Nessus PowerShell Module.msi](https://github.com/vignatenko/NessusPowerShell/releases/download/v1.0/Nessus.PowerShell.Module.msi) and install it.



## Getting Started

Every cmdlet that communicates with Nessus server supports at least two common mutually exclusive parameters:
```-ProfileFile``` or ```-Profile```. 

The ```-ProfileFile``` is a path to Nessus connection profile. 
If this parameter is not specified and ```-Profile``` is omited, then cmdlets do search default(```nessus.profile.txt```) connection profile in these pathes:
 * ```.\nessus.profile.txt```
 * ```%AppData%\NessusPowerShell\Profiles\nessus.profile.txt```
 * ```%AppData%\nessus.profile.txt```
 * ```%UserProfile%\Documents\NessusPowerShell\Profiles\nessus.profile.txt```
 * ```%UserProfile%\Documents\nessus.profile.txt```

 If profile cannot be loaded from file and -Profile parameter is omited, then error will be reported.
 
 Nessus connection profile file stores connection credentials in [encrypted](https://msdn.microsoft.com/en-us/library/2fh8203k(v=vs.110).aspx) form. 

 <a name="New-NessusProfile"></a>
 ### 1. New-NessusProfile: Create Nessus Connection Profile 
 To create profile you can use this command(you will be promted for credentials):
 ```PowerShell
 PS> New-NessusProfile -OutFile %AppData%\NessusPowerShell\Profiles\nessus.profile.txt
 ```
 It saves profile in default location. 

 For additioanl options:
 ```PowerShell
 PS> Get-Help New-NessusProfile -Full
 ```

 <a name="Test-NessusProfile"></a>
  ### 2. Test-NessusProfile: Validate Nessus Connection Profile 
 To validate profile you can use this command(you will be promted for credentials):
 ```PowerShell
 PS> Test-NessusProfile -ProfileFile %AppData%\NessusPowerShell\Profiles\nessus.profile.txt -TryLoginToServer
 ```
 It validates profile file and tries to log in to Nessus server using credentials stored in the profile. 

 For additioanl options:
 ```PowerShell
 PS> Get-Help Test-NessusProfile -Full
 ```


 ###### NOTE:
 Since profile is created and can be loaded from one of default locations it is not neccesary to specify ```-ProfileFile``` parameter for other cmdlets.

## Examples  
<a name="Get-NessusScanHistory"></a>
#### 1. Get-NessusScanHistory

Get List of All Scans and their histories

```PowerShell
 PS> Get-NessusScanHistory
 ```
Outputs list into standard output.  

For additioanl options:
```PowerShell
PS> Get-Help Get-NessusScanHistory -Full
```
<a name="Get-NessusVulnerability"></a>
#### 2. Get-NessusVulnerability

Get All Vulnerabilities for past 10 days:

```PowerShell
 PS> Get-NessusScanHistory 
 | where {$_.LastUpdateDate -GT [DateTimeOffset]::UtcNow.AddDays(-10)} 
 | Get-NessusVulnerability
 ```

For additioanl options:
```PowerShell
PS> Get-Help Get-NessusVulnerability -Full
```
<a name="Export-NessusScan"></a>
#### 3. Export-NessusScan

Export all scans for past 10 days into HTML format. File name is constructed from scan name and scan date. 

```PowerShell
 PS> Get-NessusScanHistory
        | where {$_.LastUpdateDate -GT [DateTimeOffset]::UtcNow.AddDays(-10)}
        | select Id,HistoryId, @{Name="OutFile"; Expression={"{0}-{1:yyyyMMddHHmm}" -f($_.Name, $_.LastUpdateDate.ToLocalTime())}}
        | Export-NessusScan -Format Html
 ```

For additioanl options:
```PowerShell
PS> Get-Help Export-NessusScan -Full
```

## Built With
* [.NET Nessus Client](https://github.com/vignatenko/NessusClient) - Nessus API
* [WixSharp](https://github.com/oleg-shilo/wixsharp) - Installer
* [XmlDoc2CmdletDoc](https://github.com/red-gate/XmlDoc2CmdletDoc) - Help

## Authors

* Vlad Ignatenko - *Initial work*

See also the list of [contributors](https://github.com/vignatenko/NessusClient/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE) file for details

