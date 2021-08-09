#!/usr/bin/env node
'use strict';

require('dotenv').config();

const baseFolder = process.argv[2];
if (!baseFolder || baseFolder == '') {
  console.error('Folder required as argument');
  process.exit(1);
}

const EventEmitter = require('events');
const OBSWebSocket = require('obs-websocket-js');
const irsdk = require('node-irsdk-2021');
const path = require('path');

const cams = process.argv[4] ? process.argv[4].split(',').map(e => parseInt(e)) : [9, 2, 11];
const obsAddress = process.env.OBS_ADDRESS;
const obsPassword = process.env.OBS_PASSWORD;

let [startFrame, endFrame] = process.argv[3]?.split('-').map(e => parseInt(e)) || [];
if (!startFrame) startFrame = 1;
if (!endFrame) endFrame = undefined;

const folder = path.join(baseFolder, `${startFrame}-${endFrame}`);
console.log(`Start: ${startFrame}. End: ${endFrame}`);
console.log(`Folder: ${folder}`);

async function connectToOBS() {
  try {
    const obs = new OBSWebSocket();
    await obs.connect({ address: obsAddress, password: obsPassword });
    console.log(`We're connected to OBS`);
    return obs;
  } catch (e) {
    console.error("Cannot connect to OBS", e);
    process.exit(1);
  }
}

async function run() {
  const eventEmitter = new EventEmitter();
  const obs = await connectToOBS();

  process.on('SIGINT', function() {
    console.error("Caught interrupt signal");

    if (recording) stopRecording();
    process.exit(1);
  });

  console.log("Connect to Iracing");
  const iracing = irsdk.init({ telemetryUpdateInterval: 100, sessionInfoUpdateInterval: 100 });

  var currentDriver;
  var recording = false;
  var i = 0;

  console.log("Set recording folder");
  await obs.send('SetRecordingFolder', { 'rec-folder': path.join(__dirname, folder) });
  console.log("Recording folder set");

  obs.on('RecordingStarted', () => {
    console.log('Recording started');
    iracing.playbackControls.play();
    recording = true;
  });

  obs.on('RecordingStopped', () => {
    console.log('Recording stopped');
    i >= cams.length ? process.exit(0) : setTimeout(start, 2000);
    recording = false;
  });

  console.log("Wait for session info");

  iracing.once('SessionInfo', function (evt) {
    console.log("Got session infos");
    iracing.playbackControls.pause();
    iracing.camControls.setState(iracing.Consts.CameraState.UIHidden);
    currentDriver = evt.data.DriverInfo.DriverCarIdx;

    start();

    iracing.on('Telemetry', function (evt) {
      process.stdout.write(`${evt.values.ReplayFrameNum}\r`);

      if (recording) {
        if (endFrame && evt.values.ReplayFrameNum >= endFrame || evt.values.ReplayFrameNumEnd <= 1) {
          eventEmitter.emit('restart');
        }
      }
    })
  });

  function stopRecording() {
    obs.send('StopRecording')
      .catch((error) => {
        console.error("Can't stop recording:", error);
        process.exit(-1);
      });
  }

  function restart() {
    console.log('Restart');
    i += 1;
    stopRecording();
  }

  const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

  async function start() {
    console.log('Start');

    try {
      await obs.send('SetCurrentScene', { 'scene-name': cams[i].toString() });
      await delay(1000);// Racelab reloading
    } catch(e) {
      console.error(`Can't set the current scene:`, e);
      process.exit(-1);
    }

    try {
      await obs.send('SetFilenameFormatting', { 'filename-formatting': cams[i].toString() });
    } catch (e) {
      console.error(`Can't set record filename:`, e);
      process.exit(-1);
    }

    eventEmitter.once('restart', restart);

    iracing.camControls.switchToCar(currentDriver, cams[i]);

    console.log('Reset the playback');
    while (!iracing.telemetry || iracing.telemetry.values.ReplayFrameNum != startFrame) {
      iracing.playbackControls.searchFrame(startFrame, 'begin');
      await delay(1000);
    }

    console.log('Request start recording');

    setTimeout(() => {
      obs.send('StartRecording')
        .catch((error) => {
          console.error("Can't start recording.", error);
          process.exit(-1);
        })
    }, 2000);
  }
}

run();
