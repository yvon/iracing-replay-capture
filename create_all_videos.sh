#!/usr/bin/env sh

FOLDER=out
OUTPUT="output.mp4"
TRANSITION="fade"

cd `dirname $0`
sources=()

for d in "`dirname $0`/$FOLDER/*/" ; do
  [ ! -z $1 ] || ./create_video.sh $d || exit 1
  sources+=($d$OUTPUT)
done

[ ${#sources[@]} -eq 0 ] && exit 1
npx ffmpeg-concat -t $TRANSITION -d 1000 -o $FOLDER/$OUTPUT ${sources[@]}
