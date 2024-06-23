using GestorNotificacionesApi.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GestorNotificacionesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GestorMensajeriaController : ControllerBase
    {

        private readonly Utils _utils;

        public GestorMensajeriaController(Utils utils)
        {
            _utils = utils;
        }

        [HttpPost]
        public string EncriptarString(string stringAconvertir) 
        {
            //encriptar
            string encryptedConnectionString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stringAconvertir));
            return encryptedConnectionString;
        }

        [HttpGet]
        public IActionResult GestionarNotificaciones() 
        {
            List<EmailDTO> notificaciones = new List<EmailDTO>();
            using (SqlConnection conn = _utils.GetConnection()) 
            { 
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_obtener_notificaciones_pendientes", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read()) 
                {
                    notificaciones.Add(new EmailDTO { Id = (int)dr["IdNotificacion"], Subject = (string)dr["Asunto"], 
                        Body = (string)dr["Cuerpo"], AddressTo = (string)dr["Correo"] });
                }
                conn.Close();
            }
            EnviarCorreos(notificaciones);

            return Ok();
        }

        private async void EnviarCorreos(List<EmailDTO> notificaciones) 
        {
            foreach (EmailDTO notificacion in notificaciones) 
            {
                using HttpResponseMessage response = await _utils.GetAPIHost().PostAsJsonAsync(_utils.GetEmailAPI(), notificacion);
                if (response.IsSuccessStatusCode) 
                {
                    //ALGO
                }
            }
        }
    }
}
