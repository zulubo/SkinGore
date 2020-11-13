# SkinGore
 SkinGore is a dynamic gore and injury decal system I created for Vertigo 2. It can be applied to animated, skinned meshes, and is very performant even with many instances in a scene.
 
I've included an example scene in the project where you can click on this mannequin to mutilate him!

[Here's a web build too, to play with.](https://zulubo.github.io/SkinGoreWebBuild/)

![Example of gore on mannequin](/images/gore_0.png)

## How it works
Every time damage is taken, the character mesh is rendered in its UV coordinates with a special shader that draws a white blob at the damage location. This hit buffer is combined additively with a persistent damage accumulation buffer that holds all the previous hits as well. Finally, this is used as a mask for a material that reveals blood and gore on the character. Since everything here happens on the GPU, it's insanely efficient, especially with the modest texture sizes that are required (64x64 by default).

## How to use
### Basics
Setup is very simple, and you shouldn't have trouble integrating into your own systems.
Minimal asset preparation is needed. The system uses the second UV channel if available, as some characters may have overlapping UVs in the first one. If needed, create a second UV map with no overlap. If no second UV channel is present, it will default back to the first one.
Add a SkinGoreRenderer component to your character, and select a target mesh and decal material. Example gore materials are included that you are welcome to use. The SkinGoreRenderer will automatically create a duplicate of the skinned mesh with the decal material applied when needed.
To add damage to your character, call `SkinGoreRenderer.AddDamage()`. Give it a world space position, radius, and strength for the damage. All calculations are done on the GPU and are super fast, so don't worry about calling this a lot.

### Extending the system
You are welcome to modify the code as needed. Some modifications I can imagine are:
* Using a custom gore decal shader. Couldn't be simpler, just modify my SkinGore.shader or create your own with the same `_GoreDamage` texture.
* Using a single material instead of an overlaid mesh. Some advantages of this would be fewer draw calls, and the possibility of deforming damaged vertices for cool 3D wounds. For this to work, you'll need to tweak the SkinGoreRenderer script to modify the skinnedMeshRenderer's base material rather than creating a new mesh and applying the decal material. Adding this feature is in my todos.
* Adding LOD support. Right now it just assumes you're using a single LOD, and the gore decal will overlap different LODs if you move far away. Adding LOD support is also in my todos.

![Example of gore in Vertigo 2](/images/gore_1.png)
