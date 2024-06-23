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

        [HttpPost("/EncriptarTexto")]
        public string EncriptarString(string stringAconvertir)
        {
            //encriptar
            string encryptedConnectionString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(stringAconvertir));
            return encryptedConnectionString;
        }

        [HttpGet("/GestionarNotificaciones")]
        public IActionResult GestionarNotificaciones()
        {
            List<EmailDTO> notificaciones = new List<EmailDTO>();
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_obtener_notificaciones_pendientes", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    notificaciones.Add(new EmailDTO { Id = (int)dr["IdNotificacion"], Subject = (string)dr["Asunto"],
                        Body = (string)dr["Cuerpo"], AddressTo = (string)dr["Correo"] });
                }
                conn.Close();
            }
            EnviarCorreos(notificaciones);

            return Ok();
        }


        [HttpPost("/CrearEstudiante")]
        public IActionResult CrearEstudiante(EstudianteDTO infoEstudiante) 
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_crear_estudiante", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Nombre", infoEstudiante.Nombre);
                cmd.Parameters.AddWithValue("@Apellidos", infoEstudiante.Apellidos);
                cmd.Parameters.AddWithValue("@Carne", infoEstudiante.Carne);
                cmd.Parameters.AddWithValue("@Correo", infoEstudiante.Correo);
                cmd.Parameters.AddWithValue("@Telefono", infoEstudiante.Telefono);

                SqlDataReader dr = cmd.ExecuteReader();
                conn.Close();
            }
            return Ok();
        }

        [HttpPost("/ActivarEstudiante")]
        public IActionResult ActivarEstudiante(string carne)
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_activar_estudiante", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Carne", carne);

                SqlDataReader dr = cmd.ExecuteReader();
                conn.Close();
            }
            return Ok();
        }

        private void MarcarNotificacionEnviada(int idNotificacion) 
        {
            using (SqlConnection conn = _utils.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_marcar_notificacion_enviada", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdNotificacion", idNotificacion);

                SqlDataReader dr = cmd.ExecuteReader();
                conn.Close();
            }
        }

        private async void EnviarCorreos(List<EmailDTO> notificaciones) 
        {
            foreach (EmailDTO notificacion in notificaciones) 
            {
                using HttpResponseMessage response = await _utils.GetAPIHost().PostAsJsonAsync(_utils.GetEmailAPI(), notificacion);
                if (response.IsSuccessStatusCode) 
                {
                    MarcarNotificacionEnviada(notificacion.Id);
                }
            }
        }
    }
}
