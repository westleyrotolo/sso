using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SsoServer.Data.Seeding;

namespace SsoServer.Services;

public class ClientService : IClientService
{
    private readonly ConfigurationDbContext _configurationDbContext;
    public ClientService(ConfigurationDbContext configurationDbContext)
    {
        _configurationDbContext = configurationDbContext ?? throw new ArgumentNullException(nameof(configurationDbContext));
    }
    public  Task<Client> AddClientAsync(string clientId, string clientName, string clientDescritpion)
    {
        return TryAddClient(CreateClient(clientId, clientName, clientDescritpion));
    }

    public async Task<Client> UpdateClientAsync(string clientId, string clientName, string clientDescritpion)
    {
        var client = CreateClient(clientId, clientName, clientDescritpion);
        _configurationDbContext.Clients.Update(client.ToEntity());
        await _configurationDbContext.SaveChangesAsync();
        return client;
    }

    public async Task<List<Client>> GetAllClientAsync()
    {
        var clients = await _configurationDbContext.Clients.ToListAsync();
        return clients.Select(x => x.ToModel()).ToList();
    }

    public async Task<Client> GetByIdAsync(string clientId)
    {
        var client = await _configurationDbContext.Clients.Where(x => x.ClientId == clientId).FirstAsync();

        return client.ToModel();
    }

    public async Task DeleteAsync(string clientId)
    {
        var client = await _configurationDbContext.Clients.FindAsync(clientId);
        if (client == null) return;
        _configurationDbContext.Clients.Remove(client);
        await _configurationDbContext.SaveChangesAsync();
    }

    private Client CreateClient(string clientId, string clientName, string description)
    {
        return new()
        {
            
            ClientId = clientId,
            ClientName = clientName,
            Description = description,
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            // Secret code
            
            AllowedScopes =
                    {
                
                        "api1",
                        "api2",
                        IdentityServerConfigurationDbSeeder.QEApi
                    },
           
            RequireClientSecret = true,
            ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
        };
    }

    private async Task<Client> TryAddClient(Client client)
    {
        if (!_configurationDbContext.Clients.Any(x => x.ClientId == client.ClientId))
        {
            var entity = client.ToEntity();
            var x = await _configurationDbContext.Clients.AddAsync(client.ToEntity());
            client = x.Entity.ToModel();
        }
        await _configurationDbContext.SaveChangesAsync();
        Log.Information($"Successfully added client: {client.ClientName}");
        return client;
    }
}