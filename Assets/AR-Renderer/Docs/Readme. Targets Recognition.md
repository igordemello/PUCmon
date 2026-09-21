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