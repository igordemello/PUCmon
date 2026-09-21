### Geolocation
1. In hierarchy press RMB -> AR Renderer/Geolocation Tracking
2. In hierarchy press RMB -> AR Renderer/Misc/Location and set it as child of "GeolocationController/Locations" gameobject
3. Select created Location and set all parameters ("Name" (optional), Latitude, Longitude, Altitude (optional), etc)
4. If it is necessary to show direction to your location in hierarchy press RMB -> AR Renderer/Misc/Arrow and set created Arrow as child of "GeolocationController/ND/Arrows" gameobject;
5. Select created Arrow and drag and drop your created Location into "Location" field of "Arrow" component;
6. Select "GeolocationController/LocationsManager" and set your locations and arrows (if exists).
7. Change target platform to WebGL.
8. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
9. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
10. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
11. Done.