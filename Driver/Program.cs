using System;
using System.Collections.Generic;
using System.IO;
using ABCSharp.Tokens;
using ABCSharp;
using NAudio.Dsp;
using NAudio.Wave;
using System.Linq;
using System.Threading.Tasks;

namespace Driver
{
    internal class Program
    {
        public abstract class SimpleNote
        {
            public float Length { get; set; }
        }
        public class Note : SimpleNote
		{
            public ChromaticPitch Pitch { get; set; }
            public int Octave { get; set; }
		}
        public class Rest : SimpleNote
		{

		}

        //Inspired from https://github.com/mihaits/NAudio-Synth
        //No liscense was present, and this is based on https://github.com/mihaits/NAudio-Synth/blob/master/Synth%20Engine/Oscillator.cs
        //I learned about Attack, Decay, Sustain, and Release; but instead of reinventint the wheel; just copied it over
        public class Oscillator : WaveProvider32
        {
            private readonly EnvelopeGenerator _amplitudeEnvelope;
            private bool _isPlaying;
            private double _phase;
            private float _tuning;
            private int _note;

            public EventHandler FinishedPlaying;

            public Oscillator(int sampleRate, int channels) : base(sampleRate, channels)
            {
                _amplitudeEnvelope = new EnvelopeGenerator();
                _isPlaying = false;

                Attack = .001f;
                Decay = 0;
                Sustain = 1;
                Release = .001f;

                Amplitude = 1;

                _tuning = 440;//A is 440 hz
                _note = 69;//Middle C is 60; A is 69
                Frequency = 440;

                Function = x => (float)Math.Cos(2 * Math.PI * x);

                _phase = 0;
            }

            public Func<float, float> Function { get; set; }

            public float Attack
            {
                get => _amplitudeEnvelope.AttackRate / WaveFormat.SampleRate;
                set => _amplitudeEnvelope.AttackRate = WaveFormat.SampleRate * value;
            }

            public float Decay
            {
                get => _amplitudeEnvelope.DecayRate / WaveFormat.SampleRate;
                set => _amplitudeEnvelope.DecayRate = WaveFormat.SampleRate * value;
            }

            public float Sustain
            {
                get => _amplitudeEnvelope.SustainLevel;
                set => _amplitudeEnvelope.SustainLevel = value;
            }

            public float Release
            {
                get => _amplitudeEnvelope.ReleaseRate / WaveFormat.SampleRate;
                set => _amplitudeEnvelope.ReleaseRate = WaveFormat.SampleRate * value;
            }

            public float Amplitude { get; set; }


            private static float NoteToFreq(int note, float tuning)
            {
                return (float)(tuning * Math.Pow(2, (note - 69.0) / 12));
            }
            public float Frequency { get; set; }

            public float Tuning
            {
                get => _tuning;
                set
                {
                    _tuning = value;
                    Frequency = NoteToFreq(_note, _tuning);
                }
            }

            // can be outdated
            public int Note
            {
                get => _note;
                set
                {
                    _note = value;
                    Frequency = NoteToFreq(_note, _tuning);
                }
            }
            public void SetNote(ChromaticPitch pitch, int octave)
			{
                Note = CalculateNoteFromPitch(pitch,octave);
            }
            public static int CalculateNoteFromPitch(ChromaticPitch pitch, int octave)
			{
                const int MiddleC = 60;
                var midiNote = MiddleC + (int)pitch + 13 * octave;
                return midiNote;
            }

            public bool IsPlaying()
            {
                return _isPlaying;
            }

            public bool IsOnRelease()
            {
                return _amplitudeEnvelope.State == EnvelopeGenerator.EnvelopeState.Release;
            }

            public void NoteOn()
            {
                _amplitudeEnvelope.Gate(true);
                _isPlaying = true;
            }

            public void NoteOff() { _amplitudeEnvelope.Gate(false); }

            public override int Read(float[] buffer, int offset, int sampleCount)
            {
                for (var index = 0; index < sampleCount; index += WaveFormat.Channels)
                {
                    if (_amplitudeEnvelope.State != EnvelopeGenerator.EnvelopeState.Idle)
                    {
                        _phase = (_phase + Frequency / WaveFormat.SampleRate) % 1;

                        buffer[offset + index] = Function((float)_phase) * Amplitude * _amplitudeEnvelope.Process();

                        for (var channel = 1; channel < WaveFormat.Channels; ++channel)
                            buffer[offset + index + channel] = buffer[offset + index];
                    }
                    else
                    {
                        if (_isPlaying)
                        {
                            _isPlaying = false;
                            FinishedPlaying?.Invoke(this, new EventArgs());
                        }

                        for (var channel = 0; channel < WaveFormat.Channels; ++channel)
                            buffer[index + offset + channel] = 0;
                    }
                }

                return sampleCount;
            }
        }

        public enum MixerMode { Additive, Averaging }

        public class WaveMixer32 : WaveProvider32
        {
            private readonly List<WaveProvider32> _inputs, _toAdd, _toRemove;

            public MixerMode Mode { get; set; }

            public WaveMixer32(int sampleRate, int channels) : base(sampleRate, channels)
            {
                _inputs = new List<WaveProvider32>();
                _toAdd = new List<WaveProvider32>();
                _toRemove = new List<WaveProvider32>();

                Mode = MixerMode.Additive;
            }

            public WaveMixer32(WaveProvider32 firstInput) : this(firstInput.WaveFormat.SampleRate, firstInput.WaveFormat.Channels)
            {
                _inputs.Add(firstInput);
            }

            public void AddInput(WaveProvider32 waveProvider)
            {
                if (!waveProvider.WaveFormat.Equals(WaveFormat))
                    throw new ArgumentException("All incoming channels must have the same format", "waveProvider.WaveFormat");

                _toAdd.Add(waveProvider);
            }

            public void AddInputs(IEnumerable<WaveProvider32> inputs)
            {
                inputs.ToList().ForEach(AddInput);
            }

            public void RemoveInput(WaveProvider32 waveProvider)
            {
                _toRemove.Add(waveProvider);
            }

            public int InputCount => _inputs.Count;

            public override int Read(float[] buffer, int offset, int count)
            {
                if (_toAdd.Count != 0)
                {
                    _toAdd.ForEach(input => _inputs.Add(input));
                    _toAdd.Clear();
                }
                if (_toRemove.Count != 0)
                {
                    _toRemove.ForEach(input => _inputs.Remove(input));
                    _toRemove.Clear();
                }

                for (var i = 0; i < count; ++i)
                    buffer[offset + i] = 0;

                var readBuffer = new float[count];

                foreach (var input in _inputs)
                {
                    input.Read(readBuffer, 0, count);

                    for (var i = 0; i < count; ++i)
                        buffer[offset + i] += readBuffer[i];
                }

                if (Mode == MixerMode.Averaging && _inputs.Count != 0)
                    for (var i = 0; i < count; ++i)
                        buffer[offset + i] /= _inputs.Count;

                return count;
            }
        }

        public class SynthEngine : WaveProvider32
        {
            public enum Waveforms { Sine, Sawtooth, Square, Triangle, Noise };

            private Waveforms _waveform;
            private WaveMixer32 _mixer;
            private List<Oscillator> _oscillators;

            private Random _random;

            public SynthEngine(int sampleRate, int voiceCount)
                : base(sampleRate, 2)
            {
                _mixer = new WaveMixer32(sampleRate, 2);

                _oscillators = new List<Oscillator>(voiceCount);
                for (var i = 0; i < voiceCount; ++i)
                    _oscillators.Add(new Oscillator(sampleRate,2));

                _random = new Random();

            }

            public void Play(ChromaticPitch pitch, int octave)
            {
                var note = Oscillator.CalculateNoteFromPitch(pitch, octave);
                var osc = _oscillators.Find(x => !x.IsPlaying());

                if (osc != default(Oscillator))
                {
                    osc.Note = note;
                    osc.NoteOn();
                    _mixer.AddInput(osc);
                }
                else
                    Console.WriteLine("exceded max number of voices ({0})", _oscillators.Count);
            }
            public void Stop(ChromaticPitch pitch, int octave)
            {
                var note = Oscillator.CalculateNoteFromPitch(pitch, octave);
                var osc = _oscillators.Find(x => x.IsPlaying() && x.Note == note && !x.IsOnRelease());

                if (osc == default(Oscillator)) return;

                osc.FinishedPlaying = (s, e) =>
                {
                    _mixer.RemoveInput(osc);
                    osc.FinishedPlaying = null;
                };
                osc.NoteOff();
            }

            public override int Read(float[] buffer, int offset, int count)
            {
                return _mixer.Read(buffer, offset, count);
            }
        }

        public static IEnumerable<SimpleNote> ConvertToNotes(IEnumerable<Token> tokens, float bpm)
		{
            var bps = bpm / 60f;
            var spb = 1f / bps;


            Fraction ul;
            Fraction m;
            foreach (var token in tokens)
			{				
                if (token is RestToken rest)
                {
                    ul = rest.TimeSignature.UnitLength;
                    m = rest.TimeSignature.Meter;
                    var dur = rest.Length;
                    m = m.Denominator == 0 ? new Fraction(4, 4) : m;
                    if (rest.MeasureRest)
                        dur *= m.Denominator;
                    else
                        dur *= ul;

                    dur = dur * m.Denominator;

                    yield return new Rest()
                    {
                        Length = dur.Value * spb,
                    }; 
                }
                else if(token is NoteToken note)
				{
                    ChromaticPitch pitch;
                    switch(note.Note.ToUpper()[0])
                    {
                        case 'A':
                            pitch = ChromaticPitch.A;
                            break;
                        case 'B':
                            pitch = ChromaticPitch.B;
                            break;
                        case 'C':
                            pitch = ChromaticPitch.C;
                            break;
                        case 'D':
                            pitch = ChromaticPitch.D;
                            break;
                        case 'E':
                            pitch = ChromaticPitch.E;
                            break;
                        case 'F':
                            pitch = ChromaticPitch.F;
                            break;
                        case 'G':
                            pitch = ChromaticPitch.G;
                            break;
                        default:
                            throw new Exception();
                    }
                    switch(note.Accidental)
                    {
                        case "^^":
                            pitch += 2;
                            break;
                        case "^":
                            pitch += 1;
                            break;
                        case "=":
                            //pitch += 0;
                            break;
                        case "_":
                            pitch -= 1;
                            break;
                        case "__":
                            pitch -= 2;
                            break;
                    }
                    pitch = (ChromaticPitch)(((int)pitch + 13) % 13);

                    ul = note.TimeSignature.UnitLength;
                    m = note.TimeSignature.Meter;
                    m = m.Denominator == 0 ? new Fraction(4, 4) : m;
                    var dur = note.Length * ul * m.Denominator;
                    

                    yield return new Note()
                    {
                        Pitch = pitch,
                        Octave = note.Octave,
                        Length = dur.Value * spb,
                    };
				}
			}
		}

        public class Player : IDisposable
		{
            public Player(int voices = 8)
            {
                _player = new WaveOutEvent();
                _synth = new SynthEngine(44100, voices);
                _player.Init(_synth);
            }

            public WaveOutEvent WavePlayer => _player;
            WaveOutEvent _player;
            public SynthEngine Engine => _synth;
            SynthEngine _synth;

            public static async Task PlaySong(SynthEngine engine, IEnumerable<SimpleNote> notes)
			{
                foreach(var note in notes)
				{
                    if (note is Note n)
                    {
                        await PlayNoteTimed(engine, n.Pitch, n.Octave, n.Length);
                    }
					else if (note is Rest r)
					{
                        await Wait(r.Length);
					}
                }
            }
            public static async Task Wait(float duration)
			{
                await Task.Delay((int)Math.Ceiling(duration * 1000));
            }
           public static async Task PlayNoteTimed(SynthEngine engine, ChromaticPitch pitch, int octave, float duration)
		    {
                engine.Play(pitch, octave);
                await Wait(duration);
                engine.Stop(pitch, octave);
			}


			public void Dispose()
			{
                _player.Stop();
                _player.Dispose();
			}
		}

        public static void Main(string[] args)
        {
            var wd = "";
            var song = "test.abc";

            var path = Path.Combine(wd, song);            
            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    Console.Out.WriteLine("Reading...");
                    var abc = reader.ReadToEnd();
                    Console.Out.WriteLine("\nParsing...");
                    var parser = TuneParser.FromABC(abc);
                    var tokens = StripChordSymbols(parser.Tokens);//Our file used chords to delimit bars?
                    Console.Out.WriteLine("\nPrinting Tokens...");
                    PrintTokens(tokens);

                    Console.Out.WriteLine("\nPrinting Notes...");
                    var bpm = 170f;
                    if(parser.Header.TryGetValue(InfoFields.Tempo.Name,out var tempo))
					{
                        if (float.TryParse(tempo, out var result))
                            bpm = result;
					}
                    var notes = ConvertToNotes(tokens,bpm);
                    PrintNotes(notes);

                    Console.Out.WriteLine("\nPress Any Key To Play Song...");
                    Console.In.ReadLine();

                    Console.Out.WriteLine("\nPlaying Song...");
                    using(var player = new Player())
					{
                        player.WavePlayer.Play();
                        var task = Player.PlaySong(player.Engine, notes);
                        task.Wait();
					}

                    Console.Out.WriteLine("\nPress Any Key To Continue...");
                    Console.In.ReadLine();
                }
            }
        }
        public static void PrintTokens(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                var text = token.Text;
                if (token is NewlineToken)
                {
                    text = "NewLine";
                }
                else if (token is SpaceToken)
                {
                    text = "Space";
                }

                Console.Out.WriteLine($"{token.Line}@{token.Char} '{text}'\t = {token.GetType()}");
            }
        }
        public static void PrintNotes(IEnumerable<SimpleNote> tokens)
        {
            foreach (var token in tokens)
            {
                var text = "???";
                if (token is Rest)
                {
                    text = "Rest";
                }
                else if (token is Note note)
                {
                    text = $"{note.Pitch} {note.Octave}";
                }

                Console.Out.WriteLine($"{text} [{token.Length}]");
            }
        }
        public static IEnumerable<Token> StripChordSymbols(IEnumerable<Token> tokens)
		{
            foreach(var token in tokens)
			{
                if (token is ChordBracketToken)
                    continue;
                yield return token;
			}
		}
    }
}