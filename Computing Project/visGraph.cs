using System;
using Love;
using NAudio.Wave;

namespace Computing_Project
{
    class visGraph : visWindow
    {
        private WaveBuffer buffer;
        private int intensity = 4;

        public override void Load()
        {
            WindowTitle = "Graph Visualiser";
            base.Load();

            //start audio capture
            var capture = new WasapiLoopbackCapture();
            capture.DataAvailable += DataAvailable;
            capture.RecordingStopped += (s, a) =>
            {
                capture.Dispose();
            };
            capture.StartRecording();
        }
        public void DataAvailable(object sender, WaveInEventArgs e)
        {
            buffer = new WaveBuffer(e.Buffer);
        }
        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);
            switch (key)
            {
                case KeyConstant.Up:
                    if (intensity < 10)
                    {
                        intensity += 1;
                    }
                    break;

                case KeyConstant.Down:
                    if(intensity > 1)
                    {
                        intensity -= 1;
                    }
                    break;
            }
        }
        public override void Draw()
        {
            Graphics.SetColor(1, 1, 1);
            if (buffer == null)
            {
                Graphics.Print("No buffer available");
                return;
            }
            Graphics.Print(
                "Press 'Escape' to exit" +
                "\nPress 'F' to enter or exit fullscreen mode" +
                "\nPress 'up' to increase intensity." +
                "\nPress 'down' to decrease intensity" +
                "\nCurrent intensity = " + intensity.ToString()
                );
            
            int len = buffer.FloatBuffer.Length / 10;
            int spp = len / WindowWidth; //samples per pixel

            for (int i = 0; i < len; i += spp)
            {
                //current sample
                int x = i;
                float y = buffer.FloatBuffer[i];

                //previous sample
                int prevx = i - 1;
                float prevy = buffer.FloatBuffer[Math.Max((i - spp), 0)]; //Math.Max is used to prevent out of bounds error (0 is used as a fallback).

                //render graph
                Graphics.SetColor(colour.r, colour.g, colour.b);
                Graphics.Line(prevx, WindowHeight / 2 + prevy * (WindowHeight / (intensity * 2)), x, WindowHeight / 2 + y * (WindowHeight / (intensity * 2)));
                /*
                 * For some reason the line is not "stable", it duplicates the line if it moves too fast.
                 * Going to look into this.
                 */
            }
        }
    }
}