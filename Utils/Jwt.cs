using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoRH.Models;
using Microsoft.IdentityModel.Tokens;

namespace GestaoRH.Utils;

public static class Jwt
{
    private static string GetSecretKey(IConfiguration config)
        => config["Jwt:SecretKey"]
           ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");

    public static string GenerateToken(Empresa empresa, IConfiguration config, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim("Id",           empresa.Id.ToString()),
            new Claim("Cnpj",         empresa.Cnpj),
            new Claim("RazaoSocial",  empresa.RazaoSocial),
            new Claim("Responsavel",  $"{empresa.ResponsavelNome} {empresa.ResponsavelSobrenome}"),
            new Claim("Perfil",       "empresa")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateFuncionarioToken(Models.Funcionario funcionario, IConfiguration config, int expireMinutes = 60)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim("Id",       funcionario.Id.ToString()),
            new Claim("Cpf",      funcionario.Cpf),
            new Claim("Nome",     funcionario.Nome),
            new Claim("Email",    funcionario.Email),
            new Claim("SetorId",  funcionario.SetorId.ToString()),
            new Claim("Perfil",   "funcionario")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(GetSecretKey(config));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
