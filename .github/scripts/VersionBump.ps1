# VersionBump.ps1
param(
    [string]$Mode = "minor" # minor or major
)

# Путь до файла
$propsPath = "Directory.Build.props"

# Загружаем XML
[xml]$xml = Get-Content $propsPath

# Парсим текущую версию
$currentVersion = $xml.Project.PropertyGroup.Version
Write-Host "Current Version: $currentVersion"

$parts = $currentVersion.Split('.')

$major = [int]$parts[0]
$minor = [int]$parts[1]
$patch = [int]$parts[2]

if ($Mode -eq "minor") {
    $minor += 1
    $patch = 0
}
elseif ($Mode -eq "major") {
    $major += 1
    $minor = 0
    $patch = 0
}
else {
    Write-Error "Unknown mode: $Mode"
    exit 1
}

$newVersion = "$major.$minor.$patch"
Write-Host "New Version: $newVersion"

# Обновляем версию
$xml.Project.PropertyGroup.Version = $newVersion

# Сохраняем изменения
$xml.Save($propsPath)
