#!/usr/bin/env node

const frame = process.argv[2] || process.exit(1);
const irsdk = require('node-irsdk-2021');
const iracing = irsdk.init({ telemetryUpdateInterval: 100 });

iracing.once('Connected', (evt) => {
  iracing.playbackControls.searchFrame(parseInt(frame), 'begin');
  process.exit();
});
