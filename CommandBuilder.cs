using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IracingReplayCapture
{
    static public class CommandBuilder
    {
        static Command GotoCommand()
        {
            var command = new Command("goto",
                description: "Jump to an exact frame")
            {
                new Argument<int>("frameNum")
            };

            command.Handler = CommandHandler.Create<int>(ReplayControl.Goto);
            return command;
        }

        static Command FrameCommand()
        {
            var command = new Command("frame",
                description: "Print the current frame. Usefull for --range option");

            command.Handler = CommandHandler.Create(ReplayControl.PrintCurrentFrame);
            return command;
        }

        static Command PlayCommand()
        {
            Command command = new Command("play") {
                new Option<FrameRange[]>(new[] { "--ranges", "-r" },
                    description: "Frames to play. By default it goes through the whole replay. Format: START[-END]",
                    parseArgument: ArgumentParser.parseFrameRange),

                new Argument<string[]>("cameras",
                    description: "Cameras to play sequentially. Ie: cockpit tv1") {
                    Arity = ArgumentArity.OneOrMore
                },

                RecordCommand()
            };

            command.Handler = CommandHandler.Create<string[], FrameRange[]>(Player.Play);
            return command;
        }

        static Command RecordCommand()
        {
            Command command = new Command("record")
            {
                new Option<string>(new string[] {"--output", "-o" },
                    description: "Output folder"),

                new Option<string>("--obs-address",
                    description: "Obs websocket address"),

                new Option<string>("--obs-password",
                    description: "Obs websocket password")
            };

            command.Handler = CommandHandler.Create<string, string[], FrameRange[], string, string>(Recorder.Record);
            return command;
        }

        static RootCommand RootCommand()
        {
            var rootCommand = new RootCommand("Record multiple camera angles from an Iracing replay, using OBS")
            {
                FrameCommand(),
                GotoCommand(),
                PlayCommand()
            };

            return rootCommand;
        }

        static public void InvokeRootCommand(string[] args)
        {
            RootCommand().InvokeAsync(args);
        }
    }
}
