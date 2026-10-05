param(
    [string]$SourceFile,
    [string]$Endpoint = 'https://maps.mail.ru/osm/tools/overpass/api/interpreter'
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$dataDirectory = Join-Path $repo 'Alien.WaffleHouseTeleporter/Data'
if (-not $SourceFile) {
    $SourceFile = Join-Path $dataDirectory 'overpass-source.json'
    $query = '[out:json][timeout:30][maxsize:16777216];nwr["brand"="Waffle House"];out center tags;'
    $uri = $Endpoint + '?data=' + [Uri]::EscapeDataString($query)
    Invoke-WebRequest -Uri $uri -UserAgent 'Alien.WaffleHouseTeleporter/1.0 (local catalog refresh)' -TimeoutSec 60 -OutFile $SourceFile
}
$source = Get-Content -LiteralPath $SourceFile -Raw | ConvertFrom-Json
if ($source.remark) { throw $source.remark }
$locations = [System.Collections.Generic.List[object]]::new()
foreach ($element in ($source.elements | Sort-Object type,id)) {
    $tags = $element.tags
    if ($tags.amenity -ne 'restaurant' -or $tags.brand -ne 'Waffle House') { continue }
    $latitude = if ($element.type -eq 'node') { $element.lat } else { $element.center.lat }
    $longitude = if ($element.type -eq 'node') { $element.lon } else { $element.center.lon }
    if ($null -eq $latitude -or $null -eq $longitude -or $latitude -lt 24 -or $latitude -gt 50 -or $longitude -lt -125 -or $longitude -gt -66) { continue }
    # Collapse node/building representations of the same restaurant, within about 30 m.
    $duplicate = $false
    foreach ($existing in $locations) {
        if ([Math]::Abs($existing.latitude - $latitude) -lt 0.0003 -and [Math]::Abs($existing.longitude - $longitude) -lt 0.0003) { $duplicate = $true; break }
    }
    if ($duplicate) { continue }
    $street = (@($tags.'addr:housenumber', $tags.'addr:street') | Where-Object { $_ }) -join ' '
    $region = (@($tags.'addr:city', $tags.'addr:state', $tags.'addr:postcode') | Where-Object { $_ }) -join ', '
    $address = (@($street, $region) | Where-Object { $_ }) -join ', '
    if (-not $address) { $address = 'Address not recorded — use the map to find this location' }
    $locations.Add([ordered]@{
        id = "$($element.type)/$($element.id)"
        name = if ($tags.name) { $tags.name } else { 'Waffle House' }
        address = $address
        latitude = [double]$latitude
        longitude = [double]$longitude
    })
}
if ($locations.Count -lt 100) { throw 'Unexpectedly small catalog; existing catalog was not replaced.' }
$catalog = [ordered]@{
    source = 'https://www.openstreetmap.org'
    license = 'ODbL-1.0'
    retrievedAt = [DateTime]::UtcNow.ToString('yyyy-MM-dd')
    osmTimestamp = $source.osm3s.timestamp_osm_base
    locations = @($locations.ToArray())
}
$catalog | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $dataDirectory 'waffle-houses.json') -Encoding utf8
Write-Output "Bundled $($locations.Count) Waffle House destinations."
