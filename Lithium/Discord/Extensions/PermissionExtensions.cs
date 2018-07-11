﻿namespace Lithium.Discord.Extensions
{
    using System;
    using System.Linq;

    using global::Discord;
    using global::Discord.WebSocket;

    public static class PermissionExtensions
    {
        public static SocketGuildUser CastToSocketGuildUser(this SocketUser currentUser)
        {
            if (currentUser is SocketGuildUser user)
            {
                return user;
            }

            throw new InvalidOperationException("User is unable to be cast to a SocketGuildUser");
        }

        public static bool IsHigherRankedThan(this SocketUser currentUser, SocketUser compareUser, SocketGuild guild)
        {
            var currentGuildUser = guild.GetUser(currentUser.Id);
            var compareGuildUser = guild.GetUser(compareUser.Id);
            if (currentUser == null || compareUser == null || currentGuildUser == null || compareGuildUser == null)
            {
                throw new NullReferenceException("Specified users cannot be null and must be a member of the target guild");
            }

            return IsHigherRankedThan(currentGuildUser, compareGuildUser);
        }

        public static bool IsHigherRankedThan(this SocketGuildUser currentUser, SocketGuildUser compareUser)
        {
            if (currentUser == null || compareUser == null)
            {
                throw new NullReferenceException("Specified users cannot be null.");
            }

            if (currentUser.Guild.Id != compareUser.Guild.Id)
            {
                throw new InvalidOperationException("SocketGuildUsers must be referencing the same guild");
            }

            if (currentUser.Roles.Max(x => x.Position) > compareUser.Roles.Max(x => x.Position))
            {
                return true;
            }

            return false;
        }

        public static bool IsHigherThan(this IRole currentRole, IRole compare)
        {
            if (currentRole.Position > compare.Position)
            {
                return true;
            }

            return false;
        }
    }
}
