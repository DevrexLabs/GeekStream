param($installPath, $toolsPath, $package, $project)
#check version and edition
#if($DTE.Edition -eq "Express"){
#	Write-Host "Sorry, You cant use an Express verison with XSockets nuget package!"
#	Write-Host "Install XSockets.DevelopmentServer and XSockets.JsApi manually to get started!"
#	return
#}
#if($DTE.Version -ne "10.0"){
#	Write-Host "Sorry, you need to have Visual Studio 2010"
#	return	
#}

#install.ps1 v: 1.0 RC1
$defaultProject = Get-Project
$defaultNamespace = (Get-Project $defaultProject.Name).Properties.Item("DefaultNamespace").Value
$path = $defaultProject.FullName.Replace($defaultProject.Name + '.csproj','').Replace('\\','\')
$pluginPath = $path + "XSocketServerPlugins\"
$sln = [System.IO.Path]::GetFilename($dte.DTE.Solution.FullName)
$newProjPath = $dte.DTE.Solution.FullName.Replace($sln,'').Replace('\\','\')
$sln = Get-Interface $dte.Solution ([EnvDTE80.Solution2])
###################################
#Add XSocketHandler project	      #
###################################
if(($DTE.Solution.Projects | Select-Object -ExpandProperty Name) -notcontains 'XSocketHandler'){
Write-Host "Adding XSocketHandler project"
$templatePath = $sln.GetProjectTemplate("ClassLibrary.zip","CSharp")
$sln.AddFromTemplate($templatePath, $newProjPath+"XSocketHandler","XSocketHandler")
$file = Get-ProjectItem "Class1.cs" -Project XSocketHandler
$file.Remove()
}
# Get the current Pre Build Event cmd
(Get-Project XSocketHandler).ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value = "bin\Debug"

#Add refs
(Get-Project XSocketHandler).Object.References.Add("System.Componentmodel.Composition")

##############################################################
# Install Nuget Packages
##############################################################
#Add or update XSockets.External!
Write-Host (Get-Project XSocketHandler).Name Installing : XSockets.External -ForegroundColor DarkGreen
Install-Package XSockets.External -ProjectName (Get-Project XSocketHandler).Name

###################################
#End of Add XSocketHandler project#
###################################


###################################
#Setup post and pre build events  #
###################################

# Get the current Post Build Event cmd
$currentPostBuildCmd = (Get-Project XSocketHandler).Properties.Item("PostBuildEvent").Value

$postBuildAddCmd = "copy `"`$(TargetPath)`", `"`$(SolutionDir)"+$defaultProject.Name+"\XSockets\DevelopmentServer\XSocketServerPlugins\`""
#$postBuildAddCmd = "copy `"`$(TargetPath)`", `"`$($pluginPath)`""

# Append our post build command if it's not already there
if (!$currentPostBuildCmd.Contains($postBuildAddCmd)) {
    (Get-Project XSocketHandler).Properties.Item("PostBuildEvent").Value += $postBuildAddCmd
}

# Get the current Pre Build Event cmd
$currentPreBuildCmd = (Get-Project XSocketHandler).Properties.Item("PreBuildEvent").Value

$preBuildAddCmd = "IF NOT EXIST `"`$(SolutionDir)"+$defaultProject.Name+"\XSockets\DevelopmentServer\XSocketServerPlugins\`" mkdir `"`$(SolutionDir)"+$defaultProject.Name+"\XSockets\DevelopmentServer\XSocketServerPlugins\`""
#$preBuildAddCmd = "IF NOT EXIST `"`$pluginPath`" mkdir `"`$pluginPath`""

# Append our pre build command if it's not already there
if (!$currentPreBuildCmd.Contains($preBuildAddCmd)) {
    (Get-Project XSocketHandler).Properties.Item("PreBuildEvent").Value += $preBuildAddCmd
}

###################################
#End Setup post/pre build events  #
###################################