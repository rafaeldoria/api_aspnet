using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using aspapi.Data;
using aspapi.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace aspapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDBContext database;

        public UsuariosController(ApplicationDBContext database)
        {
            this.database = database;            
        }
        [HttpPost("registro")]
        public IActionResult Registro([FromBody] Usuario usuario){
            database.Add(usuario);
            database.SaveChanges();
            return Ok(new{msg="Usuario Cadastrado com Sucesso"});
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Usuario credenciais){
            try
            {
                Usuario usuario = database.Usuarios.First(user => user.Email.Equals(credenciais.Email));
                if(usuario != null){
                    if(usuario.Password.Equals(credenciais.Password)){
                        
                        string chaveDeSeguranca = "tasdisvrevjk2334vfc,s434fsdsf466,ar32kjsdfj";
                        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveDeSeguranca));
                        var keys = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);

                        var claims = new List<Claim>();
                        claims.Add(new Claim("Id", usuario.Id.ToString()));
                        claims.Add(new Claim("Email", usuario.Email));
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));

                        var JWTToken = new JwtSecurityToken(
                            issuer: "petshop", //quem esta fornecendo o JWT para o usuario
                            expires: DateTime.Now.AddHours(1),
                            audience: "usuario_comum",
                            signingCredentials: keys,
                            claims: claims
                        );

                        return Ok(new{
                            status= "success",
                            token = new JwtSecurityTokenHandler().WriteToken(JWTToken)
                        });

                    }else{
                        Response.StatusCode = 401;
                        return new ObjectResult("");
                    }
                }else{
                    Response.StatusCode = 401;
                    return new ObjectResult("");
                }
            }
            catch (System.Exception)
            {
                Response.StatusCode = 401;
                return new ObjectResult("");
            }
        }
    }
}