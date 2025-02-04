﻿using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        private static class Leader
        { // Leader/Follower
            public static Grid grid;
            public static Vector3D offset;
            private static Grid leaderGrid;
            private static string followerGridName = "";
            private static double distance;

            public static void ProcessLeaderMessageOnFollower(string str)
            {
                // Check if tagged as a follower using gridName for reference, if not tag, then exit out
                if (!Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, FOLLOW_TAG, ref followerGridName))
                {
                    return;
                }

                Serializer.InitUnpack(str);
                leaderGrid = Serializer.UnpackLeaderGrid();
                if (leaderGrid.name == followerGridName)
                {
                    Logger.Err("Follower & leader name is same");
                    return;
                }
                grid = leaderGrid;
                if (Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, FOLLOW_FRONT_TAG, ref followerGridName) && double.TryParse(followerGridName, out distance))
                {
                    //Logger.Info("FollowFront: " + distance.ToString());
                    offset.X = distance;
                }
                else
                {
                    offset.X = 5.0;
                }
                if (Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, FOLLOW_UP_TAG, ref followerGridName) && double.TryParse(followerGridName, out distance))
                {
                    //Logger.Info("FollowUp: " + distance.ToString());
                    offset.Z = distance;
                }
                else
                {
                    offset.Z = 5.0;
                }
                if (Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, FOLLOW_RIGHT_TAG, ref followerGridName) && double.TryParse(followerGridName, out distance))
                {
                    //Logger.Info("FollowRight: " + distance.ToString());
                    offset.Y = distance;
                }
                else
                {
                    offset.Y = 5.0;
                }
            }

            public static void AdvertiseLeader(Program program)
            {
                // If no SAM.LEADER tag exit
                if (!Block.HasProperty(GridBlocks.masterProgrammableBlock.EntityId, LEADER_TAG))
                {
                    return;
                }

                // If not remote control found exit and throw error to Logger LCD
                if (RemoteControl.block == null)
                {
                    Logger.Err(MSG_NO_REMOTE_CONTROL);
                    return;
                }
                leaderGrid.pos = RemoteControl.block.CenterOfMass;

                // If dont have grids name, get it
                if (!Block.GetProperty(GridBlocks.masterProgrammableBlock.EntityId, NAME_TAG, ref followerGridName))
                {
                    followerGridName = GridBlocks.masterProgrammableBlock.CubeGrid.CustomName;
                }

                leaderGrid.name = followerGridName; // Set leaderGrid name to grid name returned from Grid Info screen
                leaderGrid.radius = GridBlocks.masterProgrammableBlock.CubeGrid.WorldVolume.Radius; // Comes out at ~4.43
                leaderGrid.linearVel = RemoteControl.block.GetShipVelocities().LinearVelocity;
                leaderGrid.up = RemoteControl.block.WorldMatrix.Up;
                leaderGrid.fwd = RemoteControl.block.WorldMatrix.Forward;
                Serializer.InitPack();
                Serializer.Pack(leaderGrid);
                program.IGC.SendBroadcastMessage<string>(LEADER_TAG, Serializer.serialized); // Intergrid communication broadcast of tag
            }
        }
    }
}
