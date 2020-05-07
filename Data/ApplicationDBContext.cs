using System;
using System.Collections.Generic;
using System.Text;
using aspapi.models;
using Microsoft.EntityFrameworkCore;

namespace aspapi.Data
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<Animal> Animais {get; set;}
        public DbSet<Usuario> Usuarios {get; set;}
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
        : base(options)
        {

        }
    }
}