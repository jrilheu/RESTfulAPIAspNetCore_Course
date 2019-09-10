using System.ComponentModel.DataAnnotations;

namespace Library.API.Models
{
    public class BookForUpdateDto : BookForManipulationDto
    {
        [Required(ErrorMessage = "Description es requerido")]
        public override string Description
        {
            get => base.Description; 
            set => base.Description = value; 
        }
    }
}
