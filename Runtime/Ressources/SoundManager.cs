// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;

namespace VirtualBeings.Tech.Resources
{
    /// <summary>
    /// This sound manager is intended as a starting point / example of how to manage the sound interactions
    /// with beings. For the sake of example, this is expecting a use with the CommunicationClient (same folder), which
    /// is also responsible for emitting speech (see CommunicationClient for more info).
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        [SerializeField]
        private Settings _settings;

        /// <summary>
        /// Register to the sound world events we want to be able to catch, and initialize our shopkeeping lists.
        /// </summary>
        public void Start()
        {
            _worldEvents = Container.Instance.WorldEvents;

            _worldEvents.AddListener<WorldEvent_BeingSoundRequest>(OnSoundRequestEvent);
            _worldEvents.AddListener<WorldEvent_BeingSoundRequestViaAnimCallback>(OnSoundRequestViaAnimCallbackEvent);
            _worldEvents.AddListener<WorldAnimEvent_BeingSoundRequest>(OnSoundRequestViaAnimEvent);
        }

        /// <summary>
        /// On update, we check if any ongoing looped sound needs to terminate, and ask the communication client
        /// to do so if required by KuteEngine.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < _listOngoingSoundEvents.Count; i++)
            {
                OngoingSoundTracker soundEvent = _listOngoingSoundEvents[i];
                if (soundEvent.Being == null || soundEvent.Being.gameObject == null)
                {
                    _listOngoingSoundEvents.RemoveAt(i--);
                }
                else
                {
                    bool shouldTerminate = soundEvent.TerminationPredicate();

                    if (shouldTerminate)
                    {
                        soundEvent.CommunicationClient.FadeOutSoundSource(
                            soundEvent.SoundSource,
                            soundEvent.FadeoutDuration
                        );

                        _listOngoingSoundEvents.RemoveAt(i--);
                    }
                }
            }
        }

        // ------------------------------
        // Audio event handlers
        //

        /// <summary>
        /// Called when we receive a direct sound request event.
        /// We start by clearing the OnGoingSoundTracker list, and then we get the CommunicationClient from the being.
        /// Then, we tell the client to start playing the sound, depending on the source. Finally, we raise a world event
        /// to notify the beings that something audio related is happening.
        /// If there's a termination predicate associated with the sound request, we start tracking it.
        /// </summary>
        /// <param name="soundRequest"></param>
        private void OnSoundRequestEvent(WorldEvent_BeingSoundRequest soundRequest)
        {
            OngoingSoundTracker.PurgeBeingFromList(soundRequest.Being, _listOngoingSoundEvents);

            CommunicationClient client = soundRequest.Being.CommunicationClient as CommunicationClient;

            if (client != null)
            {
                switch (soundRequest.SoundSource)
                {
                    case SoundSource.Body:
                        client.PrepareForBodySound();
                        client.EmitBodySound(
                            soundRequest.Loop,
                            soundRequest.Clip,
                            soundRequest.Volume01,
                            soundRequest.Pitch
                        );
                        break;

                    case SoundSource.Face:
                        client.PrepareForVocalization();
                        client.EmitVocalization(
                            soundRequest.Loop,
                            soundRequest.Clip,
                            soundRequest.Volume01,
                            soundRequest.Pitch
                        );
                        break;
                }

                _worldEvents.Raise(
                    new WorldEvent_Sound(
                        soundRequest.Type,
                        soundRequest.Poignancy01,
                        soundRequest.Being,
                        soundRequest.Being
                    )
                );
            }

            if (soundRequest.TerminationPredicate != null)
            {
                _listOngoingSoundEvents.Add(
                    new OngoingSoundTracker(
                        soundRequest.Being,
                        client,
                        soundRequest.SoundSource,
                        soundRequest.FadeoutDuration,
                        soundRequest.TerminationPredicate
                    )
                );
            }
        }

        /// <summary>
        /// Called when we receive a sound request via animation callback event.
        /// In that case, we only do two things. We tell the communication client some sound will happen on the
        /// given sound source, then we just store the sound request, to be processed when we effectively receive
        /// the anim event.
        /// </summary>
        /// <param name="soundRequest"></param>
        private void OnSoundRequestViaAnimCallbackEvent(WorldEvent_BeingSoundRequestViaAnimCallback soundRequest)
        {
            CommunicationClient communicationClient = soundRequest.Being.CommunicationClient as CommunicationClient;

            if (communicationClient)
            {
                if (soundRequest.SoundSource == SoundSource.Body)
                {
                    communicationClient.PrepareForBodySound();
                }
                else if (soundRequest.SoundSource == SoundSource.Face)
                {
                    communicationClient.PrepareForVocalization();
                }

                QueueSoundRequest(soundRequest);
            }
        }

        /// <summary>
        /// When the animation effectively triggers the callback, we receive this event.
        /// In that case, we look out for the anim event info, given the being and the sound source.
        /// If we properly find it - and it's still valid, we play it, as done for a standard sound request.
        /// </summary>
        /// <param name="soundRequestViaAnim"></param>
        private void OnSoundRequestViaAnimEvent(WorldAnimEvent_BeingSoundRequest soundRequestViaAnim)
        {
            if (TryGetAndClearQueuedSoundRequest(
                    soundRequestViaAnim,
                    out WorldEvent_BeingSoundRequestViaAnimCallback soundRequest
                ))
            {
                CommunicationClient communicationClient = soundRequestViaAnim.Being.CommunicationClient as CommunicationClient;

                if (soundRequest.Being.TopLevelParent != null && communicationClient != null &&
                    soundRequest.ValidUntil > Time.time)
                {
                    OngoingSoundTracker.PurgeBeingFromList(
                        soundRequest.Being,
                        _listOngoingSoundEvents
                    );

                    if (soundRequest.SoundSource == SoundSource.Body)
                    {
                        communicationClient.EmitBodySound(
                            soundRequest.Loop,
                            soundRequest.Clip,
                            soundRequest.Volume01,
                            soundRequest.Pitch
                        );
                    }
                    else if (soundRequest.SoundSource == SoundSource.Face)
                    {
                        communicationClient.EmitVocalization(
                            soundRequest.Loop,
                            soundRequest.Clip,
                            soundRequest.Volume01,
                            soundRequest.Pitch
                        );
                    }

                    _worldEvents.Raise(
                        new WorldEvent_Sound(
                            soundRequest.Type,
                            soundRequest.Poignancy01,
                            soundRequest.Being,
                            soundRequest.Being
                        )
                    );
                }
            }
        }

        // ------------------------------
        // Variables

        /// <summary>
        /// The KuteEngine event manager, which emits the sound events that we need to subscribe to.
        /// </summary>
        private EventManager _worldEvents;
        private readonly List<OngoingSoundTracker> _listOngoingSoundEvents = new(4);
        private readonly Dictionary<Being, Dictionary<SoundSource, WorldEvent_BeingSoundRequestViaAnimCallback>>
            _dictScheduledSoundRequestViaCallbackEvents = new(4);

        void QueueSoundRequest(WorldEvent_BeingSoundRequestViaAnimCallback soundRequest)
        {
            if (!_dictScheduledSoundRequestViaCallbackEvents.TryGetValue(
                    soundRequest.Being,
                    out Dictionary<SoundSource, WorldEvent_BeingSoundRequestViaAnimCallback> soundRequests
                ))
            {
                soundRequests = new Dictionary<SoundSource, WorldEvent_BeingSoundRequestViaAnimCallback>(2);
                _dictScheduledSoundRequestViaCallbackEvents[soundRequest.Being] = soundRequests;
            }

            soundRequests[soundRequest.SoundSource] = soundRequest;
        }

        bool TryGetAndClearQueuedSoundRequest(
            WorldAnimEvent_BeingSoundRequest soundRequest,
            out WorldEvent_BeingSoundRequestViaAnimCallback result
        )
        {
            result = null;

            if (!_dictScheduledSoundRequestViaCallbackEvents.TryGetValue(
                    soundRequest.Being,
                    out Dictionary<SoundSource, WorldEvent_BeingSoundRequestViaAnimCallback> soundRequests
                ))
            {
                return false;
            }

            if (!soundRequests.TryGetValue(soundRequest.SoundSource, out result))
            {
                return false;
            }

            soundRequests.Remove(soundRequest.SoundSource);

            return true;
        }

        // ------------------------------
        // Classes for internal use

        private class OngoingSoundTracker
        {
            public Being               Being;
            public CommunicationClient CommunicationClient;
            public SoundSource         SoundSource;
            public float               FadeoutDuration;
            public Func<bool>          TerminationPredicate;

            public OngoingSoundTracker(
                Being being,
                CommunicationClient client,
                SoundSource soundSource,
                float fadeoutDuration,
                Func<bool> terminationPredicate
            )
            {
                Being                = being;
                CommunicationClient  = client;
                SoundSource          = soundSource;
                FadeoutDuration      = fadeoutDuration;
                TerminationPredicate = terminationPredicate;
            }

            public static void PurgeBeingFromList(Being being, List<OngoingSoundTracker> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (being == list[i].Being)
                    {
                        list.RemoveAt(i--);
                    }
                }
            }
        }

        [Serializable]
        public class Settings
        {
            public float DefaultFadeOutDuration = .3f;
            public float AmbientFadeOutDuration = 5f;
            public float DefaultFadeInDuration  = 1f;
        }
    }
}
