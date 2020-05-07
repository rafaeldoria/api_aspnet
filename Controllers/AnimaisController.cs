using System.Linq;
using aspapi.Data;
using aspapi.models;
using Microsoft.AspNetCore.Mvc;
using aspapi.HATEOAS;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System;

namespace aspapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    
    public class AnimaisController : ControllerBase
    {
        private readonly ApplicationDBContext database;
        private HATEOAS.HATEOAS HATEOAS;

        public AnimaisController(ApplicationDBContext database){
            this.database = database;
            HATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/Animais");
            HATEOAS.AddAction("GET_INFO","GET");
            HATEOAS.AddAction("DELETE_ANIMAL","DELETE");
            HATEOAS.AddAction("EDIT_ANIMAL","EDIT");
        }

        [HttpGet("teste")]
        public IActionResult TesteCLaims(){
            return Ok(HttpContext.User.Claims.First(claim => claim.Type.ToString().Equals("id", StringComparison.InvariantCultureIgnoreCase)).Value);
        }

        [HttpGet]
        public IActionResult Get(){
            var animais = database.Animais.ToList();
            List<AnimalContainer> animaisHATEOAS = new List<AnimalContainer>();
            foreach (var animal in animais)
            {
                AnimalContainer animalHATEOAS = new AnimalContainer();
                animalHATEOAS.animal = animal;
                animalHATEOAS.links = HATEOAS.GetActions(animal.Id.ToString());
                animaisHATEOAS.Add(animalHATEOAS);
            }
            return Ok(new {result = "success", animaisHATEOAS});
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id){
            try
            {
                var animal = database.Animais.First(a => a.Id == id);
                AnimalContainer animalHATEOAS = new AnimalContainer();
                animalHATEOAS.animal = animal;
                animalHATEOAS.links = HATEOAS.GetActions(animal.Id.ToString());
                return Ok(new { result = "success", animalHATEOAS });
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new{ result = "error", msg = "Animal não encontrado"});
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] AnimalTemp at){
            if((at.Name == "" || at.Name.Length <= 3) || (at.Type == "" || at.Type.Length < 3)){
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Parametros inválidos"});
            }

            Animal animal = new Animal();
            animal.Name = at.Name;
            animal.Type = at.Type;
            database.Animais.Add(animal);
            database.SaveChanges();
            Response.StatusCode = 201;
            return new ObjectResult(new { msg = "Animal adicionado com sucesso" });
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] Animal animal)
        {
            if(animal.Id > 0){
                try
                {
                    var at = database.Animais.First(atemp => atemp.Id == animal.Id);
                    if(at != null){
                        at.Name = animal.Name != null ? animal.Name : at.Name;
                        at.Type = animal.Type != null ? animal.Type : at.Type;
                        
                        database.SaveChanges();
                    }else{
                        Response.StatusCode = 404;
                        return new ObjectResult(new { result = "error", msg = "Animal não encontrado" });
                    }
                }
                catch (System.Exception)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(new { result = "error", msg = "Animal não encontrado" });
                }
            }else{
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Parametros inválidos" });
            }
            Response.StatusCode = 201;
            return new ObjectResult(new { msg = "Animal atualizado com sucesso" });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                Animal animal = database.Animais.First(p => p.Id == id);
                database.Animais.Remove(animal);
                database.SaveChanges();
                return Ok(animal);
            }
            catch (System.Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new { result = "error", msg = "Animal não encontrado" });
            }
        }

        public class AnimalTemp{
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public class AnimalContainer{
            public Animal animal;
            public Link[] links;
        }
    }
}