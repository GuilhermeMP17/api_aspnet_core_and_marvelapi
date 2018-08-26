using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using apidotnetcore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace apidotnetcore.Controllers
{
    [Route("api/[controller]")]
    public class heroController : Controller
    {
        [HttpGet]
        public IActionResult Hero([FromServices]IConfiguration config)
        {
            hero personagem;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                string ts = DateTime.Now.Ticks.ToString();
                string publicKey = config.GetSection("MarvelComicsAPI:PublicKey").Value;
                string hash = GerarHash(ts, publicKey,
                    config.GetSection("MarvelComicsAPI:PrivateKey").Value);
                
                HttpResponseMessage response = client.GetAsync(
                    config.GetSection("MarvelComicsAPI:BaseURL").Value +
                    $"characters?ts={ts}&apikey={publicKey}&hash={hash}&" +
                    $"name={Uri.EscapeUriString("Captain America")}").Result;
                
                try
                {
                response.EnsureSuccessStatusCode();
                string conteudo =
                    response.Content.ReadAsStringAsync().Result;

                dynamic resultado = JsonConvert.DeserializeObject(conteudo);

                personagem = new hero();
                personagem.Nome = resultado.data.results[0].name;
                personagem.Descricao = resultado.data.results[0].description;
                personagem.UrlImagem = resultado.data.results[0].thumbnail.path + "." +
                resultado.data.results[0].thumbnail.extension;
                personagem.UrlWiki = resultado.data.results[0].urls[1].url;
                return Ok(personagem);
                }catch{
                    return Ok("erro");
                }
            }

            //return Ok(personagem);
        }

        private string GerarHash(
            string ts, string publicKey, string privateKey)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(ts + privateKey + publicKey);
            var gerador = MD5.Create();
            byte[] bytesHash = gerador.ComputeHash(bytes);
            return BitConverter.ToString(bytesHash)
                .ToLower().Replace("-", String.Empty);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}