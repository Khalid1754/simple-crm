using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Data;

/// <summary>
/// Fyller databasen med lite testdata första gången appen startar,
/// så att det finns något att titta på direkt.
/// </summary>
public static class SeedData
{
    public static async Task InitializeAsync(CrmDbContext db)
    {
        // Skapa en demo-användare om den inte redan finns.
        if (!await db.Users.AnyAsync())
        {
            var user = new User { Email = "demo@crm.se" };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "Demo123!");
            db.Users.Add(user);
        }

        // Lägg bara in kunder om tabellen är tom.
        if (!await db.Customers.AnyAsync())
        {
            var acme = new Customer
            {
                Name = "Acme AB",
                OrgNumber = "556000-0001",
                Email = "info@acme.se",
                Phone = "08-111 111",
                City = "Stockholm",
                Contacts =
                {
                    new Contact { FirstName = "Anna", LastName = "Andersson", Email = "anna@acme.se", JobTitle = "VD" },
                    new Contact { FirstName = "Erik", LastName = "Berg", Email = "erik@acme.se", JobTitle = "Inköpare" }
                },
                Deals =
                {
                    new Deal { Title = "Nytt affärssystem", Amount = 250000, Stage = DealStage.Proposal },
                    new Deal { Title = "Supportavtal", Amount = 40000, Stage = DealStage.Won }
                }
            };

            var nordic = new Customer
            {
                Name = "Nordic Bygg AB",
                Email = "kontakt@nordicbygg.se",
                City = "Göteborg",
                Contacts =
                {
                    new Contact { FirstName = "Sara", LastName = "Lind", Email = "sara@nordicbygg.se", JobTitle = "Projektledare" }
                },
                Deals =
                {
                    new Deal { Title = "Webbplats", Amount = 80000, Stage = DealStage.Lead }
                }
            };

            var cafe = new Customer { Name = "Café Solsidan", City = "Malmö", Phone = "040-123 45" };

            db.Customers.AddRange(acme, nordic, cafe);
        }

        await db.SaveChangesAsync();
    }
}
