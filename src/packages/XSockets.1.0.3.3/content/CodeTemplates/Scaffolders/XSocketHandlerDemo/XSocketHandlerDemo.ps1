[T4Scaffolding.Scaffolder(Description = "Enter a description of XSocketHandlerDemo here")][CmdletBinding()]
param(        	 
    [string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

$outputPath = "Example\ChatHandler"

Add-ProjectItemViaTemplate $outputPath -Template XSocketHandlerDemoTemplate `
	-Model @{ Dummy = "" } `
	-SuccessMessage "Added XSocketHandlerDemo output at {0}" `
	-TemplateFolders $TemplateFolders -Project XSocketHandler -CodeLanguage $CodeLanguage -Force:$Force