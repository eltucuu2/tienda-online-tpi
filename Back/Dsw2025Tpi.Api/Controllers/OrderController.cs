using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Dsw2025Tpi.Application.Dtos.OrderModel;

namespace Dsw2025Tpi.Api.Controllers
{
    // Define que esta clase es un controlador API
    [ApiController]

    // Requiere autorización por defecto para todos los métodos
    [Authorize]

    // Ruta base para todos los endpoints del controlador
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        // Servicio que contiene la lógica de negocio para la gestión de órdenes
        private readonly IOrdersManagementService _service;

        // Constructor con inyección del servicio de órdenes
        public OrderController(IOrdersManagementService service)
        {
            _service = service;
        }

        // Endpoint para que un cliente cree una nueva orden
        [HttpPost()]
        [Authorize(Roles = "Customer")]
        
        public async Task<IActionResult> AddOrder([FromBody] OrderModel.Request request)
        {
            // Crea una orden y la guarda en base de datos
            var order = await _service.AddOrder(request);

            // Devuelve 201 Created con la ruta del nuevo recurso
            return Created($"/api/order/{order.Id}", order);
        }

        // Endpoint para obtener una lista paginada de órdenes (admin y cliente)
       [HttpGet]
[Authorize(Roles = "Admin, Customer")]
public async Task<IActionResult> GetOrders(
   [FromQuery] OrderStatus? status,
   [FromQuery] Guid? customerId,
   [FromQuery] int pageNumber = 1,
   [FromQuery] int pageSize = 10)
{
    var orders = await _service.GetOrders(status, customerId, pageNumber, pageSize);

    var result = new
    {
        orders.Total,
        OrderItems = orders.OrderItems.Select(order => new
        {
            order.Id,
            order.CustomerId,
             order.CustomerName,   // <--- LO QUE TE FALTABA
            order.Date,
            order.Status,
            order.TotalAmount
        })
    };

    return Ok(result);
}

        // Endpoint para obtener los detalles de una orden por ID
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Customer")]
        
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            // Busca la orden por su identificador
            var order = await _service.GetOrderById(id);

            // Devuelve los datos de la orden, incluyendo los ítems y el total
            var result = new
            {
                order.Id,
                order.CustomerId,
                CustomerName = order.Customer?.Name,  // ← agregado
                order.ShippingAddress,
                order.BillingAddress,
                Date = order.Date.ToString("dd/MM/yyyyTHH:mm:ss"),
                order.Status,
                OrderItems = order.Items.Select(oi => new
                {
                    oi.ProductId,
                    oi.Quantity,
                    oi.UnitPrice,
                    oi.Subtotal
                }),
                Total = order.TotalAmount
            };

            return Ok(result);
        }

        // Endpoint para actualizar el estado de una orden (solo admin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            // Cambia el estado de la orden utilizando el servicio
            var updatedOrder = await _service.UpdateOrderStatus(id, request.NewStatus);

            // Devuelve los datos actualizados de la orden
            return Ok(updatedOrder);
        }
    }
}
