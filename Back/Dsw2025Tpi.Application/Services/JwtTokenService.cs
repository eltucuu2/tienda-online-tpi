using Dsw2025Tpi.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration config, ILogger<JwtTokenService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(string username, string role)
        {
            _logger.LogInformation("Generando token JWT para el usuario: {Username} con rol: {Role}", username, role);

            var jwtConfig = _config.GetSection("Jwt");

            var keyText = jwtConfig["Key"];
            if (string.IsNullOrWhiteSpace(keyText))
            {
                _logger.LogError("No se encontró la clave secreta 'Jwt:Key' en la configuración.");
                throw new NotFoundException("Clave JWT (Jwt:Key) no configurada.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

            double expireMinutes = 60; // Valor por defecto
            if (!double.TryParse(jwtConfig["ExpireInMinutes"], out expireMinutes))
            {
                _logger.LogWarning("No se encontró 'ExpireInMinutes' en configuración JWT. Usando valor por defecto: 60 minutos.");
            }

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("Token JWT generado exitosamente para el usuario: {Username}", username);

            return tokenString;
        }
    }
}
