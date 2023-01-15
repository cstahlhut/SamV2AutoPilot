using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        private static class GridProfile
        { // Profiles
            private static string[] empty = new string[] { };
            private static string[] ignore = new string[] { IGNORE_TAG };
            private static string[] name = new string[] { NAME_TAG };
            private static string[] meTags = new string[] { "DEBUG", ADVERTISE_TAG, ESCAPE_NOSE_UP_TAG, 
                SLOW_ON_APPROACH_TAG, ALLOW_DIRECT_ALIGNMENT_TAG, NO_DAMPENERS_TAG, IGNORE_GRAVITY_TAG,
                LEADER_TAG };
            private static string[] exclusiveTags = new string[] { LIST_MODE_TAG, LOOP_MODE_TAG };
            private static string[] pbAttributes = new string[]{
                FOLLOW_TAG, FOLLOW_FRONT_TAG, FOLLOW_UP_TAG, FOLLOW_RIGHT_TAG, 
                NAME_TAG, MAX_SPEED_TAG, WAIT_TAG, TAXI_DISTANCE_TAG,
                APPROACH_DISTANCE_TAG, DOCK_DISTANCE_TAG, UNDOCK_DISTANCE_TAG,
                DOCK_SPEED_TAG, TAXI_SPEED_TAG, MAX_SPEED_TAG, AGGRO_TAG,
                CONVERGING_SPEED_TAG, MASS_EXCESS_TAG, TAXI_PANEL_DISTANCE_TAG,
                APPROACH_SPEED_TAG, EFFECTIVE_THRUST_TAG,
                ESCAPE_NOSE_UP_ELEVATION_TAG, DESCEND_NOSE_DOWN_ELEVATION_TAG};
            private static string[] textPanelTags = new string[] { "OVR" };
            private static string[] textPanelExclusiveTags = new string[] { "LOG", "NAV", "CONF",
                "DATA", "STAT" };
            private static string[] connectorTags = new string[] { CONNECTOR_REVERSE_TAG, MAIN_CONNECTOR_TAG };
            public static string[] panelTags = new string[] { "Panel0", "Panel1", "Panel2", "Panel3",
                "Panel4", "Panel5", "Panel6", "Panel7", "Panel8", "Panel9" };
            private static string[] pbAttributes = new string[]{
                FOLLOW_TAG, FOLLOW_FRONT_TAG, FOLLOW_UP_TAG, FOLLOW_RIGHT_TAG,
                NAME_TAG, MAX_SPEED_TAG, WAIT_TAG, TAXI_SPEED_TAG,
                CONVERGING_SPEED_TAG, APPROACH_DISTANCE_TAG, DOCK_DISTANCE_TAG,
                DOCK_SPEED_TAG, UNDOCK_DISTANCE_TAG, APPROACH_SPEED_TAG,
                TAXI_DISTANCE_TAG, ARRIVAL_SPEED_TAG,
                ARRIVAL_DISTANCE_TAG, ESCAPE_NOSE_UP_ELEVATION_TAG, AUTO_CRUISE_ATTRIBUTE,
                DESCEND_NOSE_DOWN_ELEVATION_TAG,
                AGGRO_TAG,  MASS_EXCESS_TAG,
                TAXI_PANEL_DISTANCE_TAG, EFFECTIVE_THRUST_TAG};
            private static string[] exclusiveTags = new string[] { LIST_MODE_TAG, LOOP_MODE_TAG };
            private static string[] cockpitPanelTags = new string[] { "OVR" };
            public static string[] panelTags = new string[] { "Panel0", "Panel1", "Panel2", "Panel3", "Panel4", "Panel5", "Panel6", "Panel7", "Panel8", "Panel9" };
            private static string[] connectorTags = new string[] { REVERSE_CONNECTOR_TAG, MAIN_CONNECTOR_TAG };
            private static string[] timerTags = new string[] { "DOCKED", "NAVIGATED", "STARTED",
                "UNDOCKED", "APPROACHING"};
            public static string[] capacity = new string[] { "Full", "Empty" };
            private static string[] batteryTags = new string[] { "FORCE" };
            private static string[] gastankTags = new string[] { "FORCE", "CARGO" };

            // ** IN SC this is 'me' **
            public static BlockProfile blockProfile = new BlockProfile(ref meTags, ref exclusiveTags, ref pbAttributes);
            public static Dictionary<Type, BlockProfile> blockProfileDict = new Dictionary<Type, BlockProfile>{{typeof(IMyProgrammableBlock),blockProfile},
                {typeof(IMyRemoteControl),new BlockProfile(ref empty,ref empty,ref empty)},
                {typeof(IMyCameraBlock),new BlockProfile(ref empty,ref empty,ref empty)},
                {typeof(IMyRadioAntenna),new BlockProfile(ref empty,ref empty,ref empty)},
                {typeof(IMyLaserAntenna),new BlockProfile(ref empty,ref empty,ref empty)},
                {typeof(IMyShipConnector),new BlockProfile(ref connectorTags,ref empty,ref name)},
                {typeof(IMyTextPanel),new BlockProfile(ref textPanelTags,ref textPanelExclusiveTags,ref name)},
                {typeof(IMyCockpit),new BlockProfile(ref cockpitPanelTags,ref empty,ref panelTags)},
                {typeof(IMyTimerBlock),new BlockProfile(ref timerTags,ref empty,ref empty)},
                {typeof(IMyBatteryBlock),new BlockProfile(ref batteryTags,ref empty,ref capacity)},
                {typeof(IMyGasTank),new BlockProfile(ref gastankTags,ref empty,ref capacity)},
                {typeof(IMyCargoContainer),new BlockProfile(ref empty,ref empty,ref capacity)},
                {typeof(IMyThrust),new BlockProfile(ref ignore,ref empty,ref empty)},};
        }
    }
}
