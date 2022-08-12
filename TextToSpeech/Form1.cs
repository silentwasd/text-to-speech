using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextToSpeech
{
    public partial class Form1 : Form
    {
        string[] availableWords;

        WaveOutEvent outputDevice;

        public Form1()
        {
            InitializeComponent();
            LoadVoice();
        }

        private void LoadVoice()
        {
            var files = Directory.GetFiles("./words", "*.wav");

            var words = new List<string>();

            foreach (var file in files)
            {
                words.Add(Path.GetFileNameWithoutExtension(file));
            }

            var directories = Directory.GetDirectories("./words");

            foreach (var directory in directories)
            {
                words.Add(Path.GetFileName(directory));
            }

            words.Sort();
            availableWords = words.ToArray();

            listBox1.Items.AddRange(availableWords);
        }

        private ISampleProvider RenderVoice(string[] lines)
        {
            var samples = new List<ISampleProvider>();
            var rand = new Random();

            for (int l = 0; l < lines.Length; l++)
            {
                string[] words = lines[l].Split(' ');

                for (int w = 0; w < words.Length; w++)
                {
                    string _word = words[w].ToLower();

                    if (String.IsNullOrEmpty(_word))
                        continue;

                    if (File.Exists("./words/" + _word + ".wav"))
                    {
                        samples.Add(new AudioFileReader("./words/" + _word + ".wav"));
                    }
                    else if (Directory.Exists("./words/" + _word))
                    {
                        var files = Directory.GetFiles("./words/" + _word);
                        samples.Add(new AudioFileReader(files[rand.Next(0, files.Length)]));
                    }

                    if (w < words.Length - 1)
                        samples.Add(new SilenceProvider(samples[0].WaveFormat).ToSampleProvider().Take(TimeSpan.FromMilliseconds(50)));
                }

                if (l < lines.Length - 1)
                    samples.Add(new SilenceProvider(samples[0].WaveFormat).ToSampleProvider().Take(TimeSpan.FromMilliseconds(300)));
            }

            return new ConcatenatingSampleProvider(samples);
        }

        private void PlayVoice(ISampleProvider provider)
        {
            if (outputDevice == null)
            {
                outputDevice = new WaveOutEvent();
                outputDevice.PlaybackStopped += OnPlaybackStopped;
            }

            if (outputDevice.PlaybackState == PlaybackState.Playing)
                outputDevice.Stop();

            outputDevice.Init(provider);
            outputDevice.Play();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var text = textBox1.Text;

            string[] lines = text.Split("\r\n".ToCharArray());

            var voice = RenderVoice(lines);

            PlayVoice(voice);
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            string substring = "";

            int selectionStart = textBox1.SelectionStart;
            int offset = 1;

            while (selectionStart > 0)
            {
                substring = text.Substring(--selectionStart, offset++);

                if (substring.StartsWith(" ") || substring.StartsWith("\n"))
                {
                    substring = substring.Substring(1);
                    break;
                }
            }

            listBox2.Items.Clear();

            if (String.IsNullOrEmpty(substring))
                return;

            listBox2.Items.AddRange(availableWords.Where(word => word.StartsWith(substring.ToLower())).ToArray());
        }
    }
}
