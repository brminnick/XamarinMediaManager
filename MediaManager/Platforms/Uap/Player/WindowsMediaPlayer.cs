﻿using System;
using System.Threading.Tasks;
using MediaManager.Media;
using MediaManager.Platforms.Uap.Player;
using MediaManager.Platforms.Uap.Video;
using MediaManager.Playback;
using MediaManager.Video;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace MediaManager.Platforms.Uap.Media
{
    public class WindowsMediaPlayer : IMediaPlayer<MediaPlayer, VideoView>
    {
        public WindowsMediaPlayer()
        {
            Initialize();
        }

        protected MediaManagerImplementation MediaManager = CrossMediaManager.Windows;

        public VideoView PlayerView { get; set; }
        public IVideoView VideoView => PlayerView;

        public MediaPlayer Player { get; set; }

        public Playback.MediaPlayerState State => Player.PlaybackSession.PlaybackState.ToMediaPlayerState();

        public RepeatMode RepeatMode { get; set; }

        public event BeforePlayingEventHandler BeforePlaying;
        public event AfterPlayingEventHandler AfterPlaying;

        public void Initialize()
        {
            if (Player != null)
                return;

            Player = new MediaPlayer();
            Player.MediaEnded += Player_MediaEnded;
            Player.MediaFailed += Player_MediaFailed;
            Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            MediaManager.OnStateChanged(this, new StateChangedEventArgs(sender.PlaybackState.ToMediaPlayerState()));
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            MediaManager.OnMediaItemFailed(this, new MediaItemFailedEventArgs(MediaManager.MediaQueue.Current, new Exception(args.ErrorMessage), args.ErrorMessage));
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            MediaManager.OnMediaItemFinished(this, new MediaItemEventArgs(MediaManager.MediaQueue.Current));
        }

        public Task Pause()
        {
            Player.Pause();
            return Task.CompletedTask;
        }

        public async Task Play(IMediaItem mediaItem)
        {
            BeforePlaying?.Invoke(this, new MediaPlayerEventArgs(mediaItem, this));

            var mediaPlaybackList = new MediaPlaybackList();
            var item = new MediaPlaybackItem(mediaItem.ToMediaSource());
            mediaPlaybackList.Items.Add(item);
            Player.Source = mediaPlaybackList;
            await Play();

            AfterPlaying?.Invoke(this, new MediaPlayerEventArgs(mediaItem, this));
        }

        public Task Play()
        {
            Player.Play();
            return Task.CompletedTask;
        }

        public Task SeekTo(TimeSpan position)
        {
            Player.PlaybackSession.Position = position;
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            Player.Pause();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Player = null;
        }
    }
}
