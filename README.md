# iracing-replay-capture

## play

```
Usage:
  replay-capture [options] play <cameras>... [command]

Arguments:
  <cameras>  Cameras to play sequencially. Ie: cockpit tv1

Options:
  -r, --ranges <ranges>  Frames to play. By default it goes through the whole replay. Format: START[-END]
  -?, -h, --help         Show help and usage information

Commands:
  record
```

## record

```
Usage:
  replay-capture [options] play <cameras>... record

Arguments:
  <cameras>  Cameras to play sequencially. Ie: cockpit tv1

Options:
  -o, --output <output>          Output folder
  --obs-address <obs-address>    Obs websocket address
  --obs-password <obs-password>  Obs websocket password
  -?, -h, --help                 Show help and usage information
```
