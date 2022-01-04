using System;
using System.Windows.Forms;
using System.Collections.Generic;
using WMPLib;
using System.Speech.Recognition;
using System.Linq;
using NAudio.Wave;
using System.IO;

namespace Example
{
    public partial class Form1 : Form
    {

        public WindowsMediaPlayer myplayer = new WindowsMediaPlayer();
        public string start = "00:00";
        public string end = "00:00";
        public int segment = 1;
        public List<string> theseLines = new List<string>();
        public bool running = false;






        WaveIn sourceStream;
        WaveFileWriter waveWriter;
        public string FilePath;
        public string FileName = "dictation.wav";



        public void StartRecording(int index)
        {
            sourceStream = new WaveIn
            {
                DeviceNumber = index,
                WaveFormat =
                    new WaveFormat(44100, WaveIn.GetCapabilities(index).Channels)
            };

            sourceStream.DataAvailable += SourceStreamDataAvailable;

            waveWriter = new WaveFileWriter(FilePath + "\\" + FileName, sourceStream.WaveFormat);
            sourceStream.StartRecording();
        }

        public void SourceStreamDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter == null) return;
            waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
            waveWriter.Flush();
        }

        public void RecordEnd()
        {
            if (sourceStream != null)
            {
                sourceStream.StopRecording();
                sourceStream.Dispose();
                sourceStream = null;
            }
            if (waveWriter == null)
            {
                return;
            }
            waveWriter.Dispose();
            waveWriter = null;
        }







        public Form1()
        {
            // This makes the window appear
            InitializeComponent();

        }


        private void button4_Click(object sender, EventArgs e)
        {
            setAudio.Filter = "*.mp3|*.mp3|All files(*.*)|*.*";
            if (setAudio.ShowDialog() == DialogResult.OK)
            {
                myplayer.URL = setAudio.FileName;
                myplayer.controls.stop();
                FilePath = Path.GetDirectoryName(setAudio.FileName);                
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            myplayer.controls.play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            myplayer.controls.pause();

            List<string> existingLines = speechBox.Lines.ToList();
            foreach (string line in existingLines)
            {
                theseLines.Add(line);
            }

            theseLines.Add(segment.ToString());
            theseLines.Add(start + " --> " + myplayer.controls.currentPositionString);
            start = myplayer.controls.currentPositionString;
            segment += 1;
        }

        private void recognizeNow(string fileName)
        {
                // Create an in-process speech recognizer for the en-US locale.  
                using (
                SpeechRecognitionEngine recognizer =
                new SpeechRecognitionEngine(
                new System.Globalization.CultureInfo("en-US")))
                {

                    // Create and load a dictation grammar.  
                    recognizer.LoadGrammar(new DictationGrammar());

                    // Add a handler for the speech recognized event.  
                    recognizer.SpeechRecognized +=
                        new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                    // Configure input to the speech recognizer.  
                    recognizer.SetInputToWaveFile(fileName);

                    // Start speech recognition.
                    recognizer.Recognize();
                }

            }        






        // Handle the SpeechRecognized event.  
        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            theseLines.Add(e.Result.Text);
            theseLines.Add("");
            speechBox.Lines = theseLines.ToArray();
            theseLines.Clear();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            StartRecording(0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RecordEnd();
            recognizeNow(Path.Combine(FilePath, FileName));
        }
    }
}
