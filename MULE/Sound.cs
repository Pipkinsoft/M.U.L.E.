using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SdlDotNet.Audio;

namespace MULE
{
    public enum Sounds
    {
        IntroMusic,
        Ship,
        Beep1,
        Beep2,
        Beep3,
        Beep4,
        Auction,
        Pub,
        Walk1,
        Walk2,
        Tick,
        Tock,
        BadNews,
        //GoodNews,
        Production1,
        Production2,
        Production3,
        Production4,
        Production5,
        MarketBeep,
        Dry,
        Transaction
    }

    public class Sound
    {
        private Game game;

        private SdlDotNet.Audio.Sound[] sounds;
        private Channel[] channels;
        private int numChannels;

        public Sound(Game game)
        {
            this.game = game;
            numChannels = 8;
            initializeSounds();
        }

        private void initializeSounds()
        {
            sounds = 
                new SdlDotNet.Audio.Sound[Enum.GetNames(typeof(Sounds)).Length];
            
            for (int i = 0; i < Enum.GetNames(typeof(Sounds)).Length; i++)
                sounds[i] =
                    new SdlDotNet.Audio.Sound(
                        game.AppPath + 
                        "sfx" + Path.DirectorySeparatorChar + 
                        ((Sounds)i).ToString().ToLower() + ".ogg"
                        );

            channels = new Channel[numChannels];

            for (int i = 0; i < numChannels; i++)
                channels[i] = new Channel(i);
        }

        private Channel getEmptyChannel()
        {
            for (int i = 0; i < numChannels; i++)
                if (!channels[i].IsPlaying())
                    return channels[i];

            return null;
        }

        public void PlayBeep(int playerNum)
        {
            switch ((playerNum + 1) % 4)
            {
                case 0: PlaySound(Sounds.Beep1); break;
                case 1: PlaySound(Sounds.Beep2); break;
                case 2: PlaySound(Sounds.Beep3); break;
                case 3: PlaySound(Sounds.Beep4); break;
            }
        }

        public void PlaySound(Sounds sound, int volume = 128)
        {
            Channel channel = getEmptyChannel();

            if (channel != null)
            {
                channel.Volume = volume;
                channel.Play(
                    sounds[(int)sound], 
                    sound == Sounds.IntroMusic
                    );
            }
        }

        public void StopSound(Sounds sound)
        {
            for (int i = 0; i < numChannels; i++)
                if (channels[i].Sound == sounds[(int)sound])
                    channels[i].Stop();
        }
    }
}
