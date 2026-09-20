# Audio

The code is wired up but there are no clips yet. Drop your files in this folder and
assign them in the inspector. Nothing plays until you do, and nothing breaks either:
an empty slot is just silence.

Unity reads `.wav`, `.mp3` and `.ogg`. Use `.wav` for the shutter click (short sounds
want no decode delay) and `.ogg` or `.mp3` for the music (smaller file).

## Music

One track, looping, carried across every scene.

1. Put the track in this folder, e.g. `Assets/Audio/Theme.ogg`.
2. Open a scene and look for the **MusicPlayer** object in the hierarchy.
   - `MainMenu`, `Level01`, `Level02` and `Level03` do not have one yet. Add it with
     **Tools > Viewfinder > Add Music Player To Open Scene**, then save the scene.
   - Scenes rebuilt with the level builder get one automatically.
3. Drag your track onto the **Music** field, and set **Volume** (0.5 by default).
4. Repeat for each scene, so the music also starts if you hit Play from a level.

`MusicPlayer` survives scene loads and only lets one copy exist. The first one to wake
up keeps playing and any later copy destroys itself, so walking from Level 01 to
Level 02 does not restart the track from the top. Assign the same clip everywhere.

For the import settings, **Load Type: Streaming** and **Compression: Vorbis** are
sensible for a long track. Leave **Loop** off on the clip itself; `MusicPlayer` sets
looping on the AudioSource.

## Shutter click

Plays the instant a photo is taken, not when it is placed.

1. Put the click in this folder, e.g. `Assets/Audio/Shutter.wav`.
2. In each level scene, select **Player > Camera Holder > Polaroid**.
3. Drag the clip onto **Shutter Clip** under the Audio header, and set
   **Shutter Volume** if 1 is too loud.
4. Save the scene.

The `Polaroid` script makes its own AudioSource at runtime, so there is nothing else to
add. The sound is 2D, so it does not get quieter as you move.

## Still missing

Listed in the GDD as TBD: a sound for placing a photo, footsteps, jump and land, and a
level-complete sting. None of those are wired up yet.
