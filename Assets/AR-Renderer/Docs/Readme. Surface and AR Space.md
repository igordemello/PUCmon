### Surface and Ar-Space
1. In hierarchy press RMB -> AR Renderer/Markers Tracking
2. Set whatever you need on the scene.
3. You can subscribe to "Assets/AR-Renderer/Scripts/EventBus.cs/onHitPointUpdated" action to get the Vector3 coordinates of the point of intersection of the surface with the ray from the camera.
4. Change target platform to WebGL.
5. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
6. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
7. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
8. Done.