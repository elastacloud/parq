$toolsDirectory = $(Split-Path -parent $MyInvocation.MyCommand.Definition)

Set-Location $toolsDirectory
Add-Type -assembly "System.IO.Compression.FileSystem"
    
$pathToArchive = [System.IO.Path]::Combine($toolsDirectory, "parqInstall.zip")
Get-ChocolateyUnzip $pathToArchive $pwd
Install-BinFile -Name parq -path "$toolsDirectory\\parq.exe"