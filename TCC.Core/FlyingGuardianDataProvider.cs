﻿using System;
using TCC.Data;
using TCC.Parsing.Messages;

namespace TCC
{
    public static class FlyingGuardianDataProvider
    {
        private const uint AirEssenceId = 630400;
        private const uint FireEssenceId = 630500;
        private const uint SparkEssenceId = 631001;

        private static int _stacks;
        private static FlightStackType _stackType;
        private static bool _ignoreNextEnd;

        public static event Action<int> StacksChanged;
        public static event Action<FlightStackType> StackTypeChanged;
        public static event Action<bool> IsInProgressChanged;

        public static int Stacks
        {
            get => _stacks;
            set
            {
                if (_stacks == value) return;
                _stacks = value;
                StacksChanged?.Invoke(_stacks);
            }
        }

        public static FlightStackType StackType
        {
            get => _stackType;
            set
            {
                if(_stackType == value) return;
                _stackType = value;
                StackTypeChanged?.Invoke(_stackType);
            }
        }

        private static FlightStackType IdToStackType(uint id)
        {
            if (id == FireEssenceId) return FlightStackType.Fire;
            if (id == SparkEssenceId) return FlightStackType.Spark;
            if (id == AirEssenceId) return FlightStackType.Air;
            return FlightStackType.None;
        }
        private static bool IsEssence(uint id)
        {
            return id == AirEssenceId || id == FireEssenceId || id == SparkEssenceId;
        }

        public static bool IsInProgress => EntityManager.IsEntitySpawned(630, 9998) ||
                                           EntityManager.IsEntitySpawned(630, 2100) ||
                                           EntityManager.IsEntitySpawned(630, 2101) ||
                                           EntityManager.IsEntitySpawned(630, 2102) ||
                                           EntityManager.IsEntitySpawned(630, 2103) ||
                                           EntityManager.IsEntitySpawned(630, 2104) ||
                                           EntityManager.IsEntitySpawned(631, 1001) ||
                                           EntityManager.IsEntitySpawned(631, 1002) ||
                                           EntityManager.IsEntitySpawned(631, 3001) ||
                                           EntityManager.IsEntitySpawned(631, 9998);

        public static void InvokeProgressChanged()
        {
            IsInProgressChanged?.Invoke(IsInProgress);
        }

        public static void HandleAbnormal(S_ABNORMALITY_END p)
        {
            if (!IsEssence(p.AbnormalityId)) return;
            if (_ignoreNextEnd)
            {
                _ignoreNextEnd = false;
                return;
            }
            Stacks = 0;
        }

        public static void HandleAbnormal(S_ABNORMALITY_REFRESH p)
        {
            if (!IsEssence(p.AbnormalityId)) return;
            Stacks = p.Stacks;
            StackType = IdToStackType(p.AbnormalityId);
        }

        public static void HandleAbnormal(S_ABNORMALITY_BEGIN p)
        {
            if (!IsEssence(p.AbnormalityId)) return;
            if (IdToStackType(p.AbnormalityId) != StackType) _ignoreNextEnd = true;
            Stacks = p.Stacks;
            StackType = IdToStackType(p.AbnormalityId);
        }

    }
}
