# Unity: Game AI Project

This project implements AI Behaviour in Unity. This is made for the 'Artificial Intelligence for Games' course in Tallinn University.

### Prerequisites

* Unity Engine Version 2019.3.15f1

## Built With

* C#
* NavMeshAgents

Note: AI-related Behaviour is commented within the project as well.

## AI Behaviour

In this project, there are over 10 AI zombies spawned at specific spawn locations. There are following Behaviours:
* 1- Walking along a waypoint path, starting with the closest waypoint, or when set up moving randomly inside a sphere radius
* 2- Chasing the player if he is inside a specific distance and can be seen without beeing blocked by walls
* 3- Attacking the player when he is in attackrange
* 4- Losing the player when he is too far away or hidden by walls.
* 5- Follow the leaderzombie, when too far away having a speedboost to reach him faster

## Team Members

* Daniel Gumnikow
* Jouke Bertus Staring
* Oluwafiyikewa Aigbovbioise Alawode

## Acknowledgments

* Zombie AI video playlist by SRCoder (https://www.youtube.com/channel/UCYaNsGvyvIupxpecr4rZY9A)
* Zombie AI video playlist by Adrian C. Chase (https://www.youtube.com/channel/UCqy5niYmcLgr4zBuc_ZAyIw)
