using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly Dsw2025TpiContext _context;

        public AuthenticateController(UserManager<IdentityUser> userManager,
                                      SignInManager<IdentityUser> signInManager,
                                      JwtTokenService jwtTokenService,
                                      RoleManager<IdentityRole> roleManager,
                                      Dsw2025TpiContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var user = await _userManager.FindByNameAsync(request.Username) ?? throw new UnauthorizedException("Usuario o contraseña incorrectos");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Usuario o contraseña incorrectos");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Customer";

            var token = _jwtTokenService.GenerateToken(request.Username, userRole);

           
            return Ok(new 
            { 
                token = token,
                userId = user.Id,
                role = userRole
            });
        }

        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("El nombre de usuario y el email son obligatorios.");

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                throw new BadRequestException("Ya existe un usuario registrado con ese email.");

            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
                throw new BadRequestException("El nombre de usuario ya está en uso.");

            // --- NUEVO: Validar Teléfono para Admin ---
            if (!string.IsNullOrEmpty(model.PhoneNumber) && _userManager.Users.Any(u => u.PhoneNumber == model.PhoneNumber))
            {
                throw new BadRequestException("El número de teléfono ya está registrado en el sistema.");
            }
            // ------------------------------------------

            // --- NUEVO: Guardar Teléfono ---
            var user = new IdentityUser 
            { 
                UserName = model.Username, 
                Email = model.Email,
                PhoneNumber = model.PhoneNumber // <--- Agregado
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            return Ok("Usuario registrado correctamente con rol de administrador.");
        }

        [HttpPost("registerCustomer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
        {
            // ... (Validaciones anteriores igual que antes) ...
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("El nombre de usuario y el email son obligatorios.");

            // ... (Chequeos de usuario existente igual que antes) ...

            // Crear el usuario de Login (Identity)
            var user = new IdentityUser 
            { 
                UserName = model.Username, 
                Email = model.Email,
                PhoneNumber = model.PhoneNumber 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded) throw new BadRequestException("Error al asignar rol");

            
            // Creamos también el Cliente en la tabla de negocio con el MISMO ID
            try 
            {
                var newCustomer = new Customer
                {
                    Id = Guid.Parse(user.Id), // Usamos el mismo ID que el Login
                    Name = model.Username,
                    Email = model.Email,
                    Phone = model.PhoneNumber
                };
                
                _context.Set<Customer>().Add(newCustomer);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si falla esto, hay que borrar el usuario para no dejar datos huerfanos
                await _userManager.DeleteAsync(user);
                throw new BadRequestException("Error al crear el perfil de cliente: " + ex.Message);
            }
            // -----------------------------------------------------------

            return Ok("Usuario registrado correctamente con rol de cliente.");
        }
    }
}