using System.Collections.ObjectModel;
using System.CommandLine.Parsing;
using System.Linq;

namespace IracingReplayCapture
{
    public static class ArgumentParser
    {
        static public FrameRange[] parseFrameRange(ArgumentResult arg)
        {
            var ret = new Collection<FrameRange>();

            foreach (var token in arg.Tokens)
            {
                var frameRange = new FrameRange();
                var members = token.Value.Split('-');

                try
                {
                    frameRange.from = uint.Parse(members[0]);
                    if (members.Length > 1)
                    {
                        frameRange.to = uint.Parse(members[1]);
                    }
                    ret.Add(frameRange);
                }
                catch (System.FormatException)
                {
                    arg.ErrorMessage = $"Cannot parse given range: {token.Value}";
                    return null;
                }
            }

            return ret.ToArray();
        }
    }
}
