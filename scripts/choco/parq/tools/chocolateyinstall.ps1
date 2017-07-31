$toolsDirectory = $(Split-Path -parent $MyInvocation.MyCommand.Definition)
   
$pathToArchive = [System.IO.Path]::Combine($toolsDirectory, "parqInstall.zip")
Get-ChocolateyUnzip $pathToArchive $pwd
Install-BinFile -Name parq -path "$toolsDirectory\\parq.exe"