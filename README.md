# unity3d-billboard-generator
A Billboard Generator with support for multi-bake passes! Built in an afternoon ~

![Foliage Example](https://i.imgur.com/62du8cr.gif)

## Features
* Generates a simple cross-section billboard for a prefab (auto picks last LOD of prefab to render if it exists)
* Support for multiple "bake passes" - allows you to bake other data (masks, normals) that you want to use

## Simple Tutorial
* Create a BillboardRenderSettings via "Create/Ardenfall/Foliage/Billboard Render Settings"
* Add a new Billboard Texture. This will be your albedo+alpha texture. 
* Ensure the Billboard Texture's textureID matches the texture property in your billboard shader
* Add a new Bake Pass. Keep the values as they are by default. (this pass is using the default shader, so no custom shader needed)
* Create a BillboardAssets via "Create/Ardenfall/Foliage/Billboard Asset"
* Add your foliage prefab to the Prefab field
* Add your render settings to the Render Settings field
* Click "Generate Billboard"
* To see the results, click "Spawn Visual"

## Custom BakePasses
* Lets create a custom bakepass to use normals in our billboard!
* Add a new Billboard Texture to your Render Settings. Set the property id to "\_Normals", and toggle off Alpha Is Transparency.
* Add a new Bake Pass. Uncheck "A", since we have no need for the alpha channel (normals can be baked into 3 channels).
* Create a new shader. This will be your "normal bake pass shader". Make your shader output the normals of each fragment. (Literally rendering the normals)
* Insert this shader in the "Custom Shader" field in your new Bake Pass.
* To use this normal texture, simply sample the texture "\_Normals" in your billboard. 

## Notes
* When baking a pass, you may want to ensure certain properties of the rendered material is a certain value. You can do this via the Material Overrides field.
* There is a builtin "Cutoff" value that is applied to the generated billboard material. This is optional, but if you want to use it the property name must be "\_Cutoff".
* You don't __Have__ to use multiple bake passes per texture - you could easily pack multiple things (normals and occlusion, for example) into a single RGBA texture in a single bake pass. The multiple bake pass per texture feature is merely for convenience. 
* There are a few lines dedicated to URP - if you want to use this in builtin/HDRP it will take a little bit of work to port, but I promise it would only take a few minutes.
