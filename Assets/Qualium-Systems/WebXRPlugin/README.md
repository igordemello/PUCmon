## AR-Renderer-Unity

Unity 2021.x or later is required.
Developed in Unity 2021.3.13f1 using the URP (Universal Render Pipeline).

###
Detailed online documentation can be found here:
https://qualium-systems-ltd.github.io/WebXR-for-WebGL-Documentation/

###
Based on 
Three.js(https://threejs.org/) 
(MIT: https://github.com/mrdoob/three.js/blob/dev/LICENSE) 
and 
MindAR(https://hiukim.github.io/mind-ar-js-doc/) 
(MIT: https://github.com/hiukim/mind-ar-js/blob/master/LICENSE)

### Universal Render Pipeline(URP)
In order to run the project with URP, it is necessary to turn off post-processing (Select you camera and in inspector go to "Camera/Rendering/Post Processing") and HDR ("Camera/Output/HDR").

### Target recognition
1. Create new scene. And set it build index (Open "File/Build Settings" and add created scene into "Scenes in Build" filed).
2. In hierarchy press RMB -> AR-Renderer/Targets Tracking.
3. In hierarchy expand "TargetController". Create Target game object(RMB -> "AR-Renderer/Target") and set it as child of "AR-Target-Tracking-Scene/AR-Renderer-Scene". (You can create more than one target).
4. In hierarchy expand "TargetController/AR-Renderer-Scene/Target" and set whatever you want to display as child of "AR-Target-Tracking-Scene/AR-Renderer-Scene/Target/Displayer".
5. In hierarchy select "TargetController/AR-Renderer-Scene/Target/AR-Renderer-Target-Preview" and in Inspector set your target image ("Target Preview (Script)/Image". Important: only .png files are supported). (Image should be located in "Assets/Compiler/Targets" folder. If it is not exists, create this folder).
6. In hierarchy select "AR-Target-Tracking-Scene" and in inspector "Target Track Controller (Script)" in field "Target" set all you created targets and press "Generate" button (The scheme for compiler will be created. Schemas are located in "Assets/Compiler/Schemas" folder).
7. Open project folder and go to "Assets/Compiler". Make sure all there is "Schemas" folder with files with same name as your scene build index, and "Targets" folder with you target images.
8. Depending on your system run DotMindCompiler (DotMindCompiler-win, DotMindCompiler-macos or DotMindCompiler-linux). (Make sure you are running it from project folder in explorer and not from Unity Editor).
9. Web server will be created and page will be opened in browser automatically (If this did not happen, open the page at the address in the console manually).
10. In browser on the page you will see a block for each scene with targets images and their indices.
11. If everything is correct press "Compile .mind file for <x> scene". You can check the compilation status under this button.
12. When compilation is finished you can close tab in browser and webserver console.
13. In "File/Build Settings" change target platform to WebGL.
14. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
15. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
16. If necessary set base href (or leave it empty. By default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
17. Done.

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

### Face recognition
1. In hierarchy press RMB -> AR Renderer/Face Tracking
2. In hierarch select "FaceController/AR-Renderer-Scene" and set whatever you need to display as child of "AR-Renderer-Scene" gameobject (For correct custom model tracking and animation it should be located as close to default facemesh as possible).
3. Change target platform to WebGL.
4. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
5. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
6. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
7. Done.

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

### Surface and Ar-Space
1. In hierarchy press RMB -> AR Renderer/Markers Tracking
2. Set whatever you need on the scene.
3. You can subscribe to "Assets/AR-Renderer/Scripts/EventBus.cs/onHitPointUpdated" action to get the Vector3 coordinates of the point of intersection of the surface with the ray from the camera.
4. Change target platform to WebGL.
5. In "Edit/Player Settings/Player/Resolution and Presentation" select "AR-Renderer" WebGL template.
6. In "Edit/Player Settings/Player/Publishing Settings" change compression to Disabled.
7. If necessary set base href (or leave it empty for default value) (by default address is "http(s)://{yourip} or {localhost}:3000/") (For example, if you set href to "exampleHref/", all links will point to "http(s)://{yourip} or {localhost}:3000/exampleHref/" by default. It will be useful for Github Pages for example);
8. Done.


#### Additional information
To track certain events, you can subscribe to the desired event using a static class EventBus ("Assets/AR-Renderer/Scripts/EventBus"). (For example: EventBus.onTargetFound += "Your function");
These events are invoked automatically.
For more information about events check "Assets/AR-Renderer/Scripts/EventBus.cs".

#### Background from camera
If necessary, you can turn off the background from the camera. Select camer on the scene and change it background color alpha channel to 255.

#### Static objects on scene
Only some game objects are relative to the AR space (like "Targets/Displayer" for Targets and Surface recognition or "AR-Renderer-Scene" for Face recognition). Other objects which you have in hierarchy wil not be relative to the AR space.

#### Example scenes
In "Assets/AR Renderer/ExampleScenes" you can find examples of usage. To run most scenes, it is enough to add them to the build. Except Targets tracking. First of all you need to generate .mind file (check "Image recognition" section of this Readme, points 7-12). 

### Readme
You can find more instructions in "Assets/AR-Renderer/Docs" folder.


