#!/usr/bin/env sh

OUTPUT="output.mp4"

cd $1 &&

ffmpeg -y -i 9.mp4 -i 2.mp4 -i 11.mp4 -filter_complex '
[1]scale=1000:-1,crop=out_h=133:y=185,hflip,pad=w=4+iw:h=4+ih:x=2:y=2:color=black[a];
[2]scale=-1:300,pad=iw+2:ih+2:0:2:white@0.3[b];
[0][a]overlay=(W-w)/2:33[c];
[c][b]overlay=20:H-h-20
' $OUTPUT
