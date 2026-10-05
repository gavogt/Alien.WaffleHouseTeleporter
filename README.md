# Waffle House Teleporter

A .NET 10 Razor Pages app: click **Teleport me** to open Street View near a random Waffle House. The portal stays open in your original tab for the next jump.

## Run

Install the .NET 10 SDK, then from this repository:

```powershell
dotnet run --project Alien.WaffleHouseTeleporter --launch-profile http
```

Open http://localhost:5265. No API key, account, ZIP code, or Google billing setup is needed. JavaScript provides the destination card and repeat-jump behavior; without JavaScript the form still redirects a new tab to Street View. If a popup is blocked, use the destination card's Street View link.

The bundled catalog is read once at startup. Random selection happens locally and excludes the immediately previous destination when supplied. There are no Google Places or geocoding calls. An internet connection is required to view Google Maps imagery, but not to select destinations.

## Optional Street View inside the app

Configure a Google **Maps Embed API** key using environment variable `GoogleMaps__ApiKey`, or `GoogleMaps:ApiKey` in local configuration. The app then opens a Street View iframe in its destination panel. Restrict the key to Maps Embed API and your site's HTTP referrers. Browser embed keys are visible to visitors. Maps Embed usage is currently free; follow Google's project setup and billing requirements at https://developers.google.com/maps/documentation/embed/usage-and-billing. The key-free default simply uses Google's documented Maps URLs.

## Update the catalog

Run manually, occasionally (not per visitor):

```powershell
./scripts/Refresh-Locations.ps1
```

The script downloads Waffle House brand records from a public Overpass server, filters restaurant records to the contiguous US, removes nearby duplicate representations, and writes `Alien.WaffleHouseTeleporter/Data/waffle-houses.json`. Restart the app after refreshing. Public servers may be overloaded; retry later or pass `-Endpoint` with another public Overpass instance. To rebuild from the bundled source without a network request:

```powershell
./scripts/Refresh-Locations.ps1 -SourceFile Alien.WaffleHouseTeleporter/Data/overpass-source.json
```

`dotnet publish Alien.WaffleHouseTeleporter -c Release` includes the normalized catalog. Runtime does not need the refresh script or original Overpass export.

## Data & limitations

Location data © OpenStreetMap contributors, licensed under ODbL 1.0: https://www.openstreetmap.org/copyright. The original export and derived catalog are distributed under that license. See `Alien.WaffleHouseTeleporter/Data/README.md` for provenance. The database license applies to location data; this is an unofficial fan project and is not affiliated with Waffle House.

Community coverage is incomplete, restaurants may close, and Street View may land on a nearby road or have no panorama. Imagery and storefront-facing angles are not prevalidated. Every destination provides a map fallback and another teleport. No Google imagery is downloaded or cached.
