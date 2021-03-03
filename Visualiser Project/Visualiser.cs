﻿using Love;
using NAudio.Wave;
using NAudio.Dsp;
using System;

namespace Visualiser_Project
{
    class Visualiser : VisWindow
    {
        //bar specific variables
        int M = 6;

        //used in both
        WaveBuffer buffer;
        bool hidden = false;
        int visType = 0;
        const int maxVisType = 1;
        /*
         * Changing this to an integer allows more freedom, e.g. if I want to add more types.
         * 0 = Bar
         * 1 = Graph
         */

        //graph specific variables
        int intensity = 2;

        public override void Load()
        {
            WindowTitle = "Visualiser";
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
            buffer = new WaveBuffer(e.Buffer); //saves buffer in the class variable
        }
        public override void KeyPressed(KeyConstant key, Scancode scancode, bool isRepeat)
        {
            base.KeyPressed(key, scancode, isRepeat);

            switch (key)
            {
                case KeyConstant.S:
                    if (visType < maxVisType)
                    {
                        visType += 1;
                    }else if (visType == maxVisType)
                    {
                        visType -= 1;
                    }
                    else
                    {
                        visType = 0; //default to 0 in case of error
                    }
                    break;

                case KeyConstant.H:
                    hidden = !hidden;
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
            if (visType == 0)
            {
                if (hidden == false)
                {
                    Graphics.Print(
                        "Press 'Escape' to exit" +
                        "\nPress 'F' to enter or exit fullscreen mode" +
                        "\nPress 'H' to hide the text" +
                        "\nPress 'S' to change the visualiser style"
                        );
                }

                int len = buffer.FloatBuffer.Length / 8;

                //fft
                Complex[] values = new Complex[len];
                for (int i = 0; i < len; i++)
                {
                    values[i].Y = 0;
                    values[i].X = buffer.FloatBuffer[i];

                }
                FastFourierTransform.FFT(true, M, values);

                float size = (float)WindowWidth / ((float)Math.Pow(2, M));

                Graphics.SetColor(colour.r, colour.g, colour.b);
                for (int i = 1; i < Math.Pow(2, M); i++)
                {
                    Graphics.Rectangle(DrawMode.Fill, (i - 1) * size, WindowHeight, size, -Math.Abs(values[i].X) * (WindowHeight) * 8);
                }
            } else if (visType == 1)
            {
                if (hidden == false)
                {
                    Graphics.Print(
                        "Press 'Escape' to exit" +
                        "\nPress 'F' to enter or exit fullscreen mode" +
                        "\nPress 'H' to hide the text" +
                        "\nPress 'S' to change the visualiser style"
                        );
                }

                int len = buffer.FloatBuffer.Length / 10;
                float spp = (len / 2) / WindowWidth; //samples per pixel

                for (int i = 0; i < WindowWidth; i++)
                {
                    //current sample
                    int x = (int)Math.Round(i * spp);
                    float y = buffer.FloatBuffer[i];

                    //previous sample
                    int prevx = x - 1;
                    int previ = (int)Math.Round((i - 1) * spp);
                    float prevy = buffer.FloatBuffer[Math.Max(previ, 0)]; //Math.Max is used to prevent out of bounds error (0 is used as a fallback).

                    //render graph
                    Graphics.SetColor(colour.r, colour.g, colour.b);
                    Graphics.Line(prevx, WindowHeight / 2 + prevy * (WindowHeight / (intensity * 2)), x, WindowHeight / 2 + y * (WindowHeight / (intensity * 2)));
                }
            }
        }
    }
}