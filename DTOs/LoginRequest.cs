using System.ComponentModel.DataAnnotations;

namespace Inmobiliaria.Net8.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
        public string Correo_Electronico { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La clave es requerida")]
        public string Clave { get; set; } = string.Empty;
        
        public bool RecordarUsuario { get; set; }
    }
}

