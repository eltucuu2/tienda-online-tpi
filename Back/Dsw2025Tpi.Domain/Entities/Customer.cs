using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Domain.Entities
{
    // Clase que representa un Cliente, hereda de EntityBase para incluir la propiedad Id
    public class Customer : EntityBase
    {
        // Email del cliente, puede ser null
        public string? Email { get; set; }

        // Nombre del cliente, puede ser null
        public string? Name { get; set; }

        // Teléfono del cliente, puede ser null
        public string? Phone { get; set; }

        // Colección de pedidos (Orders) asociados a este cliente
        // Inicializada para evitar null reference y permitir añadir elementos
        public ICollection<Order> Orders { get; } = new HashSet<Order>();

        // Constructor vacío requerido para Entity Framework y serialización
        public Customer()
        {
        }

        // Constructor que permite crear un cliente inicializando sus propiedades principales
        public Customer(string email, string name, string phone)
        {
            Email = email;
            Name = name;
            Phone = phone;
        }
    }
}
