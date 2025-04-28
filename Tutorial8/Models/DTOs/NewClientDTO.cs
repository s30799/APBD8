using System.ComponentModel.DataAnnotations;

namespace Tutorial8.Models.DTOs;

public class NewClientDTO
{
    [Required(ErrorMessage ="Name is required")]
    [StringLength(50, ErrorMessage ="Name cannot be longer than 50 characters")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage ="Surname is required")]
    [StringLength(50, ErrorMessage ="Surname cannot be longer than 50 characters")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage ="Email is required")]
    [StringLength(50, ErrorMessage ="Email cannot be longer than 50 characters")]
    public string Email { get; set; }
    
    [Required(ErrorMessage ="Phone number is required")]
    [StringLength(20, ErrorMessage ="Phone number cannot be longer than 20 characters")]
    public string Phone { get; set; }
    
    [Required(ErrorMessage ="Pesel is required")]
    [StringLength(11, MinimumLength = 11, ErrorMessage ="Pesel cannot be longer than 11 characters")]
    public string Pesel { get; set; }
}