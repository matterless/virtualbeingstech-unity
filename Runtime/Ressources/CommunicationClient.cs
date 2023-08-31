// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections;
using UnityEngine;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;

namespace VirtualBeings.Tech.Resources
{
    /// <summary>
    /// This implementation of the ICommunicatonClient is here to showcase a possible way to deal with sounds
    /// and KuteEngine.
    /// Sound capabilities were put in here for mostly two reasons:
    ///  - The ICommunicationClient interface is accessible from the Being and is per-being
    ///  - The communication client is the entry-point for talking / emoting, which can require to emit sounds. Since
    ///    this can be "competing" with the vocalizations, it kinda makes sense that it's handled from there.
    /// Keep in mind that's by no means mandatory.
    ///
    /// This communication client is supposed to be attached to the being prefab, and configured (adding audio sources)
    /// from there.
    /// </summary>
    public class CommunicationClient : MonoBehaviour, ICommunicationClient
    {
        [SerializeField]
        protected AudioSource _faceAudioSource;

        [SerializeField]
        protected AudioSource _bodyAudioSource;

        public virtual void Init(Being being)
        {
            Debug.Assert(_faceAudioSource != null);

            if (_faceAudioSource == null)
            {
                throw new Exception("Primary audio source cannot be null");
            }

            _faceAudioSource.loop        = false;
            _faceAudioSource.playOnAwake = false;
            _faceAudioSource.mute        = false;

            if (_bodyAudioSource != null)
            {
                _bodyAudioSource.loop        = false;
                _bodyAudioSource.playOnAwake = false;
                _bodyAudioSource.mute        = false;
            }
        }

        // ------------------------------
        // Sound emitting functions
        //

        /// <summary>
        /// A vocalization will be requested in the near future, prepare for it.
        /// We basically just stop playing any looping audio here.
        /// </summary>
        public virtual void PrepareForVocalization()
        {
            if (_faceAudioSource.loop)
            {
                _faceAudioSource.loop = false;
                _faceCoroutine = StartCoroutine(FadeOutClip(_faceAudioSource, 0.2f));
            }
        }

        /// <summary>
        /// Effectively emit the requested vocalization.
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="clip">Can happen to be null</param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        public virtual void EmitVocalization(bool loop, AudioClip clip, float volume, float pitch)
        {
            if (clip == null)
            {
                return;
            }

            if (_faceCoroutine != null)
            {
                StopCoroutine(_faceCoroutine);
            }
            _faceCoroutine = null;

            _faceAudioSource.loop   = loop;
            _faceAudioSource.clip   = clip;
            _faceAudioSource.volume = loop ? 0f : volume;
            _faceAudioSource.pitch  = pitch;
            _faceAudioSource.Play();

            if (loop)
            {
                _faceCoroutine = StartCoroutine(FadeInClip(_faceAudioSource, 0.5f, volume));
            }
        }

        /// <summary>
        /// A body sound will be requested in the near future, prepare for it.
        /// We basically just stop playing any looping audio here.
        /// </summary>
        public virtual void PrepareForBodySound()
        {
            if (_bodyAudioSource != null && _bodyAudioSource.loop)
            {
                _bodyAudioSource.loop = false;
                _bodyCoroutine = StartCoroutine(FadeOutClip(_bodyAudioSource, 0.2f));
            }
        }

        /// <summary>
        /// Effectively emit the required body sound.
        /// </summary>
        /// <param name="loop"></param>
        /// <param name="clip">Can happen to be null</param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        public virtual void EmitBodySound(bool loop, AudioClip clip, float volume, float pitch)
        {
            if (clip == null)
            {
                return;
            }

            if (_bodyAudioSource == null)
            {
                return;
            }

            if (_bodyCoroutine != null)
            {
                StopCoroutine(_bodyCoroutine);
            }
            _bodyCoroutine = null;

            _bodyAudioSource.loop   = loop;
            _bodyAudioSource.clip   = clip;
            _bodyAudioSource.volume = loop ? 0f : volume;
            _bodyAudioSource.pitch  = pitch;
            _bodyAudioSource.Play();

            if (loop)
            {
                _bodyCoroutine = StartCoroutine(FadeInClip(_bodyAudioSource, 0.5f, volume));
            }
        }

        /// <summary>
        /// Request a fade out for the given audio source. Will stop looping the currently playing sound.
        /// </summary>
        /// <param name="soundSource"></param>
        /// <param name="fadeoutDuration"></param>
        public virtual void FadeOutSoundSource(SoundSource soundSource, float fadeoutDuration)
        {
            switch (soundSource)
            {
                case SoundSource.Body:
                    if (_bodyAudioSource != null)
                    {
                        _bodyAudioSource.loop = false;
                        _bodyCoroutine        = StartCoroutine(FadeOutClip(_faceAudioSource, fadeoutDuration));
                    }
                    break;

                case SoundSource.Face:
                    _faceAudioSource.loop = false;
                    _faceCoroutine = StartCoroutine(FadeOutClip(_faceAudioSource, fadeoutDuration));
                    break;
            }
        }

        // ------------------------------
        // Communication client interface
        //

        public virtual void DoTextDisplay(string text, float duration)
        {
        }

        public virtual void DoEmoteDisplay(int id, float duration)
        {
        }
        public virtual void DoSpeech(string text, float animationIntensity01, float volume01, float speed01, float pitch)
        {
        }

        public virtual void TerminateSpeech()
        {
        }

        public virtual bool IsSpeaking    => _isSpeaking;
        public virtual bool IsSpeechReady => false;
        public virtual float SpeechDuration => 0f;

        // ------------------------------
        // Sound emitting helpers
        //

        // https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
        // First method
        protected static IEnumerator FadeInClip(
            AudioSource source,
            float duration,
            float targetVolume
        )
        {
            float currentTime = 0f;
            float start       = source.volume;
            while (currentTime < duration)
            {
                currentTime   += Time.deltaTime;
                source.volume =  Mathf.Lerp(start, targetVolume, currentTime / duration);
                yield return null;
            }
        }

        protected IEnumerator FadeOutClip(AudioSource source, float duration)
        {
            float startTime   = Time.time;
            float startVolume = Math.Max(.01f, source.volume);

            while (source.volume > 0f)
            {
                source.volume = startVolume - (Time.time - startTime) / duration * startVolume;
                yield return null;
            }

            source.Stop();
        }

        // ------------------------------
        // Private fields
        //

        protected bool _isSpeaking;

        protected Coroutine _faceCoroutine;
        protected Coroutine _bodyCoroutine;
    }
}
