// ======================================================================
// This file contains proprietary technology owned by Virtual Beings SAS.
// Copyright 2011-2023 Virtual Beings SAS.
// ======================================================================

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VirtualBeings.Tech.ActiveCognition;
using VirtualBeings.Tech.BehaviorComposition;
using VirtualBeings.Tech.UnityIntegration;
using VirtualBeings.Tech.Utils;

namespace VirtualBeings.Tech.Shared
{
    /// <summary>
    /// Contains the data of the player
    /// </summary>
    [BinaryReadableWritableClass]
    [CreateAssetMenu(fileName = "PlayerDataAssetDb", menuName = "VirtualBeings/PlayerDataAssetDb")]
    public class PlayerData : ScriptableObject, ISerializationCallbackReceiver
    {
        #region serialized fields and their getter

        // _beingsState does not partipate in the creation of the default PlayerData.
        // this field is not updated during runtime.
        [BinaryReadableWritableField(7)]
        private BeingData[] _beingsState;

        // the ID of a standalone inventory item or a multi part inventory item. By checking the ID, we can find out if it refers to a multi-part inventory item or standalone inventory item.
        [BinaryReadableWritableField(10)]
        [SerializeField]
        [HideInInspector]
        private int _nextAvailableInventoryItemID = 0;

        [BinaryReadableWritableField(14)]
        [SerializeField]
        private bool _leftHandPreference;
        public bool LeftHandPreference
        {
            get => _leftHandPreference;
            set => _leftHandPreference = value;
        }


        [BinaryReadableWritableField(16)]
        [SerializeField]
        private float _soundEffectsVolume = 0.5f;
        public float SoundEffectsVolume
        {
            get => _soundEffectsVolume;
            set => _soundEffectsVolume = value;
        }

        #endregion

        #region fields and properties

        private DateTime _lastTimePlayerDataUpdated = DateTime.MinValue;

        // the following dictionary is only set at start up then never touched during the whole game session.
        public IReadOnlyDictionary<int, BeingData> BeingStatePerID { get; private set; }
        public Set<IInteractable> InstantiatedBeings => _interactionDb.GetInteractablesOfType(typeof(Being));

        private Dictionary<int, BeingInsidePlayerAreaData> _beingsTimeEntered;

        /**************************************************************************************************************************/

        private BeingManager _gameManager;
        private EventManager _worldEvents;
        private InteractionDB _interactionDb;
        //private DiContainer _container;

        #endregion

        #region private API & Data structures

        private struct BeingInsidePlayerAreaData
        {
            public readonly float Time;

            // maybe add more fields here in the future...

            public BeingInsidePlayerAreaData(float time)
            {
                Time = time;
            }
        }

        private void UpdateSerializedFields()
        {
        }

        #endregion

        #region editor time public API

#if UNITY_EDITOR

        // return playerData asset.
        // generating a default player data does not include all the state of the game at the moment the request is performed.
        // IMPORTANT: relationship progression with beings (_beingsState), paper balls (_giftsLeftByBeings, _giftsLeftByGame, _undeliveredBeingRewards), and feeders content (_feedersData) is not saved!
        public PlayerData EditorSavePlayerDataAsAsset(string playerDataPath, Dictionary<int, string> worldAssetPlayerDataEntriesPath, Dictionary<int, string> multiPartWorldAssetPlayerDataEntriesPath)
        {
            UpdateSerializedFields();

            AssetDatabase.DeleteAsset(playerDataPath);
            AssetDatabase.CreateAsset(CreateInstance<PlayerData>(), playerDataPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlayerData playerDataAsset = AssetDatabase.LoadAssetAtPath<PlayerData>(playerDataPath);

            // only set the values to the serialized fields. the others are unnecessary...
            playerDataAsset._nextAvailableInventoryItemID = _nextAvailableInventoryItemID;
            playerDataAsset._leftHandPreference = _leftHandPreference;
            playerDataAsset._soundEffectsVolume = _soundEffectsVolume;

            _beingsState = null;

            EditorUtility.SetDirty(playerDataAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return playerDataAsset;
        }

#endif

        #endregion

        #region SERIALIZATION_CALLBACK_RECEIVER

        public void OnBeforeSerialize()
        {
            UpdateSerializedFields();
        }

        public void OnAfterDeserialize()
        {

        }

        #endregion

    }
}
