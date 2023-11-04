# SpaceShooterDOTS
Performance aware space shooter making use of Unity DOTS, (data oriented technology stack) and ECS, (entity component system).

# Data-oriented design and ECS
Data-oriented design's main purpose is to make efficient use of the way your computer processes on an internal level. The most common approach is to structure code and data in a linear fashion. 
We do this to simplify our memory usage and reduce our need to jump around in memory. Memory reading and writing speed is a usual bottleneck compared to the cpu's ability to handle large amounts of data.

Entity component systems use this principle by grouping components together in the same chunk of memory allowing systems to quickly iterate over these with the same instructions.

# Process
I started by building everything in ECS but because I had never used it before I expected my implementations to be naive and unoptimized. Most of the process went well with systems for player control, enemy behaviour and shooting projectiles working without much trouble.
To challenge myself I decided to do my own collision using simple AABB detection. I had some trouble making these collision sending events to eachother but figured it out using CollisionReceiver components that would activate and store the collision's entity so I could infer
what kind of a collision this was in other systems. I also had problems with destroying entities as they would be destroyed at different types and cause other systems to reference them after they were invalid. My solution was to simply use a component that would schedule 
destruction at the same time. Spawning was also an issue as I wanted to control it from Monobehaviour rather than a system, but couldn't as prefabs needed to be converted to entities in a baker. I figured out a method of making spawners components with baked prefabs that
a managed system would instantiate from after being activated by my monobehaviour script.

# Profiling
Once I had everything working I did some profiling with 200 enemies in a build to see what took longest. The systems I saw that took a long time and I knew I could improve was the;
- CollisionDetectionSystem with times between .7ms to 4.5ms.
- TriggerDetectionSystem with times between .175ms to .975ms
- AvoidanceSystem with times between .075ms to .379ms.
I started by adding spatial hashing to the collision systems as I would simply iterate over all boxcolliders at this point. To do this I made a CollisionGatheringSystem that would go through all boxcolliders and store the relevant data inside of a NativeParallelMultiHashMap.
Doing this allowed me to reduced my collision checks from being two systems that would go over all the colliders to only going over the relevant ones in adjacent cells. The result yielded improvments of .04ms to .25ms for the CollisionDetectionSystem
and .02ms to .172ms for the TriggerDetectionSystem.

I also attempted doing this to the AvoidanceSystem for the enemies but failed to see an improvement. My guesses are that either my profiler test for that system was flawed or there weren't enough enemies in the game to see a benefit.
