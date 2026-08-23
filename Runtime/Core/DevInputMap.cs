using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    /// The central Model that stores hardware-aware key bindings and routes raw input events to their assigned tools.
    /// Supports multi-file modularity and explicit asset overriding via the 'overrideFor' field.
    [CreateAssetMenu(menuName = "GameLib/Debug/DevKeyboardShortcuts/Input Map", fileName = "DevInputMap")]
    public class DevInputMap : ScriptableObject
    {
        private static List<DevInputMap> _activeMaps;

        /// Returns all currently active input maps loaded from Resources, automatically removing any maps that have been overridden or disabled.
        public static IReadOnlyList<DevInputMap> ActiveMaps
        {
            get
            {
                if (_activeMaps == null || _activeMaps.Count == 0 || _activeMaps.Exists(m => m == null || !m))
                {
                    ReloadActiveMaps();
                }
                return _activeMaps;
            }
        }

        [Header("Override Settings")]
        [Tooltip("If enabled, this entire input map will be completely ignored at runtime and will not override any base maps.")]
        public bool isDisabled = false;

        [Tooltip("If assigned, this asset will completely override and disable the referenced base map at runtime.")]
        public DevInputMap overrideFor;

        [Header("Debug Settings")]
        [Tooltip("If enabled, prints diagnostic logs, VK Codes, and Device Names to the console.")]
        public bool printRawInputToConsole = false;

        [Header("Configured Bindings")]
        public List<DevKeyBinding> bindings = new List<DevKeyBinding>();

        /// Runtime O(1) lookup dictionary mapping virtual key codes directly to their relevant bindings
        private Dictionary<int, List<DevKeyBinding>> _bindingsByKeyCode;

        private void OnEnable()
        {
            _activeMaps = null;
            _bindingsByKeyCode = null;
        }

        private void OnValidate()
        {
            /// Invalidate pre-indexed cache whenever bindings or states are modified live in the Inspector
            _bindingsByKeyCode = null;
            
            // Force a reload of active maps so toggling 'isDisabled' at runtime takes effect instantly
            _activeMaps = null; 
        }

        /// Pre-indexes active bindings into an O(1) dictionary keyed by virtual key code
        private void BuildKeyCodeIndex()
        {
            _bindingsByKeyCode = new Dictionary<int, List<DevKeyBinding>>();
            if (bindings == null) return;

            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (!binding.isEnabled || binding.boundTool == null) continue;

                if (!_bindingsByKeyCode.TryGetValue(binding.virtualKeyCode, out var list))
                {
                    list = new List<DevKeyBinding>();
                    _bindingsByKeyCode[binding.virtualKeyCode] = list;
                }
                list.Add(binding);
            }
        }

        /// Scans Resources for all DevInputMap assets and strips out disabled maps and overridden maps.
        public static void ReloadActiveMaps()
        {
            var allMaps = Resources.LoadAll<DevInputMap>("");
            var overriddenMaps = new HashSet<DevInputMap>();

            // 1. Gather all legitimate overrides (ignoring overrides coming from disabled maps)
            foreach (var map in allMaps)
            {
                if (map != null && map && !map.isDisabled && map.overrideFor != null)
                {
                    overriddenMaps.Add(map.overrideFor);
                }
            }

            _activeMaps = new List<DevInputMap>();

            // 2. Build the final active list
            foreach (var map in allMaps)
            {
                if (map == null || !map) continue;

                if (map.isDisabled)
                {
                    if (map.printRawInputToConsole)
                    {
                        Debug.Log($"[DevInputMap] Skipping map '{map.name}' because it is marked as Disabled.");
                    }
                    continue;
                }

                if (overriddenMaps.Contains(map))
                {
                    if (map.printRawInputToConsole)
                    {
                        Debug.Log($"[DevInputMap] Suppressing base map '{map.name}' because an active override map is targeting it.");
                    }
                    continue;
                }

                /// Build the O(1) key code index immediately when the map becomes active
                map.BuildKeyCodeIndex();
                _activeMaps.Add(map);
            }
        }

        /// Checks if any currently active map has console debugging enabled.
        public static bool IsAnyDebugEnabled()
        {
            var maps = ActiveMaps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].printRawInputToConsole) return true;
            }
            return false;
        }

        /// Evaluates an incoming raw input press across all active modular input maps.
        public static void ProcessAllRawKeyPresses(int vkCode, string deviceName)
        {
            var maps = ActiveMaps;
            if (maps == null || maps.Count == 0)
            {
                Debug.LogWarning("[DevInputMap] No active DevInputMap assets found in Resources!");
                return;
            }

            bool isDebug = IsAnyDebugEnabled();

            if (isDebug)
            {
                Debug.Log($"[DevTools Raw Input] VK Code: {vkCode} (Hex: 0x{vkCode:X2}) | Device: '{deviceName}'");
            }

            int totalMatches = 0;
            for (int i = 0; i < maps.Count; i++)
            {
                totalMatches += maps[i].ProcessRawKeyPress(vkCode, deviceName, isDebug);
            }

            if (totalMatches == 0 && isDebug)
            {
                Debug.Log($"[DevInputMap] No matching shortcuts found across {maps.Count} active map(s) for VK: {vkCode} on device '{deviceName}'.");
            }
        }

        /// Evaluates an incoming raw input press against this specific map's pre-indexed bindings in O(1) time.
        public int ProcessRawKeyPress(int vkCode, string deviceName, bool isDebug)
        {
            // Failsafe: Double-check disabled state just in case
            if (isDisabled) return 0;

            if (_bindingsByKeyCode == null)
            {
                BuildKeyCodeIndex();
            }

            /// O(1) lookup: instantly skip evaluation if no active bindings use this key code
            if (!_bindingsByKeyCode.TryGetValue(vkCode, out var matchingBindings) || matchingBindings.Count == 0)
            {
                return 0;
            }

            int matchCount = 0;
            for (int i = 0; i < matchingBindings.Count; i++)
            {
                var binding = matchingBindings[i];
                if (binding.Matches(vkCode, deviceName))
                {
                    matchCount++;
                    if (isDebug || printRawInputToConsole)
                    {
                        Debug.Log($"[DevTools] Executing Tool: '{binding.boundTool?.name ?? "NULL TOOL"}' from Device: '{binding.deviceFriendlyName}' (Map: '{name}')");
                    }
                    
                    if (binding.boundTool != null)
                    {
                        binding.boundTool.Execute();
                    }
                    else
                    {
                        Debug.LogError($"[DevInputMap] Shortcut matched in map '{name}' (VK: {vkCode}), but the Assigned Tool is NULL!");
                    }
                }
            }
            return matchCount;
        }
    }
}