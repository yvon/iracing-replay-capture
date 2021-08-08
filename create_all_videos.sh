#!/usr/bin/env bash

output_folder=out
output=output.mp4
transition=hblur
duration=0.5

cd `dirname $0`

folders=()
for folder in $output_folder/*/; do
  folders+=($folder)
done

nb_folders=${#folders[@]}
nb_transitions=$(($nb_folders - 1))

args=()
filters=()
i=0
offset=0
video_label="0"
audio_label="0:a"

build_filters() { local IFS=";"; echo "${filters[*]}"; }

for folder in "${folders[@]}"; do
  [ ! -z $1 ] || ./create_video.sh $folder || exit 1

  source="$folder$output"
  args+=("-i $source")

  # Don't create transition for last element
  [[ $i -eq $nb_transitions ]] && break

  video_duration=`ffprobe -loglevel error -show_entries format=duration -of default=nk=1:nw=1 "$source"`
  offset=`awk "BEGIN {print $offset+$video_duration-$duration}"`

  new_video_label="v$i"
  new_audio_label="a$i"
  ((i++))

  filters+=(
    "[$video_label][$i]xfade=transition=$transition:duration=$duration:offset=$offset[$new_video_label]"
    "[$audio_label][$i:a]acrossfade=d=$duration[$new_audio_label]"
  )

  video_label=$new_video_label
  audio_label=$new_audio_label
done

cmd="ffmpeg -y ${args[*]} -filter_complex '$(build_filters)' -map '[$video_label]' -map '[$audio_label]' $output_folder/output.mp4"
echo $cmd
eval "$cmd"
