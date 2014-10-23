using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Net;




namespace soundRecordingTest
{


    public class SoundRecording : Microsoft.Xna.Framework.Game
    {
        Microphone mic = Microphone.Default;
        DynamicSoundEffectInstance DSEI;
        bool isMicrophoneRecording;
        byte[] buffer = new byte[Microphone.Default.GetSampleSizeInBytes(TimeSpan.FromSeconds(6.0))];
        int bytesRead = 0;
  
        public SoundRecording()
        {
            DSEI = new DynamicSoundEffectInstance(44100, AudioChannels.Mono);
           
            if (!isMicrophoneRecording)
            {
                // we are starting to record
    
                Microphone.Default.Start();
            }

            isMicrophoneRecording = !isMicrophoneRecording;
            
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
           
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            bytesRead += Microphone.Default.GetData(buffer, bytesRead, (buffer.Length - bytesRead));
          
            if (gameTime.TotalGameTime.Seconds >= 2)
            {
                if (DSEI.PendingBufferCount == 0)
                {        
                    //for (int i = 0; i < buffer.Length; i++)
                    //{
                    //    buffer[i] = 55;
                    //}

                    DSEI.SubmitBuffer(buffer);
                }
                else
                {
                    DSEI.Play();


           
                }
             
                if (!isMicrophoneRecording)
                {

     


                }

                else
                {

                    if (Microphone.Default.State != MicrophoneState.Stopped)
                    {
                        Microphone.Default.Stop();
                        isMicrophoneRecording = !isMicrophoneRecording;
                    }
                }


            }
            base.Update(gameTime);
        }


      
    }
}
