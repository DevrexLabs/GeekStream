[T4Scaffolding.Scaffolder(Description = "Enter a description of XDevServerConfig here")][CmdletBinding()]
param(        
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

$defaultProject = Get-Project
$defaultNamespace = (Get-Project $defaultProject.Name).Properties.Item("DefaultNamespace").Value
$portNr = (Get-Project $defaultProject.Name).Properties.Item("WebApplication.DevelopmentServerPort").Value
$iisUrl = (Get-Project $defaultProject.Name).Properties.Item("WebApplication.IISUrl").Value
$outputPath = "XSockets\DevelopmentServer\XSockets.DevelopmentServer.Console.exe"

Add-ProjectItemViaTemplate $outputPath -Template XDevServerConfigTemplate `
	-Model @{ Port = $portNr; IISUrl = $iisUrl } `
	-SuccessMessage "Added XDevServerConfig output at {0}" `
	-TemplateFolders $TemplateFolders -Project $Project -CodeLanguage $CodeLanguage -Force:$Force