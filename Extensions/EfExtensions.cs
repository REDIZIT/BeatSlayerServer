using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Extensions
{
    public static class DataExtensions
    {
        public static void RemoveRange<TEntity>(
            this DbSet<TEntity> entities,
            System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            var records = entities
                .Where(predicate)
                .ToList();
            if (records.Count > 0)
                entities.RemoveRange(records);
        }

        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            T element;

            for (int i = 0; i < collection.Count; i++)
            {
                element = collection.ElementAt(i);
                if (predicate(element))
                {
                    collection.Remove(element);
                    i--;
                }
            }
        }

        public static bool TryFindAccount(this MyDbContext ctx, string nick, out Account account)
        {
            account = ctx.Players.FirstOrDefault(c => c.Nick == nick);

            return account != null;
        }
    }
}

