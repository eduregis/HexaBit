using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    public enum DynamicParameter {
        Damage,
        Cooldown,
        Projectiles,
        Speed,
        Range,
        Angle,
        Size,
        Knockback,
        ExtraTargets,
        SplashArea,
        Radius,
        SlowAmount,
        MaxDistance,
        Hitbox,
        Duration,
        Tick
    }

    public static class DynamicParameterExtensions {
        public static string ToFieldName(this DynamicParameter parameter) {
            return parameter.ToString();
        }

        // Maps the enum to its default type automatically
        public static DynamicFieldType GetFieldType(this DynamicParameter parameter) {
            return parameter switch {
                DynamicParameter.Projectiles => DynamicFieldType.Int,
                DynamicParameter.ExtraTargets => DynamicFieldType.Int,

                DynamicParameter.Speed => DynamicFieldType.Float,
                DynamicParameter.Range => DynamicFieldType.Float,
                DynamicParameter.Angle => DynamicFieldType.Float,
                DynamicParameter.Size => DynamicFieldType.Float,
                DynamicParameter.Knockback => DynamicFieldType.Float,
                DynamicParameter.SplashArea => DynamicFieldType.Float,
                DynamicParameter.Radius => DynamicFieldType.Float,
                DynamicParameter.SlowAmount => DynamicFieldType.Float,
                DynamicParameter.MaxDistance => DynamicFieldType.Float,
                DynamicParameter.Hitbox => DynamicFieldType.Float,
                DynamicParameter.Duration => DynamicFieldType.Float,
                DynamicParameter.Tick => DynamicFieldType.Float,

                _ => DynamicFieldType.Float
            };
        }
    }
}