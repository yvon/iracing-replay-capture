#!/usr/bin/env bash

output_folder=$1
output=output.mp4
transition=hblur
duration=0.5

cd `dirname $0`

sources=()
filters=()
i=0
offset=0
video_label="clip0"
audio_label="0:a"

build_sources() { printf -- ' -i %s' "${sources[@]}"; }
build_filters() { local IFS=";"; echo "${filters[*]}"; }

for folder in $output_folder/*/; do
  sources+=("${folder}9.mp4" "${folder}2.mp4" "${folder}11.mp4")
  index=$((i*3))

  # scale mirror
  filters+=("[$(($index+1))]scale=1000:-1,crop=out_h=133:y=185,hflip,pad=w=4+iw:h=4+ih:x=2:y=2:color=black[mirror$i]")
  # scale tv
  filters+=("[$(($index+2))]scale=-1:300,pad=iw+2:ih+2:0:2:white@0.3[tv$i]")
  # overlay mirror
  filters+=("[$(($index))][mirror$i]overlay=x=(W-w)/2:y=33[omirror$i]")
  # overlay tv
  filters+=("[omirror$i][tv$i]overlay=x=20:y=H-h-20[clip$i]")

  # Don't create transition for first element
  if [ $i -gt 0 ]
  then
    new_video_label="v$i"
    new_audio_label="a$i"

    # video transition
    filters+=("[$video_label][clip$i]xfade=transition=$transition:duration=$duration:offset=$offset[$new_video_label]")
    # audio transition
    filters+=("[$audio_label][$index:a]acrossfade=d=$duration[$new_audio_label]")

    video_label=$new_video_label
    audio_label=$new_audio_label
  fi

  # increment the offset for next transition
  video_duration=`ffprobe -loglevel error -show_entries format=duration -of default=nk=1:nw=1 "${folder}9.mp4"`
  offset=`awk "BEGIN {print $offset+$video_duration-$duration}"`

  ((i++))
done

cmd="ffmpeg -y $(build_sources) -filter_complex '$(build_filters)' -map '[$video_label]' -map '[$audio_label]' $(cat ffmpeg_options) $output_folder/output.mp4"
echo $cmd
eval "$cmd"
