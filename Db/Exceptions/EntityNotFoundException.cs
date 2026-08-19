namespace Finder.Bot.Db.Exceptions;

public class EntityNotFoundException<T>(ulong id)
    : Exception($"Item {typeof(T).Name} with id '{id}' was not found.")
    where T : ICosmosItem;