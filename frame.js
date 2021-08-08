#!/usr/bin/env node

const irsdk = require('node-irsdk-2021');
const iracing = irsdk.init({ telemetryUpdateInterval: 100 });

iracing.once('Telemetry', (evt) => {
  console.log(evt.values.ReplayFrameNum);
  process.exit();
});
