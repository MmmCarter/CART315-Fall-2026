Date: September 29, 2026

WHAT?
I added gradual acceleration, adjustable ball return angles, local two-player mode, and power shots to the Basic Pong version.

During testing, I noticed that as the ball speed increased, the ball would sometimes bounce back and forth between the top and bottom walls for a long time before reaching the other side. Therefore, I limited the bounce angles and made them depend on where the ball hits the paddle: hits to the center result in straighter returns, while hits to the edges result in angled returns.

I also added a power meter, pulsating button prompts, and colored trails to help players understand the power shots. Issues encountered during development included: the scoring boundary was set as a trigger, but the script used standard collision detection; and the trail width was set to zero, making the effect invisible. For some bugs that were difficult to understand, I asked the AI for help and tested them repeatedly in Unity, and eventually resolved all of them.


SO WHAT?
The acceleration problem showed me that speed and pacing are different. A faster ball does not necessarily produce a more engaging rally if players spend too much time waiting for it to reach them. Controlling its direction was therefore as important as adjusting its speed.

The contact-based bounce angle gave paddle movement a purpose beyond simply preventing a miss: players could potentially aim their returns. However, I still need to test whether new players understand this relationship without explanation.

The energy shot introduced a deliberate choice alongside automatic acceleration. Players could decide when to prepare a stronger return. Its interface also helped me think about feedback: the bar shows progress, the pulsing prompt indicates availability, and the trail shows that the enhanced shot has occurred. These elements communicate different stages of the same action.

Debugging reminded me that functioning scripts are only part of a Unity project. Collider settings, component references, and visual parameters also determine whether a mechanic works and can be understood.


NOW WHAT?
Next, I’ll conduct a two-player trial to observe whether players will actively control the angle of the return shot and whether they can distinguish between the “Energy Full” and “Power-up Ready” states.
I’ll also test whether there are still reasonable defensive opportunities against power-up shots, and then adjust the power-up multiplier, maximum ball speed, or the energy gained per return based on the results. I’ll adjust only one parameter at a time to determine its impact.