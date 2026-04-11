using Azure.Data.Tables;
using Shared.Models;
using Shared.Util;

namespace Shared.Repository;

public class UserRepository : IUserRepository
{
    private readonly TableClient _tableClient;

    public UserRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(Constants.UsersTableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<UserEntity?> GetByIdAsync(string userId)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<UserEntity>(Constants.UserPartitionKey, userId);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        var query = _tableClient.QueryAsync<UserEntity>(
            u => u.PartitionKey == Constants.UserPartitionKey && u.Email == email);

        await foreach (var entity in query)
        {
            return entity;
        }

        return null;
    }

    public async Task<UserEntity?> GetByEntraObjectIdAsync(string entraObjectId)
    {
        var query = _tableClient.QueryAsync<UserEntity>(
            u => u.PartitionKey == Constants.UserPartitionKey && u.EntraObjectId == entraObjectId);

        await foreach (var entity in query)
        {
            return entity;
        }

        return null;
    }

    public async Task<IReadOnlyList<UserEntity>> GetAllAsync()
    {
        var users = new List<UserEntity>();
        var query = _tableClient.QueryAsync<UserEntity>(
            u => u.PartitionKey == Constants.UserPartitionKey);

        await foreach (var entity in query)
        {
            users.Add(entity);
        }

        return users;
    }

    public async Task UpsertAsync(UserEntity user)
    {
        user.PartitionKey = Constants.UserPartitionKey;
        await _tableClient.UpsertEntityAsync(user, TableUpdateMode.Replace);
    }

    public async Task DeleteAsync(string userId)
    {
        await _tableClient.DeleteEntityAsync(Constants.UserPartitionKey, userId);
    }
}
