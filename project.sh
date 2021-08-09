#!/usr/bin/env bash

xargs -a $1/frames -I{} ./record.js $1 {} && ./encode_video.sh $1
