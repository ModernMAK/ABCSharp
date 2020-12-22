using System;
using System.Collections.Generic;

namespace ABCSharp.Old
{
    public static class PitchUtility
    {
        //Stolen from:
        //http://i1231.photobucket.com/albums/ee520/AI_Joe/note_freq.png
        //TODO Values should be calculated via Octave 8 / 2^8
        private static Dictionary<Pitch, float> _frequencyLookup = new Dictionary<Pitch, float>()
        {
            {Pitch.C, 16.35f},
            {Pitch.CSharp, 17.32f},
            {Pitch.D, 18.35f},
            {Pitch.DSharp, 19.45f},
            {Pitch.E, 20.60f},
            {Pitch.F, 21.83f},
            {Pitch.FSharp, 23.12f},
            {Pitch.G, 24.50f},
            {Pitch.GSharp, 25.96f},
            {Pitch.A, 27.50f},
            {Pitch.ASharp, 29.14f},
            {Pitch.B, 30.87f}
        };

        public static float GetFrequency(Pitch pitch, int octave)
        {
            if (octave < 0)
                throw new ArgumentException("Octave cannot be negative!", nameof(octave));

            var mul = 1;
            for (var i = 0; i < octave; i++)
                mul *= 2;
            if (_frequencyLookup.TryGetValue(pitch, out var frequency))
                return frequency * mul;
            else
                throw new ArgumentException("Pitch not valid!", nameof(pitch));
        }
    }
}