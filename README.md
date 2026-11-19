# Balloon Heist

A top-down party kart game for 2 to 12 players on one shared screen.
Every kart trails balloons. Tail another kart to steal theirs, keep your own, and finish with the most.
No bumping needed: stealing happens by **position**, not contact.

Made in Unity 6 (C#) as a prototype for BattleKart.

---

## Why this game
# I need to add the blabla about what were my decisions for this game, like the simple in and out type gameplay.
I started from what a BattleKart session actually looks like: a group of friends, real karts, one big projected floor, and maybe 15 minutes per game. From there I set myself a few rules for the design:

1. **Nobody sits out.** Customers pay for the whole session, so there's no elimination. You can't be knocked out. You always have a way to keep playing and win.
2. **No bumping to score.** With real karts, a game that rewards ramming is a game that rewards crashing. Stealing by position keeps it competitive without contact.
3. **Readable at a glance, at speed.** Your score is literally trailing behind your kart. Anyone can see who's winning without looking at a scoreboard.

4. **Other players are the only way to score.** I dropped "collect and bank" ideas, because in those players tend to ignore each other. Here the only way to gain a balloon is to take it from someone.

It also fills a gap: BattleKart already has racing, territory, snake, football, pool, co-op against AI and a bounty hunt, but not a free-for-all "steal and keep" mode. And it can be explained in one sentence: *"Get behind someone to steal their balloons."*

## Modes

- **Free-for-all** (default): everyone for themselves, most balloons wins.
- **Team versus:** the same rules carry straight over to teams. You can't steal from teammates, and the score is a **team tally** (the sum of everyone's balloons). It adds real team play: one player plays bait while a teammate tails the chaser, and you can protect your team's balloon carrier.

---

## How to play

**Join:** press any button on a controller (or any key on the keyboard) to join. You can drive around while others join.
**Start:** only **P1** can start the match (START / ENTER).

| Action | Keyboard | Controller |
|---|---|---|
| Accelerate | W | RT |
| Brake / Reverse | S | LT |
| Steer | A / D | Left stick |
| Handbrake | Space | X / Square |
| Use item | E / Left Shift | RB / R1 |

## Rules

- Everyone starts with **3 balloons**, floating in a line behind the kart.
- **Steal:** keep your nose inside the cone **behind** another kart. A ring fills up, and when it's full one balloon flies over to you.
- After losing a balloon you're **invulnerable for 5 s** (your kart blinks), so nobody can drain you in one go.
- **Weight:** every balloon above 3 makes you a bit slower. Every balloon below 3 makes you a bit faster. The leader is easier to catch, and nobody is ever out of the game.
- The **crown** floats over whoever is leading, so everyone knows who to hunt.

### Win condition

The match lasts **3:00**. The player with the most balloons wins.
If several players are tied for first, the match goes to **sudden death**: the first player to take the lead on their own wins.

### Items

Pickup spots appear on the map and respawn after a few seconds (a ring on the floor shows when they'll be back). What you get is random: you only find out when you drive over it.

- **Boost:** a short burst of speed. Use it to escape someone on your tail, or to close the gap.
- **Grapple:** steal one balloon from a kart in front of you, from any angle. A reticle shows who you'll grab before you press the button.
- **Bomb:** you hold it and **set it off yourself** with the item button. Everyone inside the red ring loses power for 1 s. Drive it into the crowd, pick your moment, then go steal from the karts that are stuck.

You can hold one item at a time, and you can't pick up a new one while you're holding one.

---

## Design choices

BattleKart is real karts on a floor projected from the ceiling. I tried to design for that, even though this prototype runs on a normal screen.

**What I kept, and why**

- **One shared screen, fixed top-down camera.** That's what the projection looks like: no split screen, no camera following anyone.
- **Stealing by position, not contact.** Real karts shouldn't ram each other to score. A steal zone behind each kart turns the game into chasing and dodging instead of crashing.
- **A long, narrow tail cone.** A wide zone made steals feel random. A narrow cone behind the kart makes it a tailing game: get behind someone and stay there.
- **A short fill timer (0.4 s) before a steal.** It stops accidental steals when karts are bunched together. It's the same idea as a hit-confirmation window I built for a VR drumming mechanic: filter out unintended input without adding noticeable delay.
- **Invulnerability after a steal.** It stops one player from draining another, and it gives the victim a moment to escape. It also gives other chasers a chance.
- **Weight instead of elimination.** More balloons means slower. It's an automatic catch-up mechanic, and it creates the core tension: do I keep my lead, or stay light?
- **Catch-up you can see.** Weight and the crown make the leader an obvious, catchable target. Rounds stay close without hidden rubber-banding.
- **Same rules from 2 to 12 players.** The arena just gets busier. Nothing needs re-tuning per player count.
- **Only P1 starts.** In a group, someone has to be "the host", and it stops random button presses from starting the match.
- **Items that are readable at a glance.** Every item shows a label above the kart holding it, and the bomb shows its blast radius while you hold it.

**What I dropped, and why not**

- **Ghost mode at 0 balloons** (pass through karts, can't be stolen from). Real karts stay solid, so a "ghost" kart can still crash into people. Weight already makes empty karts fast, so they're never out of the game.
- **Dropping balloons on the floor** (for example when hit by an item). Hard to read, and it makes everyone rush for the same spot. In a real arena that means more collisions.
- **Anything that changes steal range** (bigger zone, magnet effects). One fixed rule is easier to learn and easier to read than a zone that changes size.
- **Traps** behind karts. It's hard to see where they are and who they belong to, so getting hit feels unfair.
- **A Dart (projectile).** I replaced it with the **Grapple**: it reads as "I grab you", it works from any angle, and the reticle shows the target before the shot.

**Fit with BattleKart**

- The steal zone works through the projection, the same way BattleKart's missiles do. In a real arena it becomes a projected cone behind each kart, tracked by the ceiling sensors, and the grapple can reuse the missile logic.
- The bomb's "power cut" mirrors what BattleKart can already do: slow real karts remotely.
- Obstacles on the map can be solid. BattleKart blocks karts at projected obstacles, so they work like real walls.
- The first real-world step would be to test at projection scale: check readability from the driver's seat, and size the tail cone to real kart speeds.

---

## Expanding it

The core rule (tail to steal) stays the same. Everything else is a dial:

- **Maps:** obstacles to juke around, narrow corridors where tailing is easy, risky pickup spots (a pickup at the end of a dead end, a bomb-only spot in the middle), floor zones that slow you down or speed you up.
- **Settings:** match length, starting balloons, steal time, weight strength, which items are on, number of bots.
- **Pickups:** each one only changes *how you move or who you can reach*, never the steal rule itself. For example, a shield that reuses the existing invulnerability blink, so there's nothing new to learn.
- **Win conditions:** *Domination* (hold half of all balloons for 10 s to win instantly), or first player/team to reach X balloons for a quicker format.
- **Event ideas:** a "golden balloon" worth 3 that spawns mid-match, or a final 30 s where steals happen twice as fast.
- **Tuning with data:** log steals per minute and how often the lead changes in playtests, so tuning decisions are backed by numbers.

---

## Credits

- **PROMETEO: Car Controller** by Mena (Unity Asset Store): car physics, adapted to the new Input System.
- **Teko** font (SIL Open Font License), included with PROMETEO.
- **Balloons** by Loafbrr (CC0): https://loafbrr.itch.io/
- **Low Poly Objects Pack** (Unity Asset Store): crown and pickup models.
- **Mobile Optimize: Free Low Poly Cars** by Awbmecreations (Unity Asset Store).
- Engine and tire sounds: *source to be added.*
