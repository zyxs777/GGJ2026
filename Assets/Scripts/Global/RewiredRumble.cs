using System.Collections.Generic;
using PrimeTween;
using Rewired;
using UnityEngine;

namespace Global
{
    public static class RewiredRumble {
        public enum MotorSide {
            Left,
            Right,
            Both
        }

        // 约定：Rewired motorIndex 0=Left, 1=Right（不符合你的设备就改这里）
        const int LeftMotorIndex = 0;
        const int RightMotorIndex = 1;

        // 记录每个 Joystick 当前正在跑的 rumble tween，避免叠加/残留
        static readonly Dictionary<int, Tween> Active = new(16);

        /// <summary>
        /// 单次震动（使用 PrimeTween 的 Ease 预设曲线采样）
        /// </summary>
        public static void OneShot(
            Rewired.Player player,
            MotorSide side,
            float duration,
            float maxAmplitude = 1f,
            Ease ease = Ease.OutSine,
            bool stopPreviousOnSameJoystick = true
        ) {
            if (player == null || duration <= 0f) return;

            maxAmplitude = Mathf.Clamp01(maxAmplitude);

            var joysticks = player.controllers.Joysticks;
            for (int i = 0; i < joysticks.Count; i++) {
                var j = joysticks[i];
                if (j == null || !j.supportsVibration) continue;

                // 一些设备可能只有 1 个马达
                if (side == MotorSide.Both && j.vibrationMotorCount < 2) {
                    // 退化成只震可用的第一个马达
                    StartOneShotOnJoystick(j, MotorSide.Left, duration, maxAmplitude, ease, stopPreviousOnSameJoystick);
                } else {
                    StartOneShotOnJoystick(j, side, duration, maxAmplitude, ease, stopPreviousOnSameJoystick);
                }
            }
        }

        /// <summary>
        /// 单次震动（使用自定义 AnimationCurve 采样）
        /// curve 的 time/keys 建议是 0..1（否则会按 Evaluate(t) 的结果直接用）
        /// </summary>
        public static void OneShot(
            Rewired.Player player,
            MotorSide side,
            float duration,
            float maxAmplitude,
            AnimationCurve curve,
            bool stopPreviousOnSameJoystick = true
        ) {
            if (player == null || duration <= 0f || curve == null) return;

            maxAmplitude = Mathf.Clamp01(maxAmplitude);

            var joysticks = player.controllers.Joysticks;
            for (int i = 0; i < joysticks.Count; i++) {
                var j = joysticks[i];
                if (j == null || !j.supportsVibration) continue;

                if (side == MotorSide.Both && j.vibrationMotorCount < 2) {
                    StartOneShotOnJoystick(j, MotorSide.Left, duration, maxAmplitude, curve, stopPreviousOnSameJoystick);
                } else {
                    StartOneShotOnJoystick(j, side, duration, maxAmplitude, curve, stopPreviousOnSameJoystick);
                }
            }
        }

        static void StartOneShotOnJoystick(
            Joystick joystick,
            MotorSide side,
            float duration,
            float maxAmplitude,
            Ease ease,
            bool stopPrev
        ) {
            int key = joystick.id;

            if (stopPrev) StopInternal(key, joystick);

            // t: 0..1
            var tween = Tween.Custom(
                startValue: 0f,
                endValue: 1f,
                duration: duration,
                onValueChange: t => ApplyRumbleSample(joystick, side, maxAmplitude, t),
                ease: ease
            ).OnComplete(() => StopMotors(joystick, side));

            Active[key] = tween;
        }

        static void StartOneShotOnJoystick(
            Joystick joystick,
            MotorSide side,
            float duration,
            float maxAmplitude,
            AnimationCurve curve,
            bool stopPrev
        ) {
            int key = joystick.id;

            if (stopPrev) StopInternal(key, joystick);
            var tween = Tween.Custom(
                startValue: 0f,
                endValue: 1f,
                duration: duration,
                onValueChange: t => ApplyRumbleSample(joystick, side, maxAmplitude, t)
                // animationCurve: curve
            ).OnComplete(() => StopMotors(joystick, side));

            Active[key] = tween;
        }

        /// <summary>停止 player 上所有手柄震动 + 清理 tween</summary>
        public static void StopAll(Rewired.Player player) {
            if (player == null) return;
            var joysticks = player.controllers.Joysticks;
            for (var i = 0; i < joysticks.Count; i++) {
                var j = joysticks[i];
                if (j == null) continue;
                StopInternal(j.id, j);
            }
        }

        static void StopInternal(int key, Joystick joystick) {
            if (Active.TryGetValue(key, out var running)) {
                running.Stop();
                Active.Remove(key);
            }
            if (joystick != null) joystick.StopVibration();
        }

        static void ApplyRumbleSample(Joystick j, MotorSide side, float maxAmp, float t01) {
            float amp = Mathf.Clamp01(t01) * maxAmp;

            // Rewired: 按马达索引设置更通用（并且我们要每帧改值）
            switch (side) {
                case MotorSide.Left:
                    if (j.vibrationMotorCount > 0) j.SetVibration(LeftMotorIndex, amp);
                    if (j.vibrationMotorCount > 1) j.SetVibration(RightMotorIndex, 0f);
                    break;
                case MotorSide.Right:
                    if (j.vibrationMotorCount > 0) j.SetVibration(LeftMotorIndex, 0f);
                    if (j.vibrationMotorCount > 1) j.SetVibration(RightMotorIndex, amp);
                    break;
                case MotorSide.Both:
                    if (j.vibrationMotorCount > 0) j.SetVibration(LeftMotorIndex, amp);
                    if (j.vibrationMotorCount > 1) j.SetVibration(RightMotorIndex, amp);
                    break;
            }
        }

        static void StopMotors(Joystick j, MotorSide side) {
            if (j == null) return;

            switch (side) {
                case MotorSide.Left:
                    if (j.vibrationMotorCount > 0) j.SetVibration(LeftMotorIndex, 0f);
                    break;
                case MotorSide.Right:
                    if (j.vibrationMotorCount > 1) j.SetVibration(RightMotorIndex, 0f);
                    break;
                case MotorSide.Both:
                    if (j.vibrationMotorCount > 0) j.SetVibration(LeftMotorIndex, 0f);
                    if (j.vibrationMotorCount > 1) j.SetVibration(RightMotorIndex, 0f);
                    break;
            }

            // 清理 active（如果此 joystick 的 tween 已经结束，会在下一次 OneShot 覆盖；这里不强依赖）
            Active.Remove(j.id);
        }
    }
}
