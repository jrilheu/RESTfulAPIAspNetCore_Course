using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public abstract class BookForManipulationDto
    {
        [Required(ErrorMessage = "Title es requerido")]
        [MaxLength(100, ErrorMessage = "Title no puede superar los 100 caracteres")]
        public string Title { get; set; }

        [MaxLength(500, ErrorMessage = "Description no puede superar los 100 caracteres")]
        public virtual string Description { get; set; }
    }
}
