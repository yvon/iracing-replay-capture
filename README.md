# iRacing Replay Capture

A command line application to command iRacing replays (playhead and cameras) and allowing automatic OBS records (a file by sequence).

## Dependencies

If you plan to use the `record` command it requires:
- OBS (https://obsproject.com/) with preconfigured scene(s) capturing iRacing window
- OBS websocket (https://github.com/Palakis/obs-websocket/releases) to allow remote control.

## Install

Download the latest installer from the release page: https://github.com/yvon/iracing-replay-capture/releases

It will extract the application within a folder. Execute it from a console, like PowerShell.

## Important

It is strongly recommended to disable replay spooling or the application may not be able perform long jumps in time.

Cf [this message on iRacing forums](https://forums.iracing.com/discussion/comment/35811/#Comment_35811) and David's explanations.

## Have a look at the embeded help

```
replay-capture.exe -h
replay-capture.exe play -h
replay-capture.exe play record -h
```

## Typical use case

The application allows to record multiple angles from a replay. You can then import the recorded sequences on your favorite video editor.

```
replay-capture.exe play cockpit tv1 gearbox record -o output_folder
```

By default, the application will play the whole replay but you can restrain it a range of frames using the `--ranges` option.

```
replay-capture.exe play cockpit tv1 gearbox --ranges 1042-10453 44970-47000 record -o output_folder
```

If you specify more than one range it will play the given cameras sequentially for the first one, then do the same for the second, etc...

To optain the current frame number and prepare your arguments you can use the frame command:

```
replay-capture.exe frame
```

An OBS record will be automatically started for each camera. If you specify the ouput folder they will be named after the played camera.

Also, if a scene of the camera name exists, it will automatically switch to it.

## Another use cases

Writing you own script you may control the cameras to capture a complex sequence, including jumps in time, replays, etc...

Example:

```
replay-capture.exe play cockpit --ranges 1042-1200 # start with a cockpit view
replay-capture.exe play tv1 --ranges 1100-1200 # something jump appened, replay it from tv1 perspective
replay-capture-exe play cockpit --ranges 1200- # then return to cockpit view, etc...
```

Another option will be to automatically edit the videos using ffmpeg. As an example here is a bash script I used to create a cockpit view with mirrror and tv embeded, and with blur transition between sequences: https://gist.github.com/yvon/4325bc6a447abb958dba19bdf2e22a23

And here is the kind of output it produces: https://youtu.be/Yf2NQ8Awasg
