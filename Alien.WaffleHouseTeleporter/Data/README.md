# Location data

Original export: `overpass-source.json`, downloaded 2026-10-05 via the public VK Maps Overpass endpoint https://maps.mail.ru/osm/tools/overpass/api/interpreter.

Query: `[out:json][timeout:30][maxsize:16777216];nwr["brand"="Waffle House"];out center tags;`

Derived catalog: `waffle-houses.json`, 1,672 restaurant destinations in the contiguous United States after filtering and deduplication. The catalog records its retrieval date and the upstream OSM snapshot timestamp. This is a community dataset, not a complete official restaurant list. Some records have no address.

Both data files are © OpenStreetMap contributors and available under the Open Database License (ODbL) 1.0:
https://www.openstreetmap.org/copyright
https://opendatacommons.org/licenses/odbl/1-0/

The normalized catalog is an adapted database under ODbL 1.0. When redistributing it, retain attribution and the license, and comply with applicable share-alike requirements. This notice concerns the location database.

Rebuild or refresh using `scripts/Refresh-Locations.ps1`. No Google Places data or Google imagery is stored here.
