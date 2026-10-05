param([string]$BaseUrl = 'http://localhost:5265')
$ErrorActionPreference = 'Stop'
$homeResponse = Invoke-WebRequest $BaseUrl
if ($homeResponse.StatusCode -ne 200 -or $homeResponse.Content -notmatch 'Teleport me' -or $homeResponse.Content -match 'name="ZipCode"') { throw 'Home page failed' }
$previous = ''
$ids = [System.Collections.Generic.HashSet[string]]::new()
foreach ($iteration in 1..50) {
    $response = Invoke-WebRequest "$BaseUrl/Teleport?handler=Random&exclude=$([Uri]::EscapeDataString($previous))"
    $destination = $response.Content | ConvertFrom-Json
    if ($destination.id -eq $previous) { throw 'Consecutive destination repeated' }
    if ($destination.streetViewUrl -notmatch '^https://www.google.com/maps/@\?api=1&map_action=pano&viewpoint=' -or $destination.streetViewUrl -match 'key=') { throw 'Invalid key-free Street View URL' }
    if ($response.Headers['Cache-Control'] -notmatch 'no-store') { throw 'Random response may be cached' }
    $previous = $destination.id
    [void]$ids.Add($previous)
}
if ($ids.Count -lt 2) { throw 'Random selection is stuck' }
$details = Invoke-WebRequest "$BaseUrl/Teleport?id=$([Uri]::EscapeDataString($previous))"
if ($details.Content -notmatch 'Enter Street View' -or $details.Content -notmatch 'Find this Waffle House') { throw 'Destination links missing' }
$invalid = Invoke-WebRequest "$BaseUrl/Teleport?id=missing" -SkipHttpErrorCheck
if ($invalid.StatusCode -ne 404) { throw 'Unknown destination should be 404' }
$launch = Invoke-WebRequest "$BaseUrl/Teleport?handler=Launch" -MaximumRedirection 0 -SkipHttpErrorCheck -ErrorAction SilentlyContinue
if ($launch.StatusCode -ne 302 -or $launch.Headers.Location -notmatch '^https://www.google.com/maps/') { throw 'No-JavaScript launch failed' }
if ((Invoke-WebRequest "$BaseUrl/Privacy").Content -notmatch 'OpenStreetMap contributors') { throw 'Attribution missing' }
Write-Output "PASS: homepage, 50 random jumps ($($ids.Count) distinct), repeat exclusion, no-store headers, destination links, invalid ID, key-free redirect, and attribution."
