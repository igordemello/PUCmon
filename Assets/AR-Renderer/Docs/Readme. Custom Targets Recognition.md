### Custom target recognition preparation
1. Create new scene. And set it build index ("File/Build Settings/Scenes in Build").
2. In hierarchy press RMB -> AR-Renderer/Custom Target Tracking.
3. In hierarchy expand "CustomTargetController". Create Target game object(RMB -> "AR-Renderer/Target") and set it as child of "AR-Surface-Tracking-Scene/AR-Renderer-Scene".
4. In hierarchy expand "CustomTargetController/AR-Renderer-Scene/Target" and set whatever you want to display as child of "AR-Surface-Tracking-Scene/AR-Renderer-Scene/Target/Displayer".
5. In hierarchy select "CustomTargetController" and in inspector "Custom Target Track Controller (Script)" in field "Target" set your target.
6. Create ui element for "CustomTargetController/Custom Target Track COntroller (Script)/ScanSurface" function. (This function should be called manually to start scanning the surface). 
7. In "File/Build Settings" change target platform to WebGL.
8. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
9. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
10. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
11. Done.

### Surface recognition process
1. Place your phone parallel to the surface. It is preferable that it be some object (bussines card, copybook, etc.) and not a clean surface.
2. Press on "ScanSurface" ui element ("Surface recognition preparation" section of this Readme, point 6).
3. Done.