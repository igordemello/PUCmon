### Face recognition
1. In hierarchy press RMB -> AR Renderer/Face Tracking
2. In hierarch select "FaceController/AR-Renderer-Scene" and set whatever you need to display as child of "AR-Renderer-Scene" gameobject (For correct custom model tracking and animation it should be located as close to default facemesh as possible).
3. Change target platform to WebGL.
4. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
5. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
6. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
7. Done.