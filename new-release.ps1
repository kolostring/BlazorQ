# 1. Clean old builds
Remove-Item -Recurse -Force ./bin/Release

# 2. Pack with a unique timestamp version to bypass cache
$ver = "0.1.0-dev." + (Get-Date -Format "yyMMddHHmm")
dotnet pack -c Release /p:Version=$ver

# 3. Push to your local folder
dotnet nuget push bin/Release/*.nupkg --source C:\NugetTest