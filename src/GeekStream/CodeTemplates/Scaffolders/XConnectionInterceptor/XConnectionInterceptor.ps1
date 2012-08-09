[T4Scaffolding.Scaffolder(Description = "Creates a new ConnectionInterceptor.")][CmdletBinding()]
param(        
	[parameter(Mandatory = $true, ValueFromPipelineByPropertyName = $true)][string]$InterceptorName,	
	[parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)][string]$ProjectName = "",	 
	[string]$Project,
	[string]$CodeLanguage,
	[string[]]$TemplateFolders,
	[switch]$Force = $false
)

#
#Use default proj if not set
#
if($ProjectName -eq ""){ $ProjectName = 'XSocketHandler' }

$currentProj = Get-Project $Project
$defaultProjectName = [System.IO.Path]::GetFilename($currentProj.FullName)
$refPath =  $currentProj.FullName.Replace($defaultProjectName,'')

#
#Get PluginPath to set in post build event
#
$sln = [System.IO.Path]::GetFilename($dte.DTE.Solution.FullName)
$path = $dte.DTE.Solution.FullName.Replace($sln,'').Replace('\\','\')
$pluginPath = $path + "XSocketServerPlugins"
$sln = Get-Interface $dte.Solution ([EnvDTE80.Solution2])

#
#Add new project if it does not exist
#
if(($DTE.Solution.Projects | Select-Object -ExpandProperty Name) -notcontains $ProjectName){
#Write-Host "Adding new project"
$templatePath = $sln.GetProjectTemplate("ClassLibrary.zip","CSharp")
$sln.AddFromTemplate($templatePath, $path+$ProjectName,$ProjectName)
$file = Get-ProjectItem "Class1.cs" -Project $ProjectName
$file.Remove()

#
#Add refs
#
(Get-Project $ProjectName).Object.References.Add("System.Componentmodel.Composition")
(Get-Project $ProjectName).Object.References.Add("$($refPath)Lib\XSockets.Common.dll")
(Get-Project $ProjectName).Object.References.Add("$($refPath)Lib\XSockets.Core.dll")
(Get-Project $ProjectName).Object.References.Add("$($refPath)Lib\XSockets.MaxiServer.dll")

#
#Setup post and pre build events
#

# Get the current Post Build Event cmd
$currentPostBuildCmd = (Get-Project $ProjectName).Properties.Item("PostBuildEvent").Value

$postBuildAddCmd = "copy `"`$(TargetPath)`", `"`$(SolutionDir)XSocketServerPlugins\`""

# Append our post build command if it's not already there
if (!$currentPostBuildCmd.Contains($postBuildAddCmd)) {
    (Get-Project $ProjectName).Properties.Item("PostBuildEvent").Value += $postBuildAddCmd
}

# Get the current Pre Build Event cmd
$currentPreBuildCmd = (Get-Project $ProjectName).Properties.Item("PreBuildEvent").Value

$preBuildAddCmd = "IF NOT EXIST `"`$(SolutionDir)XSocketServerPlugins\`" mkdir `"`$(SolutionDir)XSocketServerPlugins\`""

# Append our pre build command if it's not already there
if (!$currentPreBuildCmd.Contains($preBuildAddCmd)) {
    (Get-Project $ProjectName).Properties.Item("PreBuildEvent").Value += $preBuildAddCmd
}

#End Setup post and pre build events
}

$namespace = (Get-Project $ProjectName).Properties.Item("DefaultNamespace").Value

$outputPath = $InterceptorName
#
#Create interceptor
#
#Write-Host $namespace
#Write-Host $InterceptorName
#Write-Host $outputPath

Add-ProjectItemViaTemplate $outputPath -Template XConnectionInterceptorTemplate `
	-Model @{ Namespace = $namespace; InterceptorName = $InterceptorName } `
	-SuccessMessage "Added XConnectionInterceptor output at {0}" `
	-TemplateFolders $TemplateFolders -Project $ProjectName -CodeLanguage $CodeLanguage -Force:$Force
